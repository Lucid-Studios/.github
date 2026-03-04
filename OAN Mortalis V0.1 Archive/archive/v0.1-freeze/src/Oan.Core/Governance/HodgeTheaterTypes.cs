using System;

namespace Oan.Core.Governance
{
    public sealed record HodgeTheaterSeed
    {
        public required string TheaterId { get; init; }
        public required string RunId { get; init; }
        public required long GenesisTick { get; init; }
        public required string RootAtlasHash { get; init; }
        public required string TheaterPolicyVersion { get; init; }
        public required string EntropyRegime { get; init; }
        public required string InitialSatMode { get; init; }
        public required string MountRulesHash { get; init; }
    }
}
