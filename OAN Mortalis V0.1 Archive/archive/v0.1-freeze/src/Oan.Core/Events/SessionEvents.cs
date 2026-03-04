using System.Collections.Generic;

namespace Oan.Core.Events
{
    public class SessionQuiescedEvent
    {
        public required string SoulFrameSessionId { get; set; }
        public long WorldTick { get; set; }
        public int PolicyVersion { get; set; }
        public required string Provenance { get; set; } // Operator/Request
    }

    public class SessionSealedEvent
    {
        public required string SoulFrameSessionId { get; set; }
        public long WorldTick { get; set; }
        public int PolicyVersion { get; set; }
        public required string FinalWorldStateHash { get; set; }
        public required string FinalSessionStateHash { get; set; }
        public string? ActiveAgentProfileId { get; set; }
        public Dictionary<string, double> TelemetrySummary { get; set; } = new Dictionary<string, double>();
        public required string Provenance { get; set; }
    }

    public class SessionFoldedEvent
    {
        public required string SoulFrameSessionId { get; set; }
        public long WorldTick { get; set; }
        public int PolicyVersion { get; set; }
        public string? AgentiCoreProfileId { get; set; }
        public required string FoldMode { get; set; } // "Ready", "Holding", etc.
        public required string Provenance { get; set; }
    }

    public class SoulFrameClearedEvent
    {
        public required string SoulFrameSessionId { get; set; }
        public long WorldTick { get; set; }
        public int PolicyVersion { get; set; }
        public required string Provenance { get; set; }
    }
}
