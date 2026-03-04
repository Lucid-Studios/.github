using System;
using System.Threading.Tasks;
using Oan.Common;
using Oan.SoulFrame;
using Oan.Spinal;

namespace Oan.Sli
{
    public interface IDeterministicHarness
    {
        Task<string> ExecuteEngineAsync(object input);
    }

    /// <summary>
    /// The authoritative gateway for nondeterministic engines.
    /// Enforces fixed parameters and strips variance before persisting to Cryptic storage.
    /// </summary>
    public sealed class DeterministicHarness : IDeterministicHarness
    {
        private readonly ICrypticPlaneStores _crypticStores;

        public DeterministicHarness(ICrypticPlaneStores crypticStores)
        {
            _crypticStores = crypticStores ?? throw new ArgumentNullException(nameof(crypticStores));
        }

        public async Task<string> ExecuteEngineAsync(object input)
        {
            // 1. Canonicalize Input
            string canonicalInput = Primitives.ToCanonicalJson(input);

            // 2. Mock Engine Execution (Strict Parameters)
            // In a real system, this would call an LLM with temp=0, fixed seed, etc.
            // We strip any time-based variables from the input before compute.
            
            string mockResult = $"Engine expansion of: {Primitives.ComputeHash(canonicalInput).Substring(0, 8)}";
            
            // 3. Compute Result Hash
            string resultHash = Primitives.ComputeHash(mockResult);

            // 4. Persist MATERIAL payload to Cryptic (cGoA)
            // Note: The harness persists the content, but the RoutingEngine authorizes the action.
            await _crypticStores.AppendToCGoAAsync(resultHash, new { result = mockResult });

            return resultHash;
        }
    }
}
