using System;
using Oan.Core.Governance;

namespace Oan.Core.Events
{
    public sealed class SatElevationRequestedEvent
    {
        public required string RunId { get; set; }
        public long Tick { get; set; }
        public required string SessionId { get; set; }
        public required SatMode RequestedMode { get; set; }
        public required string TargetAddress { get; set; }
        public required string Reason { get; set; }
    }

    public enum SatElevationResult
    {
        Granted,
        Denied,
        HitlRequired
    }

    public sealed class SatElevationOutcomeEvent
    {
        public required string RunId { get; set; }
        public long Tick { get; set; }
        public required string SessionId { get; set; }
        public required SatElevationResult Result { get; set; }
        public required string OutcomeCode { get; set; }
        public required SatMode ResultingMode { get; set; }
    }
}
