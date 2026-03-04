using Oan.Core.Governance;

namespace Oan.Core.Events
{
    public sealed record HodgeTheaterSeededEvent
    {
        public required string TheaterId { get; init; }
        public required string RunId { get; init; }
        public required long Tick { get; init; }
        public required HodgeTheaterSeed Seed { get; init; }
    }

    public sealed record TheaterTransitionEvent
    {
        public required string SessionId { get; init; }
        public required string FromMode { get; init; }
        public required string ToMode { get; init; }
        public required string TheaterId { get; init; }
        public required string Reason { get; init; }
        public required long Tick { get; init; }
    }

    public sealed record EphemeralTheaterLogEvent
    {
        public required string TheaterId { get; init; }
        public required string TheaterMode { get; init; }
        public required string NormalFormKey { get; init; }
        public required long Tick { get; init; }
        // Non-binding payload mirroring EngrammitizedEvent
    }

    public sealed record FormationPromotedEvent
    {
        public required string SessionId { get; init; }
        public required string FromContext { get; init; }
        public required string ToContext { get; init; } // If we unify or just keep same ID
        public required string FormationLevel { get; init; }
        public required string Reason { get; init; }
        public required long Tick { get; init; }
    }
}
