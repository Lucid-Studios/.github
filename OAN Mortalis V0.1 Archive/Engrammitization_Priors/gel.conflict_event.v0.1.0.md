16) gel.conflict_event.v0.1.0
GEL Conflict Event (Layer 0, Deterministic Audit of TIP/Lineage Conflicts)
Purpose

Records a deterministic audit event when GEL rejects an attempted write due to:

TIP_CONFLICT (observed ParentTip ≠ current tip)

parent mismatch in append-only spine

batch replay conflicts

This is emitted instead of a tip advance event (since no advancement occurred).

Schema ID

gel.conflict_event.v0.1.0

Record
public record GelConflictEvent_v0_1_0(
    string Schema,                         // "gel.conflict_event.v0.1.0"

    // Conflict identity
    string ConflictHash,                   // sha256(canonicalBytes(without ConflictHash))
    string CanonicalSeal,                  // sha256(full canonical serialization)

    // Conflict type
    string ConflictType,                   // "TIP_CONFLICT" | "PARENT_MISMATCH" | "REPLAY_CONFLICT"
    string ReasonCode,                     // deterministic detail code (closed vocab)

    // What was expected vs observed
    string ExpectedParentTip,              // from commit intent
    string ObservedCurrentTip,             // from GEL at attempt time
    string ProposedNewTip,                 // optional if computed

    // Provenance references
    string CommitIntentHash,               // sli.commit_intent.CommitIntentHash
    string GateDecisionHash,               // sli.gate_decision.DecisionHash
    string BraidIndexHash,                 // attempted braid hash (if formed)
    string[] IntendedEngramIds,            // sorted; may be empty if conflict occurred pre-formation

    // Session provenance
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick,

    // Deterministic sequencing (local)
    long AttemptSequence                   // monotonic local counter; optional for cross-substrate equivalence
);
Freeze Rules

Emitted only when no write occurs.

Must not include wall-clock time.

ConflictType and ReasonCode are closed vocabulary for v0.1.0.

IntendedEngramIds may be empty if conflict is detected before engram formation; if present, must be sorted.

Required Tests

CE-1: TIP conflict emits ConflictEvent and does NOT emit TipAdvanceEvent.

CE-2: ConflictHash stable given identical fields.

CE-3: Closed-vocab enforcement for ConflictType/ReasonCode.