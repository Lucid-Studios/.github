using System;

namespace Oan.Core.Governance
{
    public enum SliChannel
    {
        Public,
        Private
    }

    public enum SliPartition
    {
        GEL,
        GOA,
        OAN
    }

    public enum SliMirror
    {
        Standard,
        Cryptic
    }

    public enum SatMode
    {
        Baseline,
        Gate,
        Standard,
        Stronger
    }

    public record struct SliAddress(SliChannel Channel, SliPartition Partition, SliMirror Mirror)
    {
        public override string ToString() => $"{Channel}/{Partition}/{Mirror}";
    }

    public record SliResolutionResult
    {
        public bool Allowed { get; init; }
        public required string ReasonCode { get; init; }
        public required string PolicyVersion { get; init; }
        public string? Handle { get; init; }
        public SliAddress ResolvedAddress { get; init; }
        public SatMode SatModeAtDecision { get; init; }
        public bool MaskingApplied { get; init; }
    }
}
