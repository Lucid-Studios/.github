15) sli.policy_bundle.v0.1.0
Policy Bundle (Layer 1, Single Root of Truth for Governance Pinning)
Purpose

Defines the single pinning artifact that locks:

RootAtlas contents

handle registry

session mount policy

telemetry emission policy

reconstruction profile pins (Layer-2 profile hash, but pinned here)

This is the “policy root” that the host loads to ensure the entire stack is consistent.

Schema ID

sli.policy_bundle.v0.1.0

Record
public record SliPolicyBundle_v0_1_0(
    string Schema,                         // "sli.policy_bundle.v0.1.0"

    // Bundle identity
    string BundleVersion,                  // "v0.1.0"
    string PolicyVersion,                  // "POLICY-0.1.0"
    string BundleHash,                     // sha256(canonicalBytes(without BundleHash))
    string CanonicalSeal,                  // sha256(full canonical serialization)

    // Pinned components (hash references)
    string RootAtlasHash,                  // container hash for root atlas
    string HandleRegistryHash,             // sli.handle_registry_manifest.RegistryHash
    string MountPolicyVersion,
    string TelemetryManifestHash,          // sli.telemetry_stream_manifest.ManifestHash

    // Runtime cognition pinning (Layer 2 pinned by hash, not stored as identity)
    string ReconstructionProfileVersion,
    string ReconstructionProfileHash,

    // Bootstrap pins (optional but recommended)
    string BootstrapVersion,
    string BootstrapHash,

    // Host discipline (optional, but useful)
    string[] RequiredModules,              // sorted list: e.g. ["SLI_GATE", "GEL_SPINE", "CANON_BUILDER"]
    string[] ForbiddenModules              // sorted list: e.g. ["HEURISTIC_HANDLE_INFERENCE"]
);
Freeze Rules

Host must refuse to start if any pinned hash mismatches the loaded artifact.

ReconstructionProfileHash is pinned here but remains Layer-2 runtime spec.

BundleHash must be stable across machines.

Required Tests

PB-1: BundleHash stable.

PB-2: If any pinned component hash differs → fail closed at boot.

PB-3: Bundle pins must be consistent: RootAtlasHash must include exactly the handles in HandleRegistryHash.