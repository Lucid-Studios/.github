using System;

namespace Oan.Core.Engrams
{
    public sealed record GlueRequest
    {
        public required string SourceEngramId { get; init; }
        public required string SourceCradleId { get; init; }
        public required ArchiveTier SourceTier { get; init; }

        public required string TargetCradleId { get; init; }
        public required ArchiveTier TargetTier { get; init; }

        public required string TargetTheaterMode { get; init; }          // "Idle", "Prime", etc.
        public required string TargetFormationLevel { get; init; }       // "Constructor", "HigherFormation"

        public required IdentityScope PreferredScope { get; init; }      // Matrix may override
        public required MorphismDescriptor Morphism { get; init; }

        public bool IsOePrivileged { get; init; }                       // for GOA/CGEL access
        public required string OperatorId { get; init; }
        public required string SessionId { get; init; }
        public long Tick { get; init; }
    }

    public sealed record GlueResult
    {
        public required string ResultEngramId { get; init; }
        public required string CleaveId { get; init; }
        public required IdentityScope ScopeUsed { get; init; }
    }
}
