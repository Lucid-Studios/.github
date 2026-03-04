using Xunit;
using Oan.Core.Lisp;
using Oan.Core.Governance;

namespace Oan.Tests.Lisp
{
    public class MinimalPolicyTests
    {
        [Fact]
        public void Decide_Freeze_WhenDomainFrozen()
        {
            var policy = new MinimalPolicy();

            var intent = new IntentForm { kind = IntentKind.Query, verb = "read", scope = "auth.cGoA", tick = 1 };
            var sat = new SatFrame { m = SatMode.Flight, scope = "root", tick = 1 };
            var ctx = new EvalContext { domain_status = "FROZEN", intent = intent };
            var form = new LispForm { op = "nop" };

            var d = policy.Decide(new PolicyInput { intent = intent, sat = sat, ctx = ctx, form = form });

            Assert.Equal(EvalDecision.Freeze, d.decision);
            Assert.Equal("POLICY_DOMAIN_FROZEN", d.rationale_code);
        }

        [Fact]
        public void Decide_Allow_WhenNotFrozen()
        {
            var policy = new MinimalPolicy();

            var intent = new IntentForm { kind = IntentKind.Query, verb = "read", scope = "auth.cGoA", tick = 1 };
            var sat = new SatFrame { m = SatMode.Flight, scope = "root", tick = 1 };
            var ctx = new EvalContext { domain_status = "ACTIVE", intent = intent };
            var form = new LispForm { op = "nop" };

            var d = policy.Decide(new PolicyInput { intent = intent, sat = sat, ctx = ctx, form = form });

            Assert.Equal(EvalDecision.Allow, d.decision);
        }

        [Fact]
        public void Decide_Deny_DiagnosticInPostFlight()
        {
            var policy = new MinimalPolicy();

            var intent = new IntentForm { kind = IntentKind.Diagnostic, verb = "health", scope = "auth", tick = 1 };
            var sat = new SatFrame { m = SatMode.PostFlight, scope = "root", tick = 1 };
            var ctx = new EvalContext { domain_status = "ACTIVE", intent = intent };
            var form = new LispForm { op = "nop" };

            var d = policy.Decide(new PolicyInput { intent = intent, sat = sat, ctx = ctx, form = form });

            Assert.Equal(EvalDecision.Deny, d.decision);
            Assert.Equal("POLICY_NO_DIAGNOSTIC_POSTFLIGHT", d.rationale_code);
        }
    }
}
