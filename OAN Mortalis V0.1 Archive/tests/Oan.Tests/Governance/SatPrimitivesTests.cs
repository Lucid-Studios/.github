using System;
using Xunit;
using Oan.Core.Governance;

namespace Oan.Tests.Governance
{
    public class SatPrimitivesTests
    {
        [Fact]
        public void Serialize_IsDeterministic_AndOrdinal_ExactJson()
        {
            var frame = new SatFrame
            {
                m = SatMode.Parked,
                scope = "cGoA.root",
                b = SatBond.Inactive,
                er = SatEntropyRegime.OAN,
                et = SatTrend.Stable,
                dl = SatDriftLevel.Low,
                tick = 100
            };

            var json = SatCanonicalizer.SerializeSatFrame(frame);

            // "b","dl","er","et","m","scope","tick"
            // Parked=0, Inactive=0, OAN=0, Stable=1, Low=0
            string expected = "{\"b\":0,\"dl\":0,\"er\":0,\"et\":1,\"m\":0,\"scope\":\"cGoA.root\",\"tick\":100}";
            Assert.Equal(expected, json);
        }

        [Fact]
        public void Serialize_OmitsOptionalFields_WhenNullOrEmpty()
        {
            var frame = new SatFrame
            {
                m = SatMode.Idling,
                scope = "TEST",
                b = SatBond.Active,
                er = SatEntropyRegime.OE,
                et = SatTrend.Rising,
                dl = SatDriftLevel.High,
                tick = 1,
                note = "",
                operator_id = null
            };

            var json = SatCanonicalizer.SerializeSatFrame(frame);
            
            Assert.DoesNotContain("note", json);
            Assert.DoesNotContain("operator_id", json);
        }

        [Fact]
        public void Hash_IsLowerHex64()
        {
            var frame = new SatFrame { scope = "ROOT", tick = 1 };
            var hash = SatCanonicalizer.HashSatFrame(frame);

            Assert.Equal(64, hash.Length);
            Assert.Matches("^[0-9a-f]{64}$", hash);
        }

        [Fact]
        public void Hash_Changes_WhenOneFieldChanges()
        {
            var f1 = new SatFrame { scope = "ROOT", tick = 1 };
            var f2 = new SatFrame { scope = "ROOT", tick = 2 };

            Assert.NotEqual(SatCanonicalizer.HashSatFrame(f1), SatCanonicalizer.HashSatFrame(f2));
        }

        [Fact]
        public void Enum_NumericOrdering_LockTest()
        {
            // Lock the numeric values of enums to prevent accidental reordering in SatPrimitives.cs
            Assert.Equal(0, (int)SatMode.Parked);
            Assert.Equal(1, (int)SatMode.Idling);
            Assert.Equal(0, (int)SatBond.Inactive);
            Assert.Equal(2, (int)SatBond.Active); // Preparing=1
            Assert.Equal(1, (int)SatTrend.Stable);
            Assert.Equal(0, (int)SatDriftLevel.Low);
            Assert.Equal(2, (int)SatDriftLevel.High);
        }
    }
}
