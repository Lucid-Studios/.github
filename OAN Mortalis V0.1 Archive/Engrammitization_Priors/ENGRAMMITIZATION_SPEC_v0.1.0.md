# ENGRAMMITIZATION_SPEC_v0.1.0
## Fully Developed, Frozen & Execution-Ready Constitutional Specification
**Freeze Date:** 20 February 2026 (America/Los_Angeles)  
**Status:** Canonical • Version-locked • Boundary-enforced • Test-harness ready

---

## 0. Purpose

This specification defines the deterministic, layer-strict process that transforms raw intake into **inert Golden Engrams** stored in the **Golden Engram Library (GEL)**, routed and addressed via the **Symbolic Language Interconnect (SLI)**, and reconstructed with full contextual salience, perspective shifts, and relevance gradients **only at runtime** in **IUTT Layer 2**.

**Core Guarantee**  
- **Identity** remains deterministic and substrate-independent (Layer 0).  
- **Cognition** remains dynamic and context-sensitive (Layer 2).  
- **Transport** remains invariant across CradleTek gluing/morphism events (Layer 1→2 reconstruction), without rewriting history.

---

## I. Layer Taxonomy (Non-Negotiable)

All objects in the OAN Mortalis stack must belong to exactly one layer.

| Layer | Prefix | Ontological Role | May Affect Identity? | Runtime Context Allowed? |
|---|---|---|---|---|
| **Layer 0** | `gel.*` | Crystallized identity storage (inert database) | **YES** | **NO** |
| **Layer 1** | `sli.*` | Structural symbolic tensorization, routing, governance | **NO** | **NO** |
| **Layer 2** | `iutt.*` | Runtime reconstruction & engineered cognition | **NO** *(unless explicitly re-committed)* | **YES** |

**Spec breach:** Any cross-layer contamination causes immediate harness failure and/or safe-fail state transition.

---

## II. Naming Convention & Schema Registry v0.1.0

### II.1 Prefix Rule (Mandatory)
Every schema begins with its layer prefix:
- `gel.` → Append-only canonical storage object (Layer 0)
- `sli.` → Symbolic routing, tensor, gate, duplex object (Layer 1)
- `iutt.` → Runtime reconstruction object (Layer 2)

### II.2 Versioning Rule
`<layer>.<object_name>.v<MAJOR>.<MINOR>.<PATCH>`
- **MAJOR**: breaking canonical serialization changes (requires migration)
- **MINOR**: additive non-breaking fields
- **PATCH**: documentation or internal non-identity changes

### II.3 Identity Eligibility Rule
Only `gel.*` schemas may:
- generate/hold identity anchors (e.g., EngramId)
- advance `ParentTip`
- be hashed into identity-bearing canonical envelopes
- affect the GEL spine (append-only ledger)

---

## III. Canonical Serialization Contract (Mandatory)

All hashes in this spec (PacketHash, EntryHash, EngramId, EvidenceHash, etc.) MUST be computed from **canonical bytes** built by a manual canonical builder.

### III.1 Canonical Builder Rules
1. Fixed field order per schema version (frozen order)
2. Arrays sorted lexicographically (StringComparer.Ordinal)
3. Dictionary keys sorted lexicographically (ordinal); values serialized after keys
4. `null` serialized as literal UTF-8 `"null"`
5. UTF-8 encoding only
6. Lowercase hex for all hashes
7. No serializer auto-ordering; manual builder only
8. No wall-clock timestamps in any canonical hash inputs
9. No Layer-2 fields may appear inside `gel.*` canonical envelopes

### III.2 Hash Conventions
- `XHash = sha256(canonicalBytes(X without XHash))`
- `CanonicalSeal = sha256(full canonical serialization)`

---

## IV. Engrammitization (Formal Definition)

**Engrammitization** is the deterministic pipeline:

**Raw Intake → PreGELBundle (proposal) → SLI Packet (explicit handles) → Gate Evaluation → CommitIntent → GoldenEngram(s) → CAS Tip Advancement**

### IV.1 Core Invariants
- **No Handle, No Action**: packets with no declared handles are non-executable.
- **Gate is capability routing only**: no semantic heuristics, no content inference.
- **Commit-only crystallization**: identity changes occur only at Commit.
- **GEL inertness**: Layer 0 stores no runtime salience, gradients, perspective shifts.
- **IUTT runtime cognition**: salience and relevance exist only in Layer 2 runtime state.
- **CAS discipline**: tip advancement requires compare-and-swap; conflicts are auditable.

---

## V. The 6-Phase Pipeline (Exact Flow)

1) **Intake (Non-authoritative)**  
   - Input: raw signal + session metadata  
   - Output: `IntakePacket` (intakeHash + genesis factors)

2) **Pre‑SLI Normalization (Non-authoritative)**  
   - Input: IntakePacket  
   - Output: `NormalizedPreSliProduct`  
   - Rule: pure translation / structural reduction; **zero inference**

3) **SLI Packetization + Handle Declaration (Proposal)**  
   - Input: normalized product + **explicit** declared handles  
   - Output: `gel.pregel_bundle` + one or more `sli.packet` with `OpCode=Propose` or `CommitIntent`  
   - Rule: **handles must be explicitly declared** (operator or role-shell). No heuristic classifier.

4) **SLI Gate Evaluation (Capability Routing Only)**  
   - Input: `sli.packet` + RootAtlas + SessionMounts + PolicyBundle pins  
   - Output: `sli.gate_evidence` + `sli.gate_decision` (Allow/Deny)  
   - Rule: deterministic checks only. No semantic interpretation.

