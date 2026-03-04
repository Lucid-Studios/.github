9) sli.telemetry_record.v0.1.0
Unified Telemetry Record (Layer 1, NDJSON-Friendly)
Purpose

Defines a unified telemetry line format for:

SLI gate events

Commit intents

GEL tip advances

Index deltas

Runtime warnings (Layer-1 safe only)

This is intended for deterministic NDJSON writing (manual builder) and audit replay.

Schema ID

sli.telemetry_record.v0.1.0

Record
public record SliTelemetryRecord_v0_1_0(
    string Schema,                         // "sli.telemetry_record.v0.1.0"

    // Deterministic event identity
    string EventType,                      // "GATE_EVAL" | "GATE_DECISION" | "COMMIT_INTENT" | "TIP_ADVANCE" | "INDEX_DELTA"
    string EventHash,                      // sha256(canonicalBytes(without EventHash))
    string CanonicalSeal,                  // sha256(full canonical serialization)

    // Deterministic ordering
    long GenesisTick,
    long EventSequence,                    // monotonic per session (deterministic if derived from observed event ordering); else local-only

    // Provenance references (hashes, not raw objects)
    string PacketHash,
    string GateDecisionHash,
    string GateEvidenceHash,
    string CommitIntentHash,
    string BraidIndexHash,
    string PreviousTip,
    string NewTip,
    string DeltaHash,

    // High-value fields (audit)
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

Telemetry must remain Layer-1 safe: no Layer-2 salience/gradients.

EventType is closed vocabulary in v0.1.0.

Fields not applicable to an EventType must be "null" (literal) or empty arrays, per canonicalization contract.

Required Tests

TEL-1: Deterministic serialization order (manual NDJSON builder).

TEL-2: EventHash stable for same record.

TEL-3: Forbidden substrings scan (salience, gradient, relevance, queryContext, perspective, basin).