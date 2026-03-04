using OAN.Core.Telemetry;

namespace Telemetry.GEL;

public sealed record GelTelemetryRecord(string EventHash, DateTime Timestamp, string RuntimeState);

public sealed class GelTelemetryAdapter
{
    private readonly List<GelTelemetryRecord> _records = new();

    public IReadOnlyList<GelTelemetryRecord> Records => _records;

    public Task AppendAsync(ITelemetryEvent telemetryEvent, string runtimeState, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(telemetryEvent);
        ArgumentException.ThrowIfNullOrWhiteSpace(runtimeState);

        _records.Add(new GelTelemetryRecord(telemetryEvent.EventHash, telemetryEvent.Timestamp, runtimeState));
        return Task.CompletedTask;
    }
}
