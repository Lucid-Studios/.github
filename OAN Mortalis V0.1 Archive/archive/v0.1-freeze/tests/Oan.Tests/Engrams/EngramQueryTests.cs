using System;
using System.Collections.Generic;
using System.Linq;
using Oan.AgentiCore.Engrams;
using Oan.Core.Engrams;
using Oan.Core.Meaning;
using Oan.Runtime; // For IntentProcessor isolation check
using Xunit;

namespace Oan.Tests.Engrams
{
    public class EngramQueryTests
    {
        [Fact]
        public void Query_ReturnsDeterministicOrder_TickThenId()
        {
            var store = new EngramStore();
            var service = new EngramQueryService(store);

            // Create 3 blocks
            // B1: Tick 10, Id "B"
            // B2: Tick 10, Id "A"
            // B3: Tick 5, Id "C"
            
            // Note: We simulate IDs and Ticks.
            // EngramStore requires Append with canonical bytes.
            
            var b3 = CreateBlock("C", 5);
            var b1 = CreateBlock("B", 10);
            var b2 = CreateBlock("A", 10);

            var bytes = System.Text.Encoding.UTF8.GetBytes("fake");

            store.Append(b1, bytes);
            store.Append(b2, bytes);
            store.Append(b3, bytes);

            var info = service.QueryByRootId("root", 10);
            
            Assert.Equal(3, info.Count);
            Assert.Equal("C", info[0].EngramId); // Tick 5
            Assert.Equal("A", info[1].EngramId); // Tick 10, Id A
            Assert.Equal("B", info[2].EngramId); // Tick 10, Id B
        }

        [Fact]
        public void QueryByStance_FiltersCorrectly()
        {
            var store = new EngramStore();
            var service = new EngramQueryService(store);
            
            var b1 = CreateBlockWithStance("1", KnowingMode.Propositional);
            var b2 = CreateBlockWithStance("2", KnowingMode.Perspectival);

            var bytes = System.Text.Encoding.UTF8.GetBytes("fake");
            store.Append(b1, bytes);
            store.Append(b2, bytes);

            var res = service.QueryByStance(knowing: KnowingMode.Perspectival);
            Assert.Single(res);
            Assert.Equal("2", res[0].EngramId);
        }

        [Fact]
        public void Isolation_IntentProcessor_HasNoEngramServices()
        {
            var ctor = typeof(IntentProcessor).GetConstructors().First();
            var paramsInfo = ctor.GetParameters();

            foreach (var p in paramsInfo)
            {
                var typeName = p.ParameterType.Name;
                Assert.NotEqual("EngramStore", typeName);
                Assert.NotEqual("EngramQueryService", typeName);
                Assert.NotEqual("EngramFormationService", typeName);
            }
        }

        private EngramBlock CreateBlock(string id, long tick)
        {
            return new EngramBlock
            {
                Header = new EngramBlockHeader
                {
                    PolicyVersion = "1", Tick = tick, SessionId = "s", OperatorId = "o",
                    Channel = EngramChannel.SelfGEL, RoutingReason = "r", RootId = "root",
                    ConstructionTier = ConstructionTier.Basic, OpalRootId = "opal"
                },
                Factors = new List<EngramFactor>(),
                Refs = new List<string>(),
                EngramId = id,
                Hash = id
            };
        }

        private EngramBlock CreateBlockWithStance(string id, KnowingMode mode)
        {
            return new EngramBlock
            {
                Header = new EngramBlockHeader
                {
                    PolicyVersion = "1", Tick = 1, SessionId = "s", OperatorId = "o",
                    Channel = EngramChannel.SelfGEL, RoutingReason = "r", RootId = "root",
                    ConstructionTier = ConstructionTier.Basic, OpalRootId = "opal"
                },
                Factors = new List<EngramFactor> 
                { 
                    new EngramFactor { Tier = FactorTier.RootBase, Kind = EngramFactorKind.KnowingMode, Key="KnowingMode", Value=mode.ToString(), Order=10 }
                },
                Refs = new List<string>(),
                EngramId = id,
                Hash = id
            };
        }
    }
}
