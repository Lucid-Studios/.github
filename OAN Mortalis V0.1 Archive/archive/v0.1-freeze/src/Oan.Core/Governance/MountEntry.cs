using System;

namespace Oan.Core.Governance
{
    /// <summary>
    /// Represents an explicit, deterministic mount of an SLI capability surface.
    /// Invariant A: MountId must be SHA256(runId + canonical_address + policy).
    /// </summary>
    public record MountEntry
    {
        public required SliAddress Address { get; init; }
        public required string MountId { get; init; }
        public required string PolicyVersion { get; init; }
        public required SatMode SatCeiling { get; init; }
        public required bool RequiresHitlForElevation { get; init; }
        public required long CreatedTick { get; init; }

        /// <summary>
        /// Deterministically computes the canonical address string.
        /// Format: "{channel}:{partition}:{mirror}" (no whitespace).
        /// </summary>
        public static string GetCanonicalAddressString(SliAddress address)
        {
            return $"{address.Channel.ToString().ToLowerInvariant()}:{address.Partition.ToString().ToLowerInvariant()}:{address.Mirror.ToString().ToLowerInvariant()}";
        }
    }
}