5) **Commit Crystallization (Identity Event)**  
   - Input: `sli.commit_intent` (post-gate allowed)  
   - Output: `gel.braid_index` + `gel.golden_engram[]` + `gel.commit_batch` + `gel.tip_advance_event`  
   - Rule: append-only; CAS tip advancement; no partial commits

6) **Optional IUTT Runtime Reconstruction (Non-canon)**  
   - Input: `sli.tensor` + `iutt.reconstruction_profile` + runtime context (query/perspective)  
   - Output: `iutt.runtime_state` (ephemeral)  
   - Rule: if new identity is desired, it must return via explicit handle → Gate → Commit.

---

## VI. Golden Engram Construction Boundary (Locked)

GoldenEngram construction MUST depend **only** on:
- explicitly declared SLI handles
- deterministic normalized intake
- gate-admitted handles
- canonical serialization contract

**Forbidden at storage time (Layer 0):**
- semantic heuristics
- runtime contextual inference
- salience gradients
- perspective shifts
- relevance scoring
- IUTT gluing metadata
- query context

**“Structurally prepared for relevance realization”** means: typed by handle and indexed by symbol pointers so runtime reconstruction can assemble cognition without storage-layer inference.

---

# VII. Canonical Schema Registry (FULL, v0.1.0)

> NOTE: The schemas below are fully specified for v0.1.0 and intended to live under `Build Contracts/` in your repository.  
> Only `gel.*` objects are identity-bearing.

---

## VII.A Layer 0 — GEL (Identity Layer)

### 1) gel.pregel_bundle.v0.1.0 (Non-authoritative staging; NEVER appended to GEL)
```csharp
public record PreGELBundle_v0_1_0(
    string Schema,                         // "gel.pregel_bundle.v0.1.0"
    string IntakeHash,                     // sha256(canonical intake bytes)
    string[] DeclaredHandles,              // sorted
    object NormalizedPreSliProduct,         // non-authoritative payload pointer/struct
    string ProposedRootBaseE,               // cap marker (optional)
    string ProposedRootCapE,                // cap marker (optional)
    Dictionary<string,string> ProposedConstructors, // handle -> constructor proposal (sorted keys)
    string ProposedBraidIndexHash,          // sha256(canonical braid index proposal)
    string BundleHash,                      // sha256(canonicalBytes(without BundleHash))
    string CanonicalSeal                    // sha256(full serialization)
);
```

### 2) gel.braid_index.v0.1.0 (Deterministic join table; hash = BraidIndexHash)
```csharp
public record BraidIndex_v0_1_0(
    string Schema,                          // "gel.braid_index.v0.1.0"
    string IntakeHash,
    string[] DeclaredHandles,               // sorted
    string[] AdmittedHandles,               // sorted
    string ParentTip,
    Dictionary<string,string> ResolvedAddresses, // handle -> "Channel/Partition/Mirror" (sorted keys)
    string PolicyVersion,
    string GateEvidenceHash,                // reference to sli.gate_evidence
    string ReconstructionProfileVersion,
    string ReconstructionProfileHash,
    string BraidIndexHash,                  // sha256(canonicalBytes(without BraidIndexHash))
    string CanonicalSeal                    // sha256(full serialization)
);
```

### 3) gel.golden_engram.v0.1.0 (Identity-bearing unit; inert)
```csharp
public record GoldenEngram_v0_1_0(
    string Schema,                          // "gel.golden_engram.v0.1.0"
    string EngramId,                        // sha256(canonical envelope) lowercase hex
    string Handle,                          // exact admitted handle
    string ResolvedAddressText,             // "Channel/Partition/Mirror"
    string IntakeHash,
    string ParentTip,
    string BraidIndexHash,
    string[] FourPTags,                     // sorted; structural only, optional
    string PayloadHash,                     // sha256(canonical constructor payload) optional but recommended
    string CanonicalSeal                    // sha256(full serialization)
);
```

### 4) gel.braided_commit_set.v0.1.0 (Macro formalization container)
```csharp
public record BraidedCommitSet_v0_1_0(
    string Schema,                          // "gel.braided_commit_set.v0.1.0"
    string BraidIndexHash,
    BraidIndex_v0_1_0 BraidIndex,
    GoldenEngram_v0_1_0[] Engrams,          // sorted by EngramId
    string SetHash,                         // sha256(canonicalBytes(without SetHash))
    string CanonicalSeal                    // sha256(full serialization)
);
```

### 5) gel.tip_advance_event.v0.1.0 (CAS success telemetry spine unit)
```csharp
public record GelTipAdvanceEvent_v0_1_0(
    string Schema,                          // "gel.tip_advance_event.v0.1.0"
    string PreviousTip,
    string NewTip,
    string CommitIntentHash,
    string GateDecisionHash,
    string BraidIndexHash,
    string[] EngramIdsWritten,              // sorted
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick,
    long SpineSequence,                     // local monotonic; optional for cross-substrate equivalence
    string EventHash,                       // sha256(canonicalBytes(without EventHash))
    string CanonicalSeal                    // sha256(full serialization)
);
```

