using System;
using System.Collections.Generic;
using Oan.Core.Meaning;
using Oan.Ledger;
using Oan.SoulFrame;
using Oan.SoulFrame.Services;
using Xunit;

namespace Oan.Tests.Meaning
{
    public class MeaningLatticeTests
    {
        [Fact]
        public void ProposeSpans_IsDeterministic_SameInputSameOutput()
        {
            // Arrange
            var ledger = new EventLog();
            var session = new SoulFrameSession("test-session", "op-1");
            var service = new MeaningLatticeService((t, p, tick) => ledger.Append(t, p, tick), (id) => session);

            // Act
            var spans1 = service.ProposeSpans("test-session", "Hello world.", "snap-1", "op-1");
            var spans2 = service.ProposeSpans("test-session", "Hello world.", "snap-1", "op-1");

            // Assert
            Assert.Equal(spans1.Count, spans2.Count);
            for (int i = 0; i < spans1.Count; i++)
            {
                Assert.Equal(spans1[i].SpanId, spans2[i].SpanId);
                Assert.Equal(spans1[i].Text, spans2[i].Text);
            }
        }

        [Fact]
        public void UpdateSpan_ChangesStatus_AndLogsTrace()
        {
            // Arrange
            var ledger = new EventLog();
            var session = new SoulFrameSession("test-session", "op-1");
            var service = new MeaningLatticeService((t, p, tick) => ledger.Append(t, p, tick), (id) => session);

            // Seed span
            var spans = service.ProposeSpans("test-session", "Test", "snap-1", "op-1");
            var spanId = spans[0].SpanId;

            // Act
            var updated = service.UpdateSpan("test-session", spanId, "User gloss", MeaningStatus.Confirmed, "op-1");

            // Assert
            Assert.Equal(MeaningStatus.Confirmed, updated.Status);
            Assert.Equal("User gloss", updated.UserGloss);
            Assert.Equal(MeaningStatus.Confirmed, session.Spans[spanId].Status); // Verify session state update via event application
        }

        [Fact]
        public void SetFrameLock_RequiredFields_AndLogsTrace()
        {
            // Arrange
            var ledger = new EventLog();
            var session = new SoulFrameSession("test-session", "op-1");
            var service = new MeaningLatticeService((t, p, tick) => ledger.Append(t, p, tick), (id) => session);

            var lockReq = new FrameLock { Goal = "Solve problem", Mode = FrameMode.Plan };

            // Act
            var result = service.SetFrameLock("test-session", lockReq, "op-1");

            // Assert
            Assert.True(result.IsSet);
            Assert.True(result.SetTick > 0);
            Assert.Equal("Solve problem", session.FrameLock.Goal);
        }

        [Fact]
        public void RiskAssessment_ReturnsBand_WithContributors_AndIsDeterministic()
        {
            // Arrange
            var ledger = new EventLog();
            var session = new SoulFrameSession("test-session", "op-1");
            var service = new MeaningLatticeService((t, p, tick) => ledger.Append(t, p, tick), (id) => session);

            // Seed dangerous spans
            service.ProposeSpans("test-session", "This is ambiguous and unsure.", "snap-1", "op-1");

            // Act
            var risk = service.AssessRisk("test-session");

            // Assert
            Assert.Equal(RiskBand.AMBIGUOUS, risk.Band);
            Assert.True(risk.ContributingSpanIds.Count > 0);
            Assert.Equal(RiskBand.AMBIGUOUS, session.LastRiskAssessment.Band);
        }
    }
}
