using OAN.Core.Services;
using OAN.Core.Sli;

namespace AgentiCore.Runtime;

public sealed class AgentiRuntime : IOanService
{
    private readonly ISliBridge _sliBridge;

    public AgentiRuntime(ISliBridge sliBridge)
    {
        _sliBridge = sliBridge;
    }

    public string ServiceId => "agenticore.runtime";

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    // Runtime can emit SLI expressions but does not interpret symbolic semantics.
    public Task<string> SendIntentPacketAsync(string sliPacket, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sliPacket);
        return _sliBridge.SendPacketAsync(sliPacket, cancellationToken);
    }
}
