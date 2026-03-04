using System;
using System.Collections.Generic;
using System.IO;
using Oan.Core.Events;
using Oan.Runtime.Persistence;
using Oan.SoulFrame.Identity;
using Xunit;

namespace Oan.Tests.Identity
{
    public class MockPersistenceSink : ISnapshotTelemetrySink
    {
        public List<object> Events { get; } = new List<object>();
        public void Emit(object evt) => Events.Add(evt);
    }

    public class OpalTipPersistenceTests : IDisposable
    {
        private readonly string _testFile = "test_tips.json";

        public void Dispose()
        {
            if (File.Exists(_testFile)) File.Delete(_testFile);
        }

        [Fact]
        public void Rehydrate_ShouldContinueChain()
        {
            // Arrange - Session 1
            var registry1 = new OpalTipRegistry();
            var sink1 = new MockPersistenceSink();
            var service1 = new TipSnapshotService(registry1, sink1);
            
            string theaterId = "theater-1";
            string runId = "test-run";
            string sessionId = "session-1";
            string atlasHash = "hash-1";
            string policy = "policy-1";

            registry1.TryAdvanceTip(theaterId, null, "tip-1");
            registry1.TryAdvanceTip(theaterId, "tip-1", "tip-2");

            // Act - Save
            service1.Save(_testFile, runId, sessionId, atlasHash, policy);

            // Arrange - Session 2 (New Registry)
            var registry2 = new OpalTipRegistry();
            var sink2 = new MockPersistenceSink();
            var service2 = new TipSnapshotService(registry2, sink2);

            // Act - Load
            bool loaded = service2.TryLoad(_testFile, atlasHash, policy);

            // Assert
            Assert.True(loaded);
            Assert.Equal("tip-2", registry2.GetTip(theaterId));
            
            // Act - Continue Chain
            bool advance = registry2.TryAdvanceTip(theaterId, "tip-2", "tip-3");
            Assert.True(advance);
            Assert.Equal("tip-3", registry2.GetTip(theaterId));
        }

        [Fact]
        public void Rehydrate_Mismatch_ShouldReject()
        {
            // Arrange
            var registry = new OpalTipRegistry();
            var sink = new MockPersistenceSink();
            var service = new TipSnapshotService(registry, sink);
            
            // Create a valid file manually but with mismatching props
            var snapshot = new OpalTipSnapshot
            {
                RootAtlasHash = "correct-hash",
                PolicyVersion = "correct-policy"
            };
            File.WriteAllText(_testFile, snapshot.ToDeterministicJson());

            // Act - Load with WRONG hash
            bool result = service.TryLoad(_testFile, "WRONG-HASH", "correct-policy");

            // Assert
            Assert.False(result);
            Assert.True(registry.GetAllTips().Count == 0); // Registry empty
            Assert.Contains(sink.Events, e => e is TipSnapshotRejectedEvent re && re.ReasonCode == "ROOT_ATLAS_MISMATCH");
        }

        [Fact]
        public void Rehydrate_Missing_ShouldStartEmpty()
        {
             // Arrange
            var registry = new OpalTipRegistry();
            var sink = new MockPersistenceSink();
            var service = new TipSnapshotService(registry, sink);
            
            if (File.Exists(_testFile)) File.Delete(_testFile);

            // Act
            bool result = service.TryLoad(_testFile, "any", "any");

            // Assert
            Assert.False(result);
            Assert.True(registry.GetAllTips().Count == 0);
            Assert.Contains(sink.Events, e => e is TipSnapshotMissingEvent);
        }
    }
}
