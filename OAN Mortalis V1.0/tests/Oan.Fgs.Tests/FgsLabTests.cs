using System;
using Oan.Fgs.Governance;
using Oan.Fgs.Identity;
using Xunit;

namespace Oan.Fgs.Tests
{
    public class FgsLabTests
    {
        [Fact]
        public void Ledger_AppendAndVerify_Success()
        {
            var ledger = new IdentityLedger(new PermissiveSignatureVerifier());

            var @event = new FgsEvent
            {
                CradleId = "did:fgs:cradle-lab",
                ActorId = "did:fgs:identity-root",
                PredicateId = "fgs.core.id.001",
                PrevTip = ledger.CurrentTip,
                Payload = new { Name = "Genesis" },

                // Required for fgs.core.id.*
                Signature = "lab-signature"
            };

            ledger.Append(@event);

            Assert.Single(ledger.Events);
            Assert.True(ledger.VerifyChain());

            Assert.NotEqual(FgsEvent.GenesisTip, ledger.CurrentTip);

            // Determinism check (within same run): EventId equals CurrentTip for last event
            Assert.Equal(ledger.CurrentTip, ledger.Events[0].EventId);
        }

        [Fact]
        public void Ledger_TipConflict_ThrowsException()
        {
            var ledger = new IdentityLedger(new PermissiveSignatureVerifier());
            var @event = new FgsEvent
            {
                CradleId = "did:fgs:cradle-lab",
                ActorId = "did:fgs:identity-root",
                PredicateId = "fgs.core.id.001",
                PrevTip = "invalid-tip",
                Payload = new { Name = "Genesis" },
                Signature = "lab-signature"
            };

            Assert.Throws<InvalidOperationException>(() => ledger.Append(@event));
        }

        [Fact]
        public void Ledger_MissingSignature_OnIdentityPredicate_FailsClosed()
        {
            var ledger = new IdentityLedger(new PermissiveSignatureVerifier());
            var @event = new FgsEvent
            {
                CradleId = "did:fgs:cradle-lab",
                ActorId = "did:fgs:identity-root",
                PredicateId = "fgs.core.id.001",
                PrevTip = ledger.CurrentTip,
                Payload = new { Name = "Genesis" }
                // Signature omitted
            };

            Assert.Throws<InvalidOperationException>(() => ledger.Append(@event));
        }

        [Fact]
        public void PredicateEngine_EvaluateResearcher_Allows()
        {
            var engine = new PredicateEngine();
            engine.RegisterPredicate(new PredicateMetadata
            {
                PredicateId = "fgs.core.lb.006",
                DisplayCode = "LB-006",
                Name = "PermitAction"
            });

            var context = new EvalContext
            {
                CradleId = "did:fgs:cradle-lab",
                RoleBinding = "Researcher"
            };

            var result = engine.Evaluate("fgs.core.lb.006", context);

            Assert.Equal(EvalResult.ALLOW, result);
        }

        [Fact]
        public void PredicateEngine_EvaluateIdentity_RequiresHITL()
        {
            var engine = new PredicateEngine();
            engine.RegisterPredicate(new PredicateMetadata
            {
                PredicateId = "fgs.core.id.001",
                DisplayCode = "ID-001",
                Name = "DeclareSovereignIdentity"
            });

            var context = new EvalContext();

            var result = engine.Evaluate("fgs.core.id.001", context);

            Assert.Equal(EvalResult.HITL_REQUIRED, result);
        }

        [Fact]
        public void PredicateEngine_EmergencyMode_FreezesIdentityAndSecurityFamilies()
        {
            var engine = new PredicateEngine();
            engine.RegisterPredicate(new PredicateMetadata
            {
                PredicateId = "fgs.core.id.001",
                DisplayCode = "ID-001",
                Name = "DeclareSovereignIdentity"
            });

            var context = new EvalContext { EmergencyMode = true };

            var result = engine.Evaluate("fgs.core.id.001", context);

            Assert.Equal(EvalResult.FREEZE, result);
        }
    }
}