### 6) gel.commit_batch.v0.1.0 (Durable atomic commit artifact)
```csharp
public record GelCommitBatch_v0_1_0(
    string Schema,                          // "gel.commit_batch.v0.1.0"
    string BatchId,                         // sha256(canonicalBytes(without BatchId))
    string ParentTip,
    string NewTip,
    long BatchSequence,                     // local monotonic; optional cross-substrate
    string CommitIntentHash,
    string GateDecisionHash,
    string GateEvidenceHash,
    string PolicyVersion,
    string BraidIndexHash,
    string IntakeHash,                      // "mixed" if not shared
    GoldenEngramRef_v0_1_0[] Engrams,       // sorted by EngramId
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick,
    string CanonicalSeal                    // sha256(full serialization)
);

public record GoldenEngramRef_v0_1_0(
    string EngramId,
    string Handle,
    string ResolvedAddressText,
    string CanonicalSeal
);
```

### 7) gel.gel_index_delta.v0.1.0 (Add-only index deltas)
```csharp
public record GelIndexDelta_v0_1_0(
    string Schema,                          // "gel.gel_index_delta.v0.1.0"
    string CommitIntentHash,
    string GateDecisionHash,
    string BraidIndexHash,
    string PreviousTip,
    string NewTip,
    SymbolIndexUpdate_v0_1_0[] SymbolUpdates, // sorted
    HandleIndexUpdate_v0_1_0[] HandleUpdates, // sorted
    IntakeIndexUpdate_v0_1_0[] IntakeUpdates, // sorted
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick,
    string DeltaHash,                       // sha256(canonicalBytes(without DeltaHash))
    string CanonicalSeal                    // sha256(full serialization)
);

public record SymbolIndexUpdate_v0_1_0(
    string Namespace,
    string SymbolText,
    string PointerHash,
    string TargetEngramId,
    string Operation                        // "ADD" only in v0.1.0
);

public record HandleIndexUpdate_v0_1_0(
    string Handle,
    string EngramId,
    string Operation                        // "ADD"
);

public record IntakeIndexUpdate_v0_1_0(
    string IntakeHash,
    string EngramId,
    string Operation                        // "ADD"
);
```

### 8) gel.bootstrap_manifest.v0.1.0 (Deterministic genesis set)
```csharp
public record GelBootstrapManifest_v0_1_0(
    string Schema,                          // "gel.bootstrap_manifest.v0.1.0"
    string BootstrapVersion,                // "v0.1.0"
    string GenesisParentTip,                // e.g. "GENESIS"
    long GenesisTick,                       // e.g. 0
    string PolicyVersion,
    BootstrapItem_v0_1_0[] Items,           // sorted by Ordinal then Handle
    string BootstrapHash,                   // sha256(canonicalBytes(without BootstrapHash))
    string CanonicalSeal                    // sha256(full serialization)
);

public record BootstrapItem_v0_1_0(
    int Ordinal,
    string Handle,
    string SourceKind,                      // "embedded_resource"|"file"|"compiled_asset"
    string SourceRef,
    string TargetResolvedAddressText,
    string ExpectedPayloadHash,
    string ExpectedEngramId                 // optional (recommended for strict deterministic builds)
);
```

### 9) gel.bootstrap_result.v0.1.0 (Boot attestation)
```csharp
public record GelBootstrapResult_v0_1_0(
    string Schema,                          // "gel.bootstrap_result.v0.1.0"
    string BootstrapHash,
    string BootstrapVersion,
    string PolicyVersion,
    string GenesisParentTip,
    long GenesisTick,
    string FinalTip,
    long BootstrapSequence,                 // local monotonic optional
    BootstrapWrite_v0_1_0[] Writes,         // sorted by Ordinal then Handle
    string ResultHash,                      // sha256(canonicalBytes(without ResultHash))
    string CanonicalSeal                    // sha256(full serialization)
);

public record BootstrapWrite_v0_1_0(
    int Ordinal,
    string Handle,
    string ResolvedAddressText,
    string PayloadHash,
    string EngramId,
    string EngramSeal
);
```

### 10) gel.root_index_entry.v0.1.0 (Atomic index payload entry)
```csharp
public record GelRootIndexEntry_v0_1_0(
    string Schema,                          // "gel.root_index_entry.v0.1.0"
    string Namespace,                       // "root"|"symbol"|"suffix"
    string KeyText,                         // canonical (recommended ASCII subset)
    string KeyHash,                         // sha256(KeyText)
    string[] EngramIds,                     // sorted add-only
    string[] Tags,                          // sorted structural tags
    string EntryHash,                       // sha256(canonicalBytes(without EntryHash))
    string CanonicalSeal                    // sha256(full serialization)
);
```

### 11) gel.conflict_event.v0.1.0 (CAS conflict audit)
```csharp
public record GelConflictEvent_v0_1_0(
    string Schema,                          // "gel.conflict_event.v0.1.0"
    string ConflictType,                    // "TIP_CONFLICT"|"PARENT_MISMATCH"|"REPLAY_CONFLICT"
    string ReasonCode,                      // closed vocab
    string ExpectedParentTip,
    string ObservedCurrentTip,
    string ProposedNewTip,
    string CommitIntentHash,
    string GateDecisionHash,
    string BraidIndexHash,
    string[] IntendedEngramIds,             // sorted
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick,
    long AttemptSequence,                   // local monotonic optional
    string ConflictHash,                    // sha256(canonicalBytes(without ConflictHash))
    string CanonicalSeal                    // sha256(full serialization)
);
```

