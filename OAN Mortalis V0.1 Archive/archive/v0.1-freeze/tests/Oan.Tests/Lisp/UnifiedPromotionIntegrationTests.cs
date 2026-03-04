using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Oan.Core.Lisp;
using Oan.Core.Governance;

namespace Oan.Tests.Lisp
{
    public class UnifiedPromotionIntegrationTests
    {
        [Fact]
        public async Task EvaluateAsync_WritesMultipleLines_WhenPromoted()
        {
            string path = Path.Combine(Path.GetTempPath(), "promotion_test_" + Guid.NewGuid().ToString("N") + ".ndjson");
            try
            {
                var pipeline = new TransformPipeline(new List<IFormTransform>());
                
                // Policy that will trigger FREEZE (which MinimalPromotionPolicy will promote to GoA)
                var policy = new FreezePolicyMock(); 
                var core = new PipelineEvaluator(pipeline, policy);
                var store = new CrypticNdjsonStore(path);
                var promotion = new MinimalPromotionPolicy();
                var unified = new PipelineEvaluatorUnified(core, store, promotion);

                var form = new LispForm { op = "read" };
                var intent = new IntentForm { kind = IntentKind.Query, verb = "GET", scope = "S", tick = 100 };
                var ctx = new EvalContext { tick = 100, intent = intent };
                var sat = new SatFrame 
                { 
                    m = SatMode.PreFlight, b = SatBond.Active, er = SatEntropyRegime.OAN, 
                    et = SatTrend.Stable, dl = SatDriftLevel.Low, scope = "root", tick = 100 
                };

                // Execute
                var envelope = await unified.EvaluateAsync(form, ctx, sat);

                // Assert: Multi-tier result
                Assert.Equal(EvalDecision.Freeze, envelope.result.decision);
                Assert.NotNull(envelope.header.cryptic_pointers);
                Assert.Equal(2, envelope.header.cryptic_pointers.Count);
                Assert.StartsWith("cGoA/", envelope.header.cryptic_pointers[0]);
                Assert.StartsWith("GoA/", envelope.header.cryptic_pointers[1]);

                // Assert: Storage lines
                string raw = File.ReadAllText(path);
                var lines = raw.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                Assert.Equal(2, lines.Length);
                Assert.Contains("\"tier\":\"cGoA\"", lines[0]);
                Assert.Contains("\"tier\":\"GoA\"", lines[1]);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        private class FreezePolicyMock : IPolicyMembrane
        {
            public PolicyDecision Decide(PolicyInput input) => new PolicyDecision { decision = EvalDecision.Freeze };
        }
    }
}
