using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Oan.Core;
using Oan.Core.Governance;
using Oan.SoulFrame;
using Oan.SoulFrame.SLI;
using Oan.Ledger;
using Oan.Runtime;

namespace Oan.Host.Cli
{
    public static class SliTelemetryCommands
    {
        public static void RunScenario(string scenarioName)
        {
            Console.WriteLine($"[SLI] Running Scenario: {scenarioName}");

            string sessionId = "telemetry-session";
            string operatorId = "telemetry-op";
            long startTick = 0;
            string runId = ComputeRunId(sessionId, startTick, scenarioName, operatorId);
            string telemetryPath = $"artifacts/telemetry/sli_telemetry_{runId}.log";

            var telemetrySink = new FileSliTelemetrySink(telemetryPath);
            var gate = new SliGateService(telemetrySink);
            var world = new WorldState();
            var session = new SoulFrameSession(sessionId, operatorId);
            session.AddToRoster("telemetry-agent");
            var ledger = new EventLog();
            var processor = new IntentProcessor(world, session, ledger, gate);

            if (scenarioName == "allow_after_mount_commit")
            {
                RunMountCommitScenario(processor, session, runId, telemetryPath);
                return;
            }

            Intent intent = scenarioName switch
            {
                "baseline_allow_move" => CreateMoveIntent(session, runId, processor),
                "deny_unmounted_partition" => CreateMoveIntent(session, runId, processor, mountPartition: false),
                "deny_sat_insufficient_private_or_cryptic" => CreateCrypticIntent(session),
                _ => throw new ArgumentException($"Unknown scenario: {scenarioName}")
            };

            Console.WriteLine($"[SLI] Telemetry Path: {telemetryPath}");
            
            var result = processor.EvaluateIntent(intent, runId);

            Console.WriteLine($"[SLI] Result: {result.Status} ({result.ReasonCode})");
            PrintLastTelemetry(telemetryPath);
        }

        private static void RunMountCommitScenario(IntentProcessor processor, SoulFrameSession session, string runId, string telemetryPath)
        {
            Console.WriteLine($"[SLI] Telemetry Path: {telemetryPath}");
            session.Apply(new Oan.Core.Events.SatElevationOutcomeEvent { 
                RunId = runId, Tick = 0, SessionId = session.SessionId, 
                Result = Oan.Core.Events.SatElevationResult.Granted, OutcomeCode = "SETUP", 
                ResultingMode = SatMode.Stronger 
            });

            // 1. Attempt Move (Unmounted)
            var moveIntent = new Intent
            {
                Id = Guid.NewGuid(),
                AgentProfileId = "telemetry-agent",
                SourceAgentId = "telemetry-agent",
                Action = "MoveTo",
                SliHandle = "public/oan/move.commit",
                Parameters = new Dictionary<string, object> { { "X", 1.0 }, { "Y", 1.0 } }
            };
            Console.WriteLine("[SLI] Step 1: Attempting move (unmounted)...");
            var res1 = processor.EvaluateIntent(moveIntent, runId);
            Console.WriteLine($"[SLI] Result 1: {res1.Status} ({res1.ReasonCode})");
            PrintLastTelemetry(telemetryPath);

            // 2. Commit Mount
            var mountIntent = new Intent
            {
                Id = Guid.NewGuid(),
                AgentProfileId = "telemetry-agent",
                SourceAgentId = "telemetry-agent",
                Action = "MountCapability",
                SliHandle = "sys/admin/mount.commit",
                Parameters = new Dictionary<string, object>
                {
                    { "Channel", "Public" },
                    { "Partition", "OAN" },
                    { "Mirror", "Standard" },
                    { "RunId", runId }
                }
            };
            Console.WriteLine("[SLI] Step 2: Committing mount...");
            var resMount = processor.CommitIntent(mountIntent);
            Console.WriteLine($"[SLI] Result Mount: {resMount.Status} ({resMount.ReasonCode})");

            // 3. Retry Move (Mounted)
            Console.WriteLine("[SLI] Step 3: Retrying move (mounted)...");
            var res2 = processor.EvaluateIntent(moveIntent, runId);
            Console.WriteLine($"[SLI] Result 2: {res2.Status} ({res2.ReasonCode})");
            PrintLastTelemetry(telemetryPath);
        }

        private static void PrintLastTelemetry(string telemetryPath)
        {
            if (System.IO.File.Exists(telemetryPath))
            {
                var lines = System.IO.File.ReadAllLines(telemetryPath);
                if (lines.Length > 0)
                {
                    Console.WriteLine("[SLI] Last Telemetry Record:");
                    Console.WriteLine(lines.Last());
                }
            }
        }

        private static string ComputeRunId(string sessionId, long tick, string scenario, string opId)
        {
            string raw = $"{sessionId}|{tick}|{scenario}|{opId}";
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private static Intent CreateMoveIntent(SoulFrameSession session, string runId, IntentProcessor processor, bool mountPartition = true)
        {
            if (mountPartition)
            {
                // Cheat for the baseline scenario: manually add mount
                var address = new SliAddress(SliChannel.Public, SliPartition.OAN, SliMirror.Standard);
                var mountId = "telemetry-mount-id-baseline"; 
                session.Mounts.TryAddMount(new MountEntry
                {
                    Address = address,
                    MountId = mountId,
                    PolicyVersion = "sli.policy.v0.1",
                    SatCeiling = SatMode.Standard,
                    RequiresHitlForElevation = false,
                    CreatedTick = 0
                });
            }
            
            return new Intent
            {
                Action = "MoveTo",
                SliHandle = "public/oan/move.commit",
                SourceAgentId = "telemetry-agent",
                AgentProfileId = "telemetry-agent",
                Parameters = new Dictionary<string, object>
                {
                    { "X", 10.0 },
                    { "Y", 5.0 }
                }
            };
        }

        private static Intent CreateCrypticIntent(SoulFrameSession session)
        {
            return new Intent
            {
                Action = "Store",
                SliHandle = "private/crypticgel/ref.store",
                SourceAgentId = "telemetry-agent",
                AgentProfileId = "telemetry-agent",
                Parameters = new Dictionary<string, object>()
            };
        }
    }
}
