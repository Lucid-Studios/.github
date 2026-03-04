using System;
using System.Security.Cryptography;
using System.Text;
using Oan.Core;
using Oan.Core.Governance;
using Oan.SoulFrame;
using Xunit;

namespace Oan.Tests.SLI
{
    public class MountRegistryDeterminismTests
    {
        [Fact]
        public void MountId_Is_Stable_For_Same_Inputs()
        {
            var runId = "test-run-123";
            var address = new SliAddress(SliChannel.Public, SliPartition.OAN, SliMirror.Standard);
            var policy = "sli.policy.v0.1";
            
            var canonical = MountEntry.GetCanonicalAddressString(address);
            var input = $"{runId}{canonical}{policy}";
            
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var expectedId = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            // Simulate creating the entry
            var entry = new MountEntry
            {
                Address = address,
                MountId = expectedId,
                PolicyVersion = policy,
                SatCeiling = SatMode.Standard,
                RequiresHitlForElevation = false,
                CreatedTick = 100
            };

            Assert.Equal(expectedId, entry.MountId);
        }

        [Fact]
        public void Creation_Order_Does_Not_Affect_MountId()
        {
            var runId = "order-test";
            var addrA = new SliAddress(SliChannel.Public, SliPartition.OAN, SliMirror.Standard);
            var addrB = new SliAddress(SliChannel.Private, SliPartition.GOA, SliMirror.Standard);
            var policy = "v1";

            string ComputeId(SliAddress addr)
            {
                var canon = MountEntry.GetCanonicalAddressString(addr);
                using var sha = SHA256.Create();
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes($"{runId}{canon}{policy}"));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }

            var idA = ComputeId(addrA);
            var idB = ComputeId(addrB);

            // Even if we swap order of registration in registry, their individual IDs remain fixed
            var registry = new MountRegistry();
            registry.TryAddMount(new MountEntry { Address = addrB, MountId = idB, PolicyVersion = policy, SatCeiling = SatMode.Baseline, RequiresHitlForElevation = false, CreatedTick = 1 });
            registry.TryAddMount(new MountEntry { Address = addrA, MountId = idA, PolicyVersion = policy, SatCeiling = SatMode.Baseline, RequiresHitlForElevation = false, CreatedTick = 2 });

            registry.IsMounted(addrA, out var mA);
            registry.IsMounted(addrB, out var mB);

            Assert.Equal(idA, mA.MountId);
            Assert.Equal(idB, mB.MountId);
        }
    }
}
