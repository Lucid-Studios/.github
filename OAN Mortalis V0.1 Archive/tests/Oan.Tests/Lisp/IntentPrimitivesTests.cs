using System;
using Xunit;
using Oan.Core.Lisp;

namespace Oan.Tests.Lisp
{
    public class IntentPrimitivesTests
    {
        [Fact]
        public void Serialize_IsDeterministic_AndOrdinal_ExactJson()
        {
            var intent = new IntentForm
            {
                kind = IntentKind.Query,
                verb = "read",
                scope = "auth.cGoA",
                tick = 1000
            };

            var json = IntentCanonicalizer.SerializeIntent(intent);

            // "kind","scope","tick","verb"
            // Query=1
            string expected = "{\"kind\":1,\"scope\":\"auth.cGoA\",\"tick\":1000,\"verb\":\"read\"}";
            Assert.Equal(expected, json);
        }

        [Fact]
        public void Serialize_OmitsOptionalFields_WhenNullOrEmpty()
        {
            var intent = new IntentForm
            {
                kind = IntentKind.Command,
                verb = "update",
                scope = "system",
                tick = 1,
                note = "",
                object_ref = null,
                subject = null
            };

            var json = IntentCanonicalizer.SerializeIntent(intent);
            
            Assert.DoesNotContain("note", json);
            Assert.DoesNotContain("object_ref", json);
            Assert.DoesNotContain("subject", json);
        }

        [Fact]
        public void Hash_IsLowerHex64()
        {
            var intent = new IntentForm { verb = "v", scope = "s", tick = 1 };
            var hash = IntentCanonicalizer.HashIntent(intent);

            Assert.Equal(64, hash.Length);
            Assert.Matches("^[0-9a-f]{64}$", hash);
        }

        [Fact]
        public void Hash_Changes_WhenFieldsChange()
        {
            var i1 = new IntentForm { verb = "v1", scope = "s", tick = 1 };
            var i2 = new IntentForm { verb = "v2", scope = "s", tick = 1 };
            var i3 = new IntentForm { verb = "v1", scope = "s2", tick = 1 };
            var i4 = new IntentForm { verb = "v1", scope = "s", tick = 2 };

            var h1 = IntentCanonicalizer.HashIntent(i1);
            Assert.NotEqual(h1, IntentCanonicalizer.HashIntent(i2));
            Assert.NotEqual(h1, IntentCanonicalizer.HashIntent(i3));
            Assert.NotEqual(h1, IntentCanonicalizer.HashIntent(i4));
        }

        [Fact]
        public void Enum_NumericOrdering_LockTest()
        {
            Assert.Equal(0, (int)IntentKind.Unknown);
            Assert.Equal(1, (int)IntentKind.Query);
            Assert.Equal(2, (int)IntentKind.Command);
            Assert.Equal(3, (int)IntentKind.TransformRequest);
            Assert.Equal(4, (int)IntentKind.Diagnostic);
        }
    }
}
