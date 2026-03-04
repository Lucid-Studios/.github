using System;
using Oan.Core.Governance; // For IdentityScope? No, that's in Oan.Core.Engrams usually or Governance... check usage.
// IdentityScope is likely in Oan.Core.Engrams or Oan.Core.Meaning?
// Previous context showed IdentityScopeMatrix in Oan.Core.Engrams?
// Let's check imports.
// IdentityScope is in Oan.Core.Engrams namespace based on previous prompts context (IdentityScopeMatrix).
// Let's assume namespaces.

namespace Oan.Core.Engrams
{
    public sealed class CleaveRecord
    {
        public required string CleaveId { get; init; }
        public string? SourceEngramId { get; init; }
        public required string ResultEngramId { get; init; }

        public required ArchiveTier SourceTier { get; init; }
        public required ArchiveTier TargetTier { get; init; }

        public required string SourceFormationLevel { get; init; } 
        public required string TargetFormationLevel { get; init; }

        public required string SourceTheaterMode { get; init; }
        public required string TargetTheaterMode { get; init; }

        public required IdentityScope ScopeUsed { get; init; }

        public required bool IsCrossCradle { get; init; }
        public required bool IsPromotion { get; init; }
    }
}
