# Engrammitization Constitution v0.1.0

"CONSOLIDATION SUMMARY
- Number of source files included: 29
- Conflicts detected: NO
- List of schema ids present:
  - gel.pregel_bundle.v0.1.0
  - gel.braid_index.v0.1.0
  - gel.golden_engram.v0.1.0
  - gel.braided_commit_set.v0.1.0
  - gel.tip_advance_event.v0.1.0
  - gel.commit_batch.v0.1.0
  - gel.gel_index_delta.v0.1.0
  - gel.bootstrap_manifest.v0.1.0
  - gel.bootstrap_result.v0.1.0
  - gel.root_index_entry.v0.1.0
  - gel.conflict_event.v0.1.0
  - gel.quarantine_event.v0.1.0
  - gel.mirror_snapshot.v0.1.0
  - sli.resolved_address.v0.1.0
  - sli.session_mounts.v0.1.0
  - sli.packet.v0.1.0
  - sli.root_atlas_entry.v0.1.0
  - sli.handle_registry_manifest.v0.1.0
  - sli.policy_bundle.v0.1.0
  - sli.gate_evidence.v0.1.0
  - sli.gate_decision.v0.1.0
  - sli.commit_intent.v0.1.0
  - sli.symbol_pointer.v0.1.0
  - sli.tensor.v0.1.0
  - sli.telemetry_record.v0.1.0
  - sli.telemetry_stream_manifest.v0.1.0
  - sli.duplex_message.v0.1.0
  - sli.symbol_resolution_request.v0.1.0
  - sli.symbol_resolution_response.v0.1.0
  - sli.refusal_record.v0.1.0
  - iutt.reconstruction_profile.v0.1.0
  - iutt.runtime_state.v0.1.0

# CONSOLIDATION SUMMARY: Engrammitization Constitution v0.1.0 (FINAL)

**STATUS**: FINAL / SOURCE OF TRUTH  
**CONFLICTS DETECTED**: NO  
**SOURCE-OF-TRUTH DECLARATION**: FINAL  
**INTEGRITY NOTES**: NO  
**LAST UPDATED**: 2026-02-21 00:30 PST

# Engrammitization
## Constitutional Corpus

### 0. Document Control

| Field | Value |
| :--- | :--- |
| **Project** | OAN Mortalis v1.0 |
| **Module** | Engrammitization |
| **Status** | FINAL |
| **Version** | v0.1.0 (Consolidated) |
| **Conflicts Detected** | NO |
| **Source-of-truth declaration** | FINAL |
| **Author** | Antigravity AI |
| **Last Verification** | 2026-02-21 |
- Consolidation date: 2026-02-20
- Included source list:
  1) Modules\Engrammitization\ENGRAMMITIZATION_SPEC_v0.1.0.md
  2) Modules\Engrammitization\Symbol Resolution RPC.md
  3) Modules\Engrammitization\gel.bootstrap_manifest.v0.1.0.md
  4) Modules\Engrammitization\gel.bootstrap_result.v0.1.0.md
  5) Modules\Engrammitization\gel.commit_batch.v0.1.0.md
  6) Modules\Engrammitization\gel.conflict_event.v0.1.0.md
  7) Modules\Engrammitization\gel.gel_index_delta.v0.1.0.md
  8) Modules\Engrammitization\gel.mirror_snapshot.v0.1.0.md
  9) Modules\Engrammitization\gel.quarantine_event.v0.1.0.md
  10) Modules\Engrammitization\gel.root_index_entry.v0.1.0.md
  11) Modules\Engrammitization\gel.tip_advance_event.v0.1.0.md
  12) Modules\Engrammitization\iutt.reconstruction_profile.v0.1.0.md
  13) Modules\Engrammitization\iutt.runtime_state.v0.1.0.md
  14) Modules\Engrammitization\sli.commit_intent.v0.1.0.md
  15) Modules\Engrammitization\sli.duplex_message.v0.1.0.md
  16) Modules\Engrammitization\sli.gate_decision.v0.1.0.md
  17) Modules\Engrammitization\sli.gate_evidence.v0.1.0.md
  18) Modules\Engrammitization\sli.handle_registry_manifest.v0.1.0.md
  19) Modules\Engrammitization\sli.packet.v0.1.0.md
  20) Modules\Engrammitization\sli.policy_bundle.v0.1.0.md
  21) Modules\Engrammitization\sli.refusal_record.v0.1.0.md
  22) Modules\Engrammitization\sli.resolved_address.v0.1.0.md
  23) Modules\Engrammitization\sli.root_atlas_entry.v0.1.0.md
  24) Modules\Engrammitization\sli.session_mounts.v0.1.0.md
  25) Modules\Engrammitization\sli.symbol_pointer.v0.1.0.md
  26) Modules\Engrammitization\sli.telemetry_record.v0.1.0.md
  27) Modules\Engrammitization\sli.telemetry_stream_manifest.v0.1.0.md
  28) Modules\Engrammitization\sli.tensor.v0.1.0.md
  29) Modules\Engrammitization\Engrammitization.md (Pre-Consolidation)
- Integrity notes (conflicts? yes/no): NO

### 1. Layer Taxonomy & Global Invariants

#### Layer Taxonomy (Non-Negotiable)
All objects in the OAN Mortalis stack must belong to exactly one layer.

| Layer       | Prefix   | Ontological Role                              | May Affect Identity?             | Runtime Context Allowed? |
|-------------|----------|-----------------------------------------------|----------------------------------|--------------------------|
| **Layer 0** | `gel.*`  | Crystallized identity storage (inert database)| YES                              | NO                       |
| **Layer 1** | `sli.*`  | Symbolic tensorization & routing              | NO                               | NO                       |
| **Layer 2** | `iutt.*` | Runtime reconstruction & engineered cognition | NO (unless explicitly committed) | YES                      |

Violation of layer boundaries is a spec breach and causes immediate test harness failure.

#### Prefix Rule (Mandatory)
Every schema begins with its layer prefix:
- `gel.` → Append-only canonical storage object (Layer 0)
- `sli.` → Symbolic routing, tensor, gate, duplex object (Layer 1)
- `iutt.` → Runtime reconstruction object (Layer 2)

#### Identity Eligibility Rule
Only `gel.*` schemas may:
- generate/hold identity anchors (e.g., EngramId)
- advance `ParentTip`
- be hashed into identity-bearing canonical envelopes
- affect the GEL spine (append-only ledger)

### 2. Canonical Serialization Contract (Mandatory)
All hashes in this spec (PacketHash, EntryHash, EngramId, EvidenceHash, etc.) MUST be computed from **canonical bytes** built by a manual canonical builder.

#### Canonical Builder Rules
1. Fixed field order per schema version (frozen order)
2. Arrays sorted lexicographically (StringComparer.Ordinal)
3. Dictionary keys sorted lexicographically (ordinal); values serialized after keys
4. `null` serialized as literal UTF-8 `"null"`
5. UTF-8 encoding only
6. Lowercase hex for all hashes
7. No serializer auto-ordering; manual builder only
8. No wall-clock timestamps in any canonical hash inputs
9. No Layer-2 fields may appear inside `gel.*` canonical envelopes

#### Hash Conventions
- `XHash = sha256(canonicalBytes(X without XHash))`
- `CanonicalSeal = sha256(full canonical serialization)`

### 3. Formal Definition of Engrammitization
**Engrammitization** is the deterministic, layer-strict pipeline:
**Raw Intake → PreGELBundle (proposal) → SLI Packet (explicit handles) → Gate Evaluation → CommitIntent → GoldenEngram(s) → CAS Tip Advancement**

#### Core Invariants
- **No Handle, No Action**: packets with no declared handles are non-executable.
- **Gate is capability routing only**: no semantic heuristics, no content inference.
- **Commit-only crystallization**: identity changes occur only at Commit.
- **GEL inertness**: Layer 0 stores no runtime salience, gradients, perspective shifts.
- **IUTT runtime cognition**: salience and relevance exist only in Layer 2 runtime state.
- **CAS discipline**: tip advancement requires compare-and-swap; conflicts are auditable.

#### The 6-Phase Pipeline
1. **Intake** → IntakePacket (intakeHash + genesis factors)
2. **Pre‑SLI Normalization** → NormalizedPreSliProduct (pure translation; zero inference)
3. **SLI Packetization + Handle Declaration** → gel.pregel_bundle + one or more sli.packet with OpCode=Propose or CommitIntent (explicitly declared handles)
4. **SLI Gate Evaluation** → sli.gate_evidence + sli.gate_decision (deterministic checks only)
5. **Commit Crystallization** → gel.braid_index + gel.golden_engram[] + gel.commit_batch + gel.tip_advance_event (append-only; CAS tip advancement)
6. **Optional IUTT Runtime Reconstruction** → iutt.runtime_state (ephemeral; salience/perspective/relevance here only)

### 4. Frozen Schema Registry (v0.1.0)

#### Layer 0 — GEL (Identity Layer)

##### 1. gel.pregel_bundle.v0.1.0
- **Purpose**: Non-authoritative staging object (intake anchor, declared handles, proposed braid, constructor proposals). Never appended to GEL.
- **Record**:
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

##### 2. gel.braid_index.v0.1.0
- **Purpose**: Deterministic join table (IntakeHash, sorted Declared/AdmittedHandles, ParentTip, sorted ResolvedAddresses, PolicyVersion, GateEvidenceSnapshot, ReconstructionProfileVersion, ReconstructionProfileHash). Hash = BraidIndexHash.
- **Record**:
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

##### 3. gel.golden_engram.v0.1.0
- **Purpose**: The only identity-bearing storage unit. Immutable crystallized symbolic unit produced by Commit.
- **Record**:
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

##### 4. gel.braided_commit_set.v0.1.0
- **Purpose**: Formalization of the macro `NameOfEngram{RootBaseE + [ECProp + ECProc + ECPers + ECPart] + RootCapE}BraidIndex`.
- **Record**:
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

##### 5. gel.bootstrap_manifest.v0.1.0
- **Purpose**: Defines the deterministic genesis set establish the boot lattice.
- **Record**:
  ```csharp
  public record GelBootstrapManifest_v0_1_0(
      string Schema,                      // "gel.bootstrap_manifest.v0.1.0"
      string BootstrapVersion,            // "v0.1.0"
      string BootstrapHash,               // sha256(canonicalBytes(without BootstrapHash))
      string CanonicalSeal,               // sha256(full canonical serialization)
      string GenesisParentTip,            // "GENESIS"
      long GenesisTick,                   // 0
      string PolicyVersion,               // "POLICY-0.1.0"
      BootstrapItem[] Items               // sorted by Ordinal then Handle
  );
  public record BootstrapItem(
      int Ordinal,                        // explicit order: 1..N
      string Handle,                      // e.g. "engram.bootstrap.rootatlas"
      string SourceKind,                  // "embedded_resource"|"file"|"compiled_asset"
      string SourceRef,                   // e.g. resource name or file relative path
      string TargetResolvedAddressText,   // "Private/GEL/Standard"
      string ExpectedPayloadHash,         // sha256(canonical bytes of source payload)
      string ExpectedEngramId             // optional
  );
  ```
- **Freeze Rules**:
  - Bootstrap is the only time you may “seed” GEL from static assets without prior engrams.
  - Ordinal is the canonical boot order; must not change without MAJOR bump.
  - ExpectedPayloadHash must match exactly or bootstrap fails closed.
- **Required Tests**:
  - BSM-1: BootstrapHash stable.
  - BSM-2: Payload hash mismatch → hard fail.

##### 6. gel.bootstrap_result.v0.1.0
- **Purpose**: Captures the outcome of applying gel.bootstrap_manifest.v0.1.0.
- **Record**:
  ```csharp
  public record GelBootstrapResult_v0_1_0(
      string Schema,                         // "gel.bootstrap_result.v0.1.0"
      string BootstrapHash,                  // gel.bootstrap_manifest.BootstrapHash
      string BootstrapVersion,               // "v0.1.0"
      string PolicyVersion,                  // "POLICY-0.1.0"
      string GenesisParentTip,
      long GenesisTick,
      string FinalTip,
      long BootstrapSequence,                // local monotonic
      BootstrapWrite[] Writes,               // sorted by Ordinal then Handle
      string ResultHash,                     // sha256(canonicalBytes(without ResultHash))
      string CanonicalSeal                   // sha256(full canonical serialization)
  );
  public record BootstrapWrite(
      int Ordinal,
      string Handle,
      string ResolvedAddressText,
      string PayloadHash,
      string EngramId,
      string EngramSeal
  );
  ```
