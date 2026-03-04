using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Oan.Core.Lisp;
using Oan.Core.Governance;

namespace Oan.Tests.Lisp
{
    public class PipelineEvaluatorUnifiedTests
    {
        [Fact]
        public async Task EvaluateAsync_WiresPointerAndPersistsEmission()
        {
            string path = Path.Combine(Path.GetTempPath(), "unified_test_" + Guid.NewGuid().ToString("N") + ".ndjson");
            try
            {
                // Setup: IA Transform + Minimal Policy + Unified Wrapper
                var pipeline = new TransformPipeline(new List<IFormTransform> 
                { 
                    new IaNormalizeFormTransform() 
                });
                var core = new PipelineEvaluator(pipeline, new MinimalPolicy());
                var store = new CrypticNdjsonStore(path);
                var unified = new PipelineEvaluatorUnified(core, store);

                var form = new LispForm { op = "  read  " };
                var intent = new IntentForm 
                { 
                    kind = IntentKind.Query,
                    verb = "GET", 
                    scope = "DOMAIN_A",
                    tick = 444
                };
                var ctx = new EvalContext { tick = 444, intent = intent };
                var sat = new SatFrame 
                { 
                    m = SatMode.PreFlight,
                    scope = "root",
                    b = SatBond.Active,
                    er = SatEntropyRegime.OAN,
                    et = SatTrend.Stable,
                    dl = SatDriftLevel.Low,
                    tick = 444 
                };

                // Execute
                var envelope = await unified.EvaluateAsync(form, ctx, sat);

                // Assert: Envelope & Header
                Assert.StartsWith("cGoA/", envelope.cryptic_ptr);
                Assert.Equal(envelope.cryptic_ptr, envelope.header.cryptic_pointers![0]);
                Assert.Equal(envelope.result.form_hash, envelope.header.form_hash);
                Assert.Equal("read", envelope.result.sealed_form.op); // Proves IA transform ran

                // Assert: Storage
                string raw = File.ReadAllText(path);
                var lines = raw.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                Assert.Single(lines);

                // Verify stored emission matches what we expect
                var expectedEmission = CrypticEmissionBuilders.BuildCGoAEvalBoundaryEmission(envelope.result, null, ctx.tick);
                string expectedJson = CrypticCanonicalizer.SerializeEmission(expectedEmission);
                Assert.Equal(expectedJson, lines[0]);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public async Task EvaluateAsync_IsDeterministic()
        {
            string path1 = Path.Combine(Path.GetTempPath(), "deterministic_1_" + Guid.NewGuid().ToString("N") + ".ndjson");
            string path2 = Path.Combine(Path.GetTempPath(), "deterministic_2_" + Guid.NewGuid().ToString("N") + ".ndjson");

            try
            {
                var pipeline = new TransformPipeline(new List<IFormTransform>());
                var core = new PipelineEvaluator(pipeline, new MinimalPolicy());

                var form = new LispForm { op = "read" };
                var intent = new IntentForm 
                { 
                    kind = IntentKind.Query,
                    verb = "GET",
                    scope = "DOMAIN_A",
                    tick = 777
                };
                var ctx = new EvalContext { tick = 777, intent = intent };
                var sat = new SatFrame 
                { 
                    m = SatMode.Flight,
                    scope = "root",
                    b = SatBond.Active,
                    er = SatEntropyRegime.OAN,
                    et = SatTrend.Stable,
                    dl = SatDriftLevel.Low,
                    tick = 777
                };

                // Run 1
                var store1 = new CrypticNdjsonStore(path1);
                var unified1 = new PipelineEvaluatorUnified(core, store1);
                var env1 = await unified1.EvaluateAsync(form, ctx, sat);

                // Run 2
                var store2 = new CrypticNdjsonStore(path2);
                var unified2 = new PipelineEvaluatorUnified(core, store2);
                var env2 = await unified2.EvaluateAsync(form, ctx, sat);

                // Assert stability
                Assert.Equal(env1.cryptic_ptr, env2.cryptic_ptr);
                Assert.Equal(env1.header.form_hash, env2.header.form_hash);
                Assert.Equal(File.ReadAllText(path1), File.ReadAllText(path2));
            }
            finally
            {
                if (File.Exists(path1)) File.Delete(path1);
                if (File.Exists(path2)) File.Delete(path2);
            }
        }
    }
}
