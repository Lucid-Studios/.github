using System;
using Oan.Core.Governance;

namespace Oan.Core.Lisp
{
    /// <summary>
    /// Baseline deterministic policy:
    /// - Freeze if domain_status == "FROZEN"
    /// - Deny if intent.kind == Diagnostic AND sat.m == PostFlight (example rule)
    /// - Allow otherwise
    ///
    /// NOTE: Keep rules tiny and auditable. Expand later.
    /// </summary>
    public sealed class MinimalPolicy : IPolicyMembrane
    {
        public PolicyDecision Decide(PolicyInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (input.intent == null) throw new ArgumentException("MANDATORY_FIELD_MISSING: intent");
            if (input.sat == null) throw new ArgumentException("MANDATORY_FIELD_MISSING: sat");
            if (input.ctx == null) throw new ArgumentException("MANDATORY_FIELD_MISSING: ctx");
            if (input.form == null) throw new ArgumentException("MANDATORY_FIELD_MISSING: form");

            // Rule 1: explicit frozen domain gate
            if (string.Equals(input.ctx.domain_status, "FROZEN", StringComparison.Ordinal))
            {
                return new PolicyDecision
                {
                    decision = EvalDecision.Freeze,
                    rationale_code = "POLICY_DOMAIN_FROZEN",
                    note = "domain_status=FROZEN"
                };
            }

            // Rule 2: example “no diagnostics in postflight”
            if (input.intent.kind == IntentKind.Diagnostic && input.sat.m == SatMode.PostFlight)
            {
                return new PolicyDecision
                {
                    decision = EvalDecision.Deny,
                    rationale_code = "POLICY_NO_DIAGNOSTIC_POSTFLIGHT"
                };
            }

            return new PolicyDecision
            {
                decision = EvalDecision.Allow
            };
        }
    }
}