- **Freeze Rules**: Every manifest item must produce exactly one BootstrapWrite. Writes must be sorted.
- **Required Tests**: BR-1: ResultHash stable.

##### 7. gel.commit_batch.v0.1.0
- **Purpose**: Durable container for a single Commit operation writing one or more engrams.
- **Record**:
  ```csharp
  public record GelCommitBatch_v0_1_0(
      string Schema,                         // "gel.commit_batch.v0.1.0"
      string BatchId,                        // sha256(canonicalBytes(batch without BatchId))
      string CanonicalSeal,                  // sha256(full canonical serialization)
      string ParentTip,
      string NewTip,
      long BatchSequence,
      string CommitIntentHash,
      string GateDecisionHash,
      string GateEvidenceHash,
      string PolicyVersion,
      string BraidIndexHash,
      string IntakeHash,
      GoldenEngramRef_v0_1_0[] Engrams,             // sorted by EngramId
      string SessionId,
      string OperatorId,
      string ScenarioName,
      long GenesisTick
  );
  public record GoldenEngramRef_v0_1_0(
      string EngramId,
      string Handle,
      string ResolvedAddressText,
      string CanonicalSeal
  );
  ```
- **Freeze Rules**: Engrams[] must be sorted; BatchId computed deterministically; atomic tip advancement.
- **Required Tests**: CB-1: BatchId stable.

##### 8. gel.conflict_event.v0.1.0
- **Purpose**: Records a deterministic audit event when GEL rejects an attempted write.
- **Record**:
  ```csharp
  public record GelConflictEvent_v0_1_0(
      string Schema,                         // "gel.conflict_event.v0.1.0"
      string ConflictHash,
      string CanonicalSeal,
      string ConflictType,                   // "TIP_CONFLICT" | "PARENT_MISMATCH" | "REPLAY_CONFLICT"
      string ReasonCode,
      string ExpectedParentTip,
      string ObservedCurrentTip,
      string ProposedNewTip,
      string CommitIntentHash,
      string GateDecisionHash,
      string BraidIndexHash,
      string[] IntendedEngramIds,            // sorted
      string SessionId,
      string OperatorId,
      string ScenarioName,
      long GenesisTick,
      long AttemptSequence
  );
  ```
- **Freeze Rules**: Emitted only when no write occurs. Closed vocab for ConflictType/ReasonCode.

##### 9. gel.gel_index_delta.v0.1.0
- **Purpose**: Defines index updates produced by a commit.
- **Record**:
  ```csharp
  public record GelIndexDelta_v0_1_0(
      string Schema,
      string CommitIntentHash,
      string GateDecisionHash,
      string BraidIndexHash,
      string PreviousTip,
      string NewTip,
      string DeltaHash,
      string CanonicalSeal,
      SymbolIndexUpdate_v0_1_0[] SymbolUpdates,     // sorted
      HandleIndexUpdate_v0_1_0[] HandleUpdates,     // sorted
      IntakeIndexUpdate_v0_1_0[] IntakeUpdates,     // sorted
      string SessionId,
      string OperatorId,
      string ScenarioName,
      long GenesisTick
  );
  public record SymbolIndexUpdate_v0_1_0(
      string Namespace,
      string SymbolText,
      string PointerHash,
      string TargetEngramId,
      string Operation                       // "ADD" only in v0.1.0
  );
  public record HandleIndexUpdate_v0_1_0(
      string Handle,
      string TargetEngramId,
      string Operation
  );
  public record IntakeIndexUpdate_v0_1_0(
      string IntakeHash,
      string TargetEngramId,
      string Operation
  );
  ```
- **Freeze Rules**: Add-only (no "REMOVE"). Sorted updates.

##### 10. gel.mirror_snapshot.v0.1.0
- **Purpose**: Deterministic snapshot artifact for mirror verification.
- **Record**:
  ```csharp
  public record GelMirrorSnapshot_v0_1_0(
      string Schema,
      string SnapshotHash,
      string CanonicalSeal,
      string Tip,
      long SnapshotSequence,
      string[] RecentTipEventHashes,         // sorted
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
      long GenesisTick
  );
  ```
- **Freeze Rules**: No wall-clock; no Layer-2 state.

##### 11. gel.quarantine_event.v0.1.0
- **Purpose**: Records safe-fail transitions in the host authority state machine.
- **Record**:
  ```csharp
  public record GelQuarantineEvent_v0_1_0(
      string Schema,
      string EventHash,
      string CanonicalSeal,
      string FromState,                      // Operational|Frozen|Quarantined|Halt
      string ToState,
      string TransitionReasonCode,
      string RelatedPacketHash,
      string RelatedGateDecisionHash,
      string RelatedCommitIntentHash,
      string HostInstanceId,
      string PolicyVersion,
      long TransitionSequence,
      long GenesisTick
  );
  ```

##### 12. gel.root_index_entry.v0.1.0
- **Purpose**: Atomic unit inside index payloads (RootIndex, SymbolicIndex, SuffixIndex).
- **Record**:
  ```csharp
  public record GelRootIndexEntry_v0_1_0(
      string Schema,
      string Namespace,                   // root|symbol|suffix
      string KeyText,
      string KeyHash,
      string[] EngramIds,                 // sorted
      string[] Tags,                      // sorted
      string EntryHash,
      string CanonicalSeal
  );
  ```

##### 13. gel.tip_advance_event.v0.1.0
- **Purpose**: Emitted on successful CAS tip advancement; replication spine unit.
- **Record**:
  ```csharp
  public record GelTipAdvanceEvent_v0_1_0(
      string Schema,
      string PreviousTip,
      string NewTip,
      string CommitIntentHash,
      string GateDecisionHash,
      string BraidIndexHash,
      string[] EngramIdsWritten,             // sorted
      string SessionId,
      string OperatorId,
      string ScenarioName,
      long GenesisTick,
      long SpineSequence,
      string EventHash,
      string CanonicalSeal
  );
  ```
#### Layer 1 — SLI (Symbolic/Governance Layer)

##### 1. sli.resolved_address.v0.1.0
- **Purpose**: Defines the canonical normal-form address that every handle resolves to: Channel × Partition × Mirror.
- **Record**:
  ```csharp
  public record SliResolvedAddress_v0_1_0(
      string Schema,            // "sli.resolved_address.v0.1.0"
      string Channel,           // "Public" | "Private"
      string Partition,         // "GEL" | "GOA" | "OAN"
      string Mirror,            // "Standard" | "Cryptic"
      string AddressText,       // $"{Channel}/{Partition}/{Mirror}" (exact)
      string AddressHash        // sha256(AddressText UTF-8) lowercase hex
  );
  ```
- **Freeze Rules**: AddressText is the canonical string form. No enum ToString() allowed; values must be exact case. AddressHash must be computed from AddressText only.
- **Required Tests**: RA-Addr-1: Same fields → same AddressText and AddressHash. RA-Addr-2: Invalid enum value must be rejected.

##### 2. sli.session_mounts.v0.1.0
- **Purpose**: Defines what address spaces are mounted (allowed to be accessed) for a session.
- **Record**:
  ```csharp
  public record SliSessionMounts_v0_1_0(
      string Schema,                   // "sli.session_mounts.v0.1.0"
      string SessionId,
      string OperatorId,
      string ScenarioName,
      long GenesisTick,
      string MountPolicyVersion,       // e.g. "MOUNT-0.1.0"
      SliResolvedAddress_v0_1_0[] MountedAddresses, // sorted by AddressText
      MountConstraint_v0_1_0[] Constraints,    // sorted by AddressText
      string MountsHash,               // sha256(canonicalBytes(without MountsHash))
      string CanonicalSeal             // sha256(full canonical serialization)
  );
  public record MountConstraint_v0_1_0(
      string AddressText,              // must match MountedAddresses.AddressText
      string[] RequiredSatModes,       // sorted
      bool RequiresHITL,
      bool ReadOnly,                   // if true, disallow write ops
      string Notes
  );
  ```
- **Freeze Rules**: Mounts are allowlist: if not mounted → deny. MountedAddresses must be sorted by AddressText.

##### 3. sli.packet.v0.1.0
- **Purpose**: Canonical, layer-1 container for any executable intent entering the OAN Mortalis stack.
- **Record**:
  ```csharp
  public record SliPacket_v0_1_0(
      string Schema,                       // "sli.packet.v0.1.0"
      string PacketHash,                   // sha256(canonicalBytes(packet without PacketHash))
      string CanonicalSeal,                // sha256(full canonical serialization)
      string SessionId,
      string OperatorId,
      string ScenarioName,
      long GenesisTick,
      PacketEnv Env,
      PacketFrame Frame,
      PacketMode Mode,
      PacketOp Op,
      string[] Handles,                    // sorted; explicit declared handles
      PacketRefs Refs,
      GateTrace GateTrace                  // MUST be empty before evaluation
  );
  ```
- **Freeze Rules**: No handle, no action. Deterministic resolution (no wall-clock).

##### 4. sli.root_atlas_entry.v0.1.0
- **Purpose**: Immutable registry entry for a single SLI handle.
- **Record**:
  ```csharp
  public record SliRootAtlasEntry_v0_1_0(
      string Schema,                        // "sli.root_atlas_entry.v0.1.0"
      string Handle,                        // e.g. "engram.construct.propositional"
      string IntentKind,                    // e.g. "EngramConstruct"
      AddressConstraint AllowedAddress,
      string[] RequiredSatModes,            // sorted
      bool RequiresHITL,
      string PolicyVersion,
      CrypticConstraint Cryptic,
      string Description,
      string Owner,
      string EntryHash,
      string CanonicalSeal
  );
  ```
- **Freeze Rules**: Static registry rule for v0.1.0 (no runtime writes). Lookup must be exact.

##### 5. sli.handle_registry_manifest.v0.1.0
- **Purpose**: Pins the exact list of valid handles and their hashes for a given policy bundle.
- **Record**:
  ```csharp
  public record SliHandleRegistryManifest_v0_1_0(
      string Schema,                         // "sli.handle_registry_manifest.v0.1.0"
      string RegistryVersion,                // "v0.1.0"
      string PolicyVersion,                  // "POLICY-0.1.0"
      HandleEntry_v0_1_0[] Handles,                 // sorted by Handle
      string RegistryHash,
      string CanonicalSeal
  );
  public record HandleEntry_v0_1_0(
      string Handle,
      string EntryHash
  );
  ```

##### 6. sli.policy_bundle.v0.1.0
- **Purpose**: Single pinning artifact that locks RootAtlas, handle registry, mount policy, and telemetry.
- **Record**:
  ```csharp
  public record SliPolicyBundle_v0_1_0(
      string Schema,                         // "sli.policy_bundle.v0.1.0"
      string BundleVersion,
      string PolicyVersion,
      string BundleHash,
      string CanonicalSeal,
      string RootAtlasHash,
      string HandleRegistryHash,
      string MountPolicyVersion,
      string TelemetryManifestHash,
      string ReconstructionProfileVersion,
      string ReconstructionProfileHash,
      string BootstrapVersion,
      string BootstrapHash,
      string[] RequiredModules,
      string[] ForbiddenModules
  );
  ```

##### 7. sli.gate_evidence.v0.1.0
- **Purpose**: Structured and deterministic evidence record for gate evaluation.
- **Record**:
  ```csharp
  public record SliGateEvidence_v0_1_0(
      string Schema,                         // "sli.gate_evidence.v0.1.0"
      string PacketHash,
      string SessionMountsHash,
      string RootAtlasHash,
      string PolicyVersion,
      string SatModeObserved,
      bool SafActiveObserved,
      string RequestedEnvChannel,
      string RequestedEnvPartition,
      string RequestedEnvMirror,
      HandleEvidence_v0_1_0[] Handles,              // sorted by Handle
      bool CrypticAttempted,
      bool MaskingApplied,
      string MaskingPolicyId,
      bool IsAllowed,
      string ReasonCode,
      string EvidenceHash,
      string CanonicalSeal
  );
  public record HandleEvidence_v0_1_0(
      string Handle,
      bool IsAllowed,
      string ReasonCode,
      string ResolutionHash
  );
  ```

