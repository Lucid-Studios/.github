using System;
using System.Collections.Generic;
using System.Linq;
using Oan.Core;
using Oan.Core.Events;
using Oan.Core.Governance;
using Oan.Ledger;
using Oan.Runtime;
using Oan.SoulFrame;
using Oan.SoulFrame.SLI;
using Xunit;

namespace Oan.Tests.Governance
{
    public class SatElevationTests
    {
        [Fact]
        public void Elevation_Evaluation_Is_Pure()
        {
            // Setup
            // Setup
            var ctx = Oan.Tests.Common.TestScaffolding.CreatePrimeSession("s1", "op1", new[] { "agent-1" });
            var session = ctx.Session;
            var processor = ctx.Processor;
            processor.ActivateAgent("agent-1", "test");

            Assert.Equal(SatMode.Baseline, session.CurrentSatMode);

            // Intent for elevation
            var intent = new Intent
            {
                Id = Guid.NewGuid(),
                AgentProfileId = "agent-1",
                SourceAgentId = "agent-1",
                SliHandle = "sys/admin/sat.elevate.request",
                Action = "RequestSatElevation",
                Parameters = new Dictionary<string, object>
                {
                    { "RequestedMode", "Standard" }
                }
            };

            // Evaluate
            var eval = processor.EvaluateIntent(intent);
            
            // Mode MUST NOT change on evaluation
            Assert.Equal(SatMode.Baseline, session.CurrentSatMode);
        }

        [Fact]
        public void Elevation_Denied_Headless_Returns_HitlRequired()
        {
            // Setup
            // Setup
            var ctx = Oan.Tests.Common.TestScaffolding.CreatePrimeSession("s1", "op1", new[] { "agent-1" });
            var session = ctx.Session;
            var processor = ctx.Processor;
            var ledger = ctx.Ledger;
            processor.ActivateAgent("agent-1", "test");

            var intent = new Intent
            {
                Id = Guid.NewGuid(),
                AgentProfileId = "agent-1",
                SourceAgentId = "agent-1",
                SliHandle = "sys/admin/sat.elevate.request",
                Action = "RequestSatElevation",
                Parameters = new Dictionary<string, object>
                {
                    { "RequestedMode", "Standard" }
                }
            };

            // Commit
            var result = processor.CommitIntent(intent);

            Assert.Equal(IntentStatus.Refused, result.Status);
            Assert.Equal("HITL_REQUIRED", result.ReasonCode);
            Assert.Equal(SatMode.Baseline, session.CurrentSatMode);
            
            // Check ledger events
            var reqEvt = ledger.GetEvents().FirstOrDefault(e => e.Type == "SatElevationRequested");
            var outEvt = ledger.GetEvents().FirstOrDefault(e => e.Type == "SatElevationOutcome");
            
            Assert.NotNull(reqEvt);
            Assert.NotNull(outEvt);
            Assert.Equal("HITL_GATED", ((SatElevationOutcomeEvent)outEvt.Payload).OutcomeCode);
        }

        [Fact]
        public void Elevation_Granted_Updates_Session_State()
        {
            // Setup
            var session = new SoulFrameSession("s1", "op1");
            Assert.Equal(SatMode.Baseline, session.CurrentSatMode);

            var outcome = new SatElevationOutcomeEvent
            {
                RunId = "run-1",
                Tick = 0,
                SessionId = "s1",
                Result = SatElevationResult.Granted,
                OutcomeCode = "OK",
                ResultingMode = SatMode.Standard
            };

            session.Apply(outcome);

            Assert.Equal(SatMode.Standard, session.CurrentSatMode);
        }
    }
}
