Below is a **freeze-grade** `iutt.runtime_state.v0.1.0` schema that is consistent with the boundary we just locked:

* **Layer 2 only** (runtime / ephemeral)
* **May contain salience gradients, perspective transforms, relevance fields**
* **Must never be hashed into GEL identity**
* **Must be reconstructable from inert sources** (GEL + SLI tensor + reconstruction profile + query context)
* **Must be auditable** (so we can prove it didn’t contaminate Layer 0)

I’m also including the **naming + invariants** that make it safe to use.

---

# iutt.runtime_state.v0.1.0

## Runtime Reconstruction State (Layer 2, Non-Canon)

## 0. Purpose

`iutt.runtime_state.v0.1.0` represents the **ephemeral cognitive state** produced by applying an `iutt.reconstruction_profile` to **Layer-1 SLI tensors** derived from **Layer-0 GoldenEngrams**, under an explicit runtime context (query + operator/session).

It exists to host:

* contextual salience
* relevance gradients
* perspective transforms
* runtime assembly decisions
* drift/compass/harmonics signals

It does **not** represent identity.

---

## 1. Non-Goals (Hard Boundary)

This schema MUST NOT:

* be appended to GEL
* influence any `gel.*` canonical envelope
* appear in any canonical hash inputs
* mutate `ParentTip` or `EngramId`
* be treated as durable memory

A runtime state becomes identity-bearing **only** if an operator explicitly declares a handle and triggers a Commit, producing new `gel.golden_engram.*`.

---

## 2. Determinism Expectations

Runtime state is **not required** to be deterministic across contexts (because context itself can vary).
However, it **must** be **auditable and reproducible given identical inputs**:

Inputs for replay:

* `ReconstructionProfileHash`
* `QueryContextHash`
* `OriginEngramIds[]` (or `OriginTensorHash`)
* `RuntimeSeed` (if any stochasticity is permitted at Layer 2; if not used, set to `"none"`)

---

## 3. Canonical Schema (JSON-shape / C# record)

### 3.1 Schema ID

`iutt.runtime_state.v0.1.0`

### 3.2 Record

```csharp
public record IuttRuntimeState_v0_1_0(
    // --- Identity of the runtime state (NOT GEL identity) ---
    string RuntimeStateId,              // sha256 over *runtime* envelope (optional), or "none"
    string CreatedTick,                 // deterministic tick relative to session genesis (not wall-clock)

    // --- Provenance (what produced this state) ---
    string ReconstructionProfileVersion,
    string ReconstructionProfileHash,   // must match iutt.reconstruction_profile.v0.1.0
    string QueryContextHash,            // sha256(canonical query context) - runtime only
    string SessionId,
    string OperatorId,
    string ScenarioName,

    // --- Origins (inert sources) ---
    string[] OriginEngramIds,           // GEL engrams used (sorted)
    string OriginTensorHash,            // sha256(canonical SLI tensor bundle)
    string OriginBraidIndexHash,        // braid hash for traceability (optional but recommended)

    // --- Perspective / coordinate frame ---
    PerspectiveFrame Perspective,       // perspective transform description (see below)

    // --- Salience / relevance fields (runtime-only) ---
    SalienceField Salience,             // weights, gradients, attention maps
    RelevanceTrace Relevance,           // why certain engrams/nodes were selected

    // --- Assembly (what the runtime state currently "holds") ---
    RuntimeAssembly Assembly,           // assembled nodes, bindings, working set, links

    // --- Drift/Compass/Harmonics telemetry (runtime signals) ---
    TriptychSignals Signals,

    // --- Safety + boundary attestations ---
    BoundaryAttestation Boundary,       // proves no Layer-0 contamination
    string[] Warnings,                  // optional
    string[] Notes                       // optional
);
```

---

## 4. Sub-Schemas

### 4.1 PerspectiveFrame (runtime coordinate transform)

```csharp
public record PerspectiveFrame(
    string FrameId,                     // e.g. "operator", "role_shell:firefighter", "analysis", "mythic"
    string TransformType,               // e.g. "rotate", "shear", "projection", "relabel", "none"
    Dictionary<string,string> Parameters, // sorted keys if hashed for QueryContext
    string SourceFrameId,
    string TargetFrameId
);
```

Notes:

* This is allowed to be semantic (it’s Layer 2), but it must remain runtime-only.
* If you compute QueryContextHash, canonicalize `Parameters`.

---

### 4.2 SalienceField (gradients + weights)

