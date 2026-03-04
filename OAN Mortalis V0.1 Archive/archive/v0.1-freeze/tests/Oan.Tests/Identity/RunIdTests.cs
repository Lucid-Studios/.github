using Xunit;
using Oan.SoulFrame.Identity;

namespace Oan.Tests.Identity
{
    public class RunIdTests
    {
        [Fact]
        public void RunId_IsDeterministic_ForSameInputs()
        {
            string sessionId = "session-1";
            string scenario = "scenario-1";
            string operatorId = "op-1";
            long tick = 1000;

            string runId1 = TheaterIdentityService.ComputeRunIdDeterministic(sessionId, scenario, operatorId, tick);
            string runId2 = TheaterIdentityService.ComputeRunIdDeterministic(sessionId, scenario, operatorId, tick);

            Assert.Equal(runId1, runId2);
            Assert.NotEmpty(runId1);
        }

        [Fact]
        public void RunId_Differs_WhenAnyInputDiffers()
        {
            string sessionId = "session-1";
            string scenario = "scenario-1";
            string operatorId = "op-1";
            long tick = 1000;

            string baseId = TheaterIdentityService.ComputeRunIdDeterministic(sessionId, scenario, operatorId, tick);

            Assert.NotEqual(baseId, TheaterIdentityService.ComputeRunIdDeterministic("diff", scenario, operatorId, tick));
            Assert.NotEqual(baseId, TheaterIdentityService.ComputeRunIdDeterministic(sessionId, "diff", operatorId, tick));
            Assert.NotEqual(baseId, TheaterIdentityService.ComputeRunIdDeterministic(sessionId, scenario, "diff", tick));
            Assert.NotEqual(baseId, TheaterIdentityService.ComputeRunIdDeterministic(sessionId, scenario, operatorId, 1001));
        }
    }
}
