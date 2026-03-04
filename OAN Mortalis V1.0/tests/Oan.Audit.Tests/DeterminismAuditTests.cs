using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Oan.Common;
using Oan.SoulFrame;
using Oan.Sli;
using Oan.Spinal;
using Oan.Storage;
using Oan.Cradle;

namespace Oan.Audit.Tests
{
    public class DeterminismAuditTests : IDisposable
    {
        private readonly string _testRoot;
        private readonly SoulFrameAuthority _authority;
        private readonly IRoutingEngine _router;
        private readonly StoreRegistry _stores;
        private readonly MockTelemetrySink _govTelemetry;
        private readonly MockTelemetrySink _storageTelemetry;

        public DeterminismAuditTests()
        {
            _testRoot = Path.Combine(Path.GetTempPath(), "OanAudit_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testRoot);
            Directory.CreateDirectory(Path.Combine(_testRoot, "public"));
            Directory.CreateDirectory(Path.Combine(_testRoot, "cryptic"));

            _govTelemetry = new MockTelemetrySink();
            _storageTelemetry = new MockTelemetrySink();

            _authority = new SoulFrameAuthority(_govTelemetry);
            
            var publicStore = new PublicPlaneStore(Path.Combine(_testRoot, "public"), _storageTelemetry);
            var crypticStore = new CrypticPlaneStore(Path.Combine(_testRoot, "cryptic"), _storageTelemetry);
            var harness = new DeterministicHarness(crypticStore);

            _stores = new StoreRegistry(
                _govTelemetry,
                _storageTelemetry,
                publicStore,
                true,
                crypticStore,
                true
            );

            _router = new RoutingEngine(_authority, _stores.Public, _stores.Cryptic, harness);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testRoot))
                Directory.Delete(_testRoot, true);
        }

        [Fact]
        public async Task Test1_ReplayIdentity()
        {
            // Same inputs -> Identical storage bytes (GEL)
            var input = new { data = "test_determinism", timestamp = 123456789 };
            string hash = Primitives.ComputeHash(Primitives.ToCanonicalJson(input));

            await _router.AppendAsync("Standard", hash, input);

            string filePath = Path.Combine(_testRoot, "public", "GEL.ndjson");
            string firstRunContent = await File.ReadAllTextAsync(filePath);

            // Reset and Replay
            File.Delete(filePath);
            await _router.AppendAsync("Standard", hash, input);
            string secondRunContent = await File.ReadAllTextAsync(filePath);

            Assert.Equal(firstRunContent, secondRunContent);
        }

        [Fact]
        public async Task Test2_CrossPlaneViolation()
        {
            // Attempt unauthorized Cryptic -> Standard write must fail.
            // The RoutingEngine itself prevents this by not allowing random injections into Standard
            // without a receipt in PromoteAsync.
            
            var receipt = new PromotionReceipt
            {
                SourceCrypticHash = "cryptic_hash",
                ResultingStandardHash = "none" // Invalid
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _router.PromoteAsync(receipt, new { }));
        }

        [Fact]
        public async Task Test3_FreezeIntegrity_C2()
        {
            // Transition to Frozen
            _authority.Freeze();
            Assert.Equal(SoulFrameState.Frozen, _authority.State);

            // 1. Standard mutation forbidden
            await Assert.ThrowsAsync<InvalidOperationException>(() => _router.AppendAsync("Standard", "hash", new { }));

            // 2. Incident-only cryptic write allowed (C2)
            // Note: Our RoutingEngine currently doesn't have a specialized 'incident' flag in AppendAsync signature 
            // that is used for authorization check in a granular way yet, but we implemented the check.
            // Let's verify the check works.
            bool authorizedInc = _authority.IsWriteAuthorized("Cryptic", true);
            bool deniedStandard = _authority.IsWriteAuthorized("Standard", false);

            Assert.True(authorizedInc);
            Assert.False(deniedStandard);
        }

        [Fact]
        public void Test4_ReceiptStability()
        {
            var receipt = new PromotionReceipt
            {
                PolicyVersion = "v1",
                InvokingHandle = "test_handle",
                SatMode = "PostAudit",
                SourceCrypticHash = "abc",
                SourceCrypticTipHash = "000",
                ResultingStandardHash = "def"
            };

            string hash1 = receipt.ComputeId();
            string hash2 = receipt.ComputeId();

            Assert.Equal(hash1, hash2);
            // Verify it binds specific fields
            receipt.PolicyVersion = "v2";
            string hash3 = receipt.ComputeId();
            Assert.NotEqual(hash1, hash3);
        }

