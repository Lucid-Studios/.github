using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Oan.Core;
using Oan.Core.Events;
using Oan.Core.Governance;
using Oan.Ledger;
using Oan.Runtime;
using Oan.SoulFrame;
using Oan.SoulFrame.SLI;

namespace Oan.Tests.Governance
{
    public class TheaterEnforcementTests
    {
        private WorldState _world;
        private SoulFrameSession _session;
        private EventLog _ledger;
        private SliGateService _sli;
        private IntentProcessor _processor;

        public TheaterEnforcementTests()
        {
            _world = new WorldState();
            _session = new SoulFrameSession("test-session", "test-operator");
            _ledger = new EventLog();
            _sli = new SliGateService(null); // Null telemetry for test
            _processor = new IntentProcessor(_world, _session, _ledger, _sli);

            // Setup Roster
            _session.AddToRoster("agent-alpha");
            
            // Add Agent to World State
            _world.AddEntity(new Entity("agent-alpha", "Agent"));
        }

        private void SetupMounts()
        {
             var mountIntent = new Intent {
                SliHandle = "sys/admin/mount.commit",
                AgentProfileId = "agent-alpha",
                SourceAgentId = "agent-alpha",
                Action = "Mount",
                Parameters = new Dictionary<string, object> {
                    { "Channel", "Public" },
                    { "Partition", "OAN" },
                    { "Mirror", "Standard" }
                }
            };
            var res = _processor.Process(mountIntent);
            Assert.True(res.Status == IntentStatus.Committed, $"Mount failed: {res.ReasonCode}");
        }

        [Fact]
        public void IdleMode_Rejects_StandardIntent()
        {
            var res = _processor.ActivateAgent("agent-alpha", "Test Start");
            Assert.Equal(IntentStatus.Committed, res.Status);
            
            SetupMounts(); // Mount partition but stay in IDLE
            Assert.Equal(TheaterMode.Idle, _session.CurrentTheaterMode);

            var intent = new Intent
            {
                SourceAgentId = "agent-alpha",
                AgentProfileId = "agent-alpha",
                Action = "Move",
                SliHandle = "public/oan/move.commit"
            };

            var result = _processor.Process(intent);
            
            Assert.Equal(IntentStatus.Refused, result.Status);
            Assert.Equal("THEATER_IDLE", result.ReasonCode);
        }

        [Fact]
        public void ExplicitTransition_ToPrime_Success()
        {
            var res = _processor.ActivateAgent("agent-alpha", "Test Start");
            Assert.Equal(IntentStatus.Committed, res.Status);

            var intent = new Intent
            {
                 SourceAgentId = "agent-alpha",
                 AgentProfileId = "agent-alpha", 
                 Action = "Transition",
                 SliHandle = "sys/admin/theater.transition",
                 Parameters = new Dictionary<string, object>
                 {
                     { "TargetMode", "Prime" },
                     { "Reason", "Unit Test" }
                 }
            };

            var result = _processor.Process(intent);

            Assert.True(result.Status == IntentStatus.Committed, $"Transition failed: {result.ReasonCode}");
            Assert.Equal(TheaterMode.Prime, _session.CurrentTheaterMode);
            Assert.NotNull(_session.CurrentTheaterId);
        }

        [Fact]
        public void PrimeMode_Generates_Engram()
        {
            var resA = _processor.ActivateAgent("agent-alpha", "Test Start");
            Assert.Equal(IntentStatus.Committed, resA.Status);
            
            SetupMounts();
            
            // Phase 2 Fix: Must be in HigherFormation to bind
            var promote = new Intent {
                SliHandle = "sys/admin/formation.promote",
                 AgentProfileId = "agent-alpha", 
                 SourceAgentId = "agent-alpha",
                 Action = "Promote"
            };
            var resP = _processor.Process(promote);
            Assert.True(resP.Status == IntentStatus.Committed, $"Promotion failed: {resP.ReasonCode}");

             var transIntent = new Intent
            {
                 SourceAgentId = "agent-alpha",
                 AgentProfileId = "agent-alpha", 
                 Action = "Transition",
                 SliHandle = "sys/admin/theater.transition",
                 Parameters = new Dictionary<string, object> { { "TargetMode", "Prime" } }
            };
            var resT = _processor.Process(transIntent);
            Assert.True(resT.Status == IntentStatus.Committed, $"Transition failed: {resT.ReasonCode}");
            
            Assert.Equal(TheaterMode.Prime, _session.CurrentTheaterMode);

            // 2. Commit Intent (should generate Engram)
            var intent = new Intent
            {
                SourceAgentId = "agent-alpha",
                AgentProfileId = "agent-alpha",
                Action = "Move",
                SliHandle = "public/oan/move.commit"
            };

            var result = _processor.Process(intent);
            Assert.Equal(IntentStatus.Committed, result.Status);
            
            var events = _ledger.GetEvents().ToList();
            var engramEvt = events.LastOrDefault(e => e.Type == "Engrammitized");
            
            Assert.NotNull(engramEvt);
        }

        [Fact]
        public void OanMode_Generates_EphemeralLog()
        {
            var resA = _processor.ActivateAgent("agent-alpha", "Test Start");
            Assert.Equal(IntentStatus.Committed, resA.Status);

             var transIntent = new Intent
            {
                 SourceAgentId = "agent-alpha",
                 AgentProfileId = "agent-alpha", 
                 Action = "Transition",
                 SliHandle = "sys/admin/theater.transition",
                 Parameters = new Dictionary<string, object> { { "TargetMode", "OAN" } }
            };
            var resT = _processor.Process(transIntent);
            Assert.True(resT.Status == IntentStatus.Committed, $"Transition failed: {resT.ReasonCode}");
            
            Assert.Equal(TheaterMode.OAN, _session.CurrentTheaterMode);

            var events = _ledger.GetEvents().ToList();
            var lastEvt = events.Last();
            
            Assert.Equal("EphemeralTheaterLog", lastEvt.Type);
            var payload = lastEvt.Payload as EphemeralTheaterLogEvent;
            Assert.Equal("OAN(Constructor)", payload.TheaterMode);
        }

        [Fact]
        public void NonCommit_DoesNotGenerate_Engram()
        {
            _processor.ActivateAgent("agent-alpha", "Test Start");
            SetupMounts();

            var intent = new Intent
            {
                SourceAgentId = "agent-alpha",
                AgentProfileId = "agent-alpha",
                Action = "Move",
                SliHandle = "public/oan/move.commit"
            };
            
            // In Idle mode this is refused
            var result = _processor.Process(intent);
            Assert.Equal(IntentStatus.Refused, result.Status);
            Assert.Equal("THEATER_IDLE", result.ReasonCode);
        }
    }
}
