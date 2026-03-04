21) gel.mirror_snapshot.v0.1.0
Mirror Snapshot Proof (Layer 0, Deterministic Snapshot Attestation)
Purpose

Defines a deterministic snapshot artifact that a remote mirror can request to verify it is consistent with a given GEL tip and indices, without streaming the entire ledger.

This is the “proof-of-state” object for mirror synchronization.

Schema ID

gel.mirror_snapshot.v0.1.0

Record
public record GelMirrorSnapshot_v0_1_0(
    string Schema,                         // "gel.mirror_snapshot.v0.1.0"

    // Snapshot identity
    string SnapshotHash,                   // sha256(canonicalBytes(without SnapshotHash))
    string CanonicalSeal,                  // sha256(full canonical serialization)

    // What state is being attested
    string Tip,                            // current GEL tip
    long SnapshotSequence,                 // local monotonic; optional cross-substrate

    // Spine proofs (lightweight)
    string[] RecentTipEventHashes,         // sorted or fixed window order (freeze one rule)
    string CommitBatchHash,                // hash of most recent gel.commit_batch (optional)
    string IndexDeltaHash,                 // hash of most recent gel.gel_index_delta (optional)

    // Index state proofs (optional but recommended)
    string SymbolIndexSnapshotHash,        // if you maintain periodic snapshots
    string HandleIndexSnapshotHash,
    string IntakeIndexSnapshotHash,

    // Bootstrap pins
    string BootstrapHash,
    string PolicyBundleHash,

    // Host provenance
    string HostInstanceId,
    string PolicyVersion,

    // Session provenance (if snapshot is requested within a session)
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick
);
Freeze Rules

Snapshot must contain enough pins to verify:

the mirror is on the same policy bundle

the mirror agrees on tip lineage (via recent event hashes)

index snapshot hashes match (if used)

No wall-clock time.

No Layer-2 runtime state.

Required Tests

MS-1: SnapshotHash stable for identical snapshot content.

MS-2: Snapshot verification fails closed if PolicyBundleHash mismatches.

MS-3: If Tip differs, mirror must request missing gel.tip_advance_event range (or equivalent) rather than “guess.”