### 12) gel.quarantine_event.v0.1.0 (Safe-fail state transitions)
```csharp
public record GelQuarantineEvent_v0_1_0(
    string Schema,                          // "gel.quarantine_event.v0.1.0"
    string FromState,                       // "Operational"|"Frozen"|"Quarantined"|"Halt"
    string ToState,                         // same closed vocab
    string TransitionReasonCode,            // closed vocab
    string RelatedPacketHash,
    string RelatedGateDecisionHash,
    string RelatedCommitIntentHash,
    string HostInstanceId,
    string PolicyVersion,
    long TransitionSequence,                // local monotonic optional
    long GenesisTick,
    string EventHash,                       // sha256(canonicalBytes(without EventHash))
    string CanonicalSeal                    // sha256(full serialization)
);
```

### 13) gel.mirror_snapshot.v0.1.0 (Mirror state proof)
```csharp
public record GelMirrorSnapshot_v0_1_0(
    string Schema,                          // "gel.mirror_snapshot.v0.1.0"
    string Tip,
    long SnapshotSequence,                  // local monotonic optional
    string[] RecentTipEventHashes,          // fixed window order or sorted (freeze one policy in implementation)
    string CommitBatchHash,
    string IndexDeltaHash,
    string SymbolIndexSnapshotHash,
    string HandleIndexSnapshotHash,
    string IntakeIndexSnapshotHash,
    string BootstrapHash,
    string PolicyBundleHash,
    string HostInstanceId,
    string PolicyVersion,
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick,
    string SnapshotHash,                    // sha256(canonicalBytes(without SnapshotHash))
    string CanonicalSeal                    // sha256(full serialization)
);
```

---

## VII.B Layer 1 — SLI (Structural/Governance Layer)

### 1) sli.resolved_address.v0.1.0 (Address normal form)
```csharp
public record SliResolvedAddress_v0_1_0(
    string Schema,                          // "sli.resolved_address.v0.1.0"
    string Channel,                         // "Public"|"Private"
    string Partition,                       // "GEL"|"GOA"|"OAN"
    string Mirror,                          // "Standard"|"Cryptic"
    string AddressText,                     // $"{Channel}/{Partition}/{Mirror}"
    string AddressHash                      // sha256(AddressText)
);
```

### 2) sli.session_mounts.v0.1.0 (Allowlist of mounted addresses)
```csharp
public record SliSessionMounts_v0_1_0(
    string Schema,                          // "sli.session_mounts.v0.1.0"
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick,
    string MountPolicyVersion,
    SliResolvedAddress_v0_1_0[] MountedAddresses, // sorted by AddressText
    MountConstraint_v0_1_0[] Constraints,   // sorted by AddressText
    string MountsHash,                      // sha256(canonicalBytes(without MountsHash))
    string CanonicalSeal                    // sha256(full serialization)
);

public record MountConstraint_v0_1_0(
    string AddressText,                     // must match a MountedAddresses entry
    string[] RequiredSatModes,              // sorted
    bool RequiresHITL,
    bool ReadOnly,
    string Notes                            // optional; if included, decide hash participation
);
```

### 3) sli.packet.v0.1.0 (SLI Packet)
```csharp
public record SliPacket_v0_1_0(
    string Schema,                          // "sli.packet.v0.1.0"
    string PacketHash,                      // sha256(canonicalBytes(without PacketHash))
    string CanonicalSeal,                   // sha256(full serialization)
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick,
    PacketEnv_v0_1_0 Env,
    PacketFrame_v0_1_0 Frame,
    PacketMode_v0_1_0 Mode,
    PacketOp_v0_1_0 Op,
    string[] Handles,                       // sorted declared handles
    PacketRefs_v0_1_0 Refs,
    GateTracePlaceholder_v0_1_0 GateTrace   // empty pre-gate; filled only by gate (or kept separate)
);

public record PacketEnv_v0_1_0(string Channel, string Partition, string Mirror);
public record PacketFrame_v0_1_0(string FrameId, string FrameVersion, Dictionary<string,string> FrameMeta);
public record PacketMode_v0_1_0(string SatMode, bool SafActive, string MaskingState);
public record PacketOp_v0_1_0(string OpCode); // "NoOp"|"Propose"|"CommitIntent"|"ExecuteIntent"
public record PacketRefs_v0_1_0(string IntakeHash, string ProposedBraidIndexHash, string[] OriginEngramIds, string[] SymbolPointers);
public record GateTracePlaceholder_v0_1_0(bool IsEmpty); // MUST be empty pre-gate in v0.1.0
```

### 4) sli.root_atlas_entry.v0.1.0 (Static capability registry entry)
```csharp
public record SliRootAtlasEntry_v0_1_0(
    string Schema,                          // "sli.root_atlas_entry.v0.1.0"
    string Handle,
    string IntentKind,
    AddressConstraint_v0_1_0 AllowedAddress,
    string[] RequiredSatModes,              // sorted
    bool RequiresHITL,
    string PolicyVersion,
    CrypticConstraint_v0_1_0 Cryptic,
    string Description,
    string Owner,
    string EntryHash,                       // sha256(canonicalBytes(without EntryHash))
    string CanonicalSeal                    // sha256(full serialization)
);

public record AddressConstraint_v0_1_0(string AllowedChannel, string AllowedPartition, string AllowedMirror);
public record CrypticConstraint_v0_1_0(bool IsCrypticCapable, string MinimumSatModeForCryptic, bool ApplyMaskingPolicy, string MaskingPolicyId);
```