##### 8. sli.gate_decision.v0.1.0
- **Purpose**: Defines the gate’s deterministic output (allow/deny) with required audit fields.
- **Record**:
  ```csharp
  public record SliGateDecision_v0_1_0(
      string Schema,
      string PacketHash,
      string RootAtlasHash,
      string SessionMountsHash,
      string PolicyVersion,
      bool IsAllowed,
      string ReasonCode,
      string SatModeObserved,
      bool SafActiveObserved,
      HandleResolution_v0_1_0[] Resolutions,       // sorted by Handle
      bool MaskingApplied,
      string MaskingPolicyId,
      string EvidenceSnapshot,
      string EvidenceSnapshotHash,
      string DecisionHash,
      string CanonicalSeal
  );
  public record HandleResolution_v0_1_0(
      string Handle,
      string ResolvedAddressText,
      string EntryHash
  );
  ```

##### 9. sli.commit_intent.v0.1.0
- **Purpose**: Represents the post-gate authorized intent to crystallize into GEL.
- **Record**:
  ```csharp
  public record SliCommitIntent_v0_1_0(
      string Schema,                         // "sli.commit_intent.v0.1.0"
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
      CommitDirective[] Directives,          // sorted by Handle
      string CommitIntentHash,
      string CanonicalSeal
  );
  ```

##### 10. sli.symbol_pointer.v0.1.0
- **Purpose**: Defines a structural, morphism-safe pointer for symbol → target resolution.
- **Record**:
  ```csharp
  public record SliSymbolPointer_v0_1_0(
      string Schema,                       // "sli.symbol_pointer.v0.1.0"
      string PointerHash,
      string CanonicalSeal,
      string Namespace,
      string SymbolText,
      string SymbolEncoding,
      string PointerKind,
      SymbolTarget Target,
      PointerProvenance Provenance
  );
  ```

##### 11. sli.tensor.v0.1.0
- **Purpose**: Structural representation produced by tensorizing one or more gel.golden_engram.* entries.
- **Record**:
  ```csharp
  public record SliTensor_v0_1_0(
      string TensorVersion,                 // "v0.1.0"
      string TensorizationProfileVersion,
      string TensorizationProfileHash,
      string[] OriginEngramIds,             // sorted
      string[] OriginBraidIndexHashes,      // sorted
      string IntakeHash,
      TensorNode[] Nodes,                   // deterministic ordering
      TensorEdge[] Edges,                   // deterministic ordering
      string[] SymbolPointers,              // sorted
      string EncodingMode,
      string[] FeatureKeys,
      string[] FeatureValues,
      string OriginTensorHash,
      string CanonicalSeal
  );
  ```

##### 12. sli.telemetry_record.v0.1.0
- **Purpose**: Unified telemetry line format for gate events, commit intents, and tip advances.
- **Record**:
  ```csharp
  public record SliTelemetryRecord_v0_1_0(
      string Schema,                         // "sli.telemetry_record.v0.1.0"
      string EventType,
      string EventHash,
      string CanonicalSeal,
      long GenesisTick,
      long EventSequence,
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
      string[] Handles,                      // sorted
      string SessionId,
      string OperatorId,
      string ScenarioName
  );
  ```

##### 13. sli.telemetry_stream_manifest.v0.1.0
- **Purpose**: Defines what telemetry events are emitted, to which sinks, at what detail level.
- **Record**:
  ```csharp
  public record SliTelemetryStreamManifest_v0_1_0(
      string Schema,
      string ManifestVersion,
      string ManifestHash,
      string CanonicalSeal,
      string PolicyVersion,
      string MountPolicyVersion,
      StreamRule[] Rules,                 // sorted
      SinkSpec[] Sinks                    // sorted
  );
  ```

##### 14. sli.duplex_message.v0.1.0
- **Purpose**: Single envelope for all bidirectional messages exchanged between host and duplex.
- **Record**:
  ```csharp
  public record SliDuplexMessage_v0_1_0(
      string Schema,
      string MessageHash,
      string CanonicalSeal,
      string Direction,                      // "HOST_TO_DUPLEX" | "DUPLEX_TO_HOST"
      string MessageType,
      string CorrelationId,
      long Sequence,
      string SessionId,
      string OperatorId,
      string ScenarioName,
      long GenesisTick,
      string PayloadSchema,
      string PayloadHash,
      string PayloadInline
  );
  ```

##### 15. sli.symbol_resolution_rpc
- ** sli.symbol_resolution_request.v0.1.0**:
  ```csharp
  public record SliSymbolResolutionRequest_v0_1_0(
      string Schema,
      string RequestHash,
      string CanonicalSeal,
      string Namespace,
      string SymbolText,
      string PointerHashHint,
      string ResolutionMode,
      string[] AllowedPointerKinds,
      string SessionId,
      string OperatorId,
      string ScenarioName,
      long GenesisTick
  );
  ```
- ** sli.symbol_resolution_response.v0.1.0**:
  ```csharp
  public record SliSymbolResolutionResponse_v0_1_0(
      string Schema,
      string ResponseHash,
      string CanonicalSeal,
      string RequestHash,
      string Namespace,
      string SymbolText,
      string PointerHashResolved,
      bool IsFound,
      string NotFoundReason,
      string PointerKind,
      SymbolTargetResolved Target,
      string IndexTip,
      string IndexHash,
      string SessionId,
      string OperatorId,
      string ScenarioName,
      long GenesisTick
  );
  ```

##### 16. sli.refusal_record.v0.1.0
- **Purpose**: Standardized record returned when SLI gate denies a packet or commit fails.
- **Record**:
  ```csharp
  public record SliRefusalRecord_v0_1_0(
      string Schema,                         // "sli.refusal_record.v0.1.0"
      string RefusalHash,
      string CanonicalSeal,
      string PacketHash,
      string GateDecisionHash,           
      string GateEvidenceHash,      
      string PolicyVersion,
      string MountPolicyVersion,
      string RefusalType,                    // "GATE_DENY"|"MOUNT_DENY"|...
      string ReasonCode,            
      string[] AffectedHandles,              // sorted
      string[] MissingHandles,               // sorted
      bool SafActiveObserved,
      string SatModeObserved,
      string GuidanceCode,                   
      string Notes                          
  );
  ```

#### Layer 2 — IUTT (Cognition Layer)

##### 1. iutt.reconstruction_profile.v0.1.0
- **Purpose**: Specifies runtime rules for reconstructing SLI tensors into runtime state.
- **Record**:
  ```csharp
  public record IuttReconstructionProfile_v0_1_0(
      string ReconstructionProfileVersion,
      string ProfileName,
      string ProfileDescription,
      string ProfileOwner,
      OperatorPolicy Operators,
      PerspectivePolicy Perspective,
      SaliencePolicy Salience,
      RelevancePolicy Relevance,
      EquivalencePolicy Equivalence,
      TelemetryPolicy Telemetry,
      BasinPolicy Basins,
      ConstraintPolicy Constraints,
      CanonicalizationPolicy Canonicalization
  );
  ```

##### 2. iutt.runtime_state.v0.1.0
- **Purpose**: Ephemeral cognitive state at Layer 2. No direct Layer 0 mutation.
- **Record**:
  ```csharp
  public record IuttRuntimeState_v0_1_0(
      string RuntimeStateId,
      string CreatedTick,
      string ReconstructionProfileVersion,
      string ReconstructionProfileHash,
      string QueryContextHash,
      string SessionId,
      string OperatorId,
      string ScenarioName,
      string[] OriginEngramIds,
      string OriginTensorHash,
      string OriginBraidIndexHash,
      PerspectiveFrame Perspective,
      SalienceField Salience,
      RelevanceTrace Relevance,
      RuntimeAssembly Assembly,
      TriptychSignals Signals,
      BoundaryAttestation Boundary,
      string[] Warnings,
      string[] Notes
  );
  ```

### 5. Appendices

#### A) Source Inclusions

##### 1. Modules\Engrammitization\ENGRAMMITIZATION_SPEC_v0.1.0.md
```markdown
# ENGRAMMITIZATION_SPEC_v0.1.0
## Fully Developed, Frozen & Execution-Ready Constitutional Specification
**Freeze Date:** 20 February 2026 (America/Los_Angeles)  
**Status:** Canonical • Version-locked • Boundary-enforced • Test-harness ready

---

## 0. Purpose

This document defines the **frozen constitutional logic** for the **Engrammitization Pipeline** (v0.1.0). 
It establishes the non-negotiable rules for how raw intake crystallized into **Inert Identity** (Layer 0) via **Symbolic Routing** (Layer 1).

Any implementation (C#, Lisp, or Rust) must adhere to these structural invariants to maintain **Golden Master** compatibility across the OAN Mortalis network.

---

## 1. Domain Primitive: The Golden Engram (Layer 0)

### 1.1 Definition
A **Golden Engram** is the atomic, immutable unit of crystallized identity. 
It represents a single "fact" or "proposition" admitted into the permanent ledger (GEL).

### 1.2 Structure (gel.golden_engram.v0.1.0)
```csharp
public record GoldenEngram_v0_1_0(
    string Schema,                          // "gel.golden_engram.v0.1.0"
    string EngramId,                        // sha256(canonical envelope) lowercase hex
    string Handle,                          // exact admitted handle (capability key)
    string ResolvedAddressText,             // "Channel/Partition/Mirror"
    string IntakeHash,                      // link to original intake
    string ParentTip,                       // GEL tip hash at time of commit
    string BraidIndexHash,                  // hash of the join index for this commit set
    string[] FourPTags,                     // sorted; "prop", "proc", "pers", "part"
    string PayloadHash,                     // sha256(constructor-normalized payload)
    string CanonicalSeal                    // sha256(full serialization)
);
```

---

## 2. Capability Primitives: Handles & Addressing (Layer 1)

### 2.1 The Handle
A **Handle** is a unique capability key (e.g., `engram.construct.propositional`). 
No action occurs without a handle. **No Handle, No Action.**

### 2.2 Resolved Address
Every handle resolves to a 3-axis coordinate:
**Channel** (Public/Private) × **Partition** (GEL/GOA/OAN) × **Mirror** (Standard/Cryptic).

```csharp
public record SliResolvedAddress_v0_1_0(
    string Channel,
    string Partition,
    string Mirror,
    string AddressText                      // "Private/GEL/Standard"
);
```

---

## 3. The Engrammitization Pipeline (The 6 Stages)

### Stage 1: Intake
Raw data enters the system. A deterministic `IntakeHash` is generated.

### Stage 2: Normalization (Pre-SLI)
Data is transformed into a **PreGELBundle**. 
This is a Layer-0 proposal object (non-authoritative).

### Stage 3: Packetization (SLI Packet)
The intent is wrapped in an `sli.packet`. 
The packet **must** declare the required handles.

### Stage 4: Gating (The SLI Gate)
The Gate checks the packet against the **Root Atlas** (Capability Registry).
- Is the handle valid?
- Are the session mounts permissive?
- Is the SAT mode sufficient?
- **Result:** `sli.gate_decision` (Allow/Deny).

### Stage 5: Commit (The CommitEngine)
If allowed, the `CommitIntent` is processed.
- Golden Engrams are minted.
- `BraidIndex` is finalized.
- **Result:** `gel.tip_advance_event` (Atomic TIP update).

### Stage 6: Reconstruction (IUTT Playback)
When read back, `sli.tensor` is fed into the **IUTT Layer**.
Runtime salience and perspective are applied here.

---

## 4. Frozen Schema Registry (v0.1.0)

### 1) gel.golden_engram.v0.1.0 (Identity Anchor)
```csharp
public record GoldenEngram_v0_1_0(
    string Schema,
    string EngramId,
    string Handle,
    string ResolvedAddressText,
    string IntakeHash,
    string ParentTip,
    string BraidIndexHash,
    string[] FourPTags,
    string PayloadHash,
    string CanonicalSeal
);
```

### 2) gel.braid_index.v0.1.0 (The Join)
```csharp
public record BraidIndex_v0_1_0(
    string Schema,
    string IntakeHash,
    string[] AdmittedHandles,               // sorted
    string ParentTip,
    string PolicyVersion,
    string ReconstructionProfileHash,       // Layer-2 pin (non-identity)
    string BraidIndexHash,
    string CanonicalSeal
);
```

### 3) sli.packet.v0.1.0 (The Intent)
```csharp
public record SliPacket_v0_1_0(
    string Schema,
    string PacketHash,
    string[] Handles,                        // explicit capabilities
    PacketEnv Env,                           // classification
    PacketOp Op                              // "CommitIntent"|"Propose"
);
```

---

## 5. Canonicalization Contract (The discipline)

All hashes in the OAN Mortalis stack **must** follow these rules:
1. **Fixed Order:** Fields serialized in schema order.
2. **Sorted Arrays:** All arrays sorted lexicographically (Ordinal).
3. **No Nulls:** Null values serialized as the literal string `"null"`.
4. **UTF-8:** All strings UTF-8 encoded.
5. **No Wall-Clock:** No timestamps inside hashes.

---

## 6. Testing Requirements

### T1: Identity Stability
`EngramId` must be identical across C#, Rust, and Lisp implementations for the same inputs.

### T2: Layer Purity Scan
No Layer-2 fields (`salience`, `perspective`, `relevance`) may exist inside a Layer-0 (`gel.*`) object.

---

## 7. Status & Sign-off
**Status:** CANONICAL FINAL  
**Authority:** OAN Mortalis Architecture Board  
**Alignment:** SLI CONSTITUTION v0.1.0  
```

