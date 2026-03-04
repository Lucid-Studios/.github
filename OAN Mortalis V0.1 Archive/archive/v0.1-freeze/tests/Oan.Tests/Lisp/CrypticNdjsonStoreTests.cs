using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Oan.Core.Lisp;

namespace Oan.Tests.Lisp
{
    public class CrypticNdjsonStoreTests
    {
        [Fact]
        public async Task Append_WritesSingleNdjsonLine_AndReturnsPointer()
        {
            string path = Path.Combine(Path.GetTempPath(), "oan_cryptic_test_" + Guid.NewGuid().ToString("N") + ".ndjson");
            try
            {
                var store = new CrypticNdjsonStore(path);

                var emission = new CrypticEmission
                {
                    tier = CrypticTier.CGoA,
                    kind = "governance.eval",
                    payload_hash = "ph",
                    tick = 1,
                    notes = null
                };

                string ptr = await store.AppendAsync(emission);

                Assert.StartsWith("cGoA/", ptr);
                Assert.Equal(CrypticPointerHelper.ComputeCGoAPtr(emission), ptr);

                string raw = File.ReadAllText(path);
                Assert.EndsWith("\n", raw);
                Assert.DoesNotContain("\r\n", raw);

                var lines = raw.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                Assert.Single(lines);
                Assert.Equal(CrypticCanonicalizer.SerializeEmission(emission), lines[0]);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public async Task Append_Twice_IsAppendOnly_TwoLines_InOrder()
        {
            string path = Path.Combine(Path.GetTempPath(), "oan_cryptic_test_" + Guid.NewGuid().ToString("N") + ".ndjson");
            try
            {
                var store = new CrypticNdjsonStore(path);

                var e1 = new CrypticEmission { tier = CrypticTier.CGoA, kind = "k", payload_hash = "h1", tick = 1 };
                var e2 = new CrypticEmission { tier = CrypticTier.CGoA, kind = "k", payload_hash = "h2", tick = 2 };

                await store.AppendAsync(e1);
                await store.AppendAsync(e2);

                string raw = File.ReadAllText(path);
                var lines = raw.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                Assert.Equal(2, lines.Length);

                // Deterministic line strings
                Assert.Equal(CrypticCanonicalizer.SerializeEmission(e1), lines[0]);
                Assert.Equal(CrypticCanonicalizer.SerializeEmission(e2), lines[1]);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }
    }
}
