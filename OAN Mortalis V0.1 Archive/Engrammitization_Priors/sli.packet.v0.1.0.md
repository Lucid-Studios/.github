Below is a **freeze-grade** `sli.packet.v0.1.0` spec that is consistent with:

* **SLI Constitution v0.1** “No Handle, No Action”, deterministic resolution, Channel×Partition×Mirror, governance-before-execution, auditability, no silent promotion 
* Your locked **Layer separation** (GEL inert / SLI structural / IUTT runtime)   

This is written as a Build-Contracts block.

---

# sli.packet.v0.1.0

## Symbolic Language Interconnect Packet (Layer 1, Deterministic)

## 0. Purpose

`**sli.packet.v0.1.0**` is the **canonical, layer-1 container** for any executable intent entering the OAN Mortalis stack.

It carries:

* an explicit **SLI handle list** (capability declaration)
* a deterministic **envelope** (`env`, `frame`, `mode`, `op`)
* deterministic provenance (session genesis factors)
* **no semantic interpretation**

It is the unit evaluated by the SLI Gate prior to execution or Commit.

---

## 1. Non-Goals (Hard Boundary)

`sli.packet` MUST NOT:

* interpret meaning
* infer permissions
* heuristically generate handles
* contain salience gradients, relevance scores, perspective shifts, basin scores
* contain wall-clock timestamps used for gating or identity
* modify GEL (that happens only after gate, via CommitEngine)

All semantic interpretation or “best match” reasoning belongs in **Layer 2** (`iutt.*`).

---

## 2. Core Invariants (Constitution Alignment)

### 2.1 No Handle, No Action

A packet with `Handles.Length == 0` is **non-executable** and must be denied by the gate. 

### 2.2 Deterministic Resolution

Gate evaluation must be deterministic over packet fields:

* no wall-clock
* no stochastic routing
* no heuristic inference 

### 2.3 Explicit Addressing

Each admitted handle resolves to:
`Channel × Partition × Mirror` 

### 2.4 Governance Before Execution

SLI gate evaluation occurs before CommitIntent is executed. 

---

## 3. Schema ID

`**sli.packet.v0.1.0**`

---

## 4. Canonical Record (C#)

```csharp
public record SliPacket_v0_1_0(
    // --- Schema ---
    string Schema,                       // "sli.packet.v0.1.0"

    // --- Packet identity (Layer 1; NOT GEL identity) ---
    string PacketHash,                   // sha256(canonicalBytes(packet without PacketHash))
    string CanonicalSeal,                // sha256(full canonical serialization)

    // --- Deterministic session genesis factors (no wall-clock) ---
    string SessionId,                    // deterministic session identifier
    string OperatorId,                   // operator identity (string stable id)
    string ScenarioName,                 // scenario or route name
    long GenesisTick,                    // deterministic tick anchor (not DateTime)

    // --- Envelope: env, frame, mode, op ---
    PacketEnv Env,                       // see §4.1
    PacketFrame Frame,                   // see §4.2
    PacketMode Mode,                     // see §4.3
    PacketOp Op,                         // see §4.4

    // --- Capability declaration ---
    string[] Handles,                    // sorted; explicit declared handles (Root Atlas keys)

    // --- Optional references (structural only) ---
    PacketRefs Refs,                     // see §4.5 (optional but recommended)

    // --- Gate result placeholder (must be empty pre-gate) ---
    GateTrace GateTrace                  // see §4.6 (MUST be empty before evaluation)
);
```

### 4.1 PacketEnv (environment classification)

```csharp
public record PacketEnv(
    string Channel,                      // "Public" | "Private"
    string Partition,                    // "GEL" | "GOA" | "OAN"
    string Mirror                         // "Standard" | "Cryptic"
);
```

**Constraint:** `Channel`, `Partition`, `Mirror` are *declarations*, not inferred.

---

### 4.2 PacketFrame (structural container / routing frame)

```csharp
public record PacketFrame(
    string FrameId,                      // e.g. "request", "telemetry", "commit_intent"
    string FrameVersion,                 // e.g. "v0.1"
    Dictionary<string,string> FrameMeta  // sorted keys; structural only
);
```

**Forbidden in FrameMeta:** query context, salience values, heuristics.

---

### 4.3 PacketMode (governance / SAT mode)

