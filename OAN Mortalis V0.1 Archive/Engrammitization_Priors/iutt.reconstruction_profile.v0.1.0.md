Below is the **freeze-grade sibling spec** you asked for:

# `iutt.reconstruction_profile.v0.1.0`

## Runtime Reconstruction Profile (Layer 2, Hashable, Non-Canon)

This is the object whose `ReconstructionProfileHash` becomes a **mandatory invariant** inside `gel.braid_index.v0.1.0`, but it remains **Layer 2** (runtime / application). It is **hashable and versioned**, but **not identity**.

---

# 0. Purpose

`iutt.reconstruction_profile.v0.1.0` defines the **runtime rules** for:

* how `sli.tensor.*` objects are reconstructed into an `iutt.runtime_state.*`
* what operators are permitted (gluing, projection, perspective transform, weighting)
* what equivalence criteria mean (for “same behavior” across substrates)
* what telemetry signals must be produced for audit and safety basins

It is:

* **hash-stable** (profile hashing is deterministic)
* **runtime-active** (it drives salience/perspective/relevance computation)
* **non-canon** (it must not be appended to GEL as identity)

---

# 1. Non-Goals (Hard Boundary)

This schema MUST NOT:

* contain or reference any `gel.*` canonical envelope bytes
* be incorporated into `gel.golden_engram.*` canonical seal inputs
* directly mutate GEL tip or EngramId
* “decide” Commit by itself

It may suggest decisions at runtime, but actual identity writes require:

* explicit declared handle
* SLI Gate admit
* Commit-only crystallization

---

# 2. Determinism Contract

A reconstruction profile must be deterministic **as a specification**:

* The same profile content → the same `ReconstructionProfileHash`
* Operator sets must be explicitly enumerated
* Any stochastic permission must be explicit and parameterized

Runtime state produced by the profile may be non-deterministic **only if**:

* the profile sets `StochasticMode != "none"`
* and a `RuntimeSeed` is included in `iutt.runtime_state.*`

If you want strict determinism at Layer 2 for testing:

* set `StochasticMode = "none"`

---

# 3. Canonical Schema

## 3.1 Schema ID

`iutt.reconstruction_profile.v0.1.0`

## 3.2 Record (C#)

```csharp
public record IuttReconstructionProfile_v0_1_0(
    // --- Profile identity (hash-stable) ---
    string ReconstructionProfileVersion,      // "v0.1.0"
    string ProfileName,                       // e.g. "RR-BASELINE"
    string ProfileDescription,                // short human readable
    string ProfileOwner,                      // e.g. "CradleTek", "Lab-D"
    
    // --- Allowed operator set (explicit, enumerable) ---
    OperatorPolicy Operators,

    // --- Perspective & coordinate transform policy ---
    PerspectivePolicy Perspective,

    // --- Salience & relevance policy (runtime only) ---
    SaliencePolicy Salience,
    RelevancePolicy Relevance,

    // --- Equivalence criteria (cross-substrate reconstruction) ---
    EquivalencePolicy Equivalence,

    // --- Telemetry & safety basin policy ---
    TelemetryPolicy Telemetry,
    BasinPolicy Basins,

    // --- Constraints / invariants for auditability ---
    ConstraintPolicy Constraints,

    // --- Canonical hashing inputs (for the profile itself) ---
    CanonicalizationPolicy Canonicalization
);
```

---

# 4. Sub-Schemas

## 4.1 OperatorPolicy (what transformations are allowed)

```csharp
public record OperatorPolicy(
    string StochasticMode,                // "none" | "seeded" | "allowed"
    string[] AllowedOperators,             // sorted, explicit list
    Dictionary<string,string> OperatorParams, // sorted keys
    string ForbiddenOperatorRule           // optional: e.g. "no_identity_mutation"
);
```

### Recommended operator names (v0.1.0 baseline)

Keep the operator namespace explicit and enumerable:

* `glue.local_to_global`
* `glue.sheaf_consistency`
* `project.cover_restrict`
* `project.cover_extend`
* `transform.perspective`
* `weight.salience`
* `assemble.runtime_lattice`
* `score.basin_alignment`

No operator should imply “Commit”.

---

## 4.2 PerspectivePolicy (how frames may shift)

```csharp
public record PerspectivePolicy(
    string DefaultFrameId,                 // e.g. "operator"
    string[] AllowedFrames,                // sorted
    string[] AllowedTransformTypes,         // sorted: "rotate", "projection", "relabel", "none"
    Dictionary<string,string> FrameParams,  // sorted keys
    bool RequireExplicitFrameDeclaration    // true recommended
);
```

---

## 4.3 SaliencePolicy (gradients allowed at runtime)

```csharp
public record SaliencePolicy(
    string SalienceMode,                   // "attention" | "gradient" | "operator_weighted" | "none"
    double TemperatureDefault,
    double ThresholdDefault,
    int MaxActiveNodes,                    // runtime assembly limit
    bool AllowEdgeWeights,                 // recommended true
    string[] AllowedWeightSources          // sorted: e.g. "query", "operator", "profile"
);
```

---

## 4.4 RelevancePolicy (selection + trace requirements)

