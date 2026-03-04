using System;
using System.Collections.Generic;
using Xunit;
using Oan.Core.Lisp;
using Oan.Core.Governance;

namespace Oan.Tests.Lisp
{
    public class PipelineEvaluatorTests
    {
        private class MockTransform : IFormTransform
        {
            public string id { get; set; } = "MOCK";
            public string version { get; set; } = "1";
            public string rationale_code { get; set; } = "TEST";
            public Func<LispForm, LispForm> Logic { get; set; } = f => f;
            public LispForm Apply(LispForm input) => Logic(input);
        }

        [Fact]
        public void Evaluate_BindsIntentAndSatHashes_AndUsesPipelineOutputs()
        {
            var t = new MockTransform { id = "T1", Logic = f => new LispForm { op = "b" } };
            var pipeline = new TransformPipeline(new List<IFormTransform> { t });
            var evaluator = new PipelineEvaluator(pipeline);

            var form = new LispForm { op = "a" };

            var intent = new IntentForm
            {
                kind = IntentKind.Query,
                verb = "read",
                scope = "auth.cGoA",
                tick = 1000
            };

            var ctx = new EvalContext { intent = intent };
            var sat = new SatFrame { scope = "root", tick = 1 };

            var result = evaluator.Evaluate(form, ctx, sat);

            Assert.Equal(EvalDecision.Allow, result.decision);

            // Pipeline outputs
            Assert.Equal("b", result.sealed_form.op);
            Assert.Single(result.receipts);
            Assert.Single(result.receipt_hashes);
            Assert.Equal(LispHasher.HashReceipt(result.receipts[0]), result.receipt_hashes[0]);
            Assert.Equal(LispHasher.HashForm(result.sealed_form), result.form_hash);
            Assert.Equal(LispHasher.HashReceiptChain(result.receipt_hashes), result.chain_hash);

            // Bindings
            Assert.Equal(IntentCanonicalizer.HashIntent(intent), result.intent_hash);
            Assert.Equal(SatCanonicalizer.HashSatFrame(sat), result.sat_hash);

            // No cryptic emissions in this sprint
            Assert.Empty(result.cryptic_emissions);

            // Canonical surface is now valid (should not throw)
            var canon = EvalResultCanonicalizer.SerializeEvalResult(result);
            Assert.Contains("\"intent_hash\"", canon);
            Assert.Contains("\"sat_hash\"", canon);
        }

        [Fact]
        public void Evaluate_IsDeterministic_ForSameInputs()
        {
            var t = new MockTransform { id = "T1", Logic = f => new LispForm { op = "b" } };
            var pipeline = new TransformPipeline(new List<IFormTransform> { t });
            var evaluator = new PipelineEvaluator(pipeline);

            var form = new LispForm { op = "a" };

            var intent = new IntentForm { kind = IntentKind.Query, verb = "read", scope = "auth.cGoA", tick = 1000 };
            var ctx = new EvalContext { intent = intent };
            var sat = new SatFrame { scope = "root", tick = 1 };

            var r1 = evaluator.Evaluate(form, ctx, sat);
            var r2 = evaluator.Evaluate(form, ctx, sat);

            Assert.Equal(r1.form_hash, r2.form_hash);
            Assert.Equal(r1.chain_hash, r2.chain_hash);
            Assert.Equal(r1.intent_hash, r2.intent_hash);
            Assert.Equal(r1.sat_hash, r2.sat_hash);

            // Optional: the overall evalresult hash is stable too
            Assert.Equal(EvalResultCanonicalizer.HashEvalResult(r1), EvalResultCanonicalizer.HashEvalResult(r2));
        }

        [Fact]
        public void Evaluate_Throws_WhenIntentMissing()
        {
            var pipeline = new TransformPipeline(new List<IFormTransform>());
            var evaluator = new PipelineEvaluator(pipeline);

            Assert.Throws<ArgumentException>(() =>
                evaluator.Evaluate(new LispForm { op = "a" }, new EvalContext(), new SatFrame { scope = "root", tick = 1 })
            );
        }
    }
}