```csharp
public record PacketMode(
    string SatMode,                      // e.g. "Open" | "SAT" | "SAT_Gate" | "HITL" (your enumerations)
    bool SafActive,                      // Safe-Action Freeze flag (observable, inert gating)
    string MaskingState                  // "none" | "placeholder" (cryptic masking indicator)
);
```

**Note:** SAT mode is used for gating decisions per constitution. 

---

### 4.4 PacketOp (operation intent)

```csharp
public record PacketOp(
    string OpCode                        // "NoOp" | "Propose" | "CommitIntent" | "ExecuteIntent"
);
```

**Freeze rule:**

* `CommitIntent` must not be executed unless gate allows it.
* `NoOp` and `Propose` are always non-authoritative.

(Commit itself is a separate Layer-0 write event executed post-gate.)

---

### 4.5 PacketRefs (structural references)

```csharp
public record PacketRefs(
    string IntakeHash,                   // sha256 canonical intake bytes (if applicable)
    string ProposedBraidIndexHash,        // if packet originates from PreGELBundle proposal
    string[] OriginEngramIds,             // sorted; if referencing existing GEL engrams
    string[] SymbolPointers               // sorted; sli.symbol_pointer references
);
```

**Constraint:** These are pointers only; no Layer-2 runtime state may appear here.

---

### 4.6 GateTrace (audit record)

GateTrace must be **empty prior to evaluation** and filled **only by the gate**.

```csharp
public record GateTrace(
    bool IsEvaluated,
    bool IsAllowed,
    string PolicyVersion,
    string ReasonCode,                   // required if denied
    Dictionary<string,string> Resolutions, // handle -> "Channel×Partition×Mirror" (sorted keys)
    string MaskingApplied,               // "true"/"false"
    string EvidenceSnapshotHash           // sha256 over canonicalized gate evidence
);
```

**Constitution alignment:**

* Every denial includes reasonCode + policyVersion
* Every allowance logs handle + resolved address + SAT mode + masking state 

---

## 5. Canonicalization Contract (Packet)

Same freeze contract as GEL hashing:

1. Fixed field order per schema version
2. Arrays sorted lexicographically
3. Dictionary keys sorted lexicographically
4. null → literal `"null"`
5. UTF-8 only
6. Lowercase hex for hashes
7. Manual builder; no serializer auto-ordering

Additionally:

* `Handles` must be sorted ordinal.
* `PacketEnv` fields must be exact case (“Public/Private”, etc.) — no enum ToString variance.

---

## 6. Gate Evaluation Semantics (How the gate uses the packet)

Gate evaluation is a deterministic function:

```text
GateDecision = Gate.Evaluate(packet, RootAtlas, SessionMounts, Policy)
```

Evaluation order (frozen):

1. Validate each handle exists in Root Atlas
2. Validate session mounts allow requested address
3. Validate SAT mode satisfies RequiredSatModes
4. Validate channel/mirror alignment
5. If Cryptic:

   * require SAT Gate or stronger
   * apply masking policy (logged)
6. Return Allow/Deny with GateTrace filled

No step may be skipped; no implicit fallback. 

---

## 7. Required Tests (Freeze-Grade)

### Test PK1 — No Handle, No Action

Create packet with empty Handles → gate must deny with reason code.

### Test PK2 — Deterministic Hash Stability

Same packet fields → same PacketHash across machines.

### Test PK3 — GateTrace immutability

Pre-gate GateTrace must be empty; only gate fills it.

### Test PK4 — Cryptic access policy

Public×*×Cryptic must deny; Private×*×Cryptic requires SAT Gate and logs masking. 

### Test PK5 — No Layer-2 contamination scan

Serialized packet must not include forbidden substrings:
`salience`, `gradient`, `relevance`, `queryContext`, `perspectiveShift`, `basinScore`.

---

## 8. Relationship to Engrammitization (Pipeline)

* Pre-SLI normalization produces `PreGELBundle` (proposal) (Layer 0 proposal object; non-authoritative)
* Packetizer emits `sli.packet` with `OpCode="Propose"` or `"CommitIntent"` and explicit Handles
* Gate evaluates `sli.packet` and fills GateTrace
* CommitEngine uses admitted handles + canonical serialization to produce `gel.golden_engram` entries

This preserves:

* Gate purity
* Commit-only crystallization
* Layer separation

---

If you want the chain complete, the next “missing lock” after `sli.packet` is usually:

**`sli.root_atlas_entry.v0.1.0`** (the schema for Root Atlas handle registry entries), since Gate determinism depends on RootAtlas being static, canonical, and hashable.
