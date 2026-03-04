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