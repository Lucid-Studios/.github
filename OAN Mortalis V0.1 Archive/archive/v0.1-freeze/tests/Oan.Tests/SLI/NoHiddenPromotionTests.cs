using System;
using System.Collections.Generic;
using Oan.Core;
using Oan.Core.Governance;
using Oan.SoulFrame;
using Oan.SoulFrame.SLI;
using Oan.Runtime;
using Oan.Ledger;
using Xunit;

namespace Oan.Tests.SLI
{
    public class NoHiddenPromotionTests
    {
        [Fact]
        public void Resolve_Must_Not_Mutate_MountRegistry()
        {
            var session = new SoulFrameSession("s1", "op1");
            var gate = new SliGateService(new NullSliTelemetrySink());
            var intent = new Intent
            {
                Id = Guid.NewGuid(),
                AgentProfileId = "a1",
                SourceAgentId = "sa1",
                Action = "MoveTo",
                SliHandle = "public/oan/move.commit"
            };

            // Call resolve multiple times
            gate.Resolve(intent, session, SatMode.Baseline, 0);
            gate.Resolve(intent, session, SatMode.Baseline, 1);
            gate.Resolve(intent, session, SatMode.Baseline, 2);

            // Verify registry remains empty
            Assert.Empty(session.Mounts.GetActiveMounts());
        }

        [Fact]
        public void MountRegistry_Only_Changes_Via_Explicit_Mount_Handle()
        {
            var world = new WorldState();
            var session = new SoulFrameSession("s1", "op1");
            session.AddToRoster("a1");
            var ledger = new EventLog();
            var gate = new SliGateService(new NullSliTelemetrySink());
            var processor = new IntentProcessor(world, session, ledger, gate);

            // Try to mount via a DIFFERENT handle (not sys/admin/mount.commit)
            // Even if the action name is "MountCapability", if the handle doesn't match the special logic, it shouldn't work.
            var fakeIntent = new Intent
            {
                Id = Guid.NewGuid(),
                AgentProfileId = "a1",
                SourceAgentId = "sa1",
                Action = "MountCapability",
                SliHandle = "public/oan/move.commit", // Standard handle
                Parameters = new Dictionary<string, object>
                {
                    { "Channel", "Public" },
                    { "Partition", "OAN" },
                    { "Mirror", "Standard" }
                }
            };

            processor.CommitIntent(fakeIntent);
            
            // Verify registry still empty
            Assert.Empty(session.Mounts.GetActiveMounts());
        }
    }
}
