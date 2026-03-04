using System.Collections.Generic;
using Oan.Core.Governance;

namespace Oan.Core.Lisp
{
    /// <summary>
    /// Minimal policy input bundle for deterministic membrane decisions.
    /// </summary>
    public sealed class PolicyInput
    {
        public IntentForm intent { get; set; } = new IntentForm();
        public SatFrame sat { get; set; } = new SatFrame { scope = "root" };
        public EvalContext ctx { get; set; } = new EvalContext();
        public LispForm form { get; set; } = new LispForm();
    }

    /// <summary>
    /// Minimal policy decision result. No transforms here (Sprint 16+).
    /// </summary>
    public sealed class PolicyDecision
    {
        public EvalDecision decision { get; set; } = EvalDecision.Allow;

        // Optional: absence-over-null
        public string? rationale_code { get; set; }  // e.g. "POLICY_FROZEN"
        public string? note { get; set; }            // diagnostic message
    }

    /// <summary>
    /// Deterministic policy membrane: no IO, no randomness, no time, no mutation.
    /// </summary>
    public interface IPolicyMembrane
    {
        PolicyDecision Decide(PolicyInput input);
    }
}
