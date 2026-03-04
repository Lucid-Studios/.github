using System;
using System.Collections.Generic;
using Xunit;
using Oan.Core.Lisp;

namespace Oan.Tests.Lisp
{
    public class EvalResultCanonicalizerTests
    {
        [Fact]
        public void Serialize_ExactOrdering_AndEnumInteger()
        {
            var r = new EvalResult
            {
                chain_hash = "abc123",
                decision = EvalDecision.Allow,
                form_hash = "xyz789",
                intent_hash = "ihash",
                sat_hash = "shash",
                receipt_hashes = Array.Empty<string>(),
                note = null
            };

            var json = EvalResultCanonicalizer.SerializeEvalResult(r);

            const string expected = "{\"chain_hash\":\"abc123\",\"decision\":0,\"form_hash\":\"xyz789\",\"intent_hash\":\"ihash\",\"receipt_hashes\":[],\"sat_hash\":\"shash\"}";
            Assert.Equal(expected, json);
        }

        [Fact]
        public void Serialize_PreservesReceiptOrder_AndOrderAffectsHash()
        {
            var r1 = new EvalResult
            {
                chain_hash = "abc",
                decision = EvalDecision.Allow,
                form_hash = "xyz",
                intent_hash = "ih1",
                sat_hash = "sh1",
                receipt_hashes = new List<string> { "r1", "r2", "r3" }
            };

            var r2 = new EvalResult
            {
                chain_hash = "abc",
                decision = EvalDecision.Allow,
                form_hash = "xyz",
                intent_hash = "ih1",
                sat_hash = "sh1",
                receipt_hashes = new List<string> { "r3", "r2", "r1" }
            };

            var json1 = EvalResultCanonicalizer.SerializeEvalResult(r1);
            var json2 = EvalResultCanonicalizer.SerializeEvalResult(r2);

            Assert.NotEqual(json1, json2);
            Assert.NotEqual(EvalResultCanonicalizer.HashEvalResult(r1), EvalResultCanonicalizer.HashEvalResult(r2));
        }

        [Fact]
        public void Serialize_OmitsOptionalNote_WhenEmptyOrNull()
        {
            var r = new EvalResult
            {
                chain_hash = "abc",
                decision = EvalDecision.Deny,
                form_hash = "xyz",
                intent_hash = "ih1",
                sat_hash = "sh1",
                receipt_hashes = Array.Empty<string>(),
                note = ""
            };

            var json = EvalResultCanonicalizer.SerializeEvalResult(r);
            Assert.DoesNotContain("\"note\"", json);
        }

        [Fact]
        public void Hash_IsLowerHex64Chars()
        {
            var r = new EvalResult
            {
                chain_hash = "testchain",
                decision = EvalDecision.Allow,
                form_hash = "testform",
                intent_hash = "ih1",
                sat_hash = "sh1",
                receipt_hashes = Array.Empty<string>()
            };

            var hash = EvalResultCanonicalizer.HashEvalResult(r);
            Assert.Equal(64, hash.Length);
            Assert.Matches("^[0-9a-f]{64}$", hash);
        }

        [Fact]
        public void Hash_Changes_WhenDecisionChanges()
        {
            var allow = new EvalResult
            {
                chain_hash = "abc",
                decision = EvalDecision.Allow,
                form_hash = "xyz",
                intent_hash = "ih1",
                sat_hash = "sh1",
                receipt_hashes = Array.Empty<string>()
            };

            var deny = new EvalResult
            {
                chain_hash = "abc",
                decision = EvalDecision.Deny,
                form_hash = "xyz",
                intent_hash = "ih1",
                sat_hash = "sh1",
                receipt_hashes = Array.Empty<string>()
            };

            Assert.NotEqual(EvalResultCanonicalizer.HashEvalResult(allow), EvalResultCanonicalizer.HashEvalResult(deny));
        }

        [Fact]
        public void Hash_Changes_WhenIntentHashChanges()
        {
            var r1 = new EvalResult
            {
                chain_hash = "abc",
                decision = EvalDecision.Allow,
                form_hash = "xyz",
                intent_hash = "ih1",
                sat_hash = "sh1",
                receipt_hashes = Array.Empty<string>()
            };

            var r2 = new EvalResult
            {
                chain_hash = "abc",
                decision = EvalDecision.Allow,
                form_hash = "xyz",
                intent_hash = "ih2",
                sat_hash = "sh1",
                receipt_hashes = Array.Empty<string>()
            };

            Assert.NotEqual(EvalResultCanonicalizer.HashEvalResult(r1), EvalResultCanonicalizer.HashEvalResult(r2));
        }

        [Fact]
        public void Hash_Changes_WhenSatHashChanges()
        {
            var r1 = new EvalResult
            {
                chain_hash = "abc",
                decision = EvalDecision.Allow,
                form_hash = "xyz",
                intent_hash = "ih1",
                sat_hash = "sh1",
                receipt_hashes = Array.Empty<string>()
            };

            var r2 = new EvalResult
            {
                chain_hash = "abc",
                decision = EvalDecision.Allow,
                form_hash = "xyz",
                intent_hash = "ih1",
                sat_hash = "sh2",
                receipt_hashes = Array.Empty<string>()
            };

            Assert.NotEqual(EvalResultCanonicalizer.HashEvalResult(r1), EvalResultCanonicalizer.HashEvalResult(r2));
        }
    }
}
