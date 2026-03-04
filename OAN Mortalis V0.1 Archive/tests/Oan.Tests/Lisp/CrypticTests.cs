using System.Collections.Generic;
using Xunit;
using Oan.Core.Lisp;

namespace Oan.Tests.Lisp
{
    public class CrypticTests
    {
        [Fact]
        public void CrypticTier_Enum_PreservesPresence()
        {
            Assert.Equal(0, (int)CrypticTier.GEL);
            Assert.Equal(1, (int)CrypticTier.GoA);
            Assert.Equal(2, (int)CrypticTier.SGEL);
            Assert.Equal(3, (int)CrypticTier.CGEL);
            Assert.Equal(4, (int)CrypticTier.CGoA);
            Assert.Equal(5, (int)CrypticTier.cSGEL);
        }

        [Fact]
        public void CrypticEmission_Pointers_PreserveOrder()
        {
            // Goal: Verify that the list preserves order as assigned (no internal sorting by DTO)
            var p1 = new CrypticPointer { tier = CrypticTier.CGoA, pointer = "hash_1" };
            var p2 = new CrypticPointer { tier = CrypticTier.cSGEL, pointer = "hash_2" };
            
            var emission = new CrypticEmission
            {
                tier = CrypticTier.CGEL,
                pointers = new List<CrypticPointer> { p2, p1 } // Assigned in specific order
            };

            Assert.NotNull(emission.pointers);
            Assert.Equal(2, emission.pointers.Count);
            Assert.Equal("hash_2", emission.pointers[0].pointer);
            Assert.Equal("hash_1", emission.pointers[1].pointer);
        }

        [Fact]
        public void DTO_Basics_InitializeManual()
        {
            var directive = new FreezeDirective { is_frozen = true, tick = 123, reason_code = "POLICY" };
            Assert.True(directive.is_frozen);
            Assert.Equal(123, directive.tick);
            Assert.Equal("POLICY", directive.reason_code);
        }
    }
}
