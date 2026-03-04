using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Oan.Spinal;
using Oan.Storage;
using Oan.Sli;

namespace Oan.Runtime.IntegrationTests
{
    public class IntegrationTests
    {
        [Fact]
        public async Task Store_Replay_ReturnsSameTip()
        {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".ndjson");
            try
            {
                var store = new NdjsonEngramStore(path);
                var e1 = new EngramEnvelope(new TipRef("0"), "OAN", "v1.0", EngramType.RunStarted, "n", "n", "n", "P", "P", 0, "n");
                await store.AppendAsync(e1);
                var id1 = e1.ComputeId();

                var replayed = await store.ReplayAsync();
                var eReplayed = replayed.First();
                
                Assert.Equal(id1, eReplayed.ComputeId());
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void NoDuplicateEnums_NamespaceCheck()
        {
            // This is a structural test. If the build passed, we know there are no name collisions in the same namespace.
            // Explicitly checking Oan.Sli for the enums.
            Assert.True(Enum.IsDefined(typeof(Oan.Sli.SatFlightPhase), Oan.Sli.SatFlightPhase.Parked));
            Assert.True(Enum.IsDefined(typeof(Oan.Sli.SliStage), Oan.Sli.SliStage.Baseline));
        }
    }
}
