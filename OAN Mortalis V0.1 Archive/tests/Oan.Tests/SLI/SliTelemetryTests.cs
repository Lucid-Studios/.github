using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Oan.Core;
using Oan.Core.Governance;
using Oan.SoulFrame;
using Oan.SoulFrame.SLI;
using Oan.Ledger;
using Oan.Runtime;
using Xunit;

namespace Oan.Tests.SLI
{
    public class SliTelemetryTests
    {
        [Fact]
        public void Telemetry_Does_Not_Affect_Decision()
        {
            var world = new WorldState();
            var session = new SoulFrameSession("test", "op");
            var ledger = new EventLog();
            
            // Allow case
            // Allow case
            session.Mounts.TryAddMount(new MountEntry
            {
                Address = new SliAddress(SliChannel.Public, SliPartition.OAN, SliMirror.Standard),
                MountId = "test-mount",
                PolicyVersion = "v1",
                SatCeiling = SatMode.Standard,
                RequiresHitlForElevation = false,
                CreatedTick = 0
            });
            var intent = new Intent { Action = "Move", SliHandle = "public/oan/move.commit", SourceAgentId = "test", AgentProfileId = "test" };
            
            // Resolve with Null sink
            var gateNull = new SliGateService(new NullSliTelemetrySink());
            var resNull = gateNull.Resolve(intent, session, SatMode.Baseline, 0, "test-null");
            
            // Resolve with File sink
            string path = "artifacts/telemetry/test/compare.log";
            if (File.Exists(path)) File.Delete(path);
            var gateFile = new SliGateService(new FileSliTelemetrySink(path));
            var resFile = gateFile.Resolve(intent, session, SatMode.Baseline, 0, "test-file");
            
            Assert.Equal(resNull.Allowed, resFile.Allowed);
            Assert.Equal(resNull.ReasonCode, resFile.ReasonCode);
            Assert.True(File.Exists(path));
        }

        [Fact]
        public void Telemetry_Fields_Are_Deterministic_And_Ordered()
        {
            var session = new SoulFrameSession("s", "o");
            // Add in non-ordinal order
            // Add in non-ordinal order
            session.Mounts.TryAddMount(new MountEntry
            {
                Address = new SliAddress(SliChannel.Public, SliPartition.OAN, SliMirror.Standard),
                MountId = "m-oan",
                PolicyVersion = "v1",
                SatCeiling = SatMode.Standard,
                RequiresHitlForElevation = false,
                CreatedTick = 0
            });
            session.Mounts.TryAddMount(new MountEntry
            {
                Address = new SliAddress(SliChannel.Private, SliPartition.GEL, SliMirror.Standard),
                MountId = "m-gel",
                PolicyVersion = "v1",
                SatCeiling = SatMode.Standard,
                RequiresHitlForElevation = false,
                CreatedTick = 0
            });
            
            var sink = new InMemoryTelemetrySink();
            var gate = new SliGateService(sink);
            var intent = new Intent { Action = "Move", SliHandle = "public/oan/move.commit", SourceAgentId = "test", AgentProfileId = "test" };
            
            var result = gate.Resolve(intent, session, SatMode.Baseline, 0, "run-1");
            Assert.True(result.Allowed);
            Assert.Equal("ADMISSIBLE", result.ReasonCode);
            
            var record = sink.LastRecord;
            Assert.NotNull(record);
            
            // GEL (ordinal 0?) vs OAN (ordinal 1?)
            // Just assert they are sorted alphabetically if ordinal is too complex for this mock
            // StringComparer.Ordinal sort on GEL vs OAN: GEL, OAN
            Assert.Equal(2, record.MountedPartitions.Length);
            Assert.Equal("GEL", record.MountedPartitions[0]);
            Assert.Equal("OAN", record.MountedPartitions[1]);
        }

        private class InMemoryTelemetrySink : ISliTelemetrySink
        {
            public SliTelemetryRecord? LastRecord { get; private set; }
            public List<object> DriverEvents { get; } = new List<object>();

            public void Append(SliTelemetryRecord record) => LastRecord = record;
            public void Append(DriverIngestionEvent record) => DriverEvents.Add(record);
            public void Append(DriverCommitEvent record) => DriverEvents.Add(record);
            public void Append(DriverSatElevationRequestEvent record) => DriverEvents.Add(record);
            public void Append(DriverSatElevationOutcomeEvent record) => DriverEvents.Add(record);
        }
    }
}
