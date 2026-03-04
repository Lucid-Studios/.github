using Oan.Core.Meaning;
using Oan.SoulFrame;
using Oan.SoulFrame.Gates;
using Xunit;

namespace Oan.Tests.Meaning
{
    public class FrameLockGateTests
    {
        [Fact]
        public void FrameLockGate_Refuses_When_NotSet()
        {
            // Arrange
            var session = new SoulFrameSession("test-session", "op-1");
            var gate = new FrameLockGate();

            // Act
            bool canProceed = gate.CanProceed(session, out string reason, out string explanation);

            // Assert
            Assert.False(canProceed);
            Assert.Equal("SOULFRAME.FRAMELOCK_REQUIRED", reason);
        }

        [Fact]
        public void FrameLockGate_Allows_When_Set()
        {
            // Arrange
            var session = new SoulFrameSession("test-session", "op-1");
            // Simulate Apply event or manually set property if internal setter (it's private set, so usage of event or reflection needed, or use service)
            // Ideally we use the service or events. Here we can use MeaningLatticeService to set it properly.
            
            // Just simulate via event application for purity
            var evt = new Oan.Core.Events.DialecticTraceEvent 
            { 
               EventId = "1", SessionId="s1", Kind = Oan.Core.Events.DialecticEventType.FrameLockSet, PolicyVersion = "1",
               Payload = new FrameLock { Goal = "Goal", IsSet = true }
            };
            session.Apply(evt);

            var gate = new FrameLockGate();

            // Act
            bool canProceed = gate.CanProceed(session, out string reason, out string explanation);

            // Assert
            Assert.True(canProceed);
            Assert.Equal("SUCCESS", reason);
        }
    }
}
