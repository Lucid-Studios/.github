using System.Collections.Generic;

namespace Oan.Core.Engrams
{
    public enum EngramChannel
    {
        SelfGEL,
        GOA,
        OAN,
        SharedGEL
    }

    public enum ArchiveTier
    {
        GEL,
        GOA,
        CGEL
    }

    public enum EngramFactorKind
    {
        FrameLock,
        MeaningSpanConfirmed,
        MeaningSpanEdited,
        Constraint,
        Assumption,
        CommitRef,
        OutcomeRef,
        TelemetrySummary,
        KnowingMode,
        MetabolicRegime,
        ResolutionMode
    }

    public enum ConstructionTier
    {
        Root,
        RootBase,
        Basic,
        Intermediate,
        Advanced,
        RootCap,
        Master,
        SupraMaster
    }

    public enum KnowingMode
    {
        Propositional,
        Procedural,
        Perspectival,
        Participatory
    }

    public enum MetabolicRegime
    {
        Exploration,
        Coherence,
        Hold
    }

    public enum ResolutionMode
    {
        Coarse,
        Normal,
        Fine
    }

    public enum FactorTier
    {
        RootBase,
        Basic,
        Intermediate,
        Advanced,
        RootCap
    }

    public record EngramFactor
    {
        public required FactorTier Tier { get; init; }
        public required EngramFactorKind Kind { get; init; }
        public required string Key { get; init; }
        public required string Value { get; init; }
        public required int Order { get; init; }
        public decimal Weight { get; init; } = 1.0m;
    }

    public record EngramRef
    {
        public required string TargetId { get; init; }
        public string Relationship { get; init; } = "Link";
        
        public override string ToString() => $"{Relationship}:{TargetId}";
    }

    public record EngramBlockHeader
    {
        public string CanonicalVersion { get; init; } = "1";
        public required string PolicyVersion { get; init; }
        public required long Tick { get; init; }
        public required string SessionId { get; init; }
        public required string OperatorId { get; init; }
        public string? AgentProfileId { get; init; }
        public string? TheaterId { get; init; }     // Added for IUTT Phase 1
        public string? TheaterMode { get; init; }   // Added for IUTT Phase 1
        
        public string? CradleId { get; init; }      // Added for IUTT Phase 2
        public string? ContextId { get; init; }     // Added for IUTT Phase 2
        public string? FormationLevel { get; init; } // Added for IUTT Phase 2
        
        public ArchiveTier ArchiveTier { get; init; } // Added for IUTT Phase 3B

        public required EngramChannel Channel { get; init; }
        public required string RoutingReason { get; init; }
        public required string RootId { get; init; }
        public required ConstructionTier ConstructionTier { get; init; }
        public IReadOnlyList<string>? ParentEngramIds { get; init; }
        public required string OpalRootId { get; init; }
        public string? PreviousOpalEngramId { get; init; }

        // Phase 4C: ThetaSeal (Paid Record Rails)
        public string? UptakePlanId { get; set; }
        public string? ResidueSetId { get; set; }
        public bool IsThetaSealed { get; set; }
    }

    public record EngramBlock
    {
        public required EngramBlockHeader Header { get; init; }
        public required IReadOnlyList<EngramFactor> Factors { get; init; }
        public required IReadOnlyList<string> Refs { get; init; }
        
        public required string Hash { get; init; }
        
        // Derived from Hash of Canonical Payload. NOT included in canonical payload itself.
        public required string EngramId { get; init; }
    }
}
