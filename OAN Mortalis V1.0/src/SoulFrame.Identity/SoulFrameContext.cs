using OAN.Core.Identity;

namespace SoulFrame.Identity;

public sealed class SoulFrameContext : ISoulFrameContext
{
    public SoulFrameContext(Guid identityId)
    {
        IdentityId = identityId;
        State = new Dictionary<string, object>(StringComparer.Ordinal);
    }

    public Guid IdentityId { get; }

    public IDictionary<string, object> State { get; }
}
