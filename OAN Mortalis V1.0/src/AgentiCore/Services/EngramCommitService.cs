using CradleTek.Host.Interfaces;
using OAN.Core.Telemetry;
using Telemetry.GEL;

namespace AgentiCore.Services;

public sealed class EngramCommitService
{
    private readonly IPublicStore _publicStore;
    private readonly ICrypticStore _crypticStore;
    private readonly GelTelemetryAdapter _telemetry;

    public EngramCommitService(
        IPublicStore publicStore,
        ICrypticStore crypticStore,
        GelTelemetryAdapter telemetry)
    {
        _publicStore = publicStore;
        _crypticStore = crypticStore;
        _telemetry = telemetry;
    }

    public async Task CommitAsync(
        Guid contextId,
        string payload,
        bool crypticClassification,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        var engramNodeId = $"engram-node:{contextId:D}";
        var relationPointer = $"engram-link:{contextId:D}:operationalizes";

        if (crypticClassification)
        {
            await _crypticStore.StorePointerAsync(engramNodeId, cancellationToken).ConfigureAwait(false);
            await _crypticStore.StorePointerAsync(relationPointer, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await _publicStore.PublishPointerAsync(engramNodeId, cancellationToken).ConfigureAwait(false);
            await _publicStore.PublishPointerAsync(relationPointer, cancellationToken).ConfigureAwait(false);
        }

        var telemetryEvent = new AgentiTelemetryEvent
        {
            EventHash = ComputeHash($"{contextId:D}|commit|{crypticClassification}|{payload}"),
            Timestamp = DateTime.UtcNow
        };
        await _telemetry.AppendAsync(telemetryEvent, "engram-commit").ConfigureAwait(false);
    }

    private static string ComputeHash(string value)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
