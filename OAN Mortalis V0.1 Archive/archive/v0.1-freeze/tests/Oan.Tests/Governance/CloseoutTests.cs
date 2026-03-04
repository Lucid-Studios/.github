using System.Collections.Generic;
using System.Linq;
using Oan.Core;
using Oan.Core.Events;
using Oan.Core.Governance;
using Oan.CradleTek;
using Oan.Ledger;
using Oan.Runtime;
using Oan.SoulFrame;
using Oan.SoulFrame.SLI;
using Xunit;

namespace Oan.Tests.Governance
{
    public class CloseoutTests
    {
        private readonly WorldState _world;
        private readonly EventLog _ledger;
        private readonly SoulFrameSession _session;
        private readonly SessionOrchestrator _orchestrator;
        private readonly IntentProcessor _processor;

        public CloseoutTests()
        {
            var ctx = Oan.Tests.Common.TestScaffolding.CreatePrimeSession("test-session", "tester", new[] { "agent-1" });
            _world = ctx.World;
            _ledger = ctx.Ledger;
            _session = ctx.Session;
            _processor = ctx.Processor;

            // Activate an agent to start
            var actEvt = new AgentActivationChangedEvent 
            { 
                ToAgentProfileId = "agent-1", 
                WorldTick = _world.Tick,
                SoulFrameSessionId = "test-session",
                OperatorId = "tester",
                Reason = "Setup"
            };
            _session.Apply(actEvt);
            _ledger.Append("AgentActivationChanged", actEvt, _world.Tick); // Ensure it's in ledger for replay

            // Mount is already added by Scaffolding (scaffold-mount).
            // We can add test-oan if needed, but scaffold-mount covers requirements.

            _orchestrator = new Oan.CradleTek.SessionOrchestrator(_ledger, _session, _world);
        }

        [Fact]
        public void Closeout_Appends_Events_In_Order()
        {
            var receipt = _orchestrator.CloseoutSession("test-session", "tester", "req-1");

            var events = _ledger.GetEvents().ToList();
            Assert.True(events.Count >= 4);

            var closeoutEvents = events.TakeLast(4).ToList();
            Assert.Equal("SessionQuiesced", closeoutEvents[0].Type);
            Assert.Equal("SessionSealed", closeoutEvents[1].Type);
            Assert.Equal("SessionFolded", closeoutEvents[2].Type);
            Assert.Equal("SoulFrameCleared", closeoutEvents[3].Type);
        }

        [Fact]
        public void Closeout_Updates_Session_State()
        {
            Assert.False(_session.IsQuiesced);
            Assert.False(_session.IsSealed);
            Assert.False(_session.IsCleared);

            _orchestrator.CloseoutSession("test-session", "tester", "req-1");

            Assert.True(_session.IsQuiesced);
            Assert.True(_session.IsSealed);
            Assert.True(_session.IsCleared);
            Assert.NotNull(_session.LastSealedHashes);
        }

        [Fact]
        public void CommitIntent_Refuses_When_Quiesced_Or_Sealed()
        {
            _orchestrator.CloseoutSession("test-session", "tester", "req-1");

            var intent = new Intent 
            { 
                SourceAgentId = "agent-1", 
                AgentProfileId = "agent-1", 
                Action = "Move",
                SliHandle = "public/oan/move.commit"
            };

            // IntentProcessor should refuse because session is closed
            var result = _processor.CommitIntent(intent);

            Assert.Equal(IntentStatus.Refused, result.Status);
            Assert.Equal("SOULFRAME.SESSION_CLOSED", result.ReasonCode);
        }

        [Fact]
        public void Replay_Restores_Closeout_State()
        {
            // 1. Run closeout
            _orchestrator.CloseoutSession("test-session", "tester", "req-1");
            var events = _ledger.GetEvents().ToList();

            // 2. Create fresh session and replay
            var replaySession = new SoulFrameSession("test-session", "tester");
            
            foreach (var evt in events)
            {
                dynamic payload = evt.Payload;
                // Poor man's dynamic dispatch for test
                try 
                {
                    replaySession.Apply((dynamic)payload);
                }
                catch 
                {
                    // Ignore events not handled by session (if any)
                }
            }

            Assert.True(replaySession.IsQuiesced);
            Assert.True(replaySession.IsSealed);
            Assert.True(replaySession.IsCleared);

            // Manifest Requirement: Replay test proves deterministic sealed hashes
            Assert.NotNull(_session.LastSealedHashes);
            Assert.NotNull(replaySession.LastSealedHashes);
            Assert.Equal(_session.LastSealedHashes?.World, replaySession.LastSealedHashes?.World);
            Assert.Equal(_session.LastSealedHashes?.Session, replaySession.LastSealedHashes?.Session);
        }
    }
}
