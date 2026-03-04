2) sli.session_mounts.v0.1.0
Session Mount Table (Layer 1, Deterministic Governance Surface)
Purpose

Defines what address spaces are mounted (allowed to be accessed) for a session, independent of handle capability. Gate evaluation must check mounts after handle existence and before commit execution. 

SLI CONSTITUTION v0_1

Schema ID

sli.session_mounts.v0.1.0

Record
public record SliSessionMounts_v0_1_0(
    string Schema,                   // "sli.session_mounts.v0.1.0"
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick,

    // Mount policy versioning
    string MountPolicyVersion,       // e.g. "MOUNT-0.1.0"

    // What is mounted (allowlist)
    SliResolvedAddress_v0_1_0[] MountedAddresses, // sorted by AddressText

    // Optional: per-address constraints
    MountConstraint[] Constraints,    // sorted by AddressText; may be empty

    // Deterministic digest
    string MountsHash,               // sha256(canonicalBytes(without MountsHash))
    string CanonicalSeal             // sha256(full canonical serialization)
);

public record MountConstraint(
    string AddressText,              // must match MountedAddresses.AddressText
    string[] RequiredSatModes,       // sorted; additional SAT requirements beyond RootAtlas
    bool RequiresHITL,               // additional HITL requirement
    bool ReadOnly,                   // if true, disallow CommitIntent/ExecuteIntent to this address
    string Notes                     // optional; not used for gating decisions if you want purity—if so, exclude from hash
);
Freeze Rules

Mounts are allowlist: if not mounted → deny.

MountedAddresses must be sorted by AddressText.

Constraints apply only to addresses that exist in MountedAddresses.

If ReadOnly == true, gate must deny any packet whose OpCode implies a write (CommitIntent) targeting that address.

Tests

MNT-1: Unmounted address → deny with reason MOUNT_DENY.

MNT-2: Mounted address but SAT insufficient per constraint → deny MOUNT_SAT_FAIL.

MNT-3: ReadOnly mount denies CommitIntent but allows Propose/NoOp.