using System;
using System.Collections.Generic;
using Xunit;
using Oan.Core.Lisp;

namespace Oan.Tests.Lisp
{
    public class CrypticHashingTests
    {
        [Fact]
        public void Serialize_Pointer_OrdinalOrdering()
        {
            var p = new CrypticPointer { tier = CrypticTier.CGoA, pointer = "h1", hint = "test" };
            var json = CrypticCanonicalizer.SerializePointer(p);
            
            // Expected exact: {"hint":"test","pointer":"h1","tier":"CGoA"} -> normalized to cGoA
            Assert.Equal("{\"hint\":\"test\",\"pointer\":\"h1\",\"tier\":\"cGoA\"}", json);
        }

        [Fact]
        public void Serialize_Pointer_OmitsNullHint()
        {
            var p = new CrypticPointer { tier = CrypticTier.CGoA, pointer = "h1", hint = null };
            var json = CrypticCanonicalizer.SerializePointer(p);
            
            // Expected exact: {"pointer":"h1","tier":"CGoA"}
            Assert.Equal("{\"pointer\":\"h1\",\"tier\":\"CGoA\"}", json);
        }

        [Fact]
        public void Serialize_Emission_PreservesListOrder()
        {
            var p1 = new CrypticPointer { tier = CrypticTier.CGoA, pointer = "h1" };
            var p2 = new CrypticPointer { tier = CrypticTier.cSGEL, pointer = "h2" };

            var eA = new CrypticEmission
            {
                kind = "log",
                payload_hash = "phash",
                tier = CrypticTier.CGEL,
                pointers = new List<CrypticPointer> { p1, p2 }
            };

            var eB = new CrypticEmission
            {
                kind = "log",
                payload_hash = "phash",
                tier = CrypticTier.CGEL,
                pointers = new List<CrypticPointer> { p2, p1 }
            };

            var jsonA = CrypticCanonicalizer.SerializeEmission(eA);
            var jsonB = CrypticCanonicalizer.SerializeEmission(eB);
            
            Assert.NotEqual(jsonA, jsonB);
            Assert.NotEqual(CrypticHasher.HashEmission(eA), CrypticHasher.HashEmission(eB));
        }

        [Fact]
        public void Serialize_MandatoryField_Missing_Throws()
        {
            var p = new CrypticPointer { pointer = "" }; // empty string
            Assert.Throws<ArgumentException>(() => CrypticCanonicalizer.SerializePointer(p));
        }

        [Fact]
        public void Hash_IsLowerHex64()
        {
            var p = new CrypticPointer { tier = CrypticTier.GEL, pointer = "root" };
            var hash = CrypticHasher.HashPointer(p);
            
            Assert.Equal(64, hash.Length);
            Assert.Matches("^[0-9a-f]{64}$", hash);
        }

        [Fact]
        public void Serialize_AccessLog_Deterministic()
        {
            var e = new AccessLogEvent { action = "read", operator_id = "gov", target = "h1", tier = CrypticTier.cSGEL, tick = 100 };
            var json = CrypticCanonicalizer.SerializeAccessLog(e);
            
            // Expected alphabetical keys: action, operator_id, target, tier, tick
            Assert.Equal("{\"action\":\"read\",\"operator_id\":\"gov\",\"target\":\"h1\",\"tier\":\"cSelfGEL\",\"tick\":100}", json);
        }
    }
}
