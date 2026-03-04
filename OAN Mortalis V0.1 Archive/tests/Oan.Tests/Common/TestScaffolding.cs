using System;
using System.Collections.Generic;
using Oan.Core;
using Oan.Core.Governance;
using Oan.SoulFrame;
using Oan.SoulFrame.SLI;
using Oan.Runtime;
using Oan.Ledger;

namespace Oan.Tests.Common
{
    public class TestContext
    {
        public WorldState World { get; init; }
        public EventLog Ledger { get; init; }
        public SoulFrameSession Session { get; init; }
        public IntentProcessor Processor { get; init; }
        public SliGateService Sli { get; init; }

        public TestContext(WorldState world, EventLog ledger, SoulFrameSession session, IntentProcessor processor, SliGateService sli)
        {
            World = world;
            Ledger = ledger;
            Session = session;
            Processor = processor;
            Sli = sli;
        }
    }

    public static class TestScaffolding
    {
        public static TestContext CreatePrimeSession(string sessionId = "test-session", string operatorId = "tester", string[] roster = null)
        {
            var world = new WorldState();
            var ledger = new EventLog();
            var session = new SoulFrameSession(sessionId, operatorId);
            
            if (roster != null)
            {
                foreach (var agent in roster)
                {
                    session.AddToRoster(agent);
                    world.AddEntity(new Entity(agent, "Agent"));
                }
            }

            // Always add OAN mount (required for transitions)
            session.Mounts.TryAddMount(new MountEntry
            {
                Address = new SliAddress(SliChannel.Public, SliPartition.OAN, SliMirror.Standard),
                MountId = "scaffold-mount",
                PolicyVersion = "v1",
                SatCeiling = SatMode.Standard,
                RequiresHitlForElevation = false,
                CreatedTick = 0
            });

            var sli = new SliGateService();
            var processor = new IntentProcessor(world, session, ledger, sli);

            // 1. Transition to Prime
            var transIntent = new Intent
            {
                 SourceAgentId = "system", // Bootstrap source
                 AgentProfileId = "system",
                 Action = "TheaterTransition",
                 SliHandle = "sys/admin/theater.transition",
                 Parameters = new Dictionary<string, object> 
                 { 
                     { "TargetMode", "Prime" },
                     { "Reason", "TestScaffolding" },
                     { "RunId", "test-init" }
                 }
            };

            var transResult = processor.Process(transIntent);
            if (transResult.Status != IntentStatus.Committed)
            {
                throw new InvalidOperationException($"Scaffolding failed to transition to Prime: {transResult.ReasonCode}");
            }

            // 2. Promote Formation
            // Note: IntentProcessor checks formation promotion logic.
            // We need a separate intent.
            var promIntent = new Intent
            {
                 SourceAgentId = "system",
                 AgentProfileId = "system",
                 Action = "FormationPromote",
                 SliHandle = "sys/admin/formation.promote",
                 Parameters = new Dictionary<string, object> 
                 { 
                     { "Reason", "TestScaffolding" }
                 }
            };
            
            var promResult = processor.Process(promIntent);
             if (promResult.Status != IntentStatus.Committed)
            {
                throw new InvalidOperationException($"Scaffolding failed to promote formation: {promResult.ReasonCode}");
            }

            return new TestContext(world, ledger, session, processor, sli);
        }
    }
}
