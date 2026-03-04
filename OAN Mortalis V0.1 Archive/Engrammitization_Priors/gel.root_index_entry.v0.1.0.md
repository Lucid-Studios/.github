12) gel.root_index_entry.v0.1.0
Root Index Entry (Layer 0, Bootstrap/Index Data Unit)
Purpose

Defines a canonical entry type for RootIndex-like structures (RootIndex, SymbolicIndex, SuffixIndex). These may be:

stored as bootstrap Golden Engrams (payloads),

and/or updated incrementally via gel.gel_index_delta.v0.1.0.

This schema is the atomic unit inside those index payloads.

Schema ID

gel.root_index_entry.v0.1.0

Record
public record GelRootIndexEntry_v0_1_0(
    string Schema,                      // "gel.root_index_entry.v0.1.0"

    // The root/symbol being indexed (structural)
    string Namespace,                   // e.g. "root", "symbol", "suffix"
    string KeyText,                     // canonical key (ASCII subset recommended)
    string KeyHash,                     // sha256(KeyText UTF-8)

    // Target identity anchors
    string[] EngramIds,                 // sorted list of EngramIds associated with this key (add-only in v0.1.0)

    // Optional metadata (structural only)
    string[] Tags,                      // sorted; e.g. "BOOTSTRAP", "ROOT"
    string EntryHash,                   // sha256(canonicalBytes(without EntryHash))
    string CanonicalSeal                // sha256(full canonical serialization)
);
Freeze Rules

KeyText must be canonicalized under the same constraints as sli.symbol_pointer.SymbolText (recommended ASCII subset).

EngramIds is add-only in v0.1.0.

No runtime fields, no salience, no heuristics.

Required Tests

RIE-1: EntryHash stable.

RIE-2: EngramIds sorted; duplicates rejected.

RIE-3: KeyHash must match KeyText exactly (recompute + compare).