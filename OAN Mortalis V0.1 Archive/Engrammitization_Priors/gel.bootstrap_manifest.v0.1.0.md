11) gel.bootstrap_manifest.v0.1.0
GEL Bootstrap Manifest (Layer 0, Deterministic Genesis Set)
Purpose

Defines the deterministic genesis set: the first committed Golden Engrams that establish the boot lattice (RootAtlas, RootIndex, SymbolicIndex, SuffixIndex, BaseSymbolCodex, etc.).

This removes ambiguity from “first 7 Golden Engrams” by making it an explicit, hashable, reproducible manifest.

Schema ID

gel.bootstrap_manifest.v0.1.0

Record
public record GelBootstrapManifest_v0_1_0(
    string Schema,                      // "gel.bootstrap_manifest.v0.1.0"

    // Manifest identity
    string BootstrapVersion,            // "v0.1.0"
    string BootstrapHash,               // sha256(canonicalBytes(without BootstrapHash))
    string CanonicalSeal,               // sha256(full canonical serialization)

    // Deterministic genesis anchors
    string GenesisParentTip,            // fixed constant tip value for boot (e.g. "GENESIS")
    long GenesisTick,                   // fixed genesis tick (e.g. 0)
    string PolicyVersion,               // "POLICY-0.1.0"

    // What to bootstrap (ordered, deterministic)
    BootstrapItem[] Items               // sorted by Ordinal then Handle
);

public record BootstrapItem(
    int Ordinal,                        // explicit order: 1..N
    string Handle,                      // e.g. "engram.bootstrap.rootatlas"
    string SourceKind,                  // "embedded_resource"|"file"|"compiled_asset"
    string SourceRef,                   // e.g. resource name or file relative path
    string TargetResolvedAddressText,   // "Private/GEL/Standard" recommended for bootstrap
    string ExpectedPayloadHash,         // sha256(canonical bytes of source payload)
    string ExpectedEngramId             // optional: if you want full determinism lock (recommended for strict builds)
);
Freeze Rules

Bootstrap is the only time you may “seed” GEL from static assets without prior engrams.

Ordinal is the canonical boot order; must not change without MAJOR bump.

ExpectedPayloadHash must match exactly or bootstrap fails closed.

If ExpectedEngramId is provided, it must match exactly or bootstrap fails closed.

Required Tests

BSM-1: BootstrapHash stable.

BSM-2: Payload hash mismatch → hard fail (no partial boot).

BSM-3: Deterministic replay produces identical EngramIds (if ExpectedEngramId locked).