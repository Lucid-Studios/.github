using OAN.Core.Telemetry;

namespace AgentiCore.Services;

internal sealed class AgentiTelemetryEvent : ITelemetryEvent
{
    public required string EventHash { get; init; }
    public required DateTime Timestamp { get; init; }
}
