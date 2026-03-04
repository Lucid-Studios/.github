namespace OAN.Core.Identity;

public interface ISoulFrameContext
{
    Guid IdentityId { get; }
    IDictionary<string, object> State { get; }
}
