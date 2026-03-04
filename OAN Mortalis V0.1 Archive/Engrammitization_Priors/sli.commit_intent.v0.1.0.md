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