##### 2. Modules\Engrammitization\Symbol Resolution RPC.md
```markdown
20) Symbol Resolution RPC
20a) sli.symbol_resolution_request.v0.1.0
20b) sli.symbol_resolution_response.v0.1.0

These formalize exact pointer resolution as an RPC pair.

20a) sli.symbol_resolution_request.v0.1.0
Purpose

Requests resolution of a canonical symbol (or symbol pointer) to a stable target reference.

Schema ID

sli.symbol_resolution_request.v0.1.0

Record
public record SliSymbolResolutionRequest_v0_1_0(
    string Schema,                          // "sli.symbol_resolution_request.v0.1.0"
    string RequestHash,                     // sha256(canonicalBytes(without RequestHash))
    string CanonicalSeal,                   // sha256(full serialization)

    // Symbol to resolve
    string Namespace,                       // e.g. "sli", "gel", "rootatlas"
    string SymbolText,                      // canonical symbol string

    // Resolution parameters
    string PointerHashHint,                 // optional; if requesting specific version
    string ResolutionMode,                  // "EXACT" (v0.1.0 only; no heuristics)
    string[] AllowedPointerKinds,           // sorted; e.g. ["engram_id", "resolved_address"]

    // Session provenance
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick
);
Freeze Rules

SymbolText must be canonicalized before request.

ResolutionMode must be EXACT in v0.1.0.

No wall-clock.

Required Tests

SR-REQ-1: RequestHash stable.

SR-REQ-2: Invalid ResolutionMode rejected.

20b) sli.symbol_resolution_response.v0.1.0
Purpose

Returns the result of a symbol resolution request.

Schema ID

sli.symbol_resolution_response.v0.1.0

Record
public record SliSymbolResolutionResponse_v0_1_0(
    string Schema,
    string ResponseHash,
    string CanonicalSeal,

    // Link to request
    string RequestHash,                     // sha256 of request

    // Resolution outcome
    string Namespace,
    string SymbolText,
    string PointerHashResolved,             // sha256(sli.symbol_pointer)
    bool IsFound,                           // true if resolved
    string NotFoundReason,                  // "null" if found; otherwise reason code

    // Target (if found)
    string PointerKind,                     // "engram_id" | "resolved_address" | ...
    SymbolTargetResolved Target,            // target details

    // Audit anchors
    string IndexTip,                        // GEL tip of the index used for resolution
    string IndexHash,                       // optional; hash of the index structure

    // Session provenance
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick
);

public record SymbolTargetResolved(
    string TargetRef,                       // e.g. EngramId
    string TargetAddress                    // "Channel/Partition/Mirror"
);
Freeze Rules

IsFound=false must provide a non-empty NotFoundReason.

PointerHashResolved must match the hash of the pointer record stored in the host's index.

No wall-clock.

Required Tests

SR-RES-1: ResponseHash stable.

SR-RES-2: Linkage to RequestHash must be valid.

SR-RES-3: PointerHashResolved check (verify vs actual pointer hash).
```

##### 3. Modules\Engrammitization\gel.bootstrap_manifest.v0.1.0.md
```markdown
11) gel.bootstrap_manifest.v0.1.0
GEL Bootstrap Manifest (Layer 0, Deterministic Genesis Set)
Purpose

Defines the deterministic genesis set: the first committed Golden Engrams that establish the boot lattice (RootAtlas, RootIndex, SymbolicIndex, SuffixIndex, BaseSymbolCodex, etc.).

This removes ambiguity from “first 7 Golden Engrams” by making it an explicit, hashable, reproducible manifest.

Schema ID

gel.bootstrap_manifest.v0.1.0

Record
public record GelBootstrapManifest_v0_1_0(
    string Schema,                      // "gel.bootstrap_manifest.v0.1.0"

    // Manifest identity
    string BootstrapVersion,            // "v0.1.0"
    string BootstrapHash,               // sha256(canonicalBytes(without BootstrapHash))
    string CanonicalSeal,               // sha256(full canonical serialization)

    // Deterministic genesis anchors
    string GenesisParentTip,            // fixed constant tip value for boot (e.g. "GENESIS")
    long GenesisTick,                   // fixed genesis tick (e.g. 0)
    string PolicyVersion,               // "POLICY-0.1.0"

    // What to bootstrap (ordered, deterministic)
    BootstrapItem[] Items               // sorted by Ordinal then Handle
);

public record BootstrapItem(
    int Ordinal,                        // explicit order: 1..N
    string Handle,                      // e.g. "engram.bootstrap.rootatlas"
    string SourceKind,                  // "embedded_resource"|"file"|"compiled_asset"
    string SourceRef,                   // e.g. resource name or file relative path
    string TargetResolvedAddressText,   // "Private/GEL/Standard" recommended for bootstrap
    string ExpectedPayloadHash,         // sha256(canonical bytes of source payload)
    string ExpectedEngramId             // optional: if you want full determinism lock (recommended for strict builds)
);
Freeze Rules

Bootstrap is the only time you may “seed” GEL from static assets without prior engrams.

Ordinal is the canonical boot order; must not change without MAJOR bump.

ExpectedPayloadHash must match exactly or bootstrap fails closed.

If ExpectedEngramId is provided, it must match exactly or bootstrap fails closed.

Required Tests

BSM-1: BootstrapHash stable.

BSM-2: Payload hash mismatch → hard fail (no partial boot).

BSM-3: Deterministic replay produces identical EngramIds (if ExpectedEngramId locked).
```

##### 4. Modules\Engrammitization\gel.bootstrap_result.v0.1.0.md
```markdown
13) gel.bootstrap_result.v0.1.0
Bootstrap Result (Layer 0, Deterministic Proof of Genesis)
Purpose

Captures the outcome of applying gel.bootstrap_manifest.v0.1.0:

which Golden Engrams were written

what the final tip became

proof hashes linking boot inputs → outputs

This enables:

deterministic verification

“boot attestation” for audits

regression checks across environments

Schema ID

gel.bootstrap_result.v0.1.0

Record
public record GelBootstrapResult_v0_1_0(
    string Schema,                         // "gel.bootstrap_result.v0.1.0"

    // Link back to manifest
    string BootstrapHash,                  // gel.bootstrap_manifest.BootstrapHash
    string BootstrapVersion,               // "v0.1.0"
    string PolicyVersion,                  // "POLICY-0.1.0"

    // Genesis anchors
    string GenesisParentTip,
    long GenesisTick,

    // Outcome
    string FinalTip,
    long BootstrapSequence,                // local monotonic; optional for cross-substrate comparisons
    BootstrapWrite[] Writes,               // sorted by Ordinal then Handle

    // Proof
    string ResultHash,                     // sha256(canonicalBytes(without ResultHash))
    string CanonicalSeal                   // sha256(full canonical serialization)
);

public record BootstrapWrite(
    int Ordinal,
    string Handle,
    string ResolvedAddressText,
    string PayloadHash,                    // sha256(source payload canonical bytes)
    string EngramId,                       // resulting gel.golden_engram.EngramId
    string EngramSeal                      // resulting gel.golden_engram.CanonicalSeal
);
Freeze Rules

Every manifest item must produce exactly one BootstrapWrite.

PayloadHash must match the manifest’s ExpectedPayloadHash; mismatch → boot fails and result is not emitted.

Writes must be sorted (Ordinal, Handle).

Required Tests

BR-1: ResultHash stable given identical writes.

BR-2: Every EngramId in Writes must exist in GEL at FinalTip lineage.

BR-3: Manifest and Result hashes match expected chain: manifest → writes → final tip.
```

##### 5. Modules\Engrammitization\gel.commit_batch.v0.1.0.md
```markdown
7) gel.commit_batch.v0.1.0
GEL Commit Batch (Layer 0, Durable Braided Commit Artifact)
Purpose

Represents the durable container for a single Commit operation that writes one or more gel.golden_engram entries in one atomic tip advancement.

This is the storage-optimized, append-only form of the conceptual:

gel.braided_commit_set.v0.1.0
(You can keep both names if you want: one conceptual, one storage artifact.)

Schema ID

gel.commit_batch.v0.1.0

Record
public record GelCommitBatch_v0_1_0(
    string Schema,                         // "gel.commit_batch.v0.1.0"
    string GateEvidenceHash,
    string PolicyVersion,
    string BraidIndexHash,
    string IntakeHash,
    GoldenEngramRef[] Engrams,             // sorted by EngramId
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick
);
```

##### 6. Modules\Engrammitization\gel.conflict_event.v0.1.0.md
```markdown
# gel.conflict_event.v0.1.0
Purpose
Records deterministic audit events for GEL rejections.

Record
public record GelConflictEvent_v0_1_0(
    string Schema,                         // "gel.conflict_event.v0.1.0"
    string ConflictHash,
    string CanonicalSeal,
    string ConflictType,                   // "TIP_CONFLICT" | "PARENT_MISMATCH" | "REPLAY_CONFLICT"
    string ReasonCode,
    string ExpectedParentTip,
    string ObservedCurrentTip,
    string ProposedNewTip,
    string CommitIntentHash,
    string GateDecisionHash,
    string BraidIndexHash,
    string[] IntendedEngramIds,            // sorted
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick,
    long AttemptSequence
);
```

##### 7. Modules\Engrammitization\gel.gel_index_delta.v0.1.0.md
```markdown
# gel.gel_index_delta.v0.1.0
Purpose
Defines index updates produced by a commit.

Record
public record GelIndexDelta_v0_1_0(
    string Schema,
    string CommitIntentHash,
    string GateDecisionHash,
    string BraidIndexHash,
    string PreviousTip,
    string NewTip,
    string DeltaHash,
    string CanonicalSeal,
    SymbolIndexUpdate[] SymbolUpdates,     // sorted
    HandleIndexUpdate[] HandleUpdates,     // sorted
    IntakeIndexUpdate[] IntakeUpdates,     // sorted
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick
);
public record SymbolIndexUpdate(
    string Namespace,
    string SymbolText,
    string PointerHash,
    string TargetEngramId,
    string Operation                       // "ADD" only in v0.1.0
);
```

##### 8. Modules\Engrammitization\gel.mirror_snapshot.v0.1.0.md
```markdown
# gel.mirror_snapshot.v0.1.0
Purpose
Deterministic snapshot artifact for mirror verification.

Record
public record GelMirrorSnapshot_v0_1_0(
    string Schema,
    string SnapshotHash,
    string CanonicalSeal,
    string Tip,
    long SnapshotSequence,
    string[] RecentTipEventHashes,         // sorted
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
    long GenesisTick
);
```

