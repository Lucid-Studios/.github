using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Oan.Core.Ingestion;
using Oan.Core.Governance;
using Oan.SoulFrame.SLI;
using Xunit;

namespace Oan.Tests.SLI
{
    public class DriverTelemetrySentinelTests
    {
        [Fact]
        public void Telemetry_NDJSON_Converged_Stream_Formatting_Is_Stable()
        {
            var sb = new StringBuilder();
            var sink = new FileSliTelemetrySink("test.ndjson"); // We'll just use Format() manually

            // 1. Ingestion Event
            var ingEvt = new DriverIngestionEvent
            {
                RunId = "run-123",
                Tick = 10,
                Attempt = 1,
                Outcome = IngestionOutcome.NEEDS_SPEC,
                MissingFields = new[] { "Scope", "Subject" },
                ReasonCode = "INGEST.NEEDS_SPECIFICATION",
                Raw = new RawDescriptor { Subject = null, Predicate = "MoveTo", Scope = null }
            };
            sb.AppendLine(sink.Format(ingEvt));

            // 2. SLI Event
            var sliRec = new SliTelemetryRecord
            {
                RunId = "run-123",
                Tick = 10,
                SessionId = "sess-1",
                OperatorId = "op-1",
                ActiveSatMode = "Baseline",
                MountedPartitions = new[] { "GEL", "OAN" },
                RequestedHandle = "public/oan/move.commit",
                RequestedKind = "MoveTo",
                ResolvedAddress = "Public/OAN/Standard",
                PartitionMounted = true,
                SatSatisfied = true,
                CrypticRequested = false,
                MaskingApplied = false,
                Allowed = true,
                ReasonCode = "ADMISSIBLE",
                PolicyVersion = "sli.policy.v0.1",
                MountPresent = true,
                MountId = "m-1"
            };
            sb.AppendLine(sink.Format(sliRec));

            // 3. Commit Event
            var comEvt = new DriverCommitEvent
            {
                RunId = "run-123",
                Tick = 10,
                IntentId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Result = "Committed",
                ReasonCode = null
            };
            sb.AppendLine(sink.Format(comEvt));

            // 4. Elevation Request Event
            var reqEvt = new DriverSatElevationRequestEvent
            {
                RunId = "run-123",
                Tick = 10,
                SessionId = "sess-1",
                RequestedMode = "Standard",
                TargetAddress = "Public/OAN/Standard",
                Reason = "Need elevation for GEL access"
            };
            sb.AppendLine(sink.Format(reqEvt));

            // 5. Elevation Outcome Event
            var outEvt = new DriverSatElevationOutcomeEvent
            {
                RunId = "run-123",
                Tick = 10,
                Result = "HitlRequired",
                ReasonCode = "HITL_GATED",
                ResultingMode = "Baseline"
            };
            sb.AppendLine(sink.Format(outEvt));

            var ndjson = sb.ToString();
            
            // Stable order check
            Assert.Contains("\"Type\":\"Ingestion\"", ndjson);
            Assert.Contains("\"Type\":\"SliResolve\"", ndjson);
            Assert.Contains("\"Type\":\"Commit\"", ndjson);
            Assert.Contains("\"Type\":\"SatElevationRequested\"", ndjson);
            Assert.Contains("\"Type\":\"SatElevationOutcome\"", ndjson);

            // Golden Master Hash (Standard Ordinal SHA256)
            var actualHash = Oan.Core.Engrams.EngramCanonicalizer.ComputeHash(ndjson);
            var expectedHash = "86daa2d39c50b073bbe0b684c34dd8490f1064b3ad49b2276578037138c846e3";
            
            Assert.Equal(expectedHash, actualHash);
        }
    }
}
