10) sli.telemetry_stream_manifest.v0.1.0
Telemetry Stream Manifest (Layer 1, Deterministic Emission Policy)
Purpose

Defines what telemetry events are emitted, to which sink(s), at what detail level, using a deterministic policy. This prevents “ad-hoc logging” from becoming a hidden control surface.

Schema ID

sli.telemetry_stream_manifest.v0.1.0

Record
public record SliTelemetryStreamManifest_v0_1_0(
    string Schema,                      // "sli.telemetry_stream_manifest.v0.1.0"

    // Manifest identity
    string ManifestVersion,             // "v0.1.0"
    string ManifestHash,                // sha256(canonicalBytes(without ManifestHash))
    string CanonicalSeal,               // sha256(full canonical serialization)

    // Policy linkage
    string PolicyVersion,               // e.g. "POLICY-0.1.0"
    string MountPolicyVersion,          // e.g. "MOUNT-0.1.0"

    // Emission rules
    StreamRule[] Rules,                 // sorted by EventType then SinkId

    // Default sinks
    SinkSpec[] Sinks                    // sorted by SinkId
);

public record StreamRule(
    string EventType,                   // closed vocab: "GATE_EVAL"|"GATE_DECISION"|"COMMIT_INTENT"|"TIP_ADVANCE"|"INDEX_DELTA"
    string SinkId,                      // must exist in Sinks
    int DetailLevel,                    // 0..N (frozen interpretation)
    bool Enabled,                       // true/false
    string SamplingMode,                // "all"|"none" (v0.1.0); future may add "rate"
    string Notes                        // optional; excluded from hashing if you want strict determinism
);

public record SinkSpec(
    string SinkId,                      // e.g. "telemetry.ndjson", "telemetry.spine"
    string SinkKind,                    // "ndjson_file"|"memory_queue"|"duplex_stream"
    Dictionary<string,string> Parameters // sorted keys; structural only (paths, buffer sizes, etc.)
);
Freeze Rules

Rules must be deterministic and sorted.

SamplingMode is closed vocabulary for v0.1.0 (all|none).

Sinks are configuration, but must be canonicalizable (sorted keys, no env-dependent paths unless explicitly allowed).

Required Tests

TSM-1: ManifestHash stable across machines (given same parameters).

TSM-2: Unknown EventType must be rejected.

TSM-3: No forbidden substrings in rules/params (salience, gradient, relevance, queryContext, etc.).