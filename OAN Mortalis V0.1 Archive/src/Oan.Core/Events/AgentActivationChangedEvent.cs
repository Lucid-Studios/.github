using System;

namespace Oan.Core.Events
{
    public class AgentActivationChangedEvent
    {
        public required string SoulFrameSessionId { get; set; }
        public required string OperatorId { get; set; }
        public string? FromAgentProfileId { get; set; } // Nullable
        public string? ToAgentProfileId { get; set; }   // Nullable
        public int PolicyVersion { get; set; }
        public long WorldTick { get; set; }
        public required string Reason { get; set; }
    }
}
