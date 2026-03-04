using Oan.SoulFrame;

namespace Oan.AgentiCore
{
    /// <summary>
    /// Core identity and memory management for an agent.
    /// </summary>
    public sealed class AgentIdentity
    {
        private readonly SoulFrameAuthority _authority;

        public AgentIdentity(SoulFrameAuthority authority)
        {
            _authority = authority ?? throw new System.ArgumentNullException(nameof(authority));
        }
    }
}
