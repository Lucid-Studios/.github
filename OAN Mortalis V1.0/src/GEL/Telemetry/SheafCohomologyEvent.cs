using OAN.Core.Telemetry;

namespace GEL.Telemetry;

public sealed class SheafCohomologyEvent : ITelemetryEvent
{
    public required string DomainName { get; init; }
    public required SheafCohomologyState State { get; init; }
    public required string EventHash { get; init; }
    public required DateTime Timestamp { get; init; }
}
