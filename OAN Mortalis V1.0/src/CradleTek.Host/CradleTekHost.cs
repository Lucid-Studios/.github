using OAN.Core.Identity;
using OAN.Core.Services;

namespace CradleTek.Host;

public sealed class CradleTekHost : IOanService
{
    private readonly ISoulFrameContext _soulFrameContext;
    private readonly IOanService _agentRuntime;

    public CradleTekHost(ISoulFrameContext soulFrameContext, IOanService agentRuntime)
    {
        _soulFrameContext = soulFrameContext;
        _agentRuntime = agentRuntime;
    }

    public string ServiceId => "cradletek.host";

    public Guid ActiveIdentityId => _soulFrameContext.IdentityId;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _agentRuntime.InitializeAsync(cancellationToken).ConfigureAwait(false);
    }
}
