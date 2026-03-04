Alright — moving into **`sli.symbol_pointer.v0.1.0`** as the next freeze-grade bridge piece.

You already locked the fault-line:

* **Layer 0 (`gel.*`)**: inert identity storage (EngramId, ParentTip, canonical seals)
* **Layer 1 (`sli.*`)**: structural routing + addressability (no salience, no perspective)
* **Layer 2 (`iutt.*`)**: runtime cognition (salience/perspective/gradients), never identity unless re-committed

So `sli.symbol_pointer` is *the* formal object that makes “symbol → stable target” executable **without importing Layer-2 meaning**.

I’m going to write this in the same “Build Contracts/ freeze” style you’re using in the registry text, and I’ll keep it consistent with the Layer rules and the already-frozen `sli.tensor` + `iutt.runtime_state` + `iutt.reconstruction_profile` docs.    

---

## sli.symbol_pointer.v0.1.0

### Symbol Pointer Object (Layer 1, Structural Only)

### 0. Purpose

`**sli.symbol_pointer.v0.1.0**` defines a **structural, morphism-safe pointer** used by the SLI + Lisp duplex layer to resolve:

* **Symbolic reference → target identity anchor**, typically a `gel.golden_engram.*` `EngramId`, *or* a bootstrap root/atlas/index entry, *or* a non-identity address (Channel×Partition×Mirror).

It exists to prevent developers from “accidentally” using raw strings, ad-hoc handles, or runtime context as pointers.

**SymbolPointer is not identity.**
It must never affect `EngramId`, `ParentTip`, or canonical sealing.

---

### 1. Non-Goals (Hard Boundary)

`SLI Symbol Pointer MUST NOT:`

* include salience, relevance, gradients, perspective transforms, basin scores
* include query context or operator intent beyond structural provenance
* be appended to GEL as identity (unless a new, explicit Commit handle is declared and the pointer itself becomes part of a GoldenEngram payload, which is a separate act)
* perform heuristic “best match” resolution

If you want “best match,” that’s **Layer 2** behavior, and it must be logged and replayable there.

---

### 2. Determinism Contract

Given the same:

* `SymbolText` (canonical form),
* `Namespace`,
* `PointerKind`,
* `TargetRef`,

…the pointer’s canonical bytes and `PointerHash` must be identical across machines.

No environment-local IDs. No wall-clock. No GUID.

---

### 3. Schema ID

`**sli.symbol_pointer.v0.1.0**`

---

### 4. Canonical Record (C#)

```csharp
public record SliSymbolPointer_v0_1_0(
    // --- Schema ---
    string Schema,                       // "sli.symbol_pointer.v0.1.0"

    // --- Pointer identity (Layer 1 structural; NOT GEL identity) ---
    string PointerHash,                  // sha256(canonicalBytes(pointer without PointerHash))
    string CanonicalSeal,                // sha256(full canonical serialization)

    // --- Symbol side (what is being referenced) ---
    string Namespace,                    // e.g. "sli", "gel", "bootstrap", "rootatlas"
    string SymbolText,                   // canonical symbol string (see §5)
    string SymbolEncoding,               // "utf-8" (frozen for v0.1.0)

    // --- Pointer kind (what type of target this is) ---
    string PointerKind,                  // "engram_id" | "bootstrap_handle" | "resolved_address" | "external_ref"

    // --- Target side (where it points) ---
    SymbolTarget Target,                 // see §4.1

    // --- Structural provenance (optional but recommended) ---
    PointerProvenance Provenance         // see §4.2
);
```

#### 4.1 SymbolTarget

```csharp
public record SymbolTarget(
    string TargetRef,                    // e.g. "sha256:<hex>" (for EngramId), or "engram.bootstrap.rootatlas"
    string ResolvedAddress,              // Channel×Partition×Mirror (optional if known)
    string TargetSchemaHint,             // e.g. "gel.golden_engram.v0.1.0" (optional)
    string[] TargetTags                  // sorted; structural only (e.g. "BOOTSTRAP", "ROOT", "4P:propositional")
);
```

#### 4.2 PointerProvenance

```csharp
public record PointerProvenance(
    string SourceSystem,                 // e.g. "AgentiCore", "LispDuplex", "SLIEncoder"
    string CreationMode,                 // "boot" | "commit" | "index_build" | "import"
    string OriginBraidIndexHash,         // optional; if pointer formed from a braided commit set
    string OriginEngramId,               // optional; if pointer is derived from a specific GoldenEngram
    string Notes                         // optional; free text, not used for resolution logic
);
```

