using System.Collections.Generic;
using System.Linq;
using Oan.Spinal;

namespace Oan.Sli
{
    public record SliDecision(bool Allowed, string ReasonCode);

    public record CapabilitySet(HashSet<string> Capabilities)
    {
        public string ComputeHash()
        {
            var sorted = Capabilities.OrderBy(c => c).ToList();
            return Primitives.ComputeHash(string.Join("|", sorted));
        }
    }

    public enum SliPlane
    {
        Public,
        Cryptic,
        SpineNative
    }

    public static class SliRouter
    {
        public static SliPlane Route(string planeStr)
        {
            return planeStr switch
            {
                "Public" => SliPlane.Public,
                "Cryptic" => SliPlane.Cryptic,
                "SpineNative" => SliPlane.SpineNative,
                _ => SliPlane.Public
            };
        }
    }

    public record CommitAuthority(string Token);
    
    public sealed class SliKernel
    {
        public SliDecision Validate(EngramEnvelope envelope, CommitAuthority authority)
        {
            // SLI never reads raw payload; only validates metadata and authority
            if (string.IsNullOrEmpty(envelope.scopeId)) 
                return new SliDecision(false, "SLI.ERR.SCOPE_MISSING");
                
            if (envelope.tick < 0)
                return new SliDecision(false, "SLI.ERR.INVALID_TICK");

            // Simple authority check for v1.0
            if (authority == null || string.IsNullOrEmpty(authority.Token))
                return new SliDecision(false, "SLI.ERR.UNAUTHORIZED");

            return new SliDecision(true, "SLI.OK.ADMISSIBLE");
        }
    }
}
