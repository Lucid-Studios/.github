using System;
using System.Threading.Tasks;
using Oan.Common;
using Oan.SoulFrame;
using Oan.Spinal;

namespace Oan.Sli
{
    public interface IRoutingEngine
    {
        Task AppendAsync(string plane, string engramHash, object payload, bool isIncident = false);
        Task PromoteAsync(PromotionReceipt receipt, object payload);
        Task<DuplexResponse> ExecuteDuplexAsync(DuplexQuery query, object engineInput);
    }

    /// <summary>
    /// The authoritative routing engine for OAN Mortalis.
    /// Enforces plane-scoped lineage and promotion rules.
    /// </summary>
    public sealed class RoutingEngine : IRoutingEngine
    {
        private readonly SoulFrameAuthority _authority;
        private readonly IPublicPlaneStores _publicStores;
        private readonly ICrypticPlaneStores _crypticStores;
        private readonly IDeterministicHarness _harness;

        // Lineage tracking (TipHashes)
        private string _standardTip = "0000000000000000000000000000000000000000000000000000000000000000";
        private string _crypticTip = "0000000000000000000000000000000000000000000000000000000000000000";

        public RoutingEngine(
            SoulFrameAuthority authority,
            IPublicPlaneStores publicStores,
            ICrypticPlaneStores crypticStores,
            IDeterministicHarness harness)
        {
            _authority = authority ?? throw new ArgumentNullException(nameof(authority));
            _publicStores = publicStores ?? throw new ArgumentNullException(nameof(publicStores));
            _crypticStores = crypticStores ?? throw new ArgumentNullException(nameof(crypticStores));
            _harness = harness ?? throw new ArgumentNullException(nameof(harness));
        }

        public async Task AppendAsync(string plane, string engramHash, object payload, bool isIncident = false)
        {
            // 1. Authorize Write
            if (!_authority.IsWriteAuthorized(plane, isIncident))
                throw new InvalidOperationException($"Write to {plane} plane denied by SoulFrame (State: {_authority.State})");

            // 2. Resolve Plane & Append
            if (plane == "Standard")
            {
                // Note: Standard lineage enforcement would typically happen here.
                // For this sprint, we treat GEL as the standard lineage.
                await _publicStores.AppendToGELAsync(engramHash, payload);
                _standardTip = engramHash;
            }
            else if (plane == "Cryptic")
            {
                await _crypticStores.AppendToCGoAAsync(engramHash, payload);
                _crypticTip = engramHash;
            }
            else
            {
                throw new ArgumentException($"Unknown plane: {plane}");
            }
        }

        public async Task PromoteAsync(PromotionReceipt receipt, object payload)
        {
            // 1. Validate Promotion Receipt
            _authority.ValidatePromotion(receipt);

            // 2. Lineage Verification
            // Ensure the cryptic source exists in our tracking (or store)
            // In a real system, we'd verify receipt.SourceCrypticHash matches actual store content.
            
            // 3. Authorize & Commit to Standard
            if (!_authority.IsWriteAuthorized("Standard", false))
                throw new InvalidOperationException("Promotion to Standard denied by SoulFrame.");

            // 4. Commit to Standard (GEL + GoA)
            await _publicStores.AppendToGELAsync(receipt.ResultingStandardHash, payload);
            await _publicStores.AppendToGoAAsync(receipt.ResultingStandardHash, payload);

            _standardTip = receipt.ResultingStandardHash;
        }

        public async Task<DuplexResponse> ExecuteDuplexAsync(DuplexQuery query, object engineInput)
        {
            // 1. Authorize (Standard consultation)
            if (query == null) throw new ArgumentNullException(nameof(query));

            // 2. Tip-Binding Verification (Acceptance Time)
            if (query.StandardTipHash != _standardTip)
            {
                throw new InvalidOperationException($"DuplexQuery denied: Stale StandardTipHash (Expected: {_standardTip}, Payload: {query.StandardTipHash})");
            }

            // 3. Emit DuplexQuery to Governance Telemetry (via authority)
            // Note: We'd typically have a specific telemetry hook here. 
            // For now, it's implicit in the SLI flow.

            // 4. Execute via Deterministic Harness
            string crypticResultHash = await _harness.ExecuteEngineAsync(engineInput);

            // 5. Construct DuplexResponse (Pointer-only)
            var response = new DuplexResponse
            {
                QueryHash = query.QueryHash,
                CrypticResultHash = crypticResultHash,
                CrypticTipHash = _crypticTip,
                ClassificationTags = new System.Collections.Generic.List<string> { "Standard-safe" } // Advisory only
            };

            // 6. Persist DuplexResponse Pointer to cGoA
            string responseHash = response.ComputeId();
            await _crypticStores.AppendToCGoAAsync(responseHash, response);
            _crypticTip = responseHash;

            return response;
        }
    }
}
