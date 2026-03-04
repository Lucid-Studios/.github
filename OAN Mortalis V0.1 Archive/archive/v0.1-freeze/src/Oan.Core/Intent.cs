using System;
using System.Collections.Generic;

namespace Oan.Core
{
    public enum IntentStatus
    {
        Pending,
        Committed,
        Refused,
        Failed
    }

    public class Intent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string SourceAgentId { get; set; }
        public required string AgentProfileId { get; set; } // The specific profile context
        public string? TargetAgentId { get; set; } // Optional
        public required string Action { get; set; } // e.g., "Speak", "Move", "Attack"
        public string? SliHandle { get; set; } // Required for SLI Gate
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }

    public class IntentResult
    {
        public Guid IntentId { get; set; }
        public IntentStatus Status { get; set; }
        public required string ReasonCode { get; set; } // e.g., "BUDGET_EXCEEDED", "ALIGNMENT_VIOLATION"
        public string? PolicyVersion { get; set; } // SAT/Kernel version applied
        public Dictionary<string, object> StateDelta { get; set; } = new Dictionary<string, object>();
    }
}