##### 9. Modules\Engrammitization\gel.quarantine_event.v0.1.0.md
```markdown
# gel.quarantine_event.v0.1.0
Purpose
Records safe-fail transitions in the GEL host.

Record
public record GelQuarantineEvent_v0_1_0(
    string Schema,
    string EventHash,
    string CanonicalSeal,
    string FromState,                      // Operational|Frozen|Quarantined|Halt
    string ToState,
    string TransitionReasonCode,
    string RelatedPacketHash,
    string RelatedGateDecisionHash,
    string RelatedCommitIntentHash,
    string HostInstanceId,
    string PolicyVersion,
    long TransitionSequence,
    long GenesisTick
);
```

##### 10. Modules\Engrammitization\gel.root_index_entry.v0.1.0.md
```markdown
# gel.root_index_entry.v0.1.0
Purpose
Atomic unit inside index payloads.

Record
public record GelRootIndexEntry_v0_1_0(
    string Schema,
    string Namespace,                   // root|symbol|suffix
    string KeyText,
    string KeyHash,
    string[] EngramIds,                 // sorted
    string[] Tags,                      // sorted
    string EntryHash,
    string CanonicalSeal
);
```

##### 11. Modules\Engrammitization\gel.tip_advance_event.v0.1.0.md
```markdown
6) gel.tip_advance_event.v0.1.0
GEL Tip Advancement Event (Layer 0 Telemetry Spine Unit)
Purpose

Represents the atomic, append-only event emitted when GEL advances its tip (CAS success). This is the telemetry spine unit your Lisp duplex / mirroring system can subscribe to for live replication.

This is not “just logs.” It is a first-class event type.

Schema ID

gel.tip_advance_event.v0.1.0

Record
public record GelTipAdvanceEvent_v0_1_0(
    string Schema,                         // "gel.tip_advance_event.v0.1.0"

    // Spine movement
    string PreviousTip,
    string NewTip,

    // What caused the move (hash references)
    string CommitIntentHash,               // sli.commit_intent.CommitIntentHash
    string GateDecisionHash,               // sli.gate_decision.DecisionHash
    string BraidIndexHash,                 // gel.braid_index hash finalized for this commit set

    // What was written (identity-bearing)
    string[] EngramIdsWritten,             // sorted; gel.golden_engram.EngramId

    // Deterministic session provenance
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick,

    // Deterministic monotonic counter (preferred over wall-clock)
    long SpineSequence,                    // monotonic within a GEL instance; deterministic if derived from tip lineage

    // Digests
    string EventHash,                      // sha256(canonicalBytes(without EventHash))
    string CanonicalSeal                   // sha256(full canonical serialization)
);
Freeze Rules

Emitted only on successful CAS tip advancement.

EngramIdsWritten must be sorted and match exactly the GEL entries appended in that advancement.

Must not include runtime context, salience, or IUTT state.

SpineSequence must be monotonic; if you can’t guarantee determinism across distributed nodes, treat it as “local monotonic only” and exclude it from cross-substrate equivalence (but keep it for ops telemetry).

Required Tests

TIP-1: Event emitted exactly once per successful tip move.

TIP-2: Rejected CAS attempts emit no tip event (but may emit a separate conflict telemetry type if you define one).

TIP-3: EventHash stable given identical fields.
```

##### 12. Modules\Engrammitization\iutt.reconstruction_profile.v0.1.0.md
```markdown
# `iutt.reconstruction_profile.v0.1.0`

## Runtime Reconstruction Profile (Layer 2, Hashable, Non-Canon)

This is the object whose `ReconstructionProfileHash` becomes a **mandatory invariant** inside `gel.braid_index.v0.1.0`, but it remains **Layer 2** (runtime / application). It is **hashable and versioned**, but **not identity**.

---

# 0. Purpose

`iutt.reconstruction_profile.v0.1.0` defines the **runtime rules** for:

1. **Perspective Shift**: how to map identity nodes to runtime perspectives.
2. **Salience Gradients**: which feature axes receive attention weight.
3. **Equivalence Policies**: what constitutes "same enough" for runtime caching.
4. **Relevance Thresholding**: when to exclude a node from the active runtime state.

It makes the **cognitive strategy** explicit and auditable, even if it isn't part of the immutable GEL identity.

---

# 1. Non-Goals

- It DOES NOT generate `EngramId`.
- It DOES NOT affect the GEL spine tip.
- It DOES NOT contain actual salience values; it contains the **rules** to compute them.

---

# 2. Schema ID

`iutt.reconstruction_profile.v0.1.0`

---

# 3. Canonical Record (C#)

```csharp
public record IuttReconstructionProfile_v0_1_0(
    // --- Metadata ---
    string ReconstructionProfileVersion,  // e.g. "STP-BASELINE-1"
    string ProfileName,
    string ProfileDescription,
    string ProfileOwner,

    // --- Policies (Sub-records) ---
    OperatorPolicy Operators,             // see §3.1
    PerspectivePolicy Perspective,       // see §3.2
    SaliencePolicy Salience,             // see §3.3
    RelevancePolicy Relevance,           // see §3.4
    EquivalencePolicy Equivalence,       // see §3.5
    TelemetryPolicy Telemetry,           // see §3.6
    BasinPolicy Basins,                  // see §3.7
    ConstraintPolicy Constraints,        // see §3.8
    CanonicalizationPolicy Canonicalization // see §3.9
);
```

### 3.1 OperatorPolicy
Defines which operator identities are valid for this profile.

### 3.2 PerspectivePolicy
Defines the `PerspectiveTransform` rules (e.g. ego-centric vs. allocentric).

### 3.3 SaliencePolicy
Defines the `SalienceFields` (attention keys) and their default weights.

### 3.4 RelevancePolicy
Defines the cutoff thresholds for `RelevanceTrace`.

### 3.5 EquivalencePolicy
Defines the rules for `BoundaryAttestation` and "identity equivalence" at runtime.

---

# 4. Canonicalization Contract (Layer 2 Version)

While this is a Layer 2 object, it still requires a **deterministic hash** to be pinned in `gel.braid_index`:

1. Fixed field order.
2. Lexicographical sort on all lists/dictionaries.
3. UTF-8 only.
4. `ProfileHash = sha256(canonicalBytes(profile))`.

---

# 5. Relationship to Engrammitization

In stage 5 (Commit), the `CommitEngine` pins the `ReconstructionProfileHash` into the `braid_index`. 
This creates a **hard audit link** between the immutable data (GEL) and the cognitive strategy used at the time of its creation.

---

# 6. Required Tests

- **RP-1: Hash Stability**: same fields → same ProfileHash.
- **RP-2: Boundary Enforcement**: verify that profile does not contain forbidden GEL mutation flags.
```

##### 13. Modules\Engrammitization\iutt.runtime_state.v0.1.0.md
```markdown
# `iutt.runtime_state.v0.1.0`

## Runtime Reconstruction State (Layer 2, Ephemeral, High-Entropy)

This is the object produced by applying an `iutt.reconstruction_profile.v0.1.0` to a `sli.tensor.v0.1.0` within a specific query context. 

It is the **only place** where salience, relevance, and perspective shifts are allowed to exist.

---

# 0. Purpose

`iutt.runtime_state.v0.1.0` represents the **active cognitive frame**. 
It is used for:
1. Decision making.
2. Narrative generation.
3. Response synthesis.
4. Audit of "how the agent was thinking" at a specific tick.

---

# 1. Non-Goals

- It is **EPHEMERAL**. It belongs in memory or a sidecar cache.
- It is **NEVER identity**. You must not hash this into an `EngramId`.
- It is **AUDITABLE**, but not **PERMANENT** unless explicitly re-tensorized and committed.

---

# 2. Schema ID

`iutt.runtime_state.v0.1.0`

---

# 3. Canonical Record (C#)

```csharp
public record IuttRuntimeState_v0_1_0(
    // --- Identifiers (Ephemeral) ---
    string RuntimeStateId,               // sha256(snapshot bytes)
    string CreatedTick,                  // GenesisTick + LocalOffset

    // --- Profile Pinning ---
    string ReconstructionProfileVersion,
    string ReconstructionProfileHash,

    // --- Query Context ---
    string QueryContextHash,             // sha256(canonical query JSON)
    string SessionId,
    string OperatorId,
    string ScenarioName,

    // --- Source Linkage ---
    string[] OriginEngramIds,            // sorted
    string OriginTensorHash,             // sli.tensor.OriginTensorHash
    string OriginBraidIndexHash,         // gel.braid_index.BraidIndexHash

    // --- The Cognitive Payload (The "Thinking") ---
    PerspectiveFrame Perspective,        // transformed coordinates
    SalienceField Salience,              // attention values / gradients
    RelevanceTrace Relevance,            // scoring results
    RuntimeAssembly Assembly,            // node graph with runtime weights

    // --- Telemetry & Audit ---
    TriptychSignals Signals,             // high-level cognitive indicators
    BoundaryAttestation Boundary,        // layer separation proof
    string[] Warnings,
    string[] Notes
);
```

---

# 4. Invariants

1. **Separation**: `iutt.runtime_state` must remain strictly isolated from GEL storage.
2. **Reproducibility**: Given the same `sli.tensor` + `iutt.reconstruction_profile` + `QueryContext`, the resulting `iutt.runtime_state` should be deterministic (modulo float precision in salience if applicable).
3. **No Silently Promoted State**: Any state here that needs to be "remembered" must go back through the **Engrammitization Pipeline** (Stage 1-5).

---

# 5. Required Tests

- **RS-1: Layer Breach Check**: verify that `gel.*` objects cannot be constructed using `iutt.runtime_state` as a primary source without a fresh normalization pass.
- **RS-2: Auditability**: verify that the `OriginTensorHash` and `ReconstructionProfileHash` allow one to prove the provenance of the salience gradients.
```

##### 14. Modules\Engrammitization\sli.commit_intent.v0.1.0.md
```markdown
5) sli.commit_intent.v0.1.0
Commit Intent (Post-Gate, Pre-GEL) (Layer 1 → Layer 0 Bridge)
Purpose

Represents the post-gate authorized intent to crystallize into GEL.

This is the object that CommitEngine consumes to produce:

gel.golden_engram.v0.1.0 entries

gel.tip_advance_event.v0.1.0 telemetry

It is the “last stop” before identity changes.

Schema ID

sli.commit_intent.v0.1.0

Record
public record SliCommitIntent_v0_1_0(
    string Schema,                         // "sli.commit_intent.v0.1.0"

    // Bridge provenance
    string PacketHash,                     // original sli.packet.PacketHash
    string GateDecisionHash,               // sli.gate_decision.DecisionHash
    string GateEvidenceHash,               // sli.gate_evidence.EvidenceHash
    string PolicyVersion,

    // Deterministic session genesis
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick,

    // Identity anchors / proposal anchors
    string IntakeHash,                     // sha256 canonical intake bytes (if applicable)
    string ProposedBraidIndexHash,         // from PreGELBundle proposal (optional)
    string ParentTip,                      // tip observed at time of intent creation (for CAS verification)

    // What is being committed (must be handle-explicit)
    CommitDirective[] Directives,          // sorted by Handle

    // Deterministic digest
    string CommitIntentHash,               // sha256(canonicalBytes(without CommitIntentHash))
    string CanonicalSeal                   // sha256(full canonical serialization)
);

public record CommitDirective(
    string Handle,                         // e.g. "engram.construct.propositional"
    string ResolvedAddressText,            // "Channel/Partition/Mirror"
    string EntryHash,                      // RootAtlasEntry.EntryHash
    string ConstructorPayloadRef,          // reference to prepared constructor payload in PreGELBundle (non-authoritative pointer)
    string ConstructorPayloadHash          // sha256(canonical bytes of payload) (optional but recommended)
);
Freeze Rules

CommitIntent MUST only be created when the gate decision is allowed.

Directives must correspond to the admitted handles; no extras.

ConstructorPayloadRef is a pointer/reference only. The canonical payload bytes (if hashed) must be produced deterministically from Pre-SLI normalization + declared handle, not Layer-2 runtime gradients.

ParentTip must be verified during CommitEngine execution; mismatch → TIP_CONFLICT.

Required Tests

CI-1: CommitIntentHash stable across machines.

CI-2: If GateDecision.IsAllowed == false, CommitIntent creation must fail.

CI-3: Directives sorted; each directive has non-null Handle, ResolvedAddressText, EntryHash.
```

