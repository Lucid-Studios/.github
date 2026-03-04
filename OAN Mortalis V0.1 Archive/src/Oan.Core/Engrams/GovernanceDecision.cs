using System;

namespace Oan.Core.Engrams
{
    public enum GovernanceIssuerKind { AgentCme, ApiKernel, System }
    public enum GovernanceVerdict { Allow, Deny }
    public enum GovernanceOpKind
    {
        BindAttempt, 
        Promotion, 
        TheaterTransition,
        CrossCradleGlue,
        ThetaSeal, 
        UptakePlanAttach, 
        ResidueCast,
        CrypticRead, 
        CrypticWrite,
        AdminMount, 
        AdminCommit
    }

    public sealed record GovernanceRequest
    {
        public required GovernanceOpKind Kind { get; init; }

        public required string SessionId { get; init; }
        public required string ScenarioName { get; init; }
        public required string OperatorId { get; init; }
        public required long Tick { get; init; }

        public required string SourceCradleId { get; init; }
        public required string SourceContextId { get; init; }
        public required string SourceTheaterId { get; init; }
        public required string SourceTheaterMode { get; init; }
        public required string SourceFormationLevel { get; init; }
        public required ArchiveTier SourceArchiveTier { get; init; }

        public required string TargetCradleId { get; init; }
        public required string TargetContextId { get; init; }
        public required string TargetTheaterId { get; init; }
        public required string TargetTheaterMode { get; init; }
        public required string TargetFormationLevel { get; init; }
        public required ArchiveTier TargetArchiveTier { get; init; }

        public bool IsBindingAttempt { get; init; }
        public bool IsPromotion { get; init; }
        public bool IsCrossCradle { get; init; }

        public string? MorphismId { get; init; }
        public required string PolicyVersion { get; init; }
        public string? CrypticHandshakeFingerprint { get; init; }
        public string? SgelFingerprint { get; init; }
        public string? UptakePlanId { get; init; }
        public string? ResidueSetId { get; init; }
        public string? ThetaCandidateEngramId { get; init; }
    }

    public sealed record GovernanceDecision
    {
        public required string DecisionId { get; init; }
        public required GovernanceIssuerKind IssuerKind { get; init; }
        public required string IssuerId { get; init; }
        public required GovernanceVerdict Verdict { get; init; }
        public required string ReasonCode { get; init; }

        public IdentityScope? RequiredScope { get; init; }
        public ArchiveTier? RequiredTier { get; init; }
        public int? MaxTransportCost { get; init; }
        public int? RequiredWitnessLevel { get; init; }

        public required string PolicyVersion { get; init; }
        public required string PolicyFingerprint { get; init; }
        public required string RequestFingerprint { get; init; }

        public required long Tick { get; init; }
    }
}
