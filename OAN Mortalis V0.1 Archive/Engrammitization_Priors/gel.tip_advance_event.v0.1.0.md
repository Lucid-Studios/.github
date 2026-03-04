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