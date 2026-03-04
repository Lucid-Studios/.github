using System;
using Oan.AgentiCore;
using Oan.Common;
using Oan.SoulFrame;
using Oan.Sli;

namespace Oan.Cradle
{
    /// <summary>
    /// Orchestrator for constructing the agent stack.
    /// </summary>
    public sealed class StackManager
    {
        private readonly StoreRegistry _stores;

        public StackManager(StoreRegistry stores)
        {
            _stores = stores ?? throw new ArgumentNullException(nameof(stores));
        }

        public AgentRuntime CreateStack(string agentId, string theaterId)
        {
            // 1. Create SoulFrame first
            var soulFrame = new SoulFrameAuthority(_stores.GovernanceTelemetry);

            // 2. Create Deterministic Harness (Engine gateway)
            var harness = new DeterministicHarness(_stores.Cryptic);

            // 3. Create Routing Engine (SLI layer)
            var router = new RoutingEngine(soulFrame, _stores.Public, _stores.Cryptic, harness);

            // 3. Create AgentiCore with SoulFrame authority reference
            var agentiCore = new AgentIdentity(soulFrame);

            // 4. Produce AgentRuntime with injected router
            return new AgentRuntime(agentId, theaterId, soulFrame, agentiCore, router);
        }
    }
}
