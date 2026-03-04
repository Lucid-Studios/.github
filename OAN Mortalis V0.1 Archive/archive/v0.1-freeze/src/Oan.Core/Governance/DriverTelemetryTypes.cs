using System;
using Oan.Core.Ingestion;
using Oan.Core.Governance;

namespace Oan.Core.Governance
{
    public record DriverIngestionEvent
    {
        public required string RunId { get; init; }
        public required long Tick { get; init; }
        public required int Attempt { get; init; }
        public required IngestionOutcome Outcome { get; init; }
        public required string[] MissingFields { get; init; }
        public string? ReasonCode { get; init; }
        public RawDescriptor? Raw { get; init; }
    }

    public record DriverSliEvent
    {
        public required SliTelemetryRecord Record { get; init; }
    }

    public record DriverCommitEvent
    {
        public required string RunId { get; init; }
        public required long Tick { get; init; }
        public required Guid IntentId { get; init; }
        public required string Result { get; init; } // Committed | Denied | Error
        public string? ReasonCode { get; init; }
    }

    public record DriverSatElevationRequestEvent
    {
        public required string RunId { get; init; }
        public required long Tick { get; init; }
        public required string SessionId { get; init; }
        public required string RequestedMode { get; init; }
        public required string TargetAddress { get; init; }
        public string? Reason { get; init; }
    }

    public record DriverSatElevationOutcomeEvent
    {
        public required string RunId { get; init; }
        public required long Tick { get; init; }
        public required string Result { get; init; } // Granted | Denied | HitlRequired
        public string? ReasonCode { get; init; }
        public string? ResultingMode { get; init; }
    }
}
