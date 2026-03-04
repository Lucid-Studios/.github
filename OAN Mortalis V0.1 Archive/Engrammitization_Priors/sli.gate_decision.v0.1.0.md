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
    HandleResolution[] Resolutions,       sorted by Handle

     Cryptic masking
    bool MaskingApplied,
    string MaskingPolicyId,               none if not cryptic

     Evidence & audit
    string EvidenceSnapshot,              canonical text form or compact JSON (deterministic)
    string EvidenceSnapshotHash,          sha256(EvidenceSnapshot UTF-8)
    string DecisionHash,                  sha256(canonicalBytes(without DecisionHash))
    string CanonicalSeal                  sha256(full canonical serialization)
);

public record HandleResolution(
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