##### 15. Modules\Engrammitization\sli.duplex_message.v0.1.0.md
```markdown
19) sli.duplex_message.v0.1.0
Duplex Message Envelope (Layer 1, Bidirectional, Deterministic Frame)
Purpose

Defines the single envelope for all bidirectional messages exchanged between:

AgentiCore/CradleTek host side

Lisp duplex side (mirror / pointer resolution / telemetry consumption)

This prevents ad-hoc JSON blobs and ensures deterministic audit logging.

Schema ID

sli.duplex_message.v0.1.0

Record
public record SliDuplexMessage_v0_1_0(
    string Schema,                         // "sli.duplex_message.v0.1.0"

    // Message identity
    string MessageHash,                    // sha256(canonicalBytes(without MessageHash))
    string CanonicalSeal,                  // sha256(full canonical serialization)

    // Routing
    string Direction,                      // "HOST_TO_DUPLEX" | "DUPLEX_TO_HOST"
    string MessageType,                    // closed vocab (see below)
    string CorrelationId,                  // deterministic id for request/response pairing (no GUID)
    long Sequence,                         // monotonic local per channel (optional cross-substrate)

    // Session provenance (structural)
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick,

    // Payload reference (typed)
    string PayloadSchema,                  // e.g. "sli.symbol_resolution_request.v0.1.0"
    string PayloadHash,                    // sha256(canonicalBytes(payload))
    string PayloadInline                   // optional: inline payload (if present, must be canonical JSON)
);
MessageType (v0.1.0 closed vocab)

TELEMETRY_PUSH

SYMBOL_RESOLVE_REQUEST

SYMBOL_RESOLVE_RESPONSE

MIRROR_SNAPSHOT_REQUEST

MIRROR_SNAPSHOT_RESPONSE

ERROR

Freeze Rules

CorrelationId must be deterministic. Recommended recipe:

sha256($"{SessionId}|{OperatorId}|{ScenarioName}|{GenesisTick}|{MessageType}|{Sequence}")

If PayloadInline is present, it must be canonical JSON (fixed field order, sorted arrays where applicable). Otherwise leave it "null" and rely on PayloadHash.

No Layer-2 content in duplex messages (no salience/gradients).

Required Tests

DM-1: Same envelope fields → same MessageHash.

DM-2: MessageType outside closed vocab rejected.

DM-3: PayloadHash must match recomputed payload hash if PayloadInline included.
```

##### 16. Modules\Engrammitization\sli.gate_decision.v0.1.0.md
```markdown
3) sli.gate_decision.v0.1.0
Gate Decision & Evidence Record (Layer 1, Auditable Output)
Purpose

Defines the gate’s deterministic output (allowdeny) with required audit fields

deny includes reasonCode + policyVersion

allow logs handle + resolved address + SAT mode + masking state 

SLI CONSTITUTION v0_1

Schema ID

sli.gate_decision.v0.1.0

Record
public record SliGateDecision_v0_1_0(
    string Schema,                        sli.gate_decision.v0.1.0

     Inputs provenance (hashes, not raw objects)
    string PacketHash,                    from sli.packet
    string RootAtlasHash,                 from sli.root_atlas (container hash) or concatenated EntryHash set
    string SessionMountsHash,             from sli.session_mounts
    string PolicyVersion,                 gate policy version (same as RootAtlas entries or higher-level policy)

     Decision
    bool IsAllowed,
    string ReasonCode,                    required if denied; optional if allowed
    string SatModeObserved,               from packet.Mode.SatMode
    bool SafActiveObserved,               from packet.Mode.SafActive

     Per-handle resolutions (explicit addressing)
    HandleResolution_v0_1_0[] Resolutions,       sorted by Handle

     Cryptic masking
    bool MaskingApplied,
    string MaskingPolicyId,               none if not cryptic

     Evidence & audit
    string EvidenceSnapshot,              canonical text form or compact JSON (deterministic)
    string EvidenceSnapshotHash,          sha256(EvidenceSnapshot UTF-8)
    string DecisionHash,                  sha256(canonicalBytes(without DecisionHash))
    string CanonicalSeal                  sha256(full canonical serialization)
);

public record HandleResolution_v0_1_0(
    string Handle,
    string AddressText,                   ChannelPartitionMirror
    string AddressHash,                   sha256(AddressText)
    string EntryHash                      RootAtlasEntry.EntryHash used
);
Freeze Rules

Resolutions must include one entry per declared handle that exists; missing handle triggers deny with reason HANDLE_NOT_FOUND.

EvidenceSnapshot must be deterministic

fixed field order

no wall-clock time

no runtime context

If IsAllowed == false, ReasonCode must be non-empty.

If cryptic access attempted

MaskingApplied must be true if policy requires it

MaskingPolicyId must be recorded 

SLI CONSTITUTION v0_1

Tests

GD-1 Deny contains PolicyVersion + ReasonCode.

GD-2 Allow contains per-handle AddressText + EntryHash.

GD-3 EvidenceSnapshotHash stable across machines.
```

##### 17. Modules\Engrammitization\sli.gate_evidence.v0.1.0.md
```markdown
4) sli.gate_evidence.v0.1.0
Gate Evidence Payload (Layer 1, Deterministic, Structured)
Purpose

Provides a structured and deterministic evidence record for gate evaluation, replacing “freeform evidence strings” with a canonical object whose hash can be referenced by:

sli.gate_decision.v0.1.0.EvidenceSnapshotHash

telemetry streams

audits

This object contains only structural gating facts, never runtime semantics.

Schema ID

sli.gate_evidence.v0.1.0

Record
public record SliGateEvidence_v0_1_0(
    string Schema,                         // "sli.gate_evidence.v0.1.0"

    // Provenance (hash references only)
    string PacketHash,                     // sli.packet.PacketHash
    string SessionMountsHash,              // sli.session_mounts.MountsHash
    string RootAtlasHash,                  // hash of root atlas container or entry set
    string PolicyVersion,

    // Gate inputs observed (structural only)
    string SatModeObserved,
    bool SafActiveObserved,
    string RequestedEnvChannel,            // from packet.Env.Channel (declared)
    string RequestedEnvPartition,
    string RequestedEnvMirror,

    // Handle evaluation results
    HandleEvidence_v0_1_0[] Handles,              // sorted by Handle

    // Cryptic evidence
    bool CrypticAttempted,
    bool MaskingApplied,
    string MaskingPolicyId,                // "none" if not applicable

    // Final decision summary (mirrors gate_decision)
    bool IsAllowed,
    string ReasonCode,                     // required if denied

    // Deterministic digests
    string EvidenceHash,                   // sha256(canonicalBytes(without EvidenceHash))
    string CanonicalSeal                   // sha256(full canonical serialization)
);

public record HandleEvidence_v0_1_0(
    string Handle,
    bool ExistsInRootAtlas,
    string EntryHash,                      // "null" if missing
    string AllowedAddressText,             // from atlas constraints normalized: "Channel/Partition/Mirror" or "Any/Any/Any"
    string ResolvedAddressText,            // final resolved address if admissible else "null"
    bool MountAllowed,
    bool SatSatisfied,
    bool HitlSatisfied,
    string FailureReason                   // "none" or a deterministic code: "HANDLE_NOT_FOUND"|"MOUNT_DENY"|...
);
Freeze Rules

Handles must list every declared handle from the packet (even missing ones), in sorted order.

FailureReason must be a closed vocabulary (enum-like), not prose.

No wall-clock, no user text, no query context.

If IsAllowed == false, ReasonCode must be set.

Required Tests

EVD-1: EvidenceHash stable across machines.

EVD-2: Missing handle yields ExistsInRootAtlas=false, deterministic FailureReason="HANDLE_NOT_FOUND".

EVD-3: No forbidden substrings: salience, relevance, gradient, queryContext, perspective, basin.
```

##### 18. Modules\Engrammitization\sli.handle_registry_manifest.v0.1.0.md
```markdown
14) sli.handle_registry_manifest.v0.1.0
Handle Registry Manifest (Layer 1, Explicit Handle Set Pinning)
Purpose

Pins the exact list of valid handles and their hashes for a given policy bundle, preventing:

silent handle addition/removal

“shadow handles”

drift between RootAtlas and what operators think exists

This is a deterministic “handle allowlist” that complements RootAtlas.

Schema ID

sli.handle_registry_manifest.v0.1.0

Record
public record SliHandleRegistryManifest_v0_1_0(
    string Schema,                         // "sli.handle_registry_manifest.v0.1.0"

    // Manifest identity
    string RegistryVersion,                // "v0.1.0"
    string PolicyVersion,                  // "POLICY-0.1.0"
    string RegistryHash,                   // sha256(canonicalBytes(without RegistryHash))
    string CanonicalSeal,                  // sha256(full canonical serialization)

    // What handles exist
    HandleEntry_v0_1_0[] Handles                  // sorted by Handle
);

public record HandleEntry_v0_1_0(
    string Handle,
    string EntryHash,                      // sli.root_atlas_entry.EntryHash
    string IntentKind,
    string AllowedAddressConstraintText,   // e.g. "Private/GEL/Standard" or "Any/Any/Any"
    string[] RequiredSatModes              // sorted
);
Freeze Rules

Each HandleEntry must correspond 1:1 with a RootAtlasEntry.

Registry is the explicit pin list; RootAtlas is the full rule record.

Handle lookups must require:

handle exists in registry, and

handle exists in RootAtlas.

Required Tests

HR-1: RegistryHash stable.

HR-2: Any RootAtlas entry not in registry causes boot/policy validation failure.

HR-3: HandleEntry.EntryHash must match the RootAtlasEntry hash exactly.
```

##### 19. Modules\Engrammitization\sli.packet.v0.1.0.md
```markdown
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
```

##### 20. Modules\Engrammitization\sli.policy_bundle.v0.1.0.md
```markdown
15) sli.policy_bundle.v0.1.0
Policy Bundle (Layer 1, Single Root of Truth for Governance Pinning)
Purpose

Defines the single pinning artifact that locks:

RootAtlas contents

handle registry

session mount policy

telemetry emission policy

reconstruction profile pins (Layer-2 profile hash, but pinned here)

This is the “policy root” that the host loads to ensure the entire stack is consistent.

Schema ID

sli.policy_bundle.v0.1.0

Record
public record SliPolicyBundle_v0_1_0(
    string Schema,                         // "sli.policy_bundle.v0.1.0"

    // Bundle identity
    string BundleVersion,                  // "v0.1.0"
    string PolicyVersion,                  // "POLICY-0.1.0"
    string BundleHash,                     // sha256(canonicalBytes(without BundleHash))
    string CanonicalSeal,                  // sha256(full canonical serialization)

    // Pinned components (hash references)
    string RootAtlasHash,                  // container hash for root atlas
    string HandleRegistryHash,             // sli.handle_registry_manifest.RegistryHash
    string MountPolicyVersion,
    string TelemetryManifestHash,          // sli.telemetry_stream_manifest.ManifestHash

    // Runtime cognition pinning (Layer 2 pinned by hash, not stored as identity)
    string ReconstructionProfileVersion,
    string ReconstructionProfileHash,

    // Bootstrap pins (optional but recommended)
    string BootstrapVersion,
    string BootstrapHash,

    // Host discipline (optional, but useful)
    string[] RequiredModules,              // sorted list: e.g. ["SLI_GATE", "GEL_SPINE", "CANON_BUILDER"]
    string[] ForbiddenModules              // sorted list: e.g. ["HEURISTIC_HANDLE_INFERENCE"]
);
Freeze Rules

Host must refuse to start if any pinned hash mismatches the loaded artifact.

ReconstructionProfileHash is pinned here but remains Layer-2 runtime spec.

BundleHash must be stable across machines.

Required Tests

PB-1: BundleHash stable.

PB-2: If any pinned component hash differs → fail closed at boot.

PB-3: Bundle pins must be consistent: RootAtlasHash must include exactly the handles in HandleRegistryHash.
```
Record
public record SliRefusalRecord_v0_1_0(
    string Schema,
    string RefusalHash,
    string RefusalType,
    string ReasonCode,
    string[] AffectedHandles,
    string[] MissingHandles
);
```

##### 21. Modules\Engrammitization\sli.refusal_record.v0.1.0.md
```markdown
18) sli.refusal_record.v0.1.0
Refusal Record (Layer 1, Standard Deny Output for Duplex + Audit)
Purpose

