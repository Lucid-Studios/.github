using CradleTek.Host.Interfaces;
using OAN.Core.Services;

namespace CradleTek.Host;

public sealed class CradleTekHost : IOanService
{
    private readonly IMantleService _mantleOfSovereignty;
    private readonly ICrypticStore _crypticLayer;
    private readonly IPublicStore _publicLayer;
    private readonly IRuntimeService _runtimeLayer;

    public CradleTekHost(
        IMantleService mantleOfSovereignty,
        ICrypticStore crypticLayer,
        IPublicStore publicLayer,
        IRuntimeService runtimeLayer)
    {
        _mantleOfSovereignty = mantleOfSovereignty;
        _crypticLayer = crypticLayer;
        _publicLayer = publicLayer;
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
        "CradleTek.RuntimeLayer.OAN"
    ];

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _mantleOfSovereignty.InitializeAsync(cancellationToken).ConfigureAwait(false);
        await _crypticLayer.InitializeAsync(cancellationToken).ConfigureAwait(false);
        await _publicLayer.InitializeAsync(cancellationToken).ConfigureAwait(false);
        await _runtimeLayer.InitializeAsync(cancellationToken).ConfigureAwait(false);
    }
}
