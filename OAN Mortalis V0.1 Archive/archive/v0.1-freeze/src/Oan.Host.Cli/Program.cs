using System;
using System.Threading.Tasks;
using Oan.Core;
using Oan.SoulFrame;
using Oan.Runtime;
using Oan.Ledger;
using Oan.CradleTek;
using Oan.SoulFrame.Services;
using Oan.Core.Meaning;
using System.Collections.Generic;
using System.Linq;
using Oan.Core.Events;
using Oan.SoulFrame.Identity;

namespace Oan.Host.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("OAN Mortalis CLI v0.1");
            
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: Oan.Cli [run|test|activate <agentId>]");
                return;
            }

            var world = new WorldState();
            var ledger = new EventLog();
            var registry = new Oan.CradleTek.HostRegistry();

            // Load Modules
            await registry.LoadModuleAsync(new Oan.Place.GEL.Service.GelServiceModule());
            await registry.LoadModuleAsync(new Oan.Place.GEL.Self.GelSelfModule());
            await registry.LoadModuleAsync(new Oan.Place.OAN.Service.OanServiceModule());
            await registry.LoadModuleAsync(new Oan.Place.OAN.Self.OanSelfModule());
            await registry.LoadModuleAsync(new Oan.Place.GOA.Service.GoaServiceModule());
            await registry.LoadModuleAsync(new Oan.Place.GOA.Self.GoaSelfModule());
            
            // Setup default session for CLI
            var session = new SoulFrameSession("cli-session", "operator-cli");
            session.AddToRoster("agent-1");
            session.AddToRoster("agent-profile-1");
            session.Mounts.TryAddMount(new Oan.Core.Governance.MountEntry { 
                Address = new Oan.Core.Governance.SliAddress(Oan.Core.Governance.SliChannel.Public, Oan.Core.Governance.SliPartition.OAN, Oan.Core.Governance.SliMirror.Standard),
                MountId = "seed-oan", PolicyVersion = "sli.policy.v0.1", SatCeiling = Oan.Core.Governance.SatMode.Standard, RequiresHitlForElevation = false, CreatedTick = 0
            });
            session.Mounts.TryAddMount(new Oan.Core.Governance.MountEntry { 
                Address = new Oan.Core.Governance.SliAddress(Oan.Core.Governance.SliChannel.Public, Oan.Core.Governance.SliPartition.GEL, Oan.Core.Governance.SliMirror.Standard),
                MountId = "seed-gel", PolicyVersion = "sli.policy.v0.1", SatCeiling = Oan.Core.Governance.SatMode.Standard, RequiresHitlForElevation = false, CreatedTick = 0
            });

            var sliGate = new Oan.SoulFrame.SLI.SliGateService();
            
            // Persistence v0.2: Tip Rehydration
            // Setup Telemetry Sink (Console for now, effectively)
            var persistenceSink = new ConsolePersistenceSink();
            var tipService = new Oan.Runtime.Persistence.TipSnapshotService(session.OpalTips, persistenceSink);
            
            // Path: default to ./tips.json in current directory
            string tipsPath = "tips.json";
            string currentRootHash = TheaterIdentityService.ComputeRootAtlasHash();
            string currentPolicy = "sli.policy.v0.1"; // Hardcoded for prototype matching session setup

            // Try Load - Must happen before any commits (IntentProcessor usage)
            if (System.IO.File.Exists(tipsPath))
            {
                Console.WriteLine($"[Persistence] Found {tipsPath}, attempting rehydration...");
                bool loaded = tipService.TryLoad(tipsPath, currentRootHash, currentPolicy);
                Console.WriteLine($"[Persistence] Rehydration {(loaded ? "SUCCESS" : "FAILED/SKIPPED")}");
            }
            else
            {
                Console.WriteLine($"[Persistence] No {tipsPath} found. Starting clean session.");
            }

            var processor = new IntentProcessor(world, session, ledger, sliGate);

            // Seed World
            var agent = new Entity("agent-1", "Agent");
            world.AddEntity(agent);

            // Activate Agent for CLI session (Anti-Swarm Invariant)
            processor.ActivateAgent("agent-1", "CLI Initialization");

            string command = args[0];
            string sessionId = "cli-session"; // Global-ish scope for CLI logic

            if (command == "test")
            {
                Console.WriteLine("Running Test Intent...");
                var evt = new Oan.Core.Events.AgentActivationChangedEvent 
                {
                   ToAgentProfileId = "agent-1",
                   WorldTick = world.Tick,
                   SoulFrameSessionId = sessionId,
                   OperatorId = "console-operator",
                   Reason = "CLI Test"
                };
                session.Apply(evt);

                var intent = new Intent
                {
                    SourceAgentId = "agent-1",
                    AgentProfileId = "agent-1",
                    Action = "Move",
                    SliHandle = "public/oan/move.commit",
                    Parameters = { { "Destination", "10,0,10" } }
                };

                var result = processor.Process(intent);
                Console.WriteLine($"Result: {result.Status} - {result.ReasonCode}");
                
                if (result.Status == IntentStatus.Committed)
                {
                    ledger.Append("TestIntent", result);
                    Console.WriteLine("Intent Committed to Ledger.");
                }
            }
            else if (command == "activate")
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: activate <agentId>");
                    return;
                }
                string agentId = args[1];
                var result = processor.ActivateAgent(agentId, "CLI Request");
                Console.WriteLine($"Activation Result: {result.Status} - {result.ReasonCode}");
            }
            else if (command == "closeout")
            {
                var orchestrator = new SessionOrchestrator(ledger, session, world);
                try 
                {
                    var receipt = orchestrator.CloseoutSession(session.SessionId, "console-operator", "cli-req");
                    Console.WriteLine($"Session Sealed.");
                    Console.WriteLine($"Final World Hash: {receipt.FinalWorldHash}");
                    Console.WriteLine($"Final Session Hash: {receipt.FinalSessionHash}");

                    // Persistence v0.2: Flush Tips
                    // Using current hardcoded values from session context (defined in Main)
                    string runId = "cli-run"; // Hardcoded for CLI context
                    
                    Console.WriteLine($"[Persistence] Saving tips to {tipsPath}...");
                    tipService.Save(tipsPath, runId, receipt.SessionId, currentRootHash, currentPolicy);
                    Console.WriteLine($"[Persistence] Tips Saved.");
                }
                catch (Exception ex)
                {
                     Console.WriteLine($"Closeout Failed: {ex.Message}");
                }
            }
            else if (command == "meaning")
            {
                if (args.Length < 3)
                {
                     Console.WriteLine("Usage: meaning <propose|update|framelock|anchored|risk> --session <id> [args...]");
                     return;
                }

                var subCommand = args[1];
                for(int i=0; i<args.Length; i++) { if(args[i] == "--session" && i+1 < args.Length) sessionId = args[i+1]; }

                if (sessionId != session.SessionId) 
                {
                    Console.WriteLine("Warning: CLI prototype only supports 'cli-session'");
                }
                
                var service = new MeaningLatticeService((t, p, tick) => ledger.Append(t, p, tick), (id) => session);

                if (subCommand == "propose")
                {
                    string text = "";
                    for(int i=0; i<args.Length; i++) { if(args[i] == "--text" && i+1 < args.Length) text = args[i+1]; }
                    
                    var spans = service.ProposeSpans(sessionId, text, "snap-1", "cli-op");
                    foreach(var s in spans)
                    {
                        Console.WriteLine($"[{s.SpanId}] {s.Text} ({s.AmbiguityScore:P0})");
                    }
                }
                else if (subCommand == "update")
                {
                    string spanId = "";
                    string statusStr = "";
                    string? gloss = null;
                     for(int i=0; i<args.Length; i++) { 
                        if(args[i] == "--span") spanId = args[i+1];
                        if(args[i] == "--status") statusStr = args[i+1];
                        if(args[i] == "--gloss") gloss = args[i+1];
                     }

                     if(Enum.TryParse<MeaningStatus>(statusStr, true, out var status))
                     {
                         var res = service.UpdateSpan(sessionId, spanId, gloss ?? "", status, "cli-op");
                         Console.WriteLine($"Span {res.SpanId} updated to {res.Status}");
                     }
                     else
                     {
                         Console.WriteLine("Invalid status.");
                     }
                }
                else if (subCommand == "framelock")
                {
                     string goal = "";
                     string modeStr = "Clarify";
                     for(int i=0; i<args.Length; i++) { 
                        if(args[i] == "--goal") goal = args[i+1];
                        if(args[i] == "--mode") modeStr = args[i+1];
                     }
                     
                     Enum.TryParse<FrameMode>(modeStr, true, out var mode);
                     var locked = service.SetFrameLock(sessionId, new FrameLock { Goal = goal, Mode = mode }, "cli-op");
                     Console.WriteLine($"Frame Locked: {locked.Goal} ({locked.Mode})");
                }
                else if (subCommand == "anchored")
                {
                     dynamic context = service.GetAnchoredContext(sessionId);
                     Console.WriteLine($"FrameLock: {context.FrameLock.Goal}");
                }
                else if (subCommand == "risk")
                {
                    var risk = service.AssessRisk(sessionId);
                    Console.WriteLine($"Risk: {risk.Band} ({risk.Uncertainty:P0}) - {risk.Explanation}");
                }
            }
            else if (command == "sli")
            {
                if (args.Length < 3)
                {
                    Console.WriteLine("Usage: sli [telemetry|ingest] <scenario>");
                    return;
                }

                if (args[1] == "telemetry")
                {
                    SliTelemetryCommands.RunScenario(args[2]);
                }
                else if (args[1] == "ingest")
                {
                    SliIngestCommands.RunScenario(args[2]);
                }
                else
                {
                    Console.WriteLine($"Unknown sli subcommand: {args[1]}");
                }
            }
            else if (command == "llm")
            {
                if (args.Length < 2 || args[1] != "drive")
                {
                    Console.WriteLine("Usage: llm drive [--commit] [--scenario <id>] [--x <num>] [--y <num>]");
                    return;
                }

                bool commit = args.Contains("--commit");
                string scenario = "standard";
                for (int i = 0; i < args.Length; i++) { if (args[i] == "--scenario" && i + 1 < args.Length) scenario = args[i + 1]; }
                
                double x = 0, y = 0;
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--x" && i + 1 < args.Length) double.TryParse(args[i + 1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out x);
                    if (args[i] == "--y" && i + 1 < args.Length) double.TryParse(args[i + 1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out y);
                }

                var model = new Oan.Place.Llm.StubLanguageModel();
                var sink = new Oan.SoulFrame.SLI.FileSliTelemetrySink("artifacts/telemetry/driver_loop.ndjson");
                
                string prompt = (scenario == "ingest_loop_demo" || scenario == "sat_elevation_demo") 
                    ? scenario 
                    : $"Move to position --x {x.ToString(System.Globalization.CultureInfo.InvariantCulture)} --y {y.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

                if (scenario == "sat_elevation_demo")
                {
                    // Pre-mount Private/GEL so we can show SAT insufficiency
                    session.Mounts.TryAddMount(new Oan.Core.Governance.MountEntry
                    {
                        Address = new Oan.Core.Governance.SliAddress(Oan.Core.Governance.SliChannel.Private, Oan.Core.Governance.SliPartition.GEL, Oan.Core.Governance.SliMirror.Cryptic),
                        MountId = "demo-gel-mount",
                        PolicyVersion = "sli.policy.v0.1",
                        SatCeiling = Oan.Core.Governance.SatMode.Standard,
                        RequiresHitlForElevation = true,
                        CreatedTick = world.Tick
                    });
                }
                
                int maxAttempts = 3;
                string runId = ComputeRunIdDeterministic(session.SessionId, scenario, session.OperatorId, world.Tick); // Deterministic run identifier

                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    Console.WriteLine($"\n[Driver] Attempt {attempt}/{maxAttempts}...");
                    Console.WriteLine($"[LLM] Proposing via {model.ModelId}...");
                    string ir = await model.ProposeAsync(prompt);
                    Console.WriteLine($"[LLM] Raw IR:\n{ir}");

                    try
                    {
                        var parser = new Oan.Place.Llm.BridgeIr.BridgeIrParser(ir);
                        var parsed = parser.Parse();
                        
                        Intent? intent = null;

                        if (parsed is Oan.Place.Llm.BridgeIr.ParsedRaw rawParsed)
                        {
                            var raw = Oan.Place.Llm.BridgeIr.BridgeIrCompiler.CompileRawDescriptor(rawParsed);
                            var ingres = Oan.Core.Ingestion.IngestionService.Ingest(raw);
                            
                            // Telemetry
                            sink.Append(new Oan.Core.Governance.DriverIngestionEvent 
                            { 
                                RunId = runId, Tick = world.Tick, Attempt = attempt, 
                                Outcome = ingres.Outcome, MissingFields = ingres.MissingFields, 
                                ReasonCode = ingres.ReasonCode, Raw = raw 
                            });

                            Console.WriteLine($"[Ingestion] Outcome: {ingres.Outcome} ({(ingres.ReasonCode ?? "OK")})");
                            if (ingres.Outcome == Oan.Core.Ingestion.IngestionOutcome.NEEDS_SPEC)
                            {
                                string missing = string.Join(", ", ingres.MissingFields);
                                Console.WriteLine($"[Ingestion] Refusal: Missing fields [{missing}]");
                                prompt += $" Refusal: MissingFields: {missing}";
                                continue; // Retry loop
                            }
                            else if (ingres.Outcome == Oan.Core.Ingestion.IngestionOutcome.REJECT)
                            {
                                Console.WriteLine($"[Ingestion] Terminating: {ingres.ReasonCode}");
                                break;
                            }
                            
                            intent = Oan.Place.Llm.BridgeIr.BridgeIrCompiler.CompileFromStructuredInput(ingres.Input!, model.ModelId);
                        }
                        else if (parsed is Oan.Place.Llm.BridgeIr.ParsedIntent intentParsed)
                        {
                            intent = Oan.Place.Llm.BridgeIr.BridgeIrCompiler.Compile(intentParsed, model.ModelId);
                        }

                        if (intent != null)
                        {
                            intent.SourceAgentId = "agent-1";
                            intent.AgentProfileId = "agent-1";

                            Console.WriteLine($"[SLI] Resolving: {intent.SliHandle}...");
                            var evalResult = processor.EvaluateIntent(intent);
                            
                            // The SliDecisionEvent is already logged to ledger by IntentProcessor.
                            // But for this direct loop, we also want the SliTelemetryRecord in the file sink.
                            // SliGateService normally emits this if provided, but SliGateService doesn't have a sink in its constructor here.
                            // We'll manually resolve for telemetry capture if needed, or update SliGateService.
                            // For v0.2, the SliGateService used by IntentProcessor is the one we created at line 52.
                            // Let's ensure it has our sink.
                            var telemetryGate = new Oan.SoulFrame.SLI.SliGateService(sink);
                            var sliResult = telemetryGate.Resolve(intent, session, session.CurrentSatMode, world.Tick, runId);

                            Console.WriteLine($"[SLI] Decision: {(sliResult.Allowed ? "ALLOWED" : "DENIED")} ({sliResult.ReasonCode})");

                            if (!sliResult.Allowed)
                            {
                                if (sliResult.ReasonCode == "SLI.SAT_MODE.INSUFFICIENT")
                                {
                                    Console.WriteLine("[Driver] SAT Insufficient. Hinting LLM to Request elevation.");
                                    prompt += " Refusal: SAT_INSUFFICIENT. Use sys/admin/sat.elevate.request to bridge.";
                                    continue; // Retry loop
                                }
                            }

                            if (commit && sliResult.Allowed && evalResult.Status == IntentStatus.Pending)
                            {
                                Console.WriteLine("[Pipeline] Committing...");
                                var commitResult = processor.CommitIntent(intent);
                                Console.WriteLine($"[Pipeline] Result: {commitResult.Status}");
                                sink.Append(new Oan.Core.Governance.DriverCommitEvent { RunId = runId, Tick = world.Tick, IntentId = intent.Id, Result = commitResult.Status.ToString(), ReasonCode = commitResult.ReasonCode });
                            }
                            else if (commit && !sliResult.Allowed && intent.SliHandle == "sys/admin/sat.elevate.request")
                            {
                                // We tried to commit an elevation request, but it was refused (e.g. HITL_REQUIRED)
                                Console.WriteLine("[Pipeline] Committing Elevation Request...");
                                var commitResult = processor.CommitIntent(intent);
                                Console.WriteLine($"[Pipeline] Result: {commitResult.Status} ({commitResult.ReasonCode})");
                                
                                // Telemetry for elevation request is handled inside Processor (Requested/Outcome)
                                // but we also want the DriverSliEvent? Actually SliGate.Resolve already logged DriverSliEvent via sink.
                                
                                if (commitResult.ReasonCode == "HITL_REQUIRED")
                                {
                                    prompt += " Refusal: HITL_REQUIRED (Manual approval pending). Proceed with alternative or fallback.";
                                    continue; // Let LLM try something else or fallback
                                }
                            }
                            
                            break; // Success or final denial
                        }
                    }
                    catch (Oan.Place.Llm.BridgeIr.BridgeIrException ex)
                    {
                        Console.WriteLine($"[Driver] IR Error: {ex.ReasonCode} - {ex.Message}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Driver] Unexpected Error: {ex.Message}");
                        break;
                    }
                }
            }
            else if (command == "engram")
            {
                if (args.Length < 3 || args[1] != "theater" || args[2] != "demo")
                {
                    Console.WriteLine("Usage: engram theater demo");
                    return;
                }
                await RunEngramTheaterDemo(processor, session, ledger, world);
            }
            else
            {
                Console.WriteLine("Starting Interactive Mode (Simulated)...");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
        }

        private static string ComputeRunIdDeterministic(string sessionId, string scenarioName, string operatorId, long genesisTick)
        {
            return TheaterIdentityService.ComputeRunIdDeterministic(sessionId, scenarioName, operatorId, genesisTick).Substring(0, 8);
        }

        static Task RunEngramTheaterDemo(IntentProcessor processor, SoulFrameSession session, Oan.Ledger.EventLog ledger, WorldState world)
        {
            return Task.Run(() => {
                Console.WriteLine("--- Engram Theater Demo ---");
            
            // Scenario 1: theater_seed_only
            Console.WriteLine("\n[Scenario: theater_seed_only]");
            var intent1 = new Intent
            {
                SourceAgentId = "agent-1",
                AgentProfileId = "agent-1",
                Action = "Move",
                SliHandle = "public/oan/move.commit"
            };
            intent1.Parameters["Destination"] = "5.0,0.0,5.0";
            intent1.Parameters["RunId"] = "demo-run-1";
            
            var res1 = processor.CommitIntent(intent1);
            Console.WriteLine($"Intent 1: {res1.Status}");
            
            if (session.TheaterSeed != null)
            {
                Console.WriteLine($"TheaterId: {session.TheaterSeed.TheaterId}");
                Console.WriteLine($"RootAtlasHash: {session.TheaterSeed.RootAtlasHash}");
                string tip1 = session.OpalTips.GetTip(session.TheaterSeed.TheaterId) ?? "none";
                Console.WriteLine($"OpalTip After Intent 1: {tip1}");
            }

            // Scenario 2: theater_commit_advances_tip
            Console.WriteLine("\n[Scenario: theater_commit_advances_tip]");
            var intent2 = new Intent
            {
                SourceAgentId = "agent-1",
                AgentProfileId = "agent-1",
                Action = "Move",
                SliHandle = "public/oan/move.commit"
            };
            intent2.Parameters["Destination"] = "10.0,0.0,10.0";
            intent2.Parameters["RunId"] = "demo-run-1";
            
            string tipBefore = session.OpalTips.GetTip(session.TheaterSeed!.TheaterId) ?? "none";
            var res2 = processor.CommitIntent(intent2);
            string tipAfter = session.OpalTips.GetTip(session.TheaterSeed.TheaterId) ?? "none";
            
            Console.WriteLine($"OpalTip Before: {tipBefore}");
            Console.WriteLine($"Intent 2: {res2.Status}");
            Console.WriteLine($"OpalTip After: {tipAfter}");

            // Display verification items
            var lastEngramEvt = ledger.GetEvents().LastOrDefault(e => e.Type == "Engrammitized");
            if (lastEngramEvt?.Payload is Oan.Core.Events.EngrammitizedEvent ee)
            {
                Console.WriteLine($"NormalFormKey: {ee.NormalFormKey}");
                Console.WriteLine($"WitnessEventIds: {string.Join(", ", ee.WitnessEventIds)}");
            }
            });
        }
    }

    public class ConsolePersistenceSink : Oan.Runtime.Persistence.ISnapshotTelemetrySink
    {
        public void Emit(object evt)
        {
            if (evt is Oan.Core.Events.TipSnapshotLoadAttemptedEvent attempt)
            {
                // Console.WriteLine($"[Persistence] Load Attempted. PathHash: {attempt.PathHash}");
            }
            else if (evt is Oan.Core.Events.TipSnapshotRejectedEvent rejected)
            {
                Console.WriteLine($"[Persistence] Snapshot Rejected: {rejected.ReasonCode}");
            }
            else if (evt is Oan.Core.Events.TipSnapshotLoadedEvent loaded)
            {
                Console.WriteLine($"[Persistence] Snapshot Loaded. Theaters: {loaded.TheaterCount}");
            }
            else if (evt is Oan.Core.Events.TipSnapshotWrittenEvent written)
            {
                Console.WriteLine($"[Persistence] Snapshot Written. Theaters: {written.TheaterCount}");
            }
        }
    }
}
