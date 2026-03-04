Below is the **freeze-grade** `sli.tensor.v0.1.0` spec, written to sit cleanly between:

* **Layer 0 (GEL)** inert GoldenEngrams, and
* **Layer 2 (IUTT)** runtime reconstruction.

This is the **bridge object** that enables morphism-safe transport and deterministic addressability **without** importing salience, perspective, or relevance into storage.

---

# sli.tensor.v0.1.0

## Symbolic Tensorization Object (Layer 1, Structural Only)

## 0. Purpose

`sli.tensor.v0.1.0` is the **structural** representation produced by tensorizing one or more `gel.golden_engram.*` entries.

It provides:

* morphism-invariant addressability
* deterministic structural encoding of symbolic content
* explicit pointer targets (EngramId, symbol pointers)
* transport-ready representation for CradleTek gluing

It does **not** provide:

* salience gradients
* relevance scores
* perspective transforms
* runtime context
* heuristic selection

Those belong strictly to **Layer 2** (`iutt.runtime_state.*`).

---

# 1. Non-Goals (Hard Boundary)

`sli.tensor.v0.1.0` MUST NOT contain:

* query context
* operator salience scoring
* gradients / weights derived from runtime relevance
* perspective shift parameters
* basin alignment scores
* any field whose values depend on runtime application context

If any of those appear, it is a spec breach (Layer 1 contamination).

---

# 2. Determinism Contract

Tensorization must be deterministic given:

* a fixed set of `OriginEngramIds[]`
* a fixed `TensorizationProfileHash` (if used)
* canonical serialization rules

Meaning:

Same inputs → same `OriginTensorHash`.

---

# 3. Schema ID

`sli.tensor.v0.1.0`

---

# 4. Canonical Record (C#)

```csharp
public record SliTensor_v0_1_0(
    // --- Tensor identity (Layer 1 structural, NOT GEL identity) ---
    string TensorVersion,                 // "v0.1.0"
    string TensorizationProfileVersion,   // e.g. "STP-BASELINE-1"
    string TensorizationProfileHash,      // sha256(profile) - optional but recommended for stability

    // --- Provenance (inert source anchors) ---
    string[] OriginEngramIds,             // sorted
    string[] OriginBraidIndexHashes,      // sorted (optional, but recommended)
    string IntakeHash,                    // optional: if all origins share same intake; else "mixed"

    // --- Structural addressability ---
    TensorNode[] Nodes,                   // deterministic ordering
    TensorEdge[] Edges,                   // deterministic ordering
    string[] SymbolPointers,              // sli.symbol_pointer references (sorted)

    // --- Structural encodings (no salience) ---
    string EncodingMode,                  // e.g. "symbolic_lattice", "mote_well", "minimal"
    string[] FeatureKeys,                 // sorted list describing feature axes
    string[] FeatureValues,               // aligned ordering; interpretation in profile

    // --- Deterministic digest for transport ---
    string OriginTensorHash,              // sha256(canonicalBytes(tensor without OriginTensorHash))
    string CanonicalSeal                  // sha256(full canonical serialization)
);
```

### Key note:

* `OriginTensorHash` is computed after canonicalization but **excluding itself** (standard self-hash rule).
* `CanonicalSeal` includes everything including `OriginTensorHash` (if you want a “final seal”).

---

# 5. Sub-Schemas

## 5.1 TensorNode

Nodes represent **structural units**. A node may correspond directly to a GoldenEngram or to a derived structural abstraction (still Layer 1, still non-salient).

```csharp
public record TensorNode(
    string NodeId,                        // deterministic: e.g. "N:" + sha256(node canonical bytes)
    string NodeType,                      // "golden_engram" | "symbol" | "root" | "composite"
    string OriginEngramId,                // required when NodeType == "golden_engram"
    string ResolvedAddress,               // Channel×Partition×Mirror (structural provenance)
    string[] Tags,                        // sorted, structural tags only (e.g. "4P:propositional")
    Dictionary<string,string> Attributes  // sorted keys; MUST be structural only
);
```