### 5) sli.handle_registry_manifest.v0.1.0 (Pinned handle set)
```csharp
public record SliHandleRegistryManifest_v0_1_0(
    string Schema,                          // "sli.handle_registry_manifest.v0.1.0"
    string RegistryVersion,                 // "v0.1.0"
    string PolicyVersion,
    HandleEntry_v0_1_0[] Handles,           // sorted by Handle
    string RegistryHash,                    // sha256(canonicalBytes(without RegistryHash))
    string CanonicalSeal                    // sha256(full serialization)
);

public record HandleEntry_v0_1_0(
    string Handle,
    string EntryHash,
    string IntentKind,
    string AllowedAddressConstraintText,
    string[] RequiredSatModes               // sorted
);
```

### 6) sli.policy_bundle.v0.1.0 (Pinned policy root)
```csharp
public record SliPolicyBundle_v0_1_0(
    string Schema,                          // "sli.policy_bundle.v0.1.0"
    string BundleVersion,                   // "v0.1.0"
    string PolicyVersion,
    string RootAtlasHash,
    string HandleRegistryHash,
    string MountPolicyVersion,
    string TelemetryManifestHash,
    string ReconstructionProfileVersion,
    string ReconstructionProfileHash,
    string BootstrapVersion,
    string BootstrapHash,
    string[] RequiredModules,               // sorted
    string[] ForbiddenModules,              // sorted
    string BundleHash,                      // sha256(canonicalBytes(without BundleHash))
    string CanonicalSeal                    // sha256(full serialization)
);
```

### 7) sli.gate_evidence.v0.1.0 (Structured gating facts)
```csharp
public record SliGateEvidence_v0_1_0(
    string Schema,                          // "sli.gate_evidence.v0.1.0"
    string PacketHash,
    string SessionMountsHash,
    string RootAtlasHash,
    string PolicyVersion,
    string SatModeObserved,
    bool SafActiveObserved,
    string RequestedEnvChannel,
    string RequestedEnvPartition,
    string RequestedEnvMirror,
    HandleEvidence_v0_1_0[] Handles,        // sorted by Handle
    bool CrypticAttempted,
    bool MaskingApplied,
    string MaskingPolicyId,
    bool IsAllowed,
    string ReasonCode,
    string EvidenceHash,                    // sha256(canonicalBytes(without EvidenceHash))
    string CanonicalSeal                    // sha256(full serialization)
);

public record HandleEvidence_v0_1_0(
    string Handle,
    bool ExistsInRootAtlas,
    string EntryHash,
    string AllowedAddressText,
    string ResolvedAddressText,
    bool MountAllowed,
    bool SatSatisfied,
    bool HitlSatisfied,
    string FailureReason                    // closed vocab
);
```

### 8) sli.gate_decision.v0.1.0 (Allow/Deny output)
```csharp
public record SliGateDecision_v0_1_0(
    string Schema,                          // "sli.gate_decision.v0.1.0"
    string PacketHash,
    string RootAtlasHash,
    string SessionMountsHash,
    string PolicyVersion,
    bool IsAllowed,
    string ReasonCode,
    string SatModeObserved,
    bool SafActiveObserved,
    HandleResolution_v0_1_0[] Resolutions,  // sorted by Handle
    bool MaskingApplied,
    string MaskingPolicyId,
    string EvidenceHash,                    // links to sli.gate_evidence.EvidenceHash
    string DecisionHash,                    // sha256(canonicalBytes(without DecisionHash))
    string CanonicalSeal                    // sha256(full serialization)
);

public record HandleResolution_v0_1_0(
    string Handle,
    string AddressText,
    string AddressHash,
    string EntryHash
);
```

### 9) sli.commit_intent.v0.1.0 (Post-gate eligible-to-commit object)
```csharp
public record SliCommitIntent_v0_1_0(
    string Schema,                          // "sli.commit_intent.v0.1.0"
    string PacketHash,
    string GateDecisionHash,
    string GateEvidenceHash,
    string PolicyVersion,
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick,
    string IntakeHash,
    string ProposedBraidIndexHash,
    string ParentTip,
    CommitDirective_v0_1_0[] Directives,    // sorted by Handle
    string CommitIntentHash,                // sha256(canonicalBytes(without CommitIntentHash))
    string CanonicalSeal                    // sha256(full serialization)
);

public record CommitDirective_v0_1_0(
    string Handle,
    string ResolvedAddressText,
    string EntryHash,
    string ConstructorPayloadRef,
    string ConstructorPayloadHash
);
```

### 10) sli.symbol_pointer.v0.1.0 (Exact symbol → target reference)
```csharp
public record SliSymbolPointer_v0_1_0(
    string Schema,                          // "sli.symbol_pointer.v0.1.0"
    string PointerHash,                     // sha256(canonicalBytes(without PointerHash))
    string CanonicalSeal,                   // sha256(full serialization)
    string Namespace,                       // "sli"|"gel"|"bootstrap"|...
    string SymbolText,                      // canonical symbol string
    string SymbolEncoding,                  // "utf-8"
    string PointerKind,                     // "engram_id"|"bootstrap_handle"|"resolved_address"|"external_ref"
    SymbolTarget_v0_1_0 Target,
    PointerProvenance_v0_1_0 Provenance
);

public record SymbolTarget_v0_1_0(
    string TargetRef,                       // e.g. "sha256:<hex>" or "engram.bootstrap.rootatlas"
    string ResolvedAddressText,
    string TargetSchemaHint,
    string[] TargetTags                     // sorted structural tags
);

public record PointerProvenance_v0_1_0(
    string SourceSystem,
    string CreationMode,                    // "boot"|"commit"|"index_build"|"import"
    string OriginBraidIndexHash,
    string OriginEngramId,
    string Notes
);
```

