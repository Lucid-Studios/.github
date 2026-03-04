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