Defines the standardized record returned when:

SLI gate denies a packet

commit intent creation fails due to denial

mounts deny

SAT/HITL requirements fail

This gives the operator + duplex system a consistent object to display/stream.

Schema ID

sli.refusal_record.v0.1.0

Record
public record SliRefusalRecord_v0_1_0(
    string Schema,                         // "sli.refusal_record.v0.1.0"

    // Refusal identity
    string RefusalHash,                    // sha256(canonicalBytes(without RefusalHash))
    string CanonicalSeal,                  // sha256(full canonical serialization)

    // Provenance
    string PacketHash,                     // sli.packet.PacketHash
    string GateDecisionHash,               // may be "null" if refusal occurred pre-gate validation
    string GateEvidenceHash,               // may be "null"
    string PolicyVersion,
    string MountPolicyVersion,

    // Decision
    string RefusalType,                    // "GATE_DENY"|"MOUNT_DENY"|"VALIDATION_FAIL"|"SAF_DEFER"
    string ReasonCode,                     // closed vocab (matches gate/mount reason codes)
    string[] AffectedHandles,              // sorted
    string[] MissingHandles,               // sorted
    bool SafActiveObserved,
    string SatModeObserved,

    // Optional guidance (non-authoritative, deterministic)
    string GuidanceCode,                   // closed vocab: "ADD_HANDLE"|"REQUEST_HITL"|...
    string Notes                           // optional; excluded from hash if you prefer strict determinism
);
Freeze Rules

Refusal must be deterministic (no prose required).

GuidanceCode is closed vocab and optional.

Must not include runtime context or salience fields.

If you include Notes, decide now whether it participates in hashing. Recommendation: exclude notes from hash to preserve determinism while allowing UI explanation.

Required Tests

REF-1: RefusalHash stable.

REF-2: RefusalType and ReasonCode must match the failure discovered by the Gate or Mount orchestrator.

REF-3: Handles sorted ordinal.
```

##### 22. Modules\Engrammitization\sli.resolved_address.v0.1.0.md
```markdown
2) sli.resolved_address.v0.1.0
Resolved Address (Layer 1, Canonical Form)
Purpose

Defines the canonical normal-form address (Channel × Partition × Mirror) that every handle resolves to.

This is NOT the handle itself. This is the coordinate in the OAN Mortalis substrate where the data actually lives.

Schema ID

sli.resolved_address.v0.1.0

Record
public record SliResolvedAddress_v0_1_0(
    string Schema,                         // "sli.resolved_address.v0.1.0"

    // The 3-Axis coordinate
    string Channel,                        // "Public" | "Private"
    string Partition,                      // "GEL" | "GOA" | "OAN"
    string Mirror,                         // "Standard" | "Cryptic"

    // Canonical Text representation
    string AddressText,                    // e.g. "Private/GEL/Standard"

    // Hash
    string AddressHash                     // sha256(AddressText UTF-8)
);
Freeze Rules

AddressText MUST be "Channel/Partition/Mirror".

No whitespace, no alternate delimiters.

Case-sensitive.

Required Tests

RA-1: AddressHash stable across machines.

RA-2: Invalid channel/partition/mirror strings rejected at construction.
```

##### 23. Modules\Engrammitization\sli.root_atlas_entry.v0.1.0.md
```markdown
Below is a **freeze-grade** `sli.root_atlas_entry.v0.1.0` spec. This is the **authoritative capability registry schema** for v0.1.0.

It is consistent with:
* **SLI Constitution v0.1** (Channel×Partition×Mirror, static resolution, etc.)
* **Locked Layer separation** (GEL identity vs SLI structural)
* **Execution readiness** (this record MUST be hashable and static for the gate to be deterministic)

---

# sli.root_atlas_entry.v0.1.0

## Root Atlas Capability Registry Entry (Layer 1, Immutable)

## 0. Purpose

`sli.root_atlas_entry.v0.1.0` defines the **immutable registry entry** for a single SLI handle. 

It maps a **Handle** (e.g., `engram.construct.propositional`) to its **authoritative gating rules**:
* Where it resolves (`AddressConstraint`)
* What governance it requires (`RequiredSatModes`, `RequiresHITL`)
* What policy version it belongs to.

The Root Atlas is a collection of these entries.

---

## 1. Non-Goals

* This is NOT an `EngramId`.
* This DOES NOT contain runtime context.
* This DOES NOT contain salience or gradients.
* It is **structural**, not **semantic**.

---

## 2. Core Invariants

### 2.1 Static Resolution
In v0.1.0, a handle always resolves to the same `AddressConstraint`. No runtime re-routing.

### 2.2 Canonical Hash
The `EntryHash` is the unique identity of this rule. If a rule changes, the hash changes, and the `PolicyBundle` must be updated.

---

## 3. Schema ID

`sli.root_atlas_entry.v0.1.0`

---

## 4. Canonical Record (C#)

```csharp
public record SliRootAtlasEntry_v0_1_0(
    // --- Schema ---
    string Schema,                       // "sli.root_atlas_entry.v0.1.0"

    // --- Key ---
    string Handle,                       // the unique capability key

    // --- Intent Classification ---
    string IntentKind,                   // "Propositional" | "Procedural" | "Perspective" | "Participatory"

    // --- Gating: Addressing ---
    AddressConstraint AllowedAddress,    // see §4.1

    // --- Gating: Governance ---
    string[] RequiredSatModes,           // sorted; e.g. ["SAT_Gate", "HITL"]
    bool RequiresHITL,                   // Human-In-The-Loop required?

    // --- Versioning ---
    string PolicyVersion,                // e.g. "POLICY-0.1.0"

    // --- Cryptic Masking ---
    CrypticConstraint Cryptic,           // see §4.2

    // --- Metadata (non-gating) ---
    string Description,
    string Owner,

    // --- Digests ---
    string EntryHash,                    // sha256(canonicalBytes(entry without EntryHash))
    string CanonicalSeal                 // sha256(full canonical serialization)
);
```

### 4.1 AddressConstraint

```csharp
public record AddressConstraint(
    string Channel,                      // "Public" | "Private" | "Any"
    string Partition,                    // "GEL" | "GOA" | "OAN" | "Any"
    string Mirror                         // "Standard" | "Cryptic" | "Any"
);
```

---

### 4.2 CrypticConstraint

```csharp
public record CrypticConstraint(
    bool AllowedInCryptic,
    string MaskingPolicyId,              // e.g. "mask.low_salience" | "none"
    string RequiredSatForCryptic         // e.g. "SAT_Gate"
);
```

---

## 5. Canonicalization Contract

Same rules as `sli.packet.v0.1.0`:

1. Fixed field order.
2. Arrays sorted lexicographically (ordinal).
3. `null` → literal `"null"`.
4. UTF-8 only.
5. Lowercase hex for hashes.

---

## 6. Required Tests

### Test AE1 — EntryHash Stability
Same rules → same `EntryHash`.

### Test AE2 — IntentKind Validation
Must be one of the four P’s (FourP).

### Test AE3 — SatMode Sorting
`RequiredSatModes` must be sorted ordinal in the canonical form.

---

## 7. Relationship to Engrammitization

In Stage 4 (Gate Evaluation), the Gate looks up the `SliRootAtlasEntry` for each handle declared in the packet. 

If:
1. Entry does not exist → **Deny**.
2. Packet address does not match `AllowedAddress` → **Deny**.
3. Packet SatMode not in `RequiredSatModes` → **Deny**.

This ensures **Governance Before Execution**.
```

##### 24. Modules\Engrammitization\sli.session_mounts.v0.1.0.md
```markdown
10) sli.session_mounts.v0.1.0
Session Mount Policy (Layer 1, Runtime Environment Allowlist)
Purpose

Defines what address spaces are "mounted" for a specific session. Even if a handle resolves to an address, if that address isn’t mounted, the Gate must deny the action.

This acts as a secondary allowlist that prevents:

leaking data between sessions

writing to partitions not intended for the current scenario

Schema ID

sli.session_mounts.v0.1.0

Record
public record SliSessionMounts_v0_1_0(
    string Schema,                         // "sli.session_mounts.v0.1.0"

    // Session identity
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick,

    // Policy version
    string MountPolicyVersion,             // "MPV-0.1.0"

    // The Allowlist
    SliResolvedAddress_v0_1_0[] MountedAddresses, // sorted by AddressHash

    // Constraints per mount
    MountConstraint_v0_1_0[] Constraints,         // sorted by MountHash/AddressHash

    // Digest
    string MountsHash,                     // sha256(canonicalBytes(without MountsHash))
    string CanonicalSeal                   // sha256(full canonical serialization)
);

public record MountConstraint_v0_1_0(
    string AddressHash,
    string PermissionMode,                 // "READ_ONLY" | "WRITE_ONLY" | "READ_WRITE"
    bool MaskingRequired
);
Freeze Rules

MountedAddresses must be sorted by AddressHash.

Constraints must be sorted by AddressHash.

Unmounted addresses default to DENY.

Required Tests

SM-1: MountsHash stable.

SM-2: Request to unmounted address is denied even if handle is valid.

SM-3: PermissionMode mismatch (e.g. write to READ_ONLY) → deny.
```

##### 25. Modules\Engrammitization\sli.symbol_pointer.v0.1.0.md
```markdown
Below is a **freeze-grade** `sli.symbol_pointer.v0.1.0` spec. This defines the **morphism-safe, structural pointer** for OAN Mortalis v0.1.0.

It is consistent with:
* **SLI Constitution v0.1** (deterministic resolution, symbol→target, identity-agnostic)
* **Locked Layer separation** (symbol is structural, not semantic)
* **Execution readiness** (no heuristics in v0.1.0)

---

# sli.symbol_pointer.v0.1.0

## Structural Symbol Pointer (Layer 1, Deterministic)

## 0. Purpose

`sli.symbol_pointer.v0.1.0` is the **canonical identity** for a link between a **SymbolText** (e.g. `engram.construct.propositional`) and its **Target** (e.g. an `EngramId` or `ResolvedAddress`).

It ensures that pointers:
1. Are **structural**: They do not carry “meaning.”
2. Are **morphism-safe**: They can be resolved without runtime context.
3. Are **not misused** for identity or runtime salience.

---

## 1. Non-Goals

* This is NOT a "keyword" or "tag."
* This DOES NOT contain salience gradients or relevance.
* This is NOT a replacement for `EngramId`.

---

## 2. Core Invariants

### 2.1 Canonicalization of SymbolText
SymbolText must be normalized (lowercase ASCII subset recommended) before hashing.

### 2.2 Deterministic Resolution
In v0.1.0, a pointer resolves to exactly one target in the context of a given `Engrammitization` state.

### 2.3 Identity Agnosticism
A pointer does not care *who* is looking; it only cares about the *mapping* from symbol space to target space.

---

## 3. Schema ID

`sli.symbol_pointer.v0.1.0`

---

## 4. Canonical Record (C#)

```csharp
public record SliSymbolPointer_v0_1_0(
    // --- Schema ---
    string Schema,                       // "sli.symbol_pointer.v0.1.0"

    // --- Pointer Identity ---
    string PointerHash,                  // sha256(canonicalBytes(pointer without PointerHash))
    string CanonicalSeal,                // sha256(full canonical serialization)

    // --- Symbol Logic ---
    string Namespace,                    // e.g. "sli", "gel", "rootatlas"
    string SymbolText,                   // the canonicalized symbol string
    string SymbolEncoding,               // "UTF8" (v0.1.0)

    // --- Target Topology ---
    string PointerKind,                  // "engram_id" | "resolved_address" | "pointer" (chain)
    SymbolTarget Target,                 // see §4.1

    // --- Provenance ---
    PointerProvenance Provenance         // see §4.2
);
```

