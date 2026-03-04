# Build Log: FormationContext v1 & Deterministic Routing
**Date**: 2026-02-15
**Goal**: Finalize FormationContext DTO and enforce explicit routing precedence.

## Summary
Refactored `FormationContext` (AgentiCore) to be a self-contained, deterministic DTO that drives Engram formation without leaking logic into `CommitIntent`.

## Architectural Changes
1.  **FormationContext v1**:
    *   **Anchors**: `Tick`, `SessionId`, `OperatorId`, `RootId`, `OpalRootId`, `PolicyVersion`.
    *   **Stance**: `KnowingMode`, `MetabolicRegime`, `ResolutionMode` (Hashed categorical enums).
    *   **Routing Flags**: `Speculative`, `RoleBound`, `SharedEligible`, `IdentityLocal`.
    *   **Refs**: `EvidenceRefs` (List of `EngramRef`), `ParentEngramIds`.
    *   **Construction**: `ConstructionTier`.
    *   **Null Safety**: `ParentEngramIds`, `Spans`, `EvidenceRefs` are backed by private fields and normalize `null` inputs to `Array.Empty<T>()` in the constructor to prevent hashing ambiguity.
2.  **Explicit Routing Precedence**:
    *   **SharedEligible** (True) -> `SharedGEL`
    *   **RoleBound** (True) -> `OAN`
    *   **Speculative** (True) -> `GOA`
    *   **Default** -> `SelfGEL`
3.  **EngramRef Definition**:
    *   Added `public record EngramRef(string TargetId, string Relationship)` to `Oan.Core.Engrams`.
    *   Serialized as `"{Relationship}:{TargetId}"` in `EngramBlock.Refs`.

## Verification Results
- **Determinism**: `EngramMvpTests.Routing_Precedence_Shared_Trumps_Role_Trumps_Speculative` confirms precedence rules are respected.
- **Formation**: `EngramFormationService` correctly populates Header and Factors from the new Context.
- **Compilation**: API and CLI updated to use new `FormationContext` structure.

## Next Steps
- Implement `EngramQueryService` for retrieval.
- Begin Compass Scalar Math integration (deferred).
