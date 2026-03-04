using System.Collections.Generic;

namespace Oan.Core.Events
{
    public sealed record EngrammitizedEvent
    {
        public required string TheaterId { get; init; }
        public string? ParentTip { get; init; }
        public required string NewTip { get; init; }
        public required string NormalFormKey { get; init; }
        public Dictionary<string, string>? Factors { get; init; } // Added for Phase 6R Round-trip
        public required IReadOnlyList<string> WitnessEventIds { get; init; }
        public required long Tick { get; init; }
    }
}
