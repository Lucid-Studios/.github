using System;
using System.Threading.Tasks;
using Oan.AgentiCore;
using Oan.Common;
using Oan.SoulFrame;
using Oan.Spinal;
using Oan.Sli;

namespace Oan.Cradle
{
    /// <summary>
    /// Represents the collective stack for a specific agent execution.
    /// </summary>
    public sealed class AgentRuntime
    {
        public string AgentId { get; }
        public string TheaterId { get; }
        public SoulFrameAuthority SoulFrame { get; }
        public AgentIdentity AgentiCore { get; }
        public IRoutingEngine Router { get; }

        public AgentRuntime(
            string agentId,
            string theaterId,
            SoulFrameAuthority soulFrame,
            AgentIdentity agentiCore,
            IRoutingEngine router)
        {
            AgentId = agentId;
            TheaterId = theaterId;
            SoulFrame = soulFrame;
            AgentiCore = agentiCore;
            Router = router;
        }

        public async Task<EvaluateEnvelope> EvaluateAsync(object input)
        {
            // 1. Compute deterministic hash for the intent
            // In a real system, AgentiCore would produce this.
            string intentHash = Primitives.ComputeHash(Primitives.ToCanonicalJson(input));

            // 2. Authoritative Routing (Standard plane append)
            await Router.AppendAsync("Standard", intentHash, input);

            // 3. Symbolic Duplexing (Consult Cryptic Plane)
            var query = new DuplexQuery
            {
                QueryHash = intentHash,
                InvokingHandle = "EvaluateAsync",
                StandardTipHash = intentHash // Binding search state to this intent commit
            };

            var duplexResponse = await Router.ExecuteDuplexAsync(query, input);

            return new EvaluateEnvelope
            {
                AgentId = AgentId,
                TheaterId = TheaterId,
                Decision = "OK",
                Note = $"Evaluated + Duplexed to Cryptic (ResultHash: {duplexResponse.CrypticResultHash.Substring(0, 8)})",
                DuplexResponseHash = duplexResponse.ComputeId()
            };
        }
    }
}
