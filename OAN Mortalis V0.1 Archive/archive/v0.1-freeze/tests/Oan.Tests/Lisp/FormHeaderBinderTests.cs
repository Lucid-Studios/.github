using System.Collections.Generic;
using Xunit;
using Oan.Core.Lisp;

namespace Oan.Tests.Lisp
{
    public class FormHeaderBinderTests
    {
        [Fact]
        public void Bind_CopiesRequiredFields_AndSetsVersion()
        {
            var r = new EvalResult
            {
                decision = EvalDecision.Allow,
                form_hash = "fh",
                chain_hash = "ch",
                intent_hash = "ih",
                sat_hash = "sh",
                receipt_hashes = new List<string> { "r1", "r2" },
                note = null
            };

            var h = FormHeaderBinder.Bind(r);

            Assert.Equal("0.1", h.v);
            Assert.Equal("fh", h.form_hash);
            Assert.Equal("ch", h.chain_hash);
            Assert.Equal("ih", h.intent_hash);
            Assert.Equal("sh", h.sat_hash);
            Assert.Equal(0, h.decision); // (int)EvalDecision.Allow
            Assert.Equal(2, h.receipt_hashes.Count);
            Assert.Equal("r1", h.receipt_hashes[0]);
            Assert.Equal("r2", h.receipt_hashes[1]);
            Assert.Null(h.cryptic_pointers);
        }

        [Fact]
        public void Bind_PreservesReceiptOrder()
        {
            var r = new EvalResult
            {
                decision = EvalDecision.Allow,
                form_hash = "fh",
                chain_hash = "ch",
                intent_hash = "ih",
                sat_hash = "sh",
                receipt_hashes = new List<string> { "a", "b", "c" }
            };

            var h = FormHeaderBinder.Bind(r);
            Assert.Equal("a", h.receipt_hashes[0]);
            Assert.Equal("b", h.receipt_hashes[1]);
            Assert.Equal("c", h.receipt_hashes[2]);
        }

        [Fact]
        public void Bind_Throws_WhenMandatoryMissing()
        {
            var r = new EvalResult
            {
                decision = EvalDecision.Allow,
                form_hash = "",
                chain_hash = "ch",
                intent_hash = "ih",
                sat_hash = "sh"
            };

            Assert.Throws<System.ArgumentException>(() => FormHeaderBinder.Bind(r));
        }
    }
}
