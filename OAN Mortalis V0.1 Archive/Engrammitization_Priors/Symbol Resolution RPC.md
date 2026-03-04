20) Symbol Resolution RPC
20a) sli.symbol_resolution_request.v0.1.0
20b) sli.symbol_resolution_response.v0.1.0

These formalize exact pointer resolution as an RPC pair.

20a) sli.symbol_resolution_request.v0.1.0
Purpose

Requests resolution of a canonical symbol (or symbol pointer) to a stable target reference.

Schema ID

sli.symbol_resolution_request.v0.1.0

Record
public record SliSymbolResolutionRequest_v0_1_0(
    string Schema,                         // "sli.symbol_resolution_request.v0.1.0"

    // Request identity
    string RequestHash,                    // sha256(canonicalBytes(without RequestHash))
    string CanonicalSeal,                  // sha256(full canonical serialization)

    // What to resolve
    string Namespace,                      // e.g. "sli" | "bootstrap" | "gel"
    string SymbolText,                     // canonical symbol text (must already be canonicalized)
    string PointerHashHint,                // optional: expected sli.symbol_pointer.PointerHash or "null"

    // Resolution context (structural only; no query semantics)
    string ResolutionMode,                 // "EXACT" (v0.1.0 only)
    string[] AllowedPointerKinds,          // sorted: "engram_id"|"bootstrap_handle"|"resolved_address"|"external_ref"

    // Provenance
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick
);
Freeze Rules

ResolutionMode is EXACT only in v0.1.0.

No “fuzzy” or “best match” flags.

SymbolText must already satisfy the symbol canonicalization contract (ASCII subset recommended).

Required Tests

SRQ-1: RequestHash stable.

SRQ-2: Non-canonical SymbolText rejected (or canonicalized before hashing, but pick one approach and freeze it).

20b) sli.symbol_resolution_response.v0.1.0
Purpose

Returns the resolution result deterministically (or NotFound) with audit-friendly fields.

Schema ID

sli.symbol_resolution_response.v0.1.0

Record
public record SliSymbolResolutionResponse_v0_1_0(
    string Schema,                         // "sli.symbol_resolution_response.v0.1.0"

    // Response identity
    string ResponseHash,                   // sha256(canonicalBytes(without ResponseHash))
    string CanonicalSeal,                  // sha256(full canonical serialization)

    // Request linkage
    string RequestHash,
    string Namespace,
    string SymbolText,
    string PointerHashResolved,            // sli.symbol_pointer.PointerHash or "null"

    // Outcome
    bool IsFound,
    string NotFoundReason,                 // required if !IsFound; closed vocab: "NOT_FOUND"|"NAMESPACE_MISSING"|"KIND_NOT_ALLOWED"
    string PointerKind,                    // "engram_id"|"bootstrap_handle"|"resolved_address"|"external_ref" or "null"
    SymbolTargetResolved Target,           // see below (nulls if !IsFound)

    // Proof (optional but recommended)
    string IndexTip,                       // GEL tip at time of resolution (structural)
    string IndexHash,                      // e.g. latest SymbolIndex snapshot hash (if you maintain one)

    // Provenance
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick
);

public record SymbolTargetResolved(
    string TargetRef,                      // e.g. "sha256:<hex>" or "engram.bootstrap.rootatlas"
    string ResolvedAddressText,            // "Channel/Partition/Mirror" or "null"
    string TargetSchemaHint,               // e.g. "gel.golden_engram.v0.1.0" or "null"
    string[] TargetTags                    // sorted; structural only
);
Freeze Rules

If IsFound == false, NotFoundReason must be set and Target.* must be "null"/empty.

If IsFound == true, PointerKind and Target.TargetRef must be non-null.

No heuristics in response; it must reflect exact lookup.

Required Tests

SRR-1: ResponseHash stable for same inputs.

SRR-2: NotFound paths deterministic (same missing symbol → same NotFoundReason).