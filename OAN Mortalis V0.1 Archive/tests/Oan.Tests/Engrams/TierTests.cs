using System;
using System.Collections.Generic;
using Xunit;
using Oan.Core.Engrams;
using Oan.Core.Meaning;
using Oan.AgentiCore.Engrams;

namespace Oan.Tests.Engrams
{
    public class TierTests
    {
        private EngramFormationService CreateService()
        {
            var store = new EngramStore();
            var router = new EngramRouter();
            return new EngramFormationService(store, router);
        }

        private FormationContext CreateContext(string sessionId = "sess-1")
        {
            return new FormationContext
            {
                PolicyVersion = "v1",
                Tick = 100,
                SessionId = sessionId,
                OperatorId = "op-1",
                RootId = "root-1",
                OpalRootId = "opal-1",
                FrameLock = new FrameLock { Goal = "Test Goal" },
                FormationLevel = "HigherFormation",
                TheaterMode = "Prime",
                CradleId = "cradle-1",
                ContextId = "ctx-1",
                TheaterId = "theater-1",
                IsOePrivileged = true // Default to privileged for convenience
            };
        }

        [Fact]
        public void GOA_NeverBinds_EvenPrimeHigherFormation()
        {
            var service = CreateService();
            var ctx = CreateContext() with 
            { 
                ArchiveTier = ArchiveTier.GOA,
                IsBindingAttempt = true, // Attempt binding
                TheaterMode = "Prime",
                FormationLevel = "HigherFormation"
            };

            // Should succeed (no exception) but store in GOA
            var engram = service.FormEngram(ctx);

            Assert.Equal(ArchiveTier.GOA, engram.Header.ArchiveTier);
            // Verify Logic: The "Never Binds" property is structural. 
            // It sits in GOA tier, so downstream systems (Indexer) will ignore it for Identity Binding.
            // We verify here that it was allowed to be created without throwing "GEL Binding" errors.
        }

        [Fact]
        public void CGEL_CannotWriteDirectlyToGEL()
        {
            var service = CreateService();
            // Default Router returns SelfGEL if not Speculative/RoleBound/Shared
            var ctx = CreateContext() with 
            { 
                ArchiveTier = ArchiveTier.CGEL
            };

            Assert.Throws<InvalidOperationException>(() => service.FormEngram(ctx));
        }

        [Fact]
        public void GEL_RefusesBinding_When_Not_Prime()
        {
            var service = CreateService();
            var ctx = CreateContext() with 
            { 
                ArchiveTier = ArchiveTier.GEL,
                IsBindingAttempt = true,
                TheaterMode = "OAN", // Not Prime
                FormationLevel = "HigherFormation"
            };

            Assert.Throws<InvalidOperationException>(() => service.FormEngram(ctx));
        }

        [Fact]
        public void GEL_RefusesBinding_When_Not_HigherFormation()
        {
            var service = CreateService();
            var ctx = CreateContext() with 
            { 
                ArchiveTier = ArchiveTier.GEL,
                IsBindingAttempt = true,
                TheaterMode = "Prime",
                FormationLevel = "Constructor" // Not Higher
            };

            Assert.Throws<InvalidOperationException>(() => service.FormEngram(ctx));
        }

        [Fact]
        public void GEL_AllowsBinding_When_Prime_And_Higher()
        {
            var service = CreateService();
            var ctx = CreateContext() with 
            { 
                ArchiveTier = ArchiveTier.GEL,
                IsBindingAttempt = true,
                TheaterMode = "Prime",
                FormationLevel = "HigherFormation"
            };

            var engram = service.FormEngram(ctx);
            Assert.Equal(ArchiveTier.GEL, engram.Header.ArchiveTier);
        }

        [Fact]
        public void GOA_Allows_Intrinsic_Hash_When_NonBinding()
        {
            var service = CreateService();
            
            // Two contexts with different session IDs but identical content
            // Using GOA tier, non-binding
            var ctx1 = CreateContext("sess-1") with { ArchiveTier = ArchiveTier.GOA };
            var ctx2 = CreateContext("sess-2") with { ArchiveTier = ArchiveTier.GOA };
            
            var e1 = service.FormEngram(ctx1);
            var e2 = service.FormEngram(ctx2);

            // Expect Intrinsic Profile (Default) -> Hashes match
            Assert.Equal(e1.Hash, e2.Hash);
            Assert.Equal(ArchiveTier.GOA, e1.Header.ArchiveTier);
        }

        [Fact]
        public void GOA_Allows_Contextual_Hash_When_BindingAttempt()
        {
            var service = CreateService();
            
            // Two contexts with different session IDs
            // Using GOA tier, BUT IsBindingAttempt = true
            // This forces Contextual Profile Hash, even though Tier is GOA
            var ctx1 = CreateContext("sess-1") with 
            { 
                ArchiveTier = ArchiveTier.GOA,
                IsBindingAttempt = true 
            };
            var ctx2 = CreateContext("sess-2") with 
            { 
                ArchiveTier = ArchiveTier.GOA,
                IsBindingAttempt = true 
            };
            
            var e1 = service.FormEngram(ctx1);
            var e2 = service.FormEngram(ctx2);

            // Expect Contextual Profile -> Hashes differ (SessionId included)
            Assert.NotEqual(e1.Hash, e2.Hash);
            Assert.Equal(ArchiveTier.GOA, e1.Header.ArchiveTier);
        }

        [Fact]
        public void GOA_WriteRejected_When_NotOePrivileged()
        {
            var service = CreateService();
            var ctx = CreateContext() with 
            { 
                ArchiveTier = ArchiveTier.GOA,
                IsOePrivileged = false // Access Denied
            };

            Assert.Throws<UnauthorizedAccessException>(() => service.FormEngram(ctx));
        }

        [Fact]
        public void GOA_RefusesBinding_When_Not_Prime()
        {
            var service = CreateService();
            var ctx = CreateContext() with 
            { 
                ArchiveTier = ArchiveTier.GOA,
                IsBindingAttempt = true,
                TheaterMode = "OAN", // Not Prime
                FormationLevel = "HigherFormation"
            };

            Assert.Throws<InvalidOperationException>(() => service.FormEngram(ctx));
        }

        [Fact]
        public void GOA_RefusesPromotion_When_Not_HigherFormation()
        {
            var service = CreateService();
            var ctx = CreateContext() with 
            { 
                ArchiveTier = ArchiveTier.GOA,
                IsPromotion = true,
                TheaterMode = "Prime",
                FormationLevel = "Constructor" // Not Higher
            };

            Assert.Throws<InvalidOperationException>(() => service.FormEngram(ctx));
        }

        [Fact]
        public void CGEL_WriteRejected_When_NotOePrivileged()
        {
            var service = CreateService();
            var ctx = CreateContext() with 
            { 
                ArchiveTier = ArchiveTier.CGEL,
                IsOePrivileged = false // Access Denied
            };

            Assert.Throws<UnauthorizedAccessException>(() => service.FormEngram(ctx));
        }
    }
}