---

### 5. Canonical SymbolText Rules (Freeze Rule)

To avoid drift and “stringly-typed meaning,” **SymbolText must be canonicalized** before hashing:

**Canonical SymbolText v0.1.0:**

* UTF-8
* trimmed
* no leading/trailing whitespace
* internal whitespace collapsed to a single space **OR** forbidden entirely (pick one rule and freeze it — recommended: forbidden)
* normalization form: **NFKC** (recommended) *or* “no Unicode normalization” (but then you must enforce a restricted alphabet)

**Recommendation for v0.1.0** (safe + stable):

* enforce ASCII subset for SymbolText in v0.1.0 (`[A-Za-z0-9._:/\-]`)
* reject anything else at the SLI Gate / Packetizer

This keeps pointers deterministic across runtimes and avoids hidden Unicode confusables.

---

### 6. Resolution Rules (No Heuristics)

Resolution is an **exact** lookup:

1. Canonicalize `SymbolText`
2. Compute/verify `PointerHash`
3. Use `Namespace + SymbolText` as the key into the duplexed index
4. Return `Target.TargetRef` exactly

**If no exact match exists:**
Return `NotFound` (with reason code) — do **not** “approximate.”

Any approximate strategy belongs in **Layer 2** and must produce a trace.

---

### 7. Relationship to GEL / Tensor / Runtime State

* **Layer 0 (`gel.golden_engram`)** stores identity anchors (`EngramId`) and braid lineage.
* **Layer 1 (`sli.symbol_pointer`)** provides stable symbolic references to those anchors.
* **Layer 1 (`sli.tensor`)** may embed `SymbolPointers[]` to carry addressability through transport. 
* **Layer 2 (`iutt.runtime_state`)** may use pointers during runtime assembly, but must never treat them as identity writes. 

---

### 8. Required Invariants (Enforced)

#### 8.1 Layer Purity

`SliSymbolPointer_v0_1_0` must not contain:

* `salience`, `gradient`, `relevance`, `queryContext`, `perspective`, `basin`, `attention`, `score`

#### 8.2 Deterministic Hashing

* `PointerHash = sha256(canonicalBytes(pointer without PointerHash))`
* `CanonicalSeal = sha256(full canonical serialization)`

#### 8.3 No Identity Mutation

* SymbolPointer creation must not advance `ParentTip`
* must not mint `EngramId`

#### 8.4 TargetRef Format

If `PointerKind == "engram_id"` then:

* `TargetRef` must match `sha256:<lowercase-hex>` (or your exact EngramId string convention, frozen)

---

### 9. Tests (Freeze-Grade)

#### Test SP1 — Deterministic replay

Create the same pointer twice → `PointerHash` must match exactly.

#### Test SP2 — Exactness / No heuristics

Lookup with a missing symbol must return `NotFound`; verify no fuzzy matching occurred.

#### Test SP3 — Layer contamination scan

Serialize pointer JSON and assert forbidden substrings are absent:

* `salience`, `relevance`, `gradient`, `query`, `perspective`, `basin`

#### Test SP4 — Transport stability

Embed pointers in a `sli.tensor`, simulate CradleTek gluing transport, ensure:

* `SymbolText` unchanged
* `PointerHash` unchanged
* resolution returns the same `TargetRef` (or equivalence policy explicitly allows remapping, but then you must log it in Layer 2) 

---

## How this connects to your macro

Your macro:

`NameOfEngram{RootBaseE + [ECProp + ECProc + ECPers + ECPart] + RootCapE}BraidIndex`

…becomes *operationally addressable* when each committed GoldenEngram (in the braided set) is:

* identity: `EngramId` (Layer 0)
* join: `BraidIndexHash` (Layer 0)
* addressability: `sli.symbol_pointer` (Layer 1)

So the BraidIndex binds the *set*, while symbol pointers make the set *navigable* without leaking runtime salience into the database.

---

## Next immediate build step

Now that `sli.symbol_pointer.v0.1.0` is defined, the “next locked piece” is usually:

**`sli.packet.v0.1.0`** (if not already frozen), because packetization is where:

* handles are declared
* env/frame/mode/op are captured
* “No Handle, No Action” is mechanically enforced

If you want, I can draft `sli.packet.v0.1.0` in the same freeze language so the whole Layer-1 surface is sealed end-to-end.
