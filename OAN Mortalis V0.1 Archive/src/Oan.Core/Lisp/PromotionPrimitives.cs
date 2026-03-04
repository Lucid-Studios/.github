using System.Collections.Generic;

namespace Oan.Core.Lisp
{
    /// <summary>
    /// Represents a single promotion rule outcome.
    /// </summary>
    public sealed class PromotionRuleResult
    {
        public CrypticTier tier { get; set; }
        public string rationale_code { get; set; } = string.Empty;
    }

    /// <summary>
    /// A set of promotion results to be executed by the host boundary.
    /// </summary>
    public sealed class PromotionPlan
    {
        public List<PromotionRuleResult> items { get; set; } = new List<PromotionRuleResult>();
    }

    /// <summary>
    /// Deterministic policy for tier promotion (multi-tier routing).
    /// </summary>
    public interface IPromotionPolicy
    {
        PromotionPlan Decide(EvalResult result);
    }
}
