using System;
using Xunit;
using Oan.Core.Lisp;
using Oan.Core.Governance;

namespace Oan.Tests.Lisp
{
    public class NullEvaluatorTests
    {
        [Fact]
        public void Evaluate_ReturnsDeterministicHashes_ForSameInput()
        {
            var evaluator = new NullEvaluator();
            var form = new LispForm { op = "nop" };
            var ctx = new EvalContext();
            var sat = new SatFrame { scope = "root", tick = 1 };

            var res1 = evaluator.Evaluate(form, ctx, sat);
            var res2 = evaluator.Evaluate(form, ctx, sat);

            Assert.Equal(res1.form_hash, res2.form_hash);
            Assert.Equal(res1.chain_hash, res2.chain_hash);
            Assert.Equal(LispHasher.HashForm(form), res1.form_hash);
        }

        [Fact]
        public void Evaluate_ChainHash_Empty_IsDeterministic()
        {
            var evaluator = new NullEvaluator();
            var form = new LispForm { op = "nop" };
            var res = evaluator.Evaluate(form, new EvalContext(), new SatFrame { scope = "t" });

            string expectedEmptyChain = LispHasher.HashReceiptChain(Array.Empty<string>());
            Assert.Equal(expectedEmptyChain, res.chain_hash);
            Assert.Empty(res.receipt_hashes);
        }

        [Fact]
        public void Evaluate_DoesNotMutate_FormReference()
        {
            var evaluator = new NullEvaluator();
            var form = new LispForm { op = "nop" };
            
            var res = evaluator.Evaluate(form, new EvalContext(), new SatFrame { scope = "t" });

            Assert.Same(form, res.sealed_form);
        }

        [Fact]
        public void Evaluate_Decision_IsAllow_AndListsEmpty()
        {
            var evaluator = new NullEvaluator();
            var res = evaluator.Evaluate(new LispForm { op = "h" }, new EvalContext(), new SatFrame { scope = "t" });

            Assert.Equal(EvalDecision.Allow, res.decision);
            Assert.Empty(res.receipts);
            Assert.Empty(res.cryptic_emissions);
        }
    }
}
