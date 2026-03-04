using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oan.AgentiCore.Engrams;
using Oan.Core.Engrams;
using Oan.Core.Meaning;
using Xunit;

namespace Oan.Tests.Engrams
{
    public class EngramMvpTests
    {
        [Fact]
        public void EngramFormation_IsDeterministic_SameInputsSameHash()
        {
            var store = new EngramStore();
            var router = new EngramRouter();
            var service = new EngramFormationService(store, router);

            var context = CreateContext(); // Helper now returns full context

            var block1 = service.FormEngram(context);
            var block2 = service.FormEngram(context);

            Assert.Equal(block1.EngramId, block2.EngramId);
            Assert.Equal(block1.Hash, block2.Hash);
        }

        [Fact]
        public void KnowingMode_ChangesHash()
        {
            var service = new EngramFormationService(new EngramStore(), new EngramRouter());
            
            var c1 = CreateContext(knowing: KnowingMode.Propositional);
            var c2 = CreateContext(knowing: KnowingMode.Participatory); // Different

            var b1 = service.FormEngram(c1);
            var b2 = service.FormEngram(c2);

            Assert.NotEqual(b1.EngramId, b2.EngramId);
        }

        [Fact]
        public void MetabolicRegime_ChangesHash()
        {
            var service = new EngramFormationService(new EngramStore(), new EngramRouter());
            
            var c1 = CreateContext(metabolic: MetabolicRegime.Exploration);
            var c2 = CreateContext(metabolic: MetabolicRegime.Coherence);

            var b1 = service.FormEngram(c1);
            var b2 = service.FormEngram(c2);

            Assert.NotEqual(b1.EngramId, b2.EngramId);
        }

        [Fact]
        public void ResolutionMode_ChangesHash()
        {
            var service = new EngramFormationService(new EngramStore(), new EngramRouter());
            
            var c1 = CreateContext(resolution: ResolutionMode.Coarse);
            var c2 = CreateContext(resolution: ResolutionMode.Fine);

            var b1 = service.FormEngram(c1);
            var b2 = service.FormEngram(c2);

            Assert.NotEqual(b1.EngramId, b2.EngramId);
        }

        [Fact]
        public void StanceFactors_Present_InCanonicalPayload()
        {
            var service = new EngramFormationService(new EngramStore(), new EngramRouter());
            var context = CreateContext(
                knowing: KnowingMode.Perspectival, 
                metabolic: MetabolicRegime.Hold, 
                resolution: ResolutionMode.Fine
            );

            var block = service.FormEngram(context);

            Assert.Contains(block.Factors, f => f.Kind == EngramFactorKind.KnowingMode && f.Value == "Perspectival");
            Assert.Contains(block.Factors, f => f.Kind == EngramFactorKind.MetabolicRegime && f.Value == "Hold");
            Assert.Contains(block.Factors, f => f.Kind == EngramFactorKind.ResolutionMode && f.Value == "Fine");
        }

        [Fact]
        public void EngramFormation_OrderIsStable_ShuffleInput()
        {
            var store = new EngramStore();
            var router = new EngramRouter();
            var service = new EngramFormationService(store, router);

            var constraints = new List<string> { "B", "A", "C" };
            var shuffledConstraints = new List<string> { "C", "B", "A" };

            var context1 = CreateContext(constraints: constraints);
            var context2 = CreateContext(constraints: shuffledConstraints);

            var block1 = service.FormEngram(context1);
            var block2 = service.FormEngram(context2);

            Assert.Equal(block1.EngramId, block2.EngramId);
        } 
        
        // ... (EngramStore test remains same/compatible) ...
        
        [Fact]
        public void EngramStore_IsAppendOnly_WithByteIntegrity()
        {
            var store = new EngramStore();
            var header = new EngramBlockHeader 
            { 
                 SessionId = "s1", OperatorId = "op1", Tick = 1, PolicyVersion = "v1", 
                 Channel = EngramChannel.SelfGEL, RoutingReason = "test", RootId = "r1", 
                 ConstructionTier = ConstructionTier.Basic, OpalRootId = "o1" 
            };
            var block = new EngramBlock 
            { 
                Header = header, 
                Factors = new List<EngramFactor>(), 
                Refs = new List<string>(), 
                EngramId = "id1",
                Hash = "hash1"
            };

            var validBytes = Encoding.UTF8.GetBytes("canonical-content");
            var invalidBytes = Encoding.UTF8.GetBytes("corrupted-content");

            store.Append(block, validBytes);
            
            // Idempotent retry (same bytes)
            store.Append(block, validBytes); 

            // Conflict (different bytes)
            Assert.Throws<InvalidOperationException>(() => store.Append(block, invalidBytes));
        }

