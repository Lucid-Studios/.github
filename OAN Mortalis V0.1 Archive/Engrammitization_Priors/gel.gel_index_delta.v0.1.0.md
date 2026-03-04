8) gel.gel_index_delta.v0.1.0
GEL Index Delta (Layer 0, Deterministic Index Update Event)
Purpose

Defines the index updates produced by a commit. This is how you keep symbol→EngramId lookup and other indices in sync without re-scanning the entire GEL.

This can be used by:

Lisp duplex mirroring

incremental rebuilds

audit tools

Schema ID

gel.gel_index_delta.v0.1.0

Record
public record GelIndexDelta_v0_1_0(
    string Schema,                         // "gel.gel_index_delta.v0.1.0"

    // Provenance
    string CommitIntentHash,
    string GateDecisionHash,
    string BraidIndexHash,
    string PreviousTip,
    string NewTip,

    // Delta identity
    string DeltaHash,                      // sha256(canonicalBytes(without DeltaHash))
    string CanonicalSeal,                  // sha256(full canonical serialization)

    // What changed
    SymbolIndexUpdate[] SymbolUpdates,     // sorted by SymbolText then EngramId
    HandleIndexUpdate[] HandleUpdates,     // sorted by Handle then EngramId
    IntakeIndexUpdate[] IntakeUpdates,     // sorted by IntakeHash then EngramId

    // Session provenance
    string SessionId,
    string OperatorId,
    string ScenarioName,
    long GenesisTick
);

public record SymbolIndexUpdate(
    string Namespace,                      // e.g. "sli" | "bootstrap" | "gel"
    string SymbolText,                     // canonical symbol
    string PointerHash,                    // sli.symbol_pointer.PointerHash
    string TargetEngramId,                 // gel.golden_engram.EngramId
    string Operation                       // "ADD" (v0.1.0 only; future may support "REMOVE" but GEL is append-only)
);

public record HandleIndexUpdate(
    string Handle,
    string EngramId,
    string Operation                       // "ADD"
);

public record IntakeIndexUpdate(
    string IntakeHash,
    string EngramId,
    string Operation                       // "ADD"
);
Freeze Rules

Deltas are add-only in v0.1.0 (append-only spine). No removes.

SymbolText must already be canonicalized (per symbol pointer contract).

Updates must be deterministic and sorted.

Required Tests

IDX-1: DeltaHash stable for same commit.

IDX-2: Every EngramId in commit batch appears in at least one index update category (handle/intake; symbol optional).

IDX-3: Add-only enforcement (no “REMOVE”).