using OAN.Core.Sli;

namespace AgentiCore.Services;

public sealed class SliExecutionService
{
    private readonly ISliBridge _sliBridge;

    public SliExecutionService(ISliBridge sliBridge)
    {
        _sliBridge = sliBridge;
    }

    public async Task<string> ExecuteAsync(
        string cmeId,
        Guid contextId,
        IReadOnlyCollection<string> activeConcepts,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);

        var concepts = string.Join(" ", activeConcepts.Select(c => c.ToLowerInvariant()));
        var packet = $"(packet :env runtime :frame agenticore :mode emit :op cognition-cycle :cme {cmeId.ToLowerInvariant()} :context {contextId:D} :concepts ({concepts}))";
        return await _sliBridge.SendPacketAsync(packet, cancellationToken).ConfigureAwait(false);
    }
}
