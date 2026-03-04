17) gel.quarantine_event.v0.1.0
GEL Quarantine Event (Layer 0, Safe-Fail State Transition)
Purpose

Records safe-fail transitions in the GEL host/authority state machine (e.g., Operational → Frozen → Quarantined → Halt).

This corresponds to your safe-fail enforcement posture (freeze blocks evaluation, allows telemetry; quarantine isolates). It is a state ledger for governance outcomes.

Schema ID

gel.quarantine_event.v0.1.0

Record
public record GelQuarantineEvent_v0_1_0(
    string Schema,                         // "gel.quarantine_event.v0.1.0"

    // Event identity
    string EventHash,                      // sha256(canonicalBytes(without EventHash))
    string CanonicalSeal,                  // sha256(full canonical serialization)

    // State transition
    string FromState,                      // "Operational" | "Frozen" | "Quarantined" | "Halt"
    string ToState,                        // same closed vocab
    string TransitionReasonCode,           // closed vocab: "SAF_ACTIVE"|"POLICY_VIOLATION"|...

    // Optional provenance
    string RelatedPacketHash,              // if triggered by a packet
    string RelatedGateDecisionHash,
    string RelatedCommitIntentHash,

    // Host provenance
    string HostInstanceId,                 // stable id for this CradleTek/GEL host instance
    string PolicyVersion,

    // Deterministic sequencing (local)
    long TransitionSequence,               // monotonic local
    long GenesisTick
);
Freeze Rules

State vocab is closed.

Must not include wall-clock.

Must not include Layer-2 runtime state.

Emitted on every state change.

Required Tests

QE-1: Transition emits exactly one event per state change.

QE-2: Invalid transitions rejected (e.g., Halt → Operational not allowed in v0.1.0).

QE-3: EventHash stable.