### Allowed Tags examples

* `4P:propositional`
* `4P:procedural`
* `4P:perspectival`
* `4P:participatory`
* `LEGENDARY:true` (structural marker only)

### Forbidden Tags

Anything implying runtime evaluation, e.g.:

* `salient:true`
* `relevance:0.87`
* `perspective_shifted`
* `context_match:yes`

---

## 5.2 TensorEdge

Edges represent **structural relations**, not weighted preferences.

```csharp
public record TensorEdge(
    string FromNodeId,
    string ToNodeId,
    string RelationType,                  // "references" | "depends_on" | "root_of" | "maps_to"
    Dictionary<string,string> Attributes  // sorted keys; MUST be structural only
);
```

### Forbidden

Any numeric weight or gradient field in Layer 1.

If you need weights, they belong in Layer 2 (`iutt.runtime_state.SalienceField`).

---

# 6. Canonicalization Rules (Mandatory)

Tensor canonicalization must follow the same contract style as GEL:

1. Fixed field order per schema version
2. Arrays sorted lexicographically
3. Dictionaries: keys sorted lexicographically
4. Null → literal `"null"`
5. UTF-8 only
6. Lowercase hex for hashes
7. Manual builder, no serializer auto-ordering

Additionally for tensors:

* **Nodes must be sorted** deterministically by `NodeId` (or by canonical bytes pre-hash).
* **Edges must be sorted** deterministically by `(FromNodeId, RelationType, ToNodeId)`.

---

# 7. Relationship to Layer 0 and Layer 2

## 7.1 Layer 0 → Layer 1

Tensorization uses only inert sources:

* `gel.golden_engram.*`
* `gel.braid_index.*` (hash references only)
* `sli.symbol_pointer.*` mappings

No runtime context.

## 7.2 Layer 1 → Layer 2

IUTT reconstruction accepts:

* `sli.tensor.*`
* `iutt.reconstruction_profile.*`
* runtime query context

…and produces `iutt.runtime_state.*` which contains salience/gradients.

---

# 8. Minimal Tensorization Profiles (Recommended)

To keep tensor behavior stable across environments, define at least one profile:

* `sli.tensorization_profile.v0.1.0` (optional schema, or embed it into `iutt.reconstruction_profile`)

Baseline suggestions:

### STP-MINIMAL

* Nodes: only GoldenEngram nodes
* Edges: none
* SymbolPointers: EngramId pointers only

### STP-LATTICE

* Nodes: GoldenEngrams + their root/symbol nodes
* Edges: references/root-of relations
* FeatureKeys/Values represent lattice coordinates (structural)

---

# 9. Required Tests (Freeze-Grade)

### Test S1 — No Layer-2 contamination

Scan serialized tensor; fail if any forbidden field names exist:

* `salience`, `gradient`, `relevance`, `queryContext`, `perspectiveShift`, `basinScore`, `attentionWeight`

### Test S2 — Deterministic replay

Tensorize the same GoldenEngrams twice under same profile:

* `OriginTensorHash` must match exactly.

### Test S3 — Transport integrity

After CradleTek gluing transport, tensor reconstruction must preserve:

* `OriginEngramIds`
* `SymbolPointers`
* `OriginTensorHash` (or explicit equivalence rule if gluing transforms representation)

### Test S4 — Canonicalization enforcement

Deliberately shuffle node order in-memory; canonical builder must still emit identical bytes/hashes.

---

# 10. Status

`sli.tensor.v0.1.0` is now frozen as:

* deterministic (given fixed inputs)
* structural-only (Layer 1)
* morphism-ready
* salience-free by construction
* auditable against layer collapse

---

If you want to keep chaining forward, the next spec that completes the bridge is:

## `sli.symbol_pointer.v0.1.0`

…because that’s how Lisp duplex lookup and “symbol → EngramId” resolution becomes formally constrained and testable.
