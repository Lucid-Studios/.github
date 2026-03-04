using CradleTek.Host.Interfaces;
using CradleTek.Host.Models;
using OAN.Core.Services;
using SoulFrame.Identity.Models;
using SoulFrame.Identity.Services;

namespace CradleTek.Host;

public sealed class CradleTekHost : IOanService
{
    private readonly IMantleService _mantleOfSovereignty;
    private readonly ICrypticStore _crypticLayer;
    private readonly IPublicStore _publicLayer;
    private readonly SoulFrameFactory _soulFrameFactory;
    private readonly SoulFrameRegistry _soulFrameRegistry;
    private readonly IRuntimeService _runtimeLayer;

    public CradleTekHost(
        IMantleService mantleOfSovereignty,
        ICrypticStore crypticLayer,
        IPublicStore publicLayer,
        SoulFrameFactory soulFrameFactory,
        SoulFrameRegistry soulFrameRegistry,
        IRuntimeService runtimeLayer)
    {
        _mantleOfSovereignty = mantleOfSovereignty;
        _crypticLayer = crypticLayer;
        _publicLayer = publicLayer;
        _soulFrameFactory = soulFrameFactory;
        _soulFrameRegistry = soulFrameRegistry;
        _runtimeLayer = runtimeLayer;
    }

    public string ServiceId => "cradletek.host";

    public IReadOnlyList<string> Topology { get; } =
    [
        "CradleTek.MantleOfSovereignty",
        "CradleTek.CrypticLayer.cGEL",
        "CradleTek.CrypticLayer.cGoA",
        "CradleTek.CrypticLayer.CrypticSLI",
        "CradleTek.PublicLayer.GEL",
        "CradleTek.PublicLayer.GoA",
        "CradleTek.PublicLayer.PrimeSLI",
        "CradleTek.SoulFrame.Identity",
        "CradleTek.RuntimeLayer.OAN"
    ];

    public SoulFrameRegistry SoulFrameRegistry => _soulFrameRegistry;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _mantleOfSovereignty.InitializeAsync(cancellationToken).ConfigureAwait(false);
        await _crypticLayer.InitializeAsync(cancellationToken).ConfigureAwait(false);
        await _publicLayer.InitializeAsync(cancellationToken).ConfigureAwait(false);
        await _soulFrameRegistry.InitializeAsync(cancellationToken).ConfigureAwait(false);
        await _runtimeLayer.InitializeAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<SoulFrame.Identity.Models.SoulFrame> CreateSoulFrameAsync(
        string cmeId,
        string opalEngramId,
        string operatorBondReference,
        RuntimePolicy runtimePolicy = RuntimePolicy.Default,
        CancellationToken cancellationToken = default)
    {
        var soulFrame = _soulFrameFactory.Create(cmeId, opalEngramId, operatorBondReference, runtimePolicy);
        var initialState = _soulFrameFactory.CreateInitialState(soulFrame);
        _soulFrameRegistry.Register(soulFrame, initialState);

        var snapshot = new SoulFrameSnapshotRequest(
            soulFrame.SoulFrameId,
            soulFrame.CMEId,
            soulFrame.OpalEngramId,
            DateTime.UtcNow);

        await _mantleOfSovereignty.RequestSoulFrameSnapshotAsync(snapshot, cancellationToken).ConfigureAwait(false);
        await _runtimeLayer.ActivateSoulFrameAsync(soulFrame, cancellationToken).ConfigureAwait(false);

        return soulFrame;
    }
}