### 11) sli.tensor.v0.1.0 (Structural tensor; no salience)
```csharp
public record SliTensor_v0_1_0(
    string Schema,                          // "sli.tensor.v0.1.0"
    string TensorVersion,                   // "v0.1.0"
    string TensorizationProfileVersion,
    string TensorizationProfileHash,
    string[] OriginEngramIds,               // sorted
    string[] OriginBraidIndexHashes,        // sorted
    string IntakeHash,                      // optional; else "mixed"
    TensorNode_v0_1_0[] Nodes,              // sorted by NodeId
    TensorEdge_v0_1_0[] Edges,              // sorted by (From,Relation,To)
    string[] SymbolPointers,                // sorted
    string EncodingMode,                    // "symbolic_lattice"|"minimal"|...
    string[] FeatureKeys,                   // sorted
    string[] FeatureValues,                 // aligned ordering
    string OriginTensorHash,                // sha256(canonicalBytes(without OriginTensorHash))
    string CanonicalSeal                    // sha256(full serialization)
);

public record TensorNode_v0_1_0(
    string NodeId,
    string NodeType,                        // "golden_engram"|"symbol"|"root"|"composite"
    string OriginEngramId,
    string ResolvedAddressText,
    string[] Tags,                          // sorted structural only
    Dictionary<string,string> Attributes    // sorted keys; structural only
);

public record TensorEdge_v0_1_0(
    string FromNodeId,
    string ToNodeId,
    string RelationType,                    // "references"|"depends_on"|"root_of"|"maps_to"
    Dictionary<string,string> Attributes    // sorted keys; structural only
);
```

### 12) sli.telemetry_record.v0.1.0 (Unified telemetry line schema; NDJSON friendly)
```csharp
public record SliTelemetryRecord_v0_1_0(
    string Schema,                          // "sli.telemetry_record.v0.1.0"
    string EventType,                       // closed vocab
    string EventHash,                       // sha256(canonicalBytes(without EventHash))
    string CanonicalSeal,                   // sha256(full serialization)
    long GenesisTick,
    long EventSequence,                     // local monotonic optional
    string PacketHash,
    string GateDecisionHash,
    string GateEvidenceHash,
    string CommitIntentHash,
    string BraidIndexHash,
    string PreviousTip,
    string NewTip,
    string DeltaHash,
    bool? IsAllowed,
    string ReasonCode,
    string PolicyVersion,
    string[] Handles,                       // sorted
    string SessionId,
    string OperatorId,
    string ScenarioName
);
```

### 13) sli.telemetry_stream_manifest.v0.1.0 (What to emit, where)
```csharp
public record SliTelemetryStreamManifest_v0_1_0(
    string Schema,                          // "sli.telemetry_stream_manifest.v0.1.0"
    string ManifestVersion,                 // "v0.1.0"
    string PolicyVersion,
    string MountPolicyVersion,
    StreamRule_v0_1_0[] Rules,              // sorted by EventType then SinkId
    SinkSpec_v0_1_0[] Sinks,                // sorted by SinkId
    string ManifestHash,                    // sha256(canonicalBytes(without ManifestHash))
    string CanonicalSeal                    // sha256(full serialization)
);

public record StreamRule_v0_1_0(
    string EventType,
    string SinkId,
    int DetailLevel,
    bool Enabled,
    string SamplingMode,                    // "all"|"none"
    string Notes
);

public record SinkSpec_v0_1_0(
    string SinkId,
    string SinkKind,                        // "ndjson_file"|"memory_queue"|"duplex_stream"
    Dictionary<string,string> Parameters    // sorted keys
);
```

### 14) sli.duplex_message.v0.1.0 (Duplex wire envelope)
```csharp
public record SliDuplexMessage_v0_1_0(
    string Schema,                          // "sli.duplex_message.v0.1.0"
    string MessageHash,                     // sha256(canonicalBytes(without MessageHash))
    string CanonicalSeal,                   // sha256(full serialization)
    string Direction,                       // "HOST_TO_DUPLEX"|"DUPLEX_TO_HOST"
    string MessageType,                     // closed vocab
    string CorrelationId,                   // deterministic (no GUID)
    long Sequence,                          // local monotonic optional
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick,
    string PayloadSchema,
    string PayloadHash,
    string PayloadInline                    // optional canonical JSON or "null"
);
```

### 15) sli.symbol_resolution_request.v0.1.0 (Exact resolution request)
```csharp
public record SliSymbolResolutionRequest_v0_1_0(
    string Schema,                          // "sli.symbol_resolution_request.v0.1.0"
    string RequestHash,                     // sha256(canonicalBytes(without RequestHash))
    string CanonicalSeal,                   // sha256(full serialization)
    string Namespace,
    string SymbolText,
    string PointerHashHint,
    string ResolutionMode,                  // "EXACT" only
    string[] AllowedPointerKinds,           // sorted
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick
);
```

### 16) sli.symbol_resolution_response.v0.1.0 (Exact resolution response)
```csharp
public record SliSymbolResolutionResponse_v0_1_0(
    string Schema,                          // "sli.symbol_resolution_response.v0.1.0"
    string ResponseHash,                    // sha256(canonicalBytes(without ResponseHash))
    string CanonicalSeal,                   // sha256(full serialization)
    string RequestHash,
    string Namespace,
    string SymbolText,
    string PointerHashResolved,
    bool IsFound,
    string NotFoundReason,
    string PointerKind,
    SymbolTargetResolved_v0_1_0 Target,
    string IndexTip,
    string IndexHash,
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick
);

public record SymbolTargetResolved_v0_1_0(
    string TargetRef,
    string ResolvedAddressText,
    string TargetSchemaHint,
    string[] TargetTags                     // sorted
);
```

