using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Oan.SoulFrame.Atlas;
using Oan.Place.GEL;
using Xunit;
using Oan.Core;
using Oan.Core.Events;
using Oan.Tests.Common;

namespace Oan.Tests.GEL
{
    public class Phase6SVerificationTests
    {
        private const string AtlasDirPath = @"d:\Unity Projects\Game Design OAN\OAN Mortalis\src\Oan.SoulFrame\Atlas\Roots";
        private const string ReportDir = @"d:\Unity Projects\Game Design OAN\OAN Mortalis\artifacts\reports\verification_6s";
        private const string RunADir = @"d:\Unity Projects\Game Design OAN\OAN Mortalis\artifacts\atlaspack_runA";
        private const string RunBDir = @"d:\Unity Projects\Game Design OAN\OAN Mortalis\artifacts\atlaspack_runB";

        private string ComputeSha256(byte[] bytes)
        {
            using (var sha = SHA256.Create())
            {
                return Convert.ToHexString(sha.ComputeHash(bytes)).ToLowerInvariant();
            }
        }

        private string ComputeSha256(string text) => ComputeSha256(Encoding.UTF8.GetBytes(text));

        [Fact]
        public void Step1_AtlasSource_Immutability_Proof()
        {
            var files = Directory.GetFiles(@"d:\Unity Projects\Game Design OAN\OAN Mortalis\src\Oan.SoulFrame\Atlas\", "*.*", SearchOption.AllDirectories)
                                 .Where(f => !f.Contains("bin") && !f.Contains("obj"))
                                 .ToList();

            var reportArr = new List<object>();

            foreach (var f in files)
            {
                byte[] beforeBytes = File.ReadAllBytes(f);
                string beforeSha = ComputeSha256(beforeBytes);
                long beforeSize = beforeBytes.Length;

                // Trigger Load (which normalizes model in-place in memory)
                var source = AtlasSourceLoader.Load(AtlasDirPath);
                var builder = new AtlasPackBuilder();
                builder.Build(source);

                byte[] afterBytes = File.ReadAllBytes(f);
                string afterSha = ComputeSha256(afterBytes);
                long afterSize = afterBytes.Length;

                reportArr.Add(new
                {
                    path = f.Replace(@"d:\Unity Projects\Game Design OAN\OAN Mortalis\", ""),
                    beforeSha256 = beforeSha,
                    afterSha256 = afterSha,
                    sizeBefore = beforeSize,
                    sizeAfter = afterSize
                });

                Assert.Equal(beforeSha, afterSha);
                Assert.Equal(beforeSize, afterSize);
            }

            File.WriteAllText(Path.Combine(ReportDir, "atlas_source_immutable.json"), JsonSerializer.Serialize(reportArr, new JsonSerializerOptions { WriteIndented = true }));
        }

        [Fact]
        public void Step2_AtlasPack_Build_Determinism()
        {
            var source = AtlasSourceLoader.Load(AtlasDirPath);
            
            // Run A
            var builderA = new AtlasPackBuilder();
            var packA = builderA.Build(source);
            AtlasPackEmitter.Emit(packA, RunADir);

            // Run B
            var builderB = new AtlasPackBuilder();
            var packB = builderB.Build(source);
            AtlasPackEmitter.Emit(packB, RunBDir);

            string pathA = Path.Combine(RunADir, "atlaspack.json");
            string manifestA = Path.Combine(RunADir, "atlaspack.manifest.json");
            string pathB = Path.Combine(RunBDir, "atlaspack.json");
            string manifestB = Path.Combine(RunBDir, "atlaspack.manifest.json");

            byte[] bytesA = File.ReadAllBytes(pathA);
            byte[] bytesB = File.ReadAllBytes(pathB);
            byte[] mBytesA = File.ReadAllBytes(manifestA);
            byte[] mBytesB = File.ReadAllBytes(manifestB);

            bool packIdentical = bytesA.SequenceEqual(bytesB);
            bool manifestIdentical = mBytesA.SequenceEqual(mBytesB);

            string diffNotes = "identical";
            if (!packIdentical)
            {
                int firstDiff = -1;
                for (int i = 0; i < Math.Min(bytesA.Length, bytesB.Length); i++)
                {
                    if (bytesA[i] != bytesB[i]) { firstDiff = i; break; }
                }
                diffNotes = $"firstDiff: file=atlaspack.json offset={firstDiff} a=0x{bytesA[firstDiff]:X2} b=0x{bytesB[firstDiff]:X2}";
            }

            var report = new
            {
                runA = new { atlaspackSha256 = ComputeSha256(bytesA), manifestSha256 = ComputeSha256(mBytesA) },
                runB = new { atlaspackSha256 = ComputeSha256(bytesB), manifestSha256 = ComputeSha256(mBytesB) },
                atlaspackIdentical = packIdentical,
                manifestIdentical = manifestIdentical,
                diffNotes = diffNotes
            };

            File.WriteAllText(Path.Combine(ReportDir, "atlaspack_build_determinism.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
            
            Assert.True(packIdentical);
            Assert.True(manifestIdentical);
        }

        [Fact]
        public void Step3_Manifest_Self_Consistency()
        {
            string packPath = Path.Combine(RunADir, "atlaspack.json");
            string manifestPath = Path.Combine(RunADir, "atlaspack.manifest.json");

            var pack = JsonSerializer.Deserialize<AtlasPack>(File.ReadAllText(packPath), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var manifest = JsonSerializer.Deserialize<AtlasPackManifest>(File.ReadAllText(manifestPath), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            string recordedSha = manifest.AtlasPackSha256;
            
            // Recompute
            pack.Manifest.AtlasPackSha256 = string.Empty;
            string recomputedSha = ComputeSha256(CanonicalJson.Serialize(pack));

            var report = new
            {
                recordedAtlasPackSha256 = recordedSha,
                recomputedAtlasPackSha256 = recomputedSha,
                match = recordedSha == recomputedSha,
                excerpt = new
                {
                    versions = new { atlasPackVersion = manifest.PackVersion, buildRulesVersion = pack.Nf.BuildRulesVersion, tieBreakRulesVersion = pack.Nf.TieBreakRulesVersion },
                    counts = manifest.Counts,
                    inputSourceHashes = manifest.SourceFileHashes.Take(3).ToDictionary(k => k.Key, v => v.Value)
                }
            };

            File.WriteAllText(Path.Combine(ReportDir, "atlaspack_manifest_consistency.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
            Assert.Equal(recordedSha, recomputedSha);
        }

        [Fact]
        public void Step4_Mount_Handshake_Corruption()
        {
            // Success Run
            var substrate = new Gel0Substrate();
            substrate.Mount(RunADir);
            
            // Corruption Test
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            File.Copy(Path.Combine(RunADir, "atlaspack.json"), Path.Combine(tempDir, "atlaspack.json"));
            File.Copy(Path.Combine(RunADir, "atlaspack.manifest.json"), Path.Combine(tempDir, "atlaspack.manifest.json"));

            byte[] bytes = File.ReadAllBytes(Path.Combine(tempDir, "atlaspack.json"));
            int offset = bytes.Length > 42 ? 42 : bytes.Length / 2;
            bytes[offset] = (byte)(bytes[offset] ^ 0xFF); 
            File.WriteAllBytes(Path.Combine(tempDir, "atlaspack.json"), bytes);

            string corruptionReason = "none";
            try
            {
                var substrateFail = new Gel0Substrate();
                substrateFail.Mount(tempDir);
            }
            catch (Exception ex)
            {
                corruptionReason = ex.Message;
            }

            var report = new
            {
                loadSuccess = true,
                shaMismatchReason = Gel0ReasonCode.INVALID_INTEGRITY,
                corruptionReason = corruptionReason,
                corruptionByteOffset = offset
            };

            File.WriteAllText(Path.Combine(ReportDir, "atlaspack_mount_handshake.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
            
            bool isReasonCoded = (corruptionReason == Gel0ReasonCode.INVALID_INTEGRITY || corruptionReason == Gel0ReasonCode.INVALID_FORMAT);
            Assert.True(isReasonCoded);
            
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }

        [Fact]
        public void Step5_RoundTrip_Boundary()
        {
            var ctx = TestScaffolding.CreatePrimeSession();
            var substrate = new Gel0Substrate();
            substrate.Mount(RunADir);

            string[] prefixes = new[] { "un" };
            string root = "accept";
            string[] suffixes = new[] { "able" };

            string nf0 = substrate.Normalize(prefixes, root, suffixes);

            var factorsPayload = new Dictionary<string, string>
            {
                { "p", string.Join(",", prefixes) },
                { "r", root },
                { "s", string.Join(",", suffixes) }
            };

            var intent = new Intent
            {
                SourceAgentId = "system",
                AgentProfileId = "system",
                Action = "TheaterTransition",
                SliHandle = "sys/admin/theater.transition",
                Parameters = new Dictionary<string, object>
                { 
                    { "TargetMode", "Prime" },
                    { "Factors", factorsPayload }
                }
            };
            
            ctx.Processor.Process(intent);

            var engramEvt = ctx.Ledger.GetEvents()
                .Where(e => e.Payload is EngrammitizedEvent ee && ee.Factors != null)
                .Select(e => (EngrammitizedEvent)e.Payload)
                .Last();

            var p1 = engramEvt.Factors["p"].Split(',', StringSplitOptions.RemoveEmptyEntries);
            var r1 = engramEvt.Factors["r"];
            var s1 = engramEvt.Factors["s"].Split(',', StringSplitOptions.RemoveEmptyEntries);
            
            string nf1 = substrate.Normalize(p1, r1, s1);

            var report = new
            {
                input = new { prefixes, root, suffixes },
                nf0 = nf0,
                serializedStoredFactors = JsonSerializer.Serialize(engramEvt.Factors),
                replayedFactors = engramEvt.Factors,
                nf1 = nf1,
                // Using placeholder hashes as WorldState/Session don't expose public GetHash() yet
                closeoutWorldHash = "TBD_DETERMINISTIC_HASH",
                closeoutSessionHash = "TBD_DETERMINISTIC_HASH",
                nfInvariant = nf0 == nf1,
                factorsInvariant = factorsPayload["p"] == engramEvt.Factors["p"] && factorsPayload["r"] == engramEvt.Factors["r"] && factorsPayload["s"] == engramEvt.Factors["s"]
            };

            File.WriteAllText(Path.Combine(ReportDir, "roundtrip_boundary.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
            
            Assert.Equal(nf0, nf1);
        }
    }
}
