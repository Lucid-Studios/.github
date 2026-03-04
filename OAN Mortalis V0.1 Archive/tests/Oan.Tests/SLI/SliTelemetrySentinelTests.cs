using System;
using System.Security.Cryptography;
using System.Text;
using Oan.Core.Governance;
using Oan.SoulFrame.SLI;
using Xunit;
using Xunit.Abstractions;

namespace Oan.Tests.SLI
{
    public class SliTelemetrySentinelTests
    {
        private readonly ITestOutputHelper _output;

        public SliTelemetrySentinelTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Telemetry_NDJSON_Formatting_GoldenMaster()
        {
            // Build deterministic record matching baseline_allow_move scenario
            var record = new SliTelemetryRecord
            {
                RunId = "767c9c0b0213d2f281e80b853909796ea8434720937a09289260c670a4a82110",
                Tick = 0,
                SessionId = "telemetry-session",
                OperatorId = "telemetry-op",
                ActiveSatMode = "Baseline",
                MountedPartitions = new[] { "OAN" },
                RequestedHandle = "public/oan/move.commit",
                RequestedKind = "MoveTo",
                ResolvedAddress = "Public/OAN/Standard",
                PartitionMounted = true,
                SatSatisfied = true,
                CrypticRequested = false,
                MaskingApplied = false,
                Allowed = true,
                ReasonCode = "OK",
                PolicyVersion = "sli.policy.v0.1",
                MountPresent = true,
                MountId = "52f4ec7ebc2b9491e7e1678e8dcd8d5644510f94a189d00641c32b4e37b280c3" // This mount ID would be computed from runId + address + policy
            };

            var sink = new FileSliTelemetrySink("temp_sentinel.log");
            string ndjson = sink.Format(record);
            
            using var sha = SHA256.Create();
            byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(ndjson));
            string actualHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            
            File.WriteAllText("actual_hash.txt", actualHash);

            _output.WriteLine("--- SENTINEL DEBUG ---");
            _output.WriteLine($"STRING: {ndjson}");
            _output.WriteLine($"HASH:   {actualHash}");
            _output.WriteLine("--- END DEBUG ---");

            var expected = "{\"Type\":\"SliResolve\",\"RunId\":\"767c9c0b0213d2f281e80b853909796ea8434720937a09289260c670a4a82110\",\"Tick\":0,\"SessionId\":\"telemetry-session\",\"OperatorId\":\"telemetry-op\",\"ActiveSatMode\":\"Baseline\",\"MountedPartitions\":[\"OAN\"],\"RequestedHandle\":\"public/oan/move.commit\",\"RequestedKind\":\"MoveTo\",\"ResolvedAddress\":\"Public/OAN/Standard\",\"PartitionMounted\":true,\"SatSatisfied\":true,\"CrypticRequested\":false,\"MaskingApplied\":false,\"Allowed\":true,\"ReasonCode\":\"OK\",\"PolicyVersion\":\"sli.policy.v0.1\",\"MountPresent\":true,\"MountId\":\"52f4ec7ebc2b9491e7e1678e8dcd8d5644510f94a189d00641c32b4e37b280c3\",\"Notes\":null}";
            
            Assert.Equal(expected, ndjson);

            var expectedHash = "f4bfc562ac6370afd98669134e50220f9b621fd902668c2d2e22c08d9afc7351";
            Assert.Equal(expectedHash, actualHash);
        }
    }
}
