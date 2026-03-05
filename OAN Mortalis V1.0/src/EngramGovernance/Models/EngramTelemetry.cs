using OAN.Core.Telemetry;

namespace EngramGovernance.Models;

public sealed class EngramTelemetry : ITelemetryEvent
{
    public required string EventType { get; init; }
    public required string EngramClass { get; init; }
    public required string SymbolId { get; init; }
    public required string SourceContext { get; init; }
    public required string CognitionTraceId { get; init; }
    public required string EventHash { get; init; }
    public required DateTime Timestamp { get; init; }
}
