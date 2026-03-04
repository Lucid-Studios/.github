using System;
using System.Collections.Generic;

namespace Oan.Core.Lisp
{
    /// <summary>
    /// Baseline promotion policy:
    /// - Always emits CGoA (v0.1 standard).
    /// - Promotes to GoA if decision is Freeze.
    /// </summary>
    public sealed class MinimalPromotionPolicy : IPromotionPolicy
    {
        public PromotionPlan Decide(EvalResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            var plan = new PromotionPlan();

            // 1) Baseline standard (always CGoA)
            plan.items.Add(new PromotionRuleResult 
            { 
                tier = CrypticTier.CGoA, 
                rationale_code = "BASELINE_CGOA" 
            });

            // 2) Multi-tier promotion rules
            if (result.decision == EvalDecision.Freeze)
            {
                plan.items.Add(new PromotionRuleResult 
                { 
                    tier = CrypticTier.GoA, 
                    rationale_code = "PROMOTION_FREEZE_INCIDENT" 
                });
            }

            return plan;
        }
    }
}
