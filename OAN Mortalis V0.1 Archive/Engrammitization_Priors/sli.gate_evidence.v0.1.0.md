4) sli.gate_evidence.v0.1.0
Gate Evidence Payload (Layer 1, Deterministic, Structured)
Purpose

Provides a structured and deterministic evidence record for gate evaluation, replacing “freeform evidence strings” with a canonical object whose hash can be referenced by:

sli.gate_decision.v0.1.0.EvidenceSnapshotHash

telemetry streams

audits

This object contains only structural gating facts, never runtime semantics.

Schema ID

sli.gate_evidence.v0.1.0

Record
public record SliGateEvidence_v0_1_0(
    string Schema,                         // "sli.gate_evidence.v0.1.0"

    // Provenance (hash references only)
    string PacketHash,                     // sli.packet.PacketHash
    string SessionMountsHash,              // sli.session_mounts.MountsHash
    string RootAtlasHash,                  // hash of root atlas container or entry set
    string PolicyVersion,

    // Gate inputs observed (structural only)
    string SatModeObserved,
    bool SafActiveObserved,
    string RequestedEnvChannel,            // from packet.Env.Channel (declared)
    string RequestedEnvPartition,
    string RequestedEnvMirror,

    // Handle evaluation results
    HandleEvidence[] Handles,              // sorted by Handle

    // Cryptic evidence
    bool CrypticAttempted,
    bool MaskingApplied,
    string MaskingPolicyId,                // "none" if not applicable

    // Final decision summary (mirrors gate_decision)
    bool IsAllowed,
    string ReasonCode,                     // required if denied

    // Deterministic digests
    string EvidenceHash,                   // sha256(canonicalBytes(without EvidenceHash))
    string CanonicalSeal                   // sha256(full canonical serialization)
);

public record HandleEvidence(
    string Handle,
    bool ExistsInRootAtlas,
    string EntryHash,                      // "null" if missing
    string AllowedAddressText,             // from atlas constraints normalized: "Channel/Partition/Mirror" or "Any/Any/Any"
    string ResolvedAddressText,            // final resolved address if admissible else "null"
    bool MountAllowed,
    bool SatSatisfied,
    bool HitlSatisfied,
    string FailureReason                   // "none" or a deterministic code: "HANDLE_NOT_FOUND"|"MOUNT_DENY"|...
);
Freeze Rules

Handles must list every declared handle from the packet (even missing ones), in sorted order.

FailureReason must be a closed vocabulary (enum-like), not prose.

No wall-clock, no user text, no query context.

If IsAllowed == false, ReasonCode must be set.

Required Tests

EVD-1: EvidenceHash stable across machines.

EVD-2: Missing handle yields ExistsInRootAtlas=false, deterministic FailureReason="HANDLE_NOT_FOUND".

EVD-3: No forbidden substrings: salience, relevance, gradient, queryContext, perspective, basin.