using CradleTek.Host.Interfaces;
using CradleTek.Host.Models;

namespace CradleTek.Mantle;

public sealed class MantleOfSovereigntyService : IMantleService
{
    private readonly Dictionary<Guid, OpalEngram> _shadows = [];

    public string ContainerName => "MantleOfSovereignty";

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    // MoS stores shadow copies only; it does not mutate live OpalEngrams.
    public Task ShadowSnapshotAsync(OpalEngram engram, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engram);
        _shadows[engram.IdentityId] = Clone(engram);
        return Task.CompletedTask;
    }

    public Task<OpalEngram?> RestoreLastKnownGoodAsync(Guid identityId, CancellationToken cancellationToken = default)
    {
        if (_shadows.TryGetValue(identityId, out var shadow))
        {
            return Task.FromResult<OpalEngram?>(Clone(shadow));
        }

        return Task.FromResult<OpalEngram?>(null);
    }

    private static OpalEngram Clone(OpalEngram source)
    {
        var cloned = new OpalEngram(source.IdentityId);
        foreach (var block in source.AppendOnlyLedgerBlockChain.cSelfGEL)
        {
            cloned.AppendOnlyLedgerBlockChain.AppendCryptic(block);
        }

        foreach (var block in source.AppendOnlyLedgerBlockChain.SelfGEL)
        {
            cloned.AppendOnlyLedgerBlockChain.AppendPublic(block);
        }

        return cloned;
    }
}
