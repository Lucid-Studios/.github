using SoulFrame.Identity.Models;
using SoulFrameModel = SoulFrame.Identity.Models.SoulFrame;

namespace SoulFrame.Identity.Services;

public sealed class SoulFrameRegistry
{
    private readonly Dictionary<Guid, SoulFrameModel> _bySoulFrameId = [];
    private readonly Dictionary<string, Guid> _soulFrameIdByCmeId = new(StringComparer.Ordinal);
    private readonly Dictionary<Guid, SoulFrameState> _stateBySoulFrameId = [];

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Register(SoulFrameModel soulFrame, SoulFrameState state)
    {
        ArgumentNullException.ThrowIfNull(soulFrame);
        ArgumentNullException.ThrowIfNull(state);

        _bySoulFrameId[soulFrame.SoulFrameId] = soulFrame;
        _soulFrameIdByCmeId[soulFrame.CMEId] = soulFrame.SoulFrameId;
        _stateBySoulFrameId[soulFrame.SoulFrameId] = state;
    }

    public SoulFrameModel? GetByCmeId(string cmeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);
        return _soulFrameIdByCmeId.TryGetValue(cmeId, out var soulFrameId)
            && _bySoulFrameId.TryGetValue(soulFrameId, out var soulFrame)
            ? soulFrame
            : null;
    }

    public SoulFrameModel? GetBySoulFrameId(Guid soulFrameId)
    {
        return _bySoulFrameId.TryGetValue(soulFrameId, out var soulFrame) ? soulFrame : null;
    }

    public SoulFrameState? GetState(Guid soulFrameId)
    {
        return _stateBySoulFrameId.TryGetValue(soulFrameId, out var state) ? state : null;
    }

    public IReadOnlyList<SoulFrameModel> ListActive()
    {
        return _bySoulFrameId.Values
            .OrderBy(sf => sf.CMEId, StringComparer.Ordinal)
            .ThenBy(sf => sf.SoulFrameId)
            .ToList();
    }
}
