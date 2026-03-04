using System.Collections.Generic;
using Oan.Core.Governance;

namespace Oan.SoulFrame.Atlas
{
    public record SliHandleEntry
    {
        public required string Handle { get; init; }
        public required string IntentKind { get; init; } // Maps to Intent.Action or similar (existing code uses Action)
        public required SliAddress Address { get; init; }
        public required IReadOnlySet<SatMode> RequiredSatModes { get; init; }
        public bool RequiresHitl { get; init; }
        public required string PolicyVersion { get; init; }
    }
}
