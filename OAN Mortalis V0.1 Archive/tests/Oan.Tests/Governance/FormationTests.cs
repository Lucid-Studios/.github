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
    public class FormationTests
    {
        [Fact]
        public void ConstructorLevel_PrimeMode_DoesNotBind()
        {
            // Setup
            var world = new WorldState();
            var session = new SoulFrameSession("test-session", "test-operator");
            var ledger = new EventLog();
            var sli = new SliGateService(null);
            var processor = new IntentProcessor(world, session, ledger, sli);
            session.AddToRoster("agent-alpha");
            world.AddEntity(new Oan.Core.Entity("agent-alpha", "Agent")); // Added registration

            // 1. Activate & Transition to Prime
            processor.ActivateAgent("agent-alpha", "Test Start");
            var trans = new Intent { 
                SliHandle = "sys/admin/theater.transition", 
                AgentProfileId = "agent-alpha",
                SourceAgentId = "agent-alpha",
                Action = "Transition", // Added
                Parameters = new Dictionary<string, object> { { "TargetMode", "Prime" } } 
            };
            processor.Process(trans);

            Assert.Equal(TheaterMode.Prime, session.CurrentTheaterMode);
            Assert.Equal(FormationLevel.Constructor, session.FormationLevel); // Default

            // 2. Commit Action
            // This relies on implicit commit logic in IntentProcessor (ApplyIntent)
            // But we need a valid handle for SLI.
            // Using sys/admin/theater.transition again is a valid commit.
            processor.Process(trans); 

            // 3. Verify NO Engram, but YES Ephemeral Log with Formation info
            var events = ledger.GetEvents().ToList();
            Assert.DoesNotContain(events, e => e.Type == "Engrammitized");
            
            var log = events.Last(e => e.Type == "EphemeralTheaterLog");
            var payload = log.Payload as EphemeralTheaterLogEvent;
            Assert.Contains("Constructor", payload.TheaterMode); // "Prime(Constructor)"
        }

        [Fact]
        public void Promotion_ElevatesTo_HigherFormation()
        {
            // Setup
            var world = new WorldState();
            var session = new SoulFrameSession("test-session", "test-operator");
            var ledger = new EventLog();
            var sli = new SliGateService(null);
            var processor = new IntentProcessor(world, session, ledger, sli);
            session.AddToRoster("agent-alpha");
            world.AddEntity(new Oan.Core.Entity("agent-alpha", "Agent")); // Added registration
            processor.ActivateAgent("agent-alpha", "Test Start");
            
            var promote = new Intent {
                SliHandle = "sys/admin/formation.promote",
                AgentProfileId = "agent-alpha",
                SourceAgentId = "agent-alpha",
                Action = "Promote", // Added
                Parameters = new Dictionary<string, object> { { "Reason", "Test Promotion" } }
            };

            var res = processor.Process(promote);
            Assert.Equal(IntentStatus.Committed, res.Status);
            Assert.Equal(FormationLevel.HigherFormation, session.FormationLevel);

            var events = ledger.GetEvents();
            Assert.Contains(events, e => e.Type == "FormationPromoted");
        }

        [Fact]
        public void HigherFormation_PrimeMode_BindsEngram()
        {
            try 
            {
                // Setup
                var world = new WorldState();
                var session = new SoulFrameSession("test-session", "op-1");
                var ledger = new EventLog();
                var sli = new SliGateService(null); 
                var processor = new IntentProcessor(world, session, ledger, sli);
                
                session.AddToRoster("agent-alpha");
                
                // Add to WorldState
                var entity = new Oan.Core.Entity("agent-alpha", "Agent");
                world.AddEntity(entity);

                processor.ActivateAgent("agent-alpha", "Test Start");

                // 1. Promote Formation
                 var promote = new Intent 
                 { 
                     SourceAgentId = "agent-alpha", 
                     Action = "Promote",
                     AgentProfileId = "agent-alpha",
                     SliHandle = "sys/admin/formation.promote" 
                 };
                 
                 var res1 = processor.Process(promote);
                 if (res1.Status != IntentStatus.Committed) throw new Exception("Promotion Refused");

                 // 2. Transition to Prime (Explicit)
                 var transition = new Intent 
                 { 
                     SourceAgentId = "agent-alpha", 
                     Action = "Transition",
                     AgentProfileId = "agent-alpha",
                     SliHandle = "sys/admin/theater.transition",
                     Parameters = new Dictionary<string, object> 
                     { 
                         { "TargetMode", "Prime" },
                         { "RunId", "run-1" }
                     }
                 };
                 
                 var res2 = processor.Process(transition);
                 if (res2.Status != IntentStatus.Committed) throw new Exception("Transition Refused");

                 // 2b. Commit Mount (Required for SLI)
                 var mount = new Intent
                 {
                     SliHandle = "sys/admin/mount.commit",
                     AgentProfileId = "agent-alpha",
                     SourceAgentId = "agent-alpha",
                     Action = "Mount",
                     Parameters = new Dictionary<string, object>
                     {
                         { "Channel", "Public" },
                         { "Partition", "OAN" },
                         { "Mirror", "Standard" },
                         { "SatCeiling", "Standard" } // Intentionally using SatCeiling
                     }
                 };
                 
                 var resMount = processor.Process(mount);
                 if (resMount.Status != IntentStatus.Committed) throw new Exception($"Mount Refused: {resMount.ReasonCode}");

                 // 3. Commit Action (Binding)
                 var action = new Intent 
                 { 
                     SourceAgentId = "agent-alpha", 
                     Action = "Move", 
                     AgentProfileId = "agent-alpha",
                     SliHandle = "public/oan/move.commit",
                     Parameters = new Dictionary<string, object> { { "Destination", "Here" } } 
                 };
                 
                 var res3 = processor.Process(action);
                 if (res3.Status != IntentStatus.Committed) throw new Exception($"Move Refused: {res3.ReasonCode}");

                // 4. Verify Engram
                var engram = ledger.GetEvents().FirstOrDefault(e => e.Type == "Engrammitized");
                Assert.NotNull(engram); 
            }
            catch (Exception ex)
            {
                // System.IO.File.WriteAllText("error.txt", ex.ToString());
                throw;
            }
        }

        private string GetMessage(IntentResult res)
        {
            if (res.StateDelta != null && res.StateDelta.ContainsKey("Message"))
            {
                return res.StateDelta["Message"]?.ToString() ?? "null";
            }
            return "No message";
        }
    }
}
