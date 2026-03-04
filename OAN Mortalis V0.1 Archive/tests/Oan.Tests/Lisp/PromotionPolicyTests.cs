using Xunit;
using Oan.Core.Lisp;

namespace Oan.Tests.Lisp
{
    public class PromotionPolicyTests
    {
        [Fact]
        public void Decide_AlwaysIncludesCGoA()
        {
            var policy = new MinimalPromotionPolicy();
            var result = new EvalResult { decision = EvalDecision.Allow };

            var plan = policy.Decide(result);

            Assert.Single(plan.items);
            Assert.Equal(CrypticTier.CGoA, plan.items[0].tier);
            Assert.Equal("BASELINE_CGOA", plan.items[0].rationale_code);
        }

        [Fact]
        public void Decide_PromotesToGoA_WhenFrozen()
        {
            var policy = new MinimalPromotionPolicy();
            var result = new EvalResult { decision = EvalDecision.Freeze };

            var plan = policy.Decide(result);

            Assert.Equal(2, plan.items.Count);
            Assert.Equal(CrypticTier.CGoA, plan.items[0].tier);
            Assert.Equal(CrypticTier.GoA, plan.items[1].tier);
            Assert.Equal("PROMOTION_FREEZE_INCIDENT", plan.items[1].rationale_code);
        }
    }
}
