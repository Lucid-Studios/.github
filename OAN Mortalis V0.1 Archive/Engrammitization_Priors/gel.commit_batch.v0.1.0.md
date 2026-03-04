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

    // Batch identity (deterministic)
    string BatchId,                        // sha256(canonicalBytes(batch without BatchId))
    string CanonicalSeal,                  // sha256(full canonical serialization)

    // Lineage and atomicity
    string ParentTip,
    string NewTip,
    long BatchSequence,                    // monotonic local; if cross-substrate nondeterministic, mark as local-only

    // Provenance (hash references)
    string CommitIntentHash,               // sli.commit_intent.CommitIntentHash
    string GateDecisionHash,               // sli.gate_decision.DecisionHash
    string GateEvidenceHash,               // sli.gate_evidence.EvidenceHash
    string PolicyVersion,

    // Braiding
    string BraidIndexHash,                 // hash of finalized gel.braid_index
    string IntakeHash,                     // optional: if all engrams share same intake; else "mixed"

    // The actual written objects (identity bearing)
    GoldenEngramRef[] Engrams,             // sorted by EngramId

    // Deterministic session provenance
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick
);

public record GoldenEngramRef(
    string EngramId,                       // gel.golden_engram.EngramId
    string Handle,                         // admitted handle used
    string ResolvedAddressText,            // "Channel/Partition/Mirror"
    string CanonicalSeal                   // copied seal of the engram for quick verification
);
Freeze Rules

Engrams[] must be sorted by EngramId.

BatchId must be computed deterministically from canonical bytes.

Batch must reflect exactly one CAS tip advancement.

No Layer-2 data. No salience. No runtime context.

Required Tests

CB-1: BatchId stable for same inputs.

CB-2: Batch’s NewTip must match gel.tip_advance_event.NewTip for the same commit.

CB-3: Batch must contain at least 1 engram (no empty commits).