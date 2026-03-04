using System.Collections.Generic;
using Xunit;
using Oan.Core.Lisp;
using Oan.Core.Governance;

namespace Oan.Tests.Lisp
{
    public class CrypticEmissionPointerTests
    {
        [Fact]
        public void Ptr_IsDeterministic_ForSameInputs()
        {
            var result = CreateSampleResult();
            long tick = 12345;
            string rationale = "ALLOW_DEFAULT";

            var emission1 = CrypticEmissionBuilders.BuildCGoAEvalBoundaryEmission(result, rationale, tick);
            var ptr1 = CrypticPointerHelper.ComputeCGoAPtr(emission1);

            var emission2 = CrypticEmissionBuilders.BuildCGoAEvalBoundaryEmission(result, rationale, tick);
            var ptr2 = CrypticPointerHelper.ComputeCGoAPtr(emission2);

            Assert.Equal(ptr1, ptr2);
            Assert.StartsWith("cGoA/", ptr1);
        }

        [Fact]
        public void Ptr_Changes_WhenDecisionChanges()
        {
            var resAllow = CreateSampleResult();
            resAllow.decision = EvalDecision.Allow;

            var resDeny = CreateSampleResult();
            resDeny.decision = EvalDecision.Deny;

            var e1 = CrypticEmissionBuilders.BuildCGoAEvalBoundaryEmission(resAllow, "R", 1);
            var e2 = CrypticEmissionBuilders.BuildCGoAEvalBoundaryEmission(resDeny, "R", 1);

            var p1 = CrypticPointerHelper.ComputeCGoAPtr(e1);
            var p2 = CrypticPointerHelper.ComputeCGoAPtr(e2);

            Assert.NotEqual(p1, p2);
        }

        [Fact]
        public void Ptr_Changes_WhenIntentHashChanges()
        {
            var r1 = CreateSampleResult();
            r1.intent_hash = "ih1";

            var r2 = CreateSampleResult();
            r2.intent_hash = "ih2";

            var e1 = CrypticEmissionBuilders.BuildCGoAEvalBoundaryEmission(r1, "R", 1);
            var e2 = CrypticEmissionBuilders.BuildCGoAEvalBoundaryEmission(r2, "R", 1);

            var p1 = CrypticPointerHelper.ComputeCGoAPtr(e1);
            var p2 = CrypticPointerHelper.ComputeCGoAPtr(e2);

            Assert.NotEqual(p1, p2);
        }

        [Fact]
        public void FormHeader_BindsPointerCorrectly()
        {
            var result = CreateSampleResult();
            string ptr = "cGoA/abc12345xyz";

            var header = FormHeaderBinder.Bind(result, new[] { ptr });

            Assert.NotNull(header.cryptic_pointers);
            Assert.Single(header.cryptic_pointers);
            Assert.Equal(ptr, header.cryptic_pointers[0]);
        }

        [Fact]
        public void Emission_Canonicalization_UsesLowercasePrefixForCGoA()
        {
            // Verify lock: CrypticCanonicalizer must use "cGoA" (lowercase c)
            // if this changes, existing pointers in the wild will break.
            var emission = new CrypticEmission
            {
                tier = CrypticTier.CGoA,
                kind = "test",
                payload_hash = "abc",
                tick = 1
            };

            string json = CrypticCanonicalizer.SerializeEmission(emission);
            
            // Asset precise JSON structure for critical tier field
            Assert.Contains("\"tier\":\"cGoA\"", json);
        }

        private EvalResult CreateSampleResult()
        {
            return new EvalResult
            {
                decision = EvalDecision.Allow,
                form_hash = "fh",
                chain_hash = "ch",
                intent_hash = "ih",
                sat_hash = "sh",
                receipt_hashes = new List<string> { "r1" }
            };
        }
    }
}