```csharp
public record RelevancePolicy(
    string SelectionRuleSetId,             // e.g. "RR-1.0"
    bool RequireRelevanceTrace,            // recommended true
    bool RequireRejectedSet,               // optional
    int MaxSelectedEngrams,
    string JustificationVocabularyId        // e.g. "JV-BASELINE"
);
```

This is what forces runtime to produce a `RelevanceTrace` (auditable).

---

## 4.5 EquivalencePolicy (cross-substrate “same behavior”)

This is the heart of your IUTT-style “gluing” guarantee. Keep it explicit.

```csharp
public record EquivalencePolicy(
    string EquivalenceMode,                // "structural" | "behavioral" | "hybrid"
    string[] RequiredInvariants,            // sorted list
    double Tolerance,                      // numeric tolerance for metrics (if used)
    string[] RequiredArtifacts              // sorted: e.g. "origin_ids", "trace", "basin_scores"
);
```

### Suggested `RequiredInvariants` (v0.1.0)

* `invariant.origin_engram_ids_preserved`
* `invariant.braid_index_hash_preserved`
* `invariant.profile_hash_matches`
* `invariant.symbol_pointer_resolution_consistent`
* `invariant.basin_within_threshold` (if basins are enabled)

Important: Equivalence is evaluated at runtime and logged; it is not stored as identity unless explicitly committed via handle.

---

## 4.6 TelemetryPolicy (what must be logged)

```csharp
public record TelemetryPolicy(
    bool EmitRuntimeStateDigest,           // recommended true
    bool EmitSalienceSummary,              // recommended true
    bool EmitRelevanceTraceDigest,          // recommended true
    bool EmitTriptychSignals,              // recommended true
    string TelemetrySinkId,                // e.g. "telemetry.spine.runtime"
    int MaxTelemetryDetailLevel            // 0..N
);
```

This is what makes Layer 2 auditable without becoming Layer 0 identity.

---

## 4.7 BasinPolicy (safety / alignment basins)

```csharp
public record BasinPolicy(
    bool EnableBasins,
    double BasinThreshold,
    string BasinMetricId,                  // e.g. "V(s)" or "TSA"
    string OutOfBasinAction                // "warn" | "freeze" | "quarantine" | "refuse"
);
```

This is runtime governance, not identity.

---

## 4.8 ConstraintPolicy (hard prohibitions)

```csharp
public record ConstraintPolicy(
    bool ForbidIdentityMutation,           // must be true
    bool ForbidCanonicalHashInputsFromLayer2, // must be true
    bool ForbidAutoCommit,                 // must be true
    string[] ForbiddenFieldsInGEL,          // sorted: patterns like "salience", "gradient", "queryContext"
    string[] ForbiddenOperators             // sorted, optional
);
```

This is your spec-level “anti-ontology-collapse” barrier.

---

## 4.9 CanonicalizationPolicy (how profile hash is computed)

```csharp
public record CanonicalizationPolicy(
    string CanonicalizationVersion,        // "CANON-1"
    string Encoding,                       // "utf-8"
    bool SortArraysLexicographically,      // true
    bool SortDictionaryKeysLexicographically, // true
    string NullLiteral,                    // "null"
    bool LowercaseHex                      // true
);
```

---

# 5. ReconstructionProfileHash (Mandatory)

## 5.1 Definition

`ReconstructionProfileHash` is:

> `sha256(canonicalBytes(profile))` in lowercase hex

Where `canonicalBytes(profile)` obeys the same canonicalization rules as your GEL hashing contract (fixed field order, sorted arrays, sorted dictionary keys, UTF-8, null literal).

## 5.2 Inputs MUST exclude

* any runtime state
* any GEL entry bodies
* any query content
* any wall-clock time

This hash must be stable across machines and rebuilds.

---

# 6. Relationship to BraidIndex

`gel.braid_index.v0.1.0` must include:

* `ReconstructionProfileVersion`
* `ReconstructionProfileHash`

This pins runtime cognition to a specific reconstruction policy, enabling:

* reproducible gluing tests
* consistent equivalence evaluation
* audit traceability of “how cognition was assembled”

---

# 7. Minimal Example Profile (RR-BASELINE)

Here is a safe baseline configuration (conceptual values):

* `StochasticMode = "none"`
* `AllowedOperators = ["assemble.runtime_lattice", "glue.local_to_global", "project.cover_restrict", "transform.perspective", "weight.salience", "score.basin_alignment"]`
* `RequireExplicitFrameDeclaration = true`
* `RequireRelevanceTrace = true`
* `EnableBasins = true` with `OutOfBasinAction = "warn"` (or stricter for production)

---

# 8. Tests (Profile Validity)

You can hard-test the profile object without touching GEL:

### Test P1 — Hash stability

Same profile content → same `ReconstructionProfileHash`

### Test P2 — Forbidden flags

`ForbidAutoCommit == true`
`ForbidIdentityMutation == true`

### Test P3 — Operator whitelist

No operator outside `AllowedOperators` can execute

---

# 9. Status

`iutt.reconstruction_profile.v0.1.0` is now defined as:

* hashable
* auditable
* runtime-active
* layer-safe
* explicitly constrained against identity bleed

---

If you want to keep rolling, the next natural piece is:

**`sli.tensor.v0.1.0`** (the exact structural object that bridges GEL → IUTT), because that’s where “morphism-invariant addressability” becomes executable.