### 17) sli.refusal_record.v0.1.0 (Standard denial object)
```csharp
public record SliRefusalRecord_v0_1_0(
    string Schema,                          // "sli.refusal_record.v0.1.0"
    string RefusalHash,                     // sha256(canonicalBytes(without RefusalHash))
    string CanonicalSeal,                   // sha256(full serialization)
    string PacketHash,
    string GateDecisionHash,
    string GateEvidenceHash,
    string PolicyVersion,
    string MountPolicyVersion,
    string RefusalType,                     // "GATE_DENY"|"MOUNT_DENY"|"VALIDATION_FAIL"|"SAF_DEFER"
    string ReasonCode,                      // closed vocab
    string[] AffectedHandles,               // sorted
    string[] MissingHandles,                // sorted
    bool SafActiveObserved,
    string SatModeObserved,
    string GuidanceCode,                    // closed vocab optional
    string Notes                            // optional; decide hash participation in implementation
);
```

---

## VII.C Layer 2 — IUTT (Runtime Cognition Layer)

### 1) iutt.reconstruction_profile.v0.1.0 (Hashable runtime reconstruction spec)
```csharp
public record IuttReconstructionProfile_v0_1_0(
    string Schema,                          // "iutt.reconstruction_profile.v0.1.0"
    string ReconstructionProfileVersion,    // "v0.1.0"
    string ProfileName,
    string ProfileDescription,
    string ProfileOwner,
    OperatorPolicy_v0_1_0 Operators,
    PerspectivePolicy_v0_1_0 Perspective,
    SaliencePolicy_v0_1_0 Salience,
    RelevancePolicy_v0_1_0 Relevance,
    EquivalencePolicy_v0_1_0 Equivalence,
    TelemetryPolicy_v0_1_0 Telemetry,
    BasinPolicy_v0_1_0 Basins,
    ConstraintPolicy_v0_1_0 Constraints,
    CanonicalizationPolicy_v0_1_0 Canonicalization,
    string ReconstructionProfileHash,       // sha256(canonicalBytes(without ReconstructionProfileHash))
    string CanonicalSeal                    // sha256(full serialization)
);

public record OperatorPolicy_v0_1_0(
    string StochasticMode,                  // "none"|"seeded"|"allowed"
    string[] AllowedOperators,              // sorted
    Dictionary<string,string> OperatorParams,// sorted keys
    string ForbiddenOperatorRule
);

public record PerspectivePolicy_v0_1_0(
    string DefaultFrameId,
    string[] AllowedFrames,                 // sorted
    string[] AllowedTransformTypes,         // sorted
    Dictionary<string,string> FrameParams,  // sorted keys
    bool RequireExplicitFrameDeclaration
);

public record SaliencePolicy_v0_1_0(
    string SalienceMode,                    // "attention"|"gradient"|"operator_weighted"|"none"
    double TemperatureDefault,
    double ThresholdDefault,
    int MaxActiveNodes,
    bool AllowEdgeWeights,
    string[] AllowedWeightSources           // sorted
);

public record RelevancePolicy_v0_1_0(
    string SelectionRuleSetId,
    bool RequireRelevanceTrace,
    bool RequireRejectedSet,
    int MaxSelectedEngrams,
    string JustificationVocabularyId
);

public record EquivalencePolicy_v0_1_0(
    string EquivalenceMode,                 // "structural"|"behavioral"|"hybrid"
    string[] RequiredInvariants,            // sorted
    double Tolerance,
    string[] RequiredArtifacts              // sorted
);

public record TelemetryPolicy_v0_1_0(
    bool EmitRuntimeStateDigest,
    bool EmitSalienceSummary,
    bool EmitRelevanceTraceDigest,
    bool EmitTriptychSignals,
    string TelemetrySinkId,
    int MaxTelemetryDetailLevel
);

public record BasinPolicy_v0_1_0(
    bool EnableBasins,
    double BasinThreshold,
    string BasinMetricId,
    string OutOfBasinAction                 // "warn"|"freeze"|"quarantine"|"refuse"
);

public record ConstraintPolicy_v0_1_0(
    bool ForbidIdentityMutation,
    bool ForbidCanonicalHashInputsFromLayer2,
    bool ForbidAutoCommit,
    string[] ForbiddenFieldsInGEL,          // sorted
    string[] ForbiddenOperators             // sorted
);

public record CanonicalizationPolicy_v0_1_0(
    string CanonicalizationVersion,
    string Encoding,
    bool SortArraysLexicographically,
    bool SortDictionaryKeysLexicographically,
    string NullLiteral,
    bool LowercaseHex
);
```

