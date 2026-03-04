namespace OAN.Core.Telemetry;

public interface ITelemetryEvent
{
    string EventHash { get; }
    DateTime Timestamp { get; }
}
