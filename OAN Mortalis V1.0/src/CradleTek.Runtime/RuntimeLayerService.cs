using AgentiCore.Models;
using AgentiCore.Services;
using CradleTek.Host.Interfaces;
using SoulFrame.Identity.Services;

namespace CradleTek.Runtime;

public sealed class RuntimeLayerService : IRuntimeService
{
    private readonly SoulFrameRegistry _soulFrameRegistry;
    private readonly AgentiCore.Services.AgentiCore _agentiCore;
    private readonly Dictionary<Guid, AgentiContext> _activeContexts = [];

    public RuntimeLayerService(
        SoulFrameRegistry soulFrameRegistry,
        AgentiCore.Services.AgentiCore agentiCore)
    {
        _soulFrameRegistry = soulFrameRegistry;
        _agentiCore = agentiCore;
    }

    public string ContainerName => "RuntimeLayer";
    public string OanService => "OAN";

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task ActivateSoulFrameAsync(SoulFrame.Identity.Models.SoulFrame soulFrame, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(soulFrame);

        // Runtime attaches cognition orchestration only for registered SoulFrames.
        var registered = _soulFrameRegistry.GetBySoulFrameId(soulFrame.SoulFrameId);
        if (registered is null)
        {
            throw new InvalidOperationException($"SoulFrame '{soulFrame.SoulFrameId:D}' is not registered.");
        }

        var context = _agentiCore.InitializeContext(registered);
        _activeContexts[registered.SoulFrameId] = context;
        return Task.CompletedTask;
    }

    public async Task RunCycleAsync(CancellationToken cancellationToken = default)
    {
        foreach (var context in _activeContexts.Values.OrderBy(c => c.CMEId, StringComparer.Ordinal).ToList())
        {
            var result = await _agentiCore.ExecuteCognitionCycleAsync(context, cancellationToken).ConfigureAwait(false);
            await _agentiCore.ProcessCognitionResult(context, result, cancellationToken).ConfigureAwait(false);
        }
    }
}