### 4.1 SymbolTarget

```csharp
public record SymbolTarget(
    string TargetId,                     // e.g. EngramId or AddressHash
    string TargetSchema                  // e.g. "gel.golden_engram.v0.1.0"
);
```

---

### 4.2 PointerProvenance

```csharp
public record PointerProvenance(
    string CreatorPolicyHash,            // policy version that generated this link
    string SourceIntakeHash,             // intake that necessitated this symbol
    string BraidIndexHash                 // context linkage (optional for v0.1.0)
);
```

---

## 5. Canonicalization Contract

Same contract as `gel.golden_engram.v0.1.0`:

1. Field order as defined in §4. 
2. Arrays sorted lexicographically (Ordinal).
3. `null` → literal `"null"`.
4. UTF-8 only.

---

## 6. Required Tests

### Test SP1 — PointerHash Stability
Same SymbolText + same Target → same `PointerHash`.

### Test SP2 — Case Normalization
`SymbolText` normalization (e.g. lowercase) must be verified before hashing.

### Test SP3 — Invalid Target Rejected
Constructing a pointer with an empty `TargetId` must fail.

---

## 7. Relationship to Engrammitization

In Stage 3 (SLI Packetization), symbol pointers may be used in `PacketRefs` to declare dependencies or targets without naming them by ID.

In Stage 5 (Commit Optimization), the Lisp duplex can use these pointers to deduplicate or route engrams before they reach the CommitEngine.
```

##### 26. Modules\Engrammitization\sli.telemetry_record.v0.1.0.md
```markdown
9) sli.telemetry_record.v0.1.0
SLI Telemetry Record (Layer 1, NDJSON Unit)
Purpose

Defines a unified telemetry line format for various events (gate, commit, tip advance, quarantine, etc.). This ensures your NDJSON writers are deterministic and searchable.

Schema ID

sli.telemetry_record.v0.1.0

Record
public record SliTelemetryRecord_v0_1_0(
    string Schema,                         // "sli.telemetry_record.v0.1.0"

    // Event identity
    string EventType,                      // "GATE_DECISION"|"COMMIT_ADVANCE"|"STATE_CHANGE"|...
    string EventHash,                      // links to the underlying object hash (e.g. DecisionHash)
    string CanonicalSeal,                  // seal of the underlying object

    // Time-like sequencing (no wall-clock for identity; wall-clock allowed here for ops)
    long GenesisTick,
    long EventSequence,                    // monotonic local
    string WallTimeIso8601,                // optional; for UI/ops only; NOT in event identifier

    // High-value audit fields (copies of fields for easier querying)
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
    string[] Handles,                      // sorted

    // Session provenance
    string SessionId,
    string OperatorId,
    string ScenarioName
);
Freeze Rules

NDJSON output must use fixed field order.

EventSequence must be monotonic per event stream.

No Layer-2 runtime state.

Required Tests

TEL-1: Telemetry record correctly mirrors the underlying object hashes.

TEL-2: NDJSON serialization is stable.

TEL-3: wallTime is excluded from EventHash computation if you decide EventHash should be deterministic over the "fact."
```

##### 27. Modules\Engrammitization\sli.telemetry_stream_manifest.v0.1.0.md
```markdown
15b) sli.telemetry_stream_manifest.v0.1.0
Telemetry Stream Manifest (Layer 1, Emission Policy)
Purpose

Defines the policy for telemetry event emission:

which events go to which sinks (local file, Lisp duplex, remote audit)?

what detail level?

which handles are "high value" for auditing?

This matches your TelemetryPolicy in the reconstruction profile (Layer 2) but exists here (Layer 1) to gate the actual emission.

Schema ID

sli.telemetry_stream_manifest.v0.1.0

Record
public record SliTelemetryStreamManifest_v0_1_0(
    string Schema,                         // "sli.telemetry_stream_manifest.v0.1.0"

    // Manifest identity
    string ManifestVersion,
    string ManifestHash,                   // sha256(canonicalBytes(without ManifestHash))
    string CanonicalSeal,                  // sha256(full canonical serialization)

    // Policy linkage
    string PolicyVersion,
    string MountPolicyVersion,

    // Rules
    StreamRule_v0_1_0[] Rules,                    // sorted by RuleId
    SinkSpec_v0_1_0[] Sinks                      // sorted by SinkId
);

public record StreamRule_v0_1_0(
    string RuleId,
    string EventTypeSelector,              // "*" | "GATE.*" | "COMMIT.*"
    string DetailLevel,                    // "Minimal" | "Normal" | "FullEvidence"
    string[] TargetSinkIds                 // sorted
);

public record SinkSpec_v0_1_0(
    string SinkId,
    string SinkKind,                       // "File" | "Duplex" | "UDP"
    string SinkAddress                     // e.g. path or ip
);
Freeze Rules

ManifestHash must be deterministic.

Sinks kind vocab is closed.

Required Tests

TSM-1: ManifestHash stable.

TSM-2: Event emission correctly filters by DetailLevel.
```

##### 28. Modules\Engrammitization\sli.tensor.v0.1.0.md
```markdown
Below is a **freeze-grade** `sli.tensor.v0.1.0` spec. This defines the **structural representation for tensorizing GoldenEngrams**, enabling Layer 2 reconstruction.

It is consistent with:
* **Constitutional Purity**: No salience or gradients in the tensor (structural only).
* **Morphism-Invariant**: Pointers and nodes are stable and hashable.
* **Deterministic Layout**: Order of nodes, edges, and symbols is fixed.

---

# sli.tensor.v0.1.0

## Symbolic Language Interconnect Tensor (Layer 1, Structural)

## 0. Purpose

`**sli.tensor.v0.1.0**` is the **transport-optimized container** for a set of crystallized results from GEL.

It acts as the **bridge between Layer 0** (Identity) and **Layer 2** (Cognitive Playback).

It provides:
* **Structural Topology**: Nodes and Edges connecting Engrams.
* **Morphism-Invariant Addressability**: Stable pointers.
* **Deterministic Encoding**: No salience or semantics.

---

## 1. Non-Goals

* A tensor DOES NOT contain salience gradients or relevance weights.
* A tensor DOES NOT perform perspective shifts.
* A tensor is NOT a "world state"; it is a **view** of the ledger.

---

## 2. Core Invariants

### 2.1 Salience-Free / Gradient-Free
Layer 1 tensors MUST NOT contain floating-point salience values or runtime attention weights. They carry only the **existence** of nodes and edges.

### 2.2 Canonicalization
All elements (Nodes, Edges, Symbols) must be sorted and serialized using the **Canonical Serialization Contract**.

### 2.3 Transport Integrity
The `OriginTensorHash` ensures that the Layer-2 reconstruction layer knows exactly what it is starting from.

---

## 3. Schema ID

`**sli.tensor.v0.1.0**`

---

## 4. Canonical Record (C#)

```csharp
public record SliTensor_v0_1_0(
    // --- Metadata ---
    string TensorVersion,                  // "v0.1.0"
    string TensorizationProfileVersion,    // e.g. "DEFAULT"
    string TensorizationProfileHash,       // (optional) link to profile used to slice the GEL

    // --- Identity Anchors (Sorted) ---
    string[] OriginEngramIds,              // the set of identity nodes in this tensor
    string IntakeHash,                     // source intake anchor

    // --- Structural Topology (Sorted) ---
    TensorNode_v0_1_0[] Nodes,                    // see §4.1
    TensorEdge_v0_1_0[] Edges,                    // see §4.2
    string[] SymbolPointers,               // sorted list of sli.symbol_pointer.PointerHash

    // --- Deterministic Encoding ---
    string EncodingMode,                   // "Structural" | "Sparse"
    string[] FeatureKeys,                  // sorted; optional structural keys
    string[] FeatureValues,                // parallel to and matched with keys

    // --- Digests ---
    string OriginTensorHash,               // sha256(canonicalBytes(tensor without hash))
    string CanonicalSeal                   // sha256(full canonical serialization)
);
```

### 4.1 TensorNode

```csharp
public record TensorNode_v0_1_0(
    string EngramId,
    string Handle,
    string AddressText,
    string PayloadHash
);
```

---

### 4.2 TensorEdge

```csharp
public record TensorEdge_v0_1_0(
    string EdgeId,                       // sha256(SourceEngramId + TargetEngramId + Relation)
    string SourceEngramId,
    string TargetEngramId,
    string RelationHandle,               // the handle defining the morphism
    string PolicyVersion
);
```

---

## 5. Canonicalization Contract

1. **Nodes** sorted by `EngramId`.
2. **Edges** sorted by `EdgeId`.
3. **Pointers** sorted by `PointerHash`.
4. Field order per schema.
5. UTF-8 strings.
6. Null → `"null"`.

---

## 6. Required Tests

### Test TS1 — TensorHash Stability
Same nodes/edges → same `OriginTensorHash`.

### Test TS2 — Salience Leak Check
Verify no `salience`, `gradient`, or `relevance` fields exist in the serialized NDJSON.

### Test TS3 — Sort Enforcement
Verify that out-of-order nodes/edges are rejected by the canonical builder.

---

## 7. Relationship to Engrammitization

In Stage 6 (IUTT Playback), the reconstructed `sli.tensor` is passed to the **IUTT Layer**. 

Only there, at runtime, are **Salience Gradients** and **Relevance Scores** applied to the nodes and edges defined in the tensor. 

This preserves **Inert Identity** in the storage/transport layer.
```

##### 29. Modules\Engrammitization\Engrammitization.md (Pre-Consolidation)
```markdown
# Engrammitization Overview (Feb 2026 Archive)

This file previously served as the summary of the Engrammitization effort before the 2026-02-20 consolidation.

**STATUS**: ARCHIVED / SUPERSEDED BY FINAL CONSTITUTION  
**DATE**: 2026-02-20  
**CONTEXT**: OAN Mortalis v1.0 Alignment Phase  

Engrammitization is the deterministic process of transforming raw intake into inert identity-bearing Golden Engrams.

- **Layer 0**: GEL (Golden Engram Library) - Immutable Identity.
- **Layer 1**: SLI (Symbolic Language Interconnect) - Structural Routing.
- **Layer 2**: IUTT (Independent Universal Task Transformer) - Runtime Cognition.

**Key Accomplishments**:
- Canonical Serialization Contract finalized.
- 6-Phase Pipeline formalized.
- Layer boundaries enforced (no L2 state in L0/L1).
- 29 source files consolidated into this constitution.
```

#### B) Resolved Conflicts Log (Audit Only)

| Conflict ID | File Paths | Conflicting Excerpts | Resolution (Applied to Registry) | Status |
| :--- | :--- | :--- | :--- | :--- |
| **C-01: Schema Record Suffixes** | SPEC vs gel.tip_advance_event.v0.1.0.md | SPEC uses `GelTipAdvanceEvent_v0_1_0` vs `v0.1.0.md` non-suffixed. | **RESOLVED**: Standardized on SPEC suffixes for all sub-records. | **RESOLVED** |
| **C-02: Record Names** | SPEC vs gel.commit_batch.v0.1.0.md | SPEC uses `GoldenEngramRef_v0_1_0` vs `GoldenEngramRef`. | **RESOLVED**: Standardized on versioned names. | **RESOLVED** |
| **C-03: Detail Level** | SPEC vs iutt.reconstruction_profile.v0.1.0.md | SPEC provides collapsed record; MD provides full nested sub-schemas. | **RESOLVED**: Registry references full nested sub-schemas for audit authority. | **RESOLVED** |
| **C-04: Record Naming** | SPEC vs sli.session_mounts.v0.1.0.md | SPEC uses `MountConstraint_v0_1_0` vs `MountConstraint`. | **RESOLVED**: Standardized on versioned names. | **RESOLVED** |

#### C) Change Log
- 2026-02-20: Consolidation of 29 files. Conflict Report generated. [Antigravity]
"