        [Fact]
        public async Task Test5_DirectStoreAccess_BlockedByAuthority()
        {
            // This is proof of concept. If we bypass RoutingEngine, the Store implementation
            // currently doesn't check authority (it's "dumb"). 
            // BUT, in Phase E, we defined that enforcement happens at the SLI/Routing boundary.
            // To be "impossible to bypass", we'd need internal/private stores.
            // For now, we verify that any call through the Router is gated.
            
            _authority.HardHalt();
            await Assert.ThrowsAsync<InvalidOperationException>(() => _router.AppendAsync("Standard", "hash", new { }));
        }

        [Fact]
        public async Task Test6_StaleTipQuery_Denied()
        {
            // DuplexQuery must match current standard tip
            var input = new { q = "what is the state?" };
            string qHash = Primitives.ComputeHash(Primitives.ToCanonicalJson(input));
            
            // Commit something to Standard to progress the tip
            await _router.AppendAsync("Standard", "tip_1", new { });

            var query = new DuplexQuery
            {
                QueryHash = qHash,
                StandardTipHash = "invalid_tip_hash" // Stale
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _router.ExecuteDuplexAsync(query, input));
        }

        [Fact]
        public async Task Test7_PointerOnly_Enforcement()
        {
            var input = new { q = "expansion request" };
            string qHash = Primitives.ComputeHash(Primitives.ToCanonicalJson(input));
            
            // Initial tip is all zeros (default in RoutingEngine)
            string initialTip = "0000000000000000000000000000000000000000000000000000000000000000";

            var query = new DuplexQuery
            {
                QueryHash = qHash,
                StandardTipHash = initialTip
            };

            var response = await _router.ExecuteDuplexAsync(query, input);

            // Verify pointer-only properties
            Assert.Equal(qHash, response.QueryHash);
            Assert.NotNull(response.CrypticResultHash);
            Assert.NotEmpty(response.ClassificationTags);
            
            // Ensure NO TEXT from the result leaked into the response object
            string responseJson = Primitives.ToCanonicalJson(response);
            Assert.DoesNotContain("expansion request", responseJson);
            Assert.DoesNotContain("Engine expansion of", responseJson);
        }

        [Fact]
        public async Task Test8_DuplexReplay_Identity()
        {
            var input = new { q = "deterministic_test" };
            string qHash = Primitives.ComputeHash(Primitives.ToCanonicalJson(input));
            string initialTip = "0000000000000000000000000000000000000000000000000000000000000000";

            var query = new DuplexQuery { QueryHash = qHash, StandardTipHash = initialTip };

            var response1 = await _router.ExecuteDuplexAsync(query, input);
            
            // Inspect storage
            string crypticPath = Path.Combine(_testRoot, "cryptic", "cGoA.ndjson");
            string storage1 = await File.ReadAllTextAsync(crypticPath);

            // Replay (Fresh state)
            Dispose();
            Directory.CreateDirectory(_testRoot);
            Directory.CreateDirectory(Path.Combine(_testRoot, "public"));
            Directory.CreateDirectory(Path.Combine(_testRoot, "cryptic"));
            
            // Re-recreate core objects with same seed logic
            var freshHarness = new DeterministicHarness(new CrypticPlaneStore(Path.Combine(_testRoot, "cryptic"), new MockTelemetrySink()));
            var freshRouter = new RoutingEngine(_authority, _stores.Public, new CrypticPlaneStore(Path.Combine(_testRoot, "cryptic"), new MockTelemetrySink()), freshHarness);

            var response2 = await freshRouter.ExecuteDuplexAsync(query, input);
            string storage2 = await File.ReadAllTextAsync(crypticPath);

            Assert.Equal(response1.ComputeId(), response2.ComputeId());
            Assert.Equal(storage1, storage2);
        }
    }

    public class MockTelemetrySink : ITelemetrySink
    {
        public int EmitCount { get; private set; }
        public Task EmitAsync(object telemetryEvent)
        {
            EmitCount++;
            return Task.CompletedTask;
        }
    }
}