### 2) iutt.runtime_state.v0.1.0 (Ephemeral runtime cognition state; Layer-2 only)
```csharp
public record IuttRuntimeState_v0_1_0(
    string Schema,                          // "iutt.runtime_state.v0.1.0"
    string RuntimeStateId,                  // optional (runtime envelope hash) or "none"
    string CreatedTick,                     // deterministic tick relative to session genesis
    string ReconstructionProfileVersion,
    string ReconstructionProfileHash,
    string QueryContextHash,
    string SessionId,
    string OperatorId,
    string ScenarioName,
    string[] OriginEngramIds,               // sorted
    string OriginTensorHash,
    string OriginBraidIndexHash,
    PerspectiveFrame_v0_1_0 Perspective,
    SalienceField_v0_1_0 Salience,
    RelevanceTrace_v0_1_0 Relevance,
    RuntimeAssembly_v0_1_0 Assembly,
    TriptychSignals_v0_1_0 Signals,
    BoundaryAttestation_v0_1_0 Boundary,
    string[] Warnings,
    string[] Notes
);

public record PerspectiveFrame_v0_1_0(
    string FrameId,
    string TransformType,
    Dictionary<string,string> Parameters,   // sorted keys
    string SourceFrameId,
    string TargetFrameId
);

public record SalienceField_v0_1_0(
    string SalienceMode,
    Dictionary<string,double> NodeWeights,
    Dictionary<string,double> EdgeWeights,
    string GradientDescriptor,
    double Temperature,
    double Threshold
);

public record RelevanceTrace_v0_1_0(
    string SelectionRuleSet,
    string[] SelectedEngramIds,             // sorted
    string[] RejectedEngramIds,             // sorted
    Dictionary<string,string> Justifications,// sorted keys
    string EvidenceDigest
);

public record RuntimeAssembly_v0_1_0(
    string AssemblyId,
    RuntimeNode_v0_1_0[] Nodes,
    RuntimeBinding_v0_1_0[] Bindings,
    string[] ActiveSymbolPointers,          // sorted
    string WorkingSummary
);

public record RuntimeNode_v0_1_0(
    string NodeId,
    string NodeType,
    string OriginEngramId,
    Dictionary<string,string> Tags          // sorted keys
);

public record RuntimeBinding_v0_1_0(
    string FromNodeId,
    string ToNodeId,
    string Relation,
    double Weight
);

public record TriptychSignals_v0_1_0(
    DriftSignals_v0_1_0 Drift,
    CompassSignals_v0_1_0 Compass,
    HarmonicsSignals_v0_1_0 Harmonics
);

public record DriftSignals_v0_1_0(double DriftMagnitude, double DriftCurvature, string DriftWindowId);
public record CompassSignals_v0_1_0(double EthicalGradient, double EpistemicStability, double NarrativeCoherence);
public record HarmonicsSignals_v0_1_0(double Resonance, double Dissonance, double Coupling);

public record BoundaryAttestation_v0_1_0(
    bool Layer0Untouched,                   // MUST be true
    bool CanonicalHashInputsClean,          // MUST be true
    string[] ForbiddenFieldsDetected,       // MUST be empty
    string AttestationHash
);
```

---

# VIII. Gate Evaluation Rules (Deterministic)

Given a `sli.packet` and pinned policy artifacts, Gate evaluation proceeds in frozen order:

1) Validate `Handles.Length > 0` else deny (`NO_HANDLE`).  
2) For each handle: exact lookup in RootAtlas (and registry pin) else deny (`HANDLE_NOT_FOUND`).  
3) Resolve address constraints (Channel×Partition×Mirror) deterministically.  
4) Validate `sli.session_mounts` allows the resolved address else deny (`MOUNT_DENY`).  
5) Validate SAT/HITL requirements else deny (`SAT_FAIL` / `HITL_REQUIRED`).  
6) If Cryptic: enforce minimum SAT mode and masking policy; log deterministically.  
7) Emit `sli.gate_evidence` and `sli.gate_decision` with hash proofs.

No fallback. No heuristics. No semantic inference.

---

# IX. Commit Rules (Identity Event)

CommitEngine consumes `sli.commit_intent` and performs:

1) Verify gate decision allowed and evidence hashes match pins.  
2) Verify `ParentTip` matches current GEL tip (CAS precondition).  
3) Build `gel.braid_index` (sorted handles, sorted maps, pinned profile hash).  
4) For each directive: build one `gel.golden_engram` deterministically.  
5) Append engrams to GEL in one atomic CAS.  
6) On success: emit `gel.tip_advance_event`, `gel.commit_batch`, and index deltas.  
7) On conflict: emit `gel.conflict_event` and do not advance tip.

---

# X. Testing & Validation (Harness Expectations)

Minimum harness tests for v0.1.0:

1) **No Handle, No Action**: empty handles → deny with deterministic reason.  
2) **Deterministic Replay**: same intake + same declared handles + same genesis factors → identical EngramIds & BraidIndexHash.  
3) **Layer Boundary**: verify `iutt.runtime_state.Boundary.Layer0Untouched == true`.  
4) **CAS Discipline**: simulate tip conflict → conflict event, no tip advance event.  
5) **Transport Round-trip**: tensorize → glue → reconstruct; origin anchors preserved (per equivalence policy).  
6) **Bootstrap Stability**: bootstrap manifest replay yields stable EngramIds (if ExpectedEngramId locked).  
7) **Index Delta Correctness**: commit batch EngramIds appear in add-only deltas.  
8) **Cryptic Containment**: Cryptic mirror requires SAT gate; masking rules logged.

---

# XI. Freeze Statement (v0.1.0)

This specification is frozen until MAJOR version increment. It prevents:
- ontology collapse
- storage-layer salience contamination
- heuristic drift into identity
- nondeterministic identity surfaces

The system is now a mechanical cognitive ledger:\n- deterministic identity (Layer 0)\n- structural routing (Layer 1)\n- runtime engineered cognition (Layer 2)\n\n**End of ENGRAMMITIZATION_SPEC_v0.1.0**\n