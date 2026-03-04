# Build Log: Vervaeke Stance Integration
**Date**: 2026-02-15
**Goal**: Integrate Vervaeke-aligned stance metadata into Engram formation as hashed categorical factors.

## Summary
Extended Engram Kernel to support participation/perspective metadata without altering the CommitIntent execution path or introducing scalar math.

## Architectural Changes
1.  **Core Enums**:
    *   `KnowingMode`: Propositional, Procedural, Perspectival, Participatory. (4P Cognitive Science)
    *   `MetabolicRegime`: Exploration, Coherence, Hold. (Opponent Processing)
    *   `ResolutionMode`: Coarse, Normal, Fine. (Relevance Realization)
2.  **Engram Formation**:
    *   Injects Stance Factors at `Tier: RootBase`.
    *   `KnowingMode` (Order 10), `MetabolicRegime` (Order 20), `ResolutionMode` (Order 30).
    *   Strict canonical sorting ensures these factors stabilize the hash.
3.  **Engram Store**:
    *   Now stores `(EngramBlock, byte[])` tuples.
    *   Enforces strict byte-level idempotency to detect even subtle canonicalization drifts.

## Verification Results
- **Hashing**: Changing any stance mode alters the SHA-256 hash. `KnowingMode_ChangesHash` passed.
- **Canonical Payload**: Stance factors are confirmed present in the canonical serialization stream.
- **Integrity**: `EngramStore` successfully rejects conflicting byte payloads for the same ID.

## Known Invariants Preserved
- **No Scalar Math**: Stances are categorical only. Compass math is deferred.
- **Append-Only**: Mutation remains impossible.
- **Execution Safety**: Stances influence memory formation (Engrams) but do NOT alter `CommitIntent` logic directly.
