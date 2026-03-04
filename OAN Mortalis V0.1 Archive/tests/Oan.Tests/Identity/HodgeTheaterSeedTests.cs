using Xunit;
using Oan.Core.Governance;
using Oan.SoulFrame.Identity;
using Oan.SoulFrame.Atlas;

namespace Oan.Tests.Identity
{
    public class HodgeTheaterSeedTests
    {
        [Fact]
        public void ComputeRootAtlasHash_IsDeterministic()
        {
            // Act
            string hash1 = TheaterIdentityService.ComputeRootAtlasHash();
            string hash2 = TheaterIdentityService.ComputeRootAtlasHash();

            // Assert
            Assert.Equal(hash1, hash2);
            Assert.NotEmpty(hash1);
        }

        [Fact]
        public void ComputeTheaterId_ProducesConsistentHash()
        {
            // Arrange
            string runId = "test-run";
            string atlasHash = "atlas-hash";
            string policy = "v1";
            long tick = 100;

            // Act
            string id1 = TheaterIdentityService.ComputeTheaterId(runId, atlasHash, policy, tick);
            string id2 = TheaterIdentityService.ComputeTheaterId(runId, atlasHash, policy, tick);

            // Assert
            Assert.Equal(id1, id2);
            // Verify it's lowercase hex string of 64 chars (SHA256)
            Assert.Matches("^[a-f0-9]{64}$", id1);
        }
    }
}
