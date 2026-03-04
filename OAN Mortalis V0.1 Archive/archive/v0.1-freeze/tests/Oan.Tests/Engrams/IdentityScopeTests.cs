using System;
using System.Collections.Generic;
using Xunit;
using Oan.Core.Engrams;
using Oan.Core.Meaning;
using Oan.AgentiCore.Engrams;

namespace Oan.Tests.Engrams
{
    public class IdentityScopeTests
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
                TheaterId = "theater-1"
            };
        }

        [Fact]
        public void BindingAttempt_UsesContextualProfile()
        {
            var service = CreateService();
            
            // Context 1: T1
            var ctx1 = CreateContext() with 
            { 
                IsBindingAttempt = true, 
                TheaterId = "theater-A" 
            };
            var engram1 = service.FormEngram(ctx1);

            // Context 2: T2 (Content same, Context differs)
            var ctx2 = CreateContext() with 
            { 
                IsBindingAttempt = true, 
                TheaterId = "theater-B" 
            };
            var engram2 = service.FormEngram(ctx2);

            // Should be different because Contextual Profile includes TheaterId
            Assert.NotEqual(engram1.Hash, engram2.Hash);
        }

        [Fact]
        public void Promotion_UsesContextualProfile()
        {
            var service = CreateService();
            
            // Context 1: Session A
            var ctx1 = CreateContext() with 
            { 
                IsPromotion = true, 
                SessionId = "SessA"
            };
            var engram1 = service.FormEngram(ctx1);

            // Context 2: Session B
            var ctx2 = CreateContext() with 
            { 
                IsPromotion = true, 
                SessionId = "SessB"
            };
            var engram2 = service.FormEngram(ctx2);

            // Should be different because Contextual Profile includes SessionId
            Assert.NotEqual(engram1.Hash, engram2.Hash);
        }

        [Fact]
        public void CrossCradle_UsesIntrinsicProfile()
        {
            var service = CreateService();
            
            // Context 1
            var ctx1 = CreateContext() with 
            { 
                IsCrossCradle = true, 
                CradleId = "cradle-A",
                SessionId = "sess-A"
            };
            var engram1 = service.FormEngram(ctx1);

            // Context 2 (Different Cradle/Session, same Content)
            var ctx2 = CreateContext() with 
            { 
                IsCrossCradle = true, 
                CradleId = "cradle-B",
                SessionId = "sess-B"
            };
            var engram2 = service.FormEngram(ctx2);

            // Should match because Intrinsic Profile excludes CradleId/SessionId
            // Note: Header.SessionId will differ in the object, but Hash is derived from Canonical String.
            Assert.Equal(engram1.Hash, engram2.Hash);
            
            // Verify IDs explicitly
            Assert.Equal(engram1.EngramId, engram2.EngramId);
        }

        [Fact]
        public void IntrinsicProfile_HashStableAcrossCradle()
        {
            // Same as CrossCradle, just explicit naming for requirement
            CrossCradle_UsesIntrinsicProfile();
        }

        [Fact]
        public void ContextualProfile_HashDiffersAcrossTheater()
        {
            var service = CreateService();
            
            // Force Contextual by Binding
            var ctx1 = CreateContext() with { IsBindingAttempt = true, TheaterId = "T1" };
            var ctx2 = CreateContext() with { IsBindingAttempt = true, TheaterId = "T2" };

            var e1 = service.FormEngram(ctx1);
            var e2 = service.FormEngram(ctx2);

            Assert.NotEqual(e1.Hash, e2.Hash);
        }

        [Fact]
        public void Default_IsIntrinsic()
        {
            // If neither Binding, Promotion, nor CrossCradle -> Default Intrinsic
            var service = CreateService();
            
            var ctx1 = CreateContext() with { SessionId = "S1" };
            var ctx2 = CreateContext() with { SessionId = "S2" }; // Change Context field

            var e1 = service.FormEngram(ctx1);
            var e2 = service.FormEngram(ctx2);

            // Should match (Intrinsic ignores SessionId)
            Assert.Equal(e1.Hash, e2.Hash);
        }
    }
}