```csharp
public record SalienceField(
    string SalienceMode,                // e.g. "attention", "gradient", "bayes", "heuristic", "operator_weighted"
    Dictionary<string,double> NodeWeights,  // key: EngramId or NodeId
    Dictionary<string,double> EdgeWeights,  // key: "A->B"
    string GradientDescriptor,          // free text or structured label
    double Temperature,                 // optional; runtime only
    double Threshold                     // selection threshold used
);
```

Constraint:

* NodeWeights/EdgeWeights are runtime only.
* Never stored in GEL.
* If you want replay determinism, store the selection threshold and mode.

---

### 4.3 RelevanceTrace (why the selection happened)

```csharp
public record RelevanceTrace(
    string SelectionRuleSet,            // e.g. "profile:RR-1.0"
    string[] SelectedEngramIds,         // runtime working set (sorted)
    string[] RejectedEngramIds,         // optional (sorted)
    Dictionary<string,string> Justifications, // key: EngramId -> reason label
    string EvidenceDigest               // sha256 over structured evidence (optional)
);
```

This makes relevance auditable without hard-baking it into identity.

---

### 4.4 RuntimeAssembly (the working cognitive lattice)

```csharp
public record RuntimeAssembly(
    string AssemblyId,                  // runtime id
    RuntimeNode[] Nodes,                // active nodes
    RuntimeBinding[] Bindings,          // links / edges
    string[] ActiveSymbolPointers,      // sli.symbol_pointer references
    string WorkingSummary               // optional summary snapshot (runtime only)
);

public record RuntimeNode(
    string NodeId,                      // may equal EngramId or derived node
    string NodeType,                    // e.g. "golden_engram", "composite", "query_anchor"
    string OriginEngramId,              // if derived from a GoldenEngram
    Dictionary<string,string> Tags       // runtime tags
);

public record RuntimeBinding(
    string FromNodeId,
    string ToNodeId,
    string Relation,                    // e.g. "supports", "contradicts", "extends", "maps_to"
    double Weight                        // runtime weight
);
```

---

### 4.5 TriptychSignals (drift/compass/harmonics telemetry)

```csharp
public record TriptychSignals(
    DriftSignals Drift,
    CompassSignals Compass,
    HarmonicsSignals Harmonics
);

public record DriftSignals(
    double DriftMagnitude,
    double DriftCurvature,
    string DriftWindowId
);

public record CompassSignals(
    double EthicalGradient,
    double EpistemicStability,
    double NarrativeCoherence
);

public record HarmonicsSignals(
    double Resonance,
    double Dissonance,
    double Coupling
);
```

All runtime only. These signals can be logged to telemetry, but not canonized unless committed via handle.

---

### 4.6 BoundaryAttestation (proof of separation)

```csharp
public record BoundaryAttestation(
    bool Layer0Untouched,               // must be true
    bool CanonicalHashInputsClean,       // must be true
    string[] ForbiddenFieldsDetected,    // should be empty
    string AttestationHash              // sha256 over attestation (optional)
);
```

This is the “self-audit stamp” that makes Layer separation enforceable.

---

## 5. Required Invariants (Enforced)

### 5.1 Layer Separation

* `iutt.runtime_state.*` MUST NOT be written to GEL.
* `iutt.runtime_state.*` MUST NOT be inputs to any canonical envelope hash.
* `Layer0Untouched == true` MUST be present and true.

### 5.2 Provenance Completeness

Runtime state MUST include:

* `ReconstructionProfileHash`
* `QueryContextHash`
* `OriginEngramIds[]` (or `OriginTensorHash`)

### 5.3 Commit Bridge Rule

If any runtime state leads to a Commit, the Commit must be formed from:

* **declared handle**
* **deterministic normalized intake**
* **admitted handles**
* **canonical serialization contract**
  …and must NOT include salience fields or runtime assembly artifacts except as **non-authoritative references** (e.g., pointers to evidence logs).

---

## 6. Naming & File Placement

Schema name: `iutt.runtime_state.v0.1.0`
Suggested file: `Build Contracts/Schemas/iutt.runtime_state.v0.1.0.json` (or `.md` + `.json`)

Type name suggestion:

* `IuttRuntimeState_v0_1_0`

---

If you want the next step immediately: I can also write the sibling schema `iutt.reconstruction_profile.v0.1.0` in the same freeze style, including how `ReconstructionProfileHash` is computed and what “equivalence criteria” are allowed.
