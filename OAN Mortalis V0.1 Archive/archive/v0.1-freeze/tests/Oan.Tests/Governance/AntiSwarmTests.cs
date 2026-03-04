using System;
using System.Collections.Generic;
using Oan.Core;
using Oan.Core.Governance;
using Oan.SoulFrame;
using Oan.SoulFrame.SLI;
using Oan.Runtime;
using Oan.Ledger;
using Xunit;

namespace Oan.Tests.Governance
{
    public class AntiSwarmTests
    {
        private readonly WorldState _world;
        private readonly EventLog _ledger;
        private readonly SoulFrameSession _session;
        private readonly IntentProcessor _processor;
        private const string AGENT_1 = "agent-1";
        private const string AGENT_2 = "agent-2";

        public AntiSwarmTests()
        {
            var ctx = Oan.Tests.Common.TestScaffolding.CreatePrimeSession("test-session", "tester", new[] { AGENT_1, AGENT_2 });
            _world = ctx.World;
            _ledger = ctx.Ledger;
            _session = ctx.Session;
            _processor = ctx.Processor;

            // Add dummy entity to world
            _world.AddEntity(new Entity("source-entity", "Agent"));
        }


        
        [Fact]
        public void Evaluate_Refuses_If_No_Active_Agent()
        {
            var intent = new Intent { SourceAgentId = "source-entity", AgentProfileId = AGENT_1, Action = "Test", SliHandle = "public/oan/move.commit" };
            
            var result = _processor.EvaluateIntent(intent);

            Assert.Equal(IntentStatus.Refused, result.Status);
            Assert.Equal("SOULFRAME.NO_ACTIVE_AGENT", result.ReasonCode);
        }

        [Fact]
        public void Evaluate_Refuses_If_Wrong_Agent()
        {
            // Activate Agent 1
             _session.Apply(new Oan.Core.Events.AgentActivationChangedEvent 
             { 
                 ToAgentProfileId = AGENT_1, 
                 WorldTick = 0,
                 SoulFrameSessionId = "test-session",
                 OperatorId = "tester",
                 Reason = "Test"
             });

            // Send intent as Agent 2
            var intent = new Intent { SourceAgentId = "source-entity", AgentProfileId = AGENT_2, Action = "Test", SliHandle = "public/oan/move.commit" };
            
            var result = _processor.EvaluateIntent(intent);

            Assert.Equal(IntentStatus.Refused, result.Status);
            Assert.Equal("SOULFRAME.AGENT_NOT_ACTIVE", result.ReasonCode);
        }

        [Fact]
        public void Evaluate_Succeeds_If_Correct_Agent()
        {
            // Activate Agent 1
             _session.Apply(new Oan.Core.Events.AgentActivationChangedEvent 
             { 
                 ToAgentProfileId = AGENT_1, 
                 WorldTick = 0,
                 SoulFrameSessionId = "test-session",
                 OperatorId = "tester",
                 Reason = "Test"
             });

            // Send intent as Agent 1
            var intent = new Intent { SourceAgentId = "source-entity", AgentProfileId = AGENT_1, Action = "Test", SliHandle = "public/oan/move.commit" };
            
            // Assuming other checks pass (WorldState has entity)
            var result = _processor.EvaluateIntent(intent);

            // Evaluate returns Pending (Admissible) on success
            Assert.Equal(IntentStatus.Pending, result.Status);
        }

        [Fact]
        public void Commit_Refuses_If_Wrong_Agent()
        {
             // Activate Agent 1
             _session.Apply(new Oan.Core.Events.AgentActivationChangedEvent 
             { 
                 ToAgentProfileId = AGENT_1, 
                 WorldTick = 0,
                 SoulFrameSessionId = "test-session",
                 OperatorId = "tester",
                 Reason = "Test"
             });

            // Send intent as Agent 2 directly to Commit (Trying to bypass Evaluate)
            var intent = new Intent { SourceAgentId = "source-entity", AgentProfileId = AGENT_2, Action = "Test", SliHandle = "public/oan/move.commit" };
            
            var result = _processor.CommitIntent(intent);

            Assert.Equal(IntentStatus.Refused, result.Status);
            Assert.Equal("SOULFRAME.AGENT_NOT_ACTIVE", result.ReasonCode);
        }

        [Fact]
        public void Activate_Succeeds_If_In_Roster()
        {
            var result = _processor.ActivateAgent(AGENT_1, "Test Activation");

            Assert.Equal(IntentStatus.Committed, result.Status);
            Assert.Equal(AGENT_1, _session.ActiveAgentProfileId);
        }

        [Fact]
        public void Activate_Fails_If_Not_In_Roster()
        {
            var result = _processor.ActivateAgent("unknown-agent", "Test Activation");

            Assert.Equal(IntentStatus.Refused, result.Status);
            Assert.Equal("SOULFRAME.AGENT_NOT_IN_ROSTER", result.ReasonCode);
        }

        [Fact]
        public void Activate_Fails_If_Cooldown_Active()
        {
            // First activation (Tick 0) via manual event application to simulate prior state
            _session.Apply(new Oan.Core.Events.AgentActivationChangedEvent 
            { 
                ToAgentProfileId = AGENT_1, 
                WorldTick = 0,
                SoulFrameSessionId = "test-session",
                OperatorId = "tester",
                Reason = "Test Setup"
            });

            // Try to switch immediately at Tick 5 (Duration < 10)
            for(int i=0; i<5; i++) _world.IncrementTick(); 
            
            var result = _processor.ActivateAgent(AGENT_2, "Fast Switch");

            // Debug: currently failing with "Committed"
            // Let's assert Committed if cooldown logic is not actually enforced yet or default duration is small?
            // Or fix ValidateActivation logic in SoulFrameSession.cs if it's broken.
            // For now, let's fix the test to match reality or implementation.
            // Wait, strict requirements say cooldown MUST be enforced.
            // So implementation is likely wrong.
            // Let's see failure message first.
            Assert.Equal(IntentStatus.Refused, result.Status);
            Assert.Equal("SOULFRAME.SWITCH_COOLDOWN", result.ReasonCode);
        }

        [Fact]
        public void Replay_RestoreSessionState_Correctly()
        {
            // Simulate Ledger Replay
            var profileId = AGENT_2;
            var tick = 500L;
            
            var evt = new Oan.Core.Events.AgentActivationChangedEvent 
            { 
                ToAgentProfileId = profileId, 
                WorldTick = tick,
                SoulFrameSessionId = "test-session",
                OperatorId = "tester",
                Reason = "Replay Test"
            };
            
            _session.Apply(new Oan.Core.Events.AgentActivationChangedEvent 
            { 
                ToAgentProfileId = "AGENT_A", 
                WorldTick = 10,
                SoulFrameSessionId = "test-session",
                OperatorId = "test-op",
                Reason = "Test Setup"
            });
            
            // Apply event (Replay)
            _session.Apply(evt);
            
            Assert.Equal(profileId, _session.ActiveAgentProfileId);
            Assert.Equal(tick, _session.LastAgentSwitchTick);
            
            // Verify session enforces this restored state
            var intent = new Intent { SourceAgentId = "source-entity", AgentProfileId = AGENT_1, Action = "FailMe", SliHandle = "public/oan/move.commit" };
            var result = _processor.EvaluateIntent(intent);
            
            Assert.Equal(IntentStatus.Refused, result.Status);
            Assert.Equal("SOULFRAME.AGENT_NOT_ACTIVE", result.ReasonCode);
        }
    }
}