        // New Tests for Routing Precedence & Refs
        [Fact]
        public void Routing_Precedence_Shared_Trumps_Role_Trumps_Speculative()
        {
             var router = new EngramRouter();
             
             // All true -> Shared
             var c1 = CreateContext(speculative: true, role: true, shared: true);
             var (ch1, r1) = router.Route(c1);
             Assert.Equal(EngramChannel.SharedGEL, ch1);

             // Role + Speculative -> OAN
             var c2 = CreateContext(speculative: true, role: true, shared: false);
             var (ch2, r2) = router.Route(c2);
             Assert.Equal(EngramChannel.OAN, ch2);

             // Speculative Only -> GOA
             var c3 = CreateContext(speculative: true, role: false, shared: false);
             var (ch3, r3) = router.Route(c3);
             Assert.Equal(EngramChannel.GOA, ch3);
             
             // None -> Self
             var c4 = CreateContext();
             var (ch4, r4) = router.Route(c4);
             Assert.Equal(EngramChannel.SelfGEL, ch4);
        }

        [Fact]
        public void EvidenceRefs_Shuffle_SameHash()
        {
            var store = new EngramStore();
            var router = new EngramRouter();
            var service = new EngramFormationService(store, router);

            var r1 = new EngramRef { TargetId = "A", Relationship = "Link" };
            var r2 = new EngramRef { TargetId = "B", Relationship = "Link" };

            var c1 = CreateContext();
            c1 = c1 with { EvidenceRefs = new List<EngramRef> { r1, r2 } };

            var c2 = CreateContext();
            c2 = c2 with { EvidenceRefs = new List<EngramRef> { r2, r1 } }; // Shuffled

            var b1 = service.FormEngram(c1);
            var b2 = service.FormEngram(c2);

            Assert.Equal(b1.EngramId, b2.EngramId);
            Assert.Equal(2, b1.Refs.Count);
            Assert.Equal("Link:A", b1.Refs[0]); // Sorted
        }

        [Fact]
        public void FormationContext_NullLists_NormalizeToEmpty()
        {
            var context = new FormationContext
            {
                PolicyVersion = "v1",
                Tick = 1,
                SessionId = "s",
                OperatorId = "o",
                RootId = "r",
                OpalRootId = "or",
                FrameLock = new FrameLock { Goal = "g", Constraints = new List<string>() },
                // Explicitly set lists to null (simulating unsafe caller)
                ParentEngramIds = null!,
                Spans = null!,
                EvidenceRefs = null!
            };

            Assert.NotNull(context.ParentEngramIds);
            Assert.Empty(context.ParentEngramIds);

            Assert.NotNull(context.Spans);
            Assert.Empty(context.Spans);

            Assert.NotNull(context.EvidenceRefs);
            Assert.Empty(context.EvidenceRefs);
        }

        private FormationContext CreateContext(
            List<string>? constraints = null, 
            bool speculative = false,
            bool role = false,
            bool shared = false,
            KnowingMode knowing = KnowingMode.Propositional,
            MetabolicRegime metabolic = MetabolicRegime.Exploration,
            ResolutionMode resolution = ResolutionMode.Normal)
        {
            return new FormationContext
            {
                PolicyVersion = "v1",
                Tick = 100,
                SessionId = "s1",
                OperatorId = "op1",
                RootId = "r1",
                OpalRootId = "o1",
                
                FrameLock = new FrameLock 
                { 
                    Goal = "Test Goal", 
                    Constraints = constraints ?? new List<string>(),
                    Assumptions = new List<string>() 
                },
                Spans = new List<MeaningSpan>(),
                
                Speculative = speculative,
                RoleBound = role,
                SharedEligible = shared,
                IdentityLocal = true,
                
                KnowingMode = knowing,
                MetabolicRegime = metabolic,
                ResolutionMode = resolution,
                
                ParentEngramIds = new List<string>(),
                EvidenceRefs = new List<EngramRef>()
            };
        }
    }
}
