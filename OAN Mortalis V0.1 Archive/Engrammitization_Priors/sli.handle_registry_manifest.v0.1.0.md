14) sli.handle_registry_manifest.v0.1.0
Handle Registry Manifest (Layer 1, Explicit Handle Set Pinning)
Purpose

Pins the exact list of valid handles and their hashes for a given policy bundle, preventing:

silent handle addition/removal

“shadow handles”

drift between RootAtlas and what operators think exists

This is a deterministic “handle allowlist” that complements RootAtlas.

Schema ID

sli.handle_registry_manifest.v0.1.0

Record
public record SliHandleRegistryManifest_v0_1_0(
    string Schema,                         // "sli.handle_registry_manifest.v0.1.0"

    // Manifest identity
    string RegistryVersion,                // "v0.1.0"
    string PolicyVersion,                  // "POLICY-0.1.0"
    string RegistryHash,                   // sha256(canonicalBytes(without RegistryHash))
    string CanonicalSeal,                  // sha256(full canonical serialization)

    // What handles exist
    HandleEntry[] Handles                  // sorted by Handle
);

public record HandleEntry(
    string Handle,
    string EntryHash,                      // sli.root_atlas_entry.EntryHash
    string IntentKind,
    string AllowedAddressConstraintText,   // e.g. "Private/GEL/Standard" or "Any/Any/Any"
    string[] RequiredSatModes              // sorted
);
Freeze Rules

Each HandleEntry must correspond 1:1 with a RootAtlasEntry.

Registry is the explicit pin list; RootAtlas is the full rule record.

Handle lookups must require:

handle exists in registry, and

handle exists in RootAtlas.

Required Tests

HR-1: RegistryHash stable.

HR-2: Any RootAtlas entry not in registry causes boot/policy validation failure.

HR-3: HandleEntry.EntryHash must match the RootAtlasEntry hash exactly.