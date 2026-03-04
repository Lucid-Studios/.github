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
    public class SliGateMountTests
    {
        [Fact]
        public void Valid_Handle_But_Unmounted_Denies_Mount_Not_Present()
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

            var result = gate.Resolve(intent, session, SatMode.Baseline, 0);

            Assert.False(result.Allowed);
            Assert.Equal("MOUNT_NOT_PRESENT", result.ReasonCode);
        }

        [Fact]
        public void Action_Allowed_After_Mount_Commit()
        {
            var world = new WorldState();
            var session = new SoulFrameSession("s1", "op1");
            session.AddToRoster("a1");
            var ledger = new EventLog();
            var gate = new SliGateService(new NullSliTelemetrySink());
            var processor = new IntentProcessor(world, session, ledger, gate);
            world.AddEntity(new Entity("sa1", "Agent"));
            session.Apply(new Oan.Core.Events.SatElevationOutcomeEvent { 
                RunId = "setup", Tick = 0, SessionId = session.SessionId, 
                Result = Oan.Core.Events.SatElevationResult.Granted, OutcomeCode = "SETUP", 
                ResultingMode = SatMode.Stronger 
            }); // Needed for mount handle
            processor.ActivateAgent("a1", "Setup");
            session.SetTheaterMode(TheaterMode.Prime, "test-theater");

            // 1. Initial attempt fails
            var intent = new Intent
            {
                Id = Guid.NewGuid(),
                AgentProfileId = "a1",
                SourceAgentId = "sa1",
                Action = "MoveTo",
                SliHandle = "public/oan/move.commit"
            };
            var res1 = processor.EvaluateIntent(intent);
            Assert.Equal("MOUNT_NOT_PRESENT", res1.ReasonCode);

            // 2. Commit mount
            var mountIntent = new Intent
            {
                Id = Guid.NewGuid(),
                AgentProfileId = "a1",
                SourceAgentId = "sa1",
                Action = "MountCapability",
                SliHandle = "sys/admin/mount.commit",
                Parameters = new Dictionary<string, object>
                {
                    { "Channel", "Public" },
                    { "Partition", "OAN" },
                    { "Mirror", "Standard" },
                    { "SatCeiling", "Standard" },
                    { "RequiresHitl", false }
                }
            };
            var resMount = processor.CommitIntent(mountIntent);
            Assert.Equal("MOUNT_SUCCESS", resMount.ReasonCode);

            // 3. Retry action -> Success
            var res2 = processor.EvaluateIntent(intent);
            Assert.Equal("ADMISSIBLE", res2.ReasonCode);
        }
    }
}
