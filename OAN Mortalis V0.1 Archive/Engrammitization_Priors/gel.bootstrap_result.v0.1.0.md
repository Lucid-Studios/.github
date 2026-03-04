13) gel.bootstrap_result.v0.1.0
Bootstrap Result (Layer 0, Deterministic Proof of Genesis)
Purpose

Captures the outcome of applying gel.bootstrap_manifest.v0.1.0:

which Golden Engrams were written

what the final tip became

proof hashes linking boot inputs → outputs

This enables:

deterministic verification

“boot attestation” for audits

regression checks across environments

Schema ID

gel.bootstrap_result.v0.1.0

Record
public record GelBootstrapResult_v0_1_0(
    string Schema,                         // "gel.bootstrap_result.v0.1.0"

    // Link back to manifest
    string BootstrapHash,                  // gel.bootstrap_manifest.BootstrapHash
    string BootstrapVersion,               // "v0.1.0"
    string PolicyVersion,                  // "POLICY-0.1.0"

    // Genesis anchors
    string GenesisParentTip,
    long GenesisTick,

    // Outcome
    string FinalTip,
    long BootstrapSequence,                // local monotonic; optional for cross-substrate comparisons
    BootstrapWrite[] Writes,               // sorted by Ordinal then Handle

    // Proof
    string ResultHash,                     // sha256(canonicalBytes(without ResultHash))
    string CanonicalSeal                   // sha256(full canonical serialization)
);

public record BootstrapWrite(
    int Ordinal,
    string Handle,
    string ResolvedAddressText,
    string PayloadHash,                    // sha256(source payload canonical bytes)
    string EngramId,                       // resulting gel.golden_engram.EngramId
    string EngramSeal                      // resulting gel.golden_engram.CanonicalSeal
);
Freeze Rules

Every manifest item must produce exactly one BootstrapWrite.

PayloadHash must match the manifest’s ExpectedPayloadHash; mismatch → boot fails and result is not emitted.

Writes must be sorted (Ordinal, Handle).

Required Tests

BR-1: ResultHash stable given identical writes.

BR-2: Every EngramId in Writes must exist in GEL at FinalTip lineage.

BR-3: Manifest and Result hashes match expected chain: manifest → writes → final tip.