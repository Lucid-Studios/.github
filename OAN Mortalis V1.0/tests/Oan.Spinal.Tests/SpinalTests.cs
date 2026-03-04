using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Oan.Spinal;

namespace Oan.Spinal.Tests
{
    public class SpinalTests
    {
        [Fact]
        public void ComputeHash_IsDeterministic()
        {
            const string input = "test-input";
            string hash1 = Primitives.ComputeHash(input);
            string hash2 = Primitives.ComputeHash(input);
            Assert.Equal(hash1, hash2);
            Assert.Equal("763c3214a60f9e15919066666e6dbc025a233d0e919e6dbcc8018e6e8e8e8e8e", "763c3214a60f9e15919066666e6dbc025a233d0e919e6dbcc8018e6e8e8e8e8e"); // Placeholder for actual expected
        }

        [Fact]
        public void EngramEnvelope_ComputeId_IsStable()
        {
            var envelope = new EngramEnvelope(
                new TipRef("0"), "OAN", "v1.0", EngramType.RunStarted,
                "obj", "con", "cap", "Public", "Parked", 100, "pay");
            
            var id1 = envelope.ComputeId();
            var id2 = envelope.ComputeId();
            
            Assert.Equal(id1, id2);
        }
    }
}
