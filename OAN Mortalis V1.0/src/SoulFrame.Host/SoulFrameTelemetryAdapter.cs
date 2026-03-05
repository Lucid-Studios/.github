using System.Security.Cryptography;
using System.Text;
using OAN.Core.Telemetry;
using Telemetry.GEL;

namespace SoulFrame.Host;

public enum SoulFrameTelemetryEventType
{
    InferenceRequested,
    InferenceCompleted,
    InferenceRefused,
    ConstraintViolation,
    DriftDetected
}

public sealed class SoulFrameTelemetryAdapter
{
    private readonly GelTelemetryAdapter _telemetry;

    public SoulFrameTelemetryAdapter(GelTelemetryAdapter telemetry)
    {
        _telemetry = telemetry;
    }

    public Task EmitAsync(
        SoulFrameTelemetryEventType eventType,
        Guid soulFrameId,
        Guid contextId,
        string detail,
        CancellationToken cancellationToken = default)
    {
        var payload = $"{eventType}|{soulFrameId:D}|{contextId:D}|{detail}";
        ITelemetryEvent telemetryEvent = new SoulFrameHostTelemetryEvent
        {
            EventHash = HashHex(payload),
            Timestamp = DateTime.UtcNow
        };

        return _telemetry.AppendAsync(
            telemetryEvent,
            $"soulframe-host:{eventType.ToString().ToLowerInvariant()}",
            cancellationToken);
    }

    private static string HashHex(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private sealed class SoulFrameHostTelemetryEvent : ITelemetryEvent
    {
        public required string EventHash { get; init; }
        public required DateTime Timestamp { get; init; }
    }
}

