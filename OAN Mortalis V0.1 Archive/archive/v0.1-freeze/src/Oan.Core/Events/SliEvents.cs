using Oan.Core.Governance;

namespace Oan.Core.Events
{
    public record SliDecisionEvent
    {
        public long Tick { get; init; }
        public required string SessionId { get; init; }
        public required string OperatorId { get; init; }
        public string? Handle { get; init; }
        public SliAddress ResolvedAddress { get; init; }
        public bool Allowed { get; init; }
        public required string ReasonCode { get; init; }
        public required string PolicyVersion { get; init; }
        public SatMode SatMode { get; init; }
        public bool MaskingApplied { get; init; }
    }
}
