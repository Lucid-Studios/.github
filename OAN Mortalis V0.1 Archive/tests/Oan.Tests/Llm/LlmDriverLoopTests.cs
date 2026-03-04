using System.Collections.Generic;
using System.Threading.Tasks;
using Oan.Core;
using Oan.Core.Governance;
using Oan.Place.Llm;
using Oan.Place.Llm.BridgeIr;
using Oan.Runtime;
using Oan.SoulFrame;
using Oan.SoulFrame.SLI;
using Oan.Ledger;
using Xunit;

namespace Oan.Tests.Llm
{
    public class LlmDriverLoopTests
    {
        private readonly WorldState _world;
        private readonly SoulFrameSession _session;
        private readonly EventLog _ledger;
        private readonly SliGateService _gate;
        private readonly IntentProcessor _processor;

        public LlmDriverLoopTests()
        {
            _world = new WorldState();
            _session = new SoulFrameSession("test-session", "tester");
            _ledger = new EventLog();
            _gate = new SliGateService();
            _processor = new IntentProcessor(_world, _session, _ledger, _gate);

            // Setup
            _session.AddToRoster("agent-1");
            _session.Mounts.TryAddMount(new MountEntry
            {
                Address = new SliAddress(SliChannel.Public, SliPartition.OAN, SliMirror.Standard),
                MountId = "test-oan",
                PolicyVersion = "v1",
                SatCeiling = SatMode.Standard,
                RequiresHitlForElevation = false,
                CreatedTick = 0
            });
            _world.AddEntity(new Entity("agent-1", "Agent"));
            _session.Apply(new Oan.Core.Events.AgentActivationChangedEvent 
            { 
               ToAgentProfileId = "agent-1", WorldTick = 0, SoulFrameSessionId = "s", OperatorId = "o", Reason = "r" 
            });
            _session.SetTheaterMode(TheaterMode.Prime, "llm-test");
        }

        [Fact]
        public async Task Stub_Generates_Valid_IR()
        {
            var model = new StubLanguageModel();
            string prompt = "Move to 10.5, -20.0 --x 10.5 --y -20.0";
            string ir = await model.ProposeAsync(prompt);

            var parser = new BridgeIrParser(ir);
            var parsed = (ParsedIntent)parser.Parse();

            Assert.Equal("MoveTo", parsed.Kind);
            Assert.Equal(10.5, parsed.X);
            Assert.Equal(-20.0, parsed.Y);
        }

        [Fact]
        public async Task Pipeline_Gate_Guards_LLM_Intent()
        {
            var model = new StubLanguageModel();
            string ir = await model.ProposeAsync("move --x 5 --y 5");
            var parsed = parser_cheat(ir);
            
            var intent = BridgeIrCompiler.Compile(parsed, model.ModelId);
            intent.SourceAgentId = "agent-1";
            intent.AgentProfileId = "agent-1";

            // Case 1: Session has OAN mounted (Set in Ctor). Should Allow.
            var result = _processor.EvaluateIntent(intent);
            Assert.Equal(IntentStatus.Pending, result.Status);

            // Case 2: Session has NO partitions mounted. Should Deny.
            // (Note: MountRegistry is append-only, so we'd normally need a new session or just not mount in Case 1)
            // For the sake of the test logic, we'll use a fresh session.
            var session2 = new SoulFrameSession("s2", "tester");
            session2.SetTheaterMode(TheaterMode.Prime, "llm-test");
            session2.AddToRoster("agent-1");
            var processor2 = new IntentProcessor(_world, session2, _ledger, _gate);
            var denied = processor2.EvaluateIntent(intent);
            Assert.Equal(IntentStatus.Refused, denied.Status);
            Assert.Equal("MOUNT_NOT_PRESENT", denied.ReasonCode);
        }

        private ParsedIntent parser_cheat(string ir) => (ParsedIntent)new BridgeIrParser(ir).Parse();
    }
}
