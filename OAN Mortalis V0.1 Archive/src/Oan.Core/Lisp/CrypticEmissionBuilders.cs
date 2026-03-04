using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Oan.Core.Lisp
{
    /// <summary>
    /// Pure builders for deterministic cryptic emissions.
    /// </summary>
    public static class CrypticEmissionBuilders
    {
        private static readonly JsonSerializerOptions _deterministicOptions = new JsonSerializerOptions
        {
            WriteIndented = false
        };

        /// <summary>
        /// Produces a deterministic CrypticEmission for an evaluation boundary.
        /// Only captures fingerprints (hashes) and meta-data. Zero raw form content.
        /// </summary>
        public static CrypticEmission BuildCGoAEvalBoundaryEmission(
            EvalResult result,
            string? policyRationaleCode,
            long tick)
        {
            return BuildEvalBoundaryEmission(result, CrypticTier.CGoA, policyRationaleCode, tick);
        }

        /// <summary>
        /// Produces a deterministic CrypticEmission for an evaluation boundary targeting a specific tier.
        /// Only captures fingerprints (hashes) and meta-data. Zero raw form content.
        /// </summary>
        public static CrypticEmission BuildEvalBoundaryEmission(
            EvalResult result,
            CrypticTier tier,
            string? policyRationaleCode,
            long tick)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            // Mandatory fingerprints check
            if (string.IsNullOrEmpty(result.form_hash)) throw new ArgumentException("MANDATORY_FIELD_MISSING: form_hash");
            if (string.IsNullOrEmpty(result.chain_hash)) throw new ArgumentException("MANDATORY_FIELD_MISSING: chain_hash");
            if (string.IsNullOrEmpty(result.intent_hash)) throw new ArgumentException("MANDATORY_FIELD_MISSING: intent_hash");
            if (string.IsNullOrEmpty(result.sat_hash)) throw new ArgumentException("MANDATORY_FIELD_MISSING: sat_hash");
            
            // 1) Bundle fingerprints for payload hashing
            var bundle = new SortedDictionary<string, object?>(StringComparer.Ordinal)
            {
                ["chain_hash"] = result.chain_hash,
                ["decision"] = (int)result.decision,
                ["form_hash"] = result.form_hash,
                ["intent_hash"] = result.intent_hash,
                ["sat_hash"] = result.sat_hash
            };

            if (!string.IsNullOrEmpty(policyRationaleCode))
            {
                bundle["policy_rationale_code"] = policyRationaleCode;
            }

            // 2) Deterministic payload hash
            string bundleJson = JsonSerializer.Serialize(bundle, _deterministicOptions);
            string payloadHash;
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(bundleJson);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                payloadHash = Convert.ToHexString(hashBytes).ToLowerInvariant();
            }

            // 3) Construct emission
            return new CrypticEmission
            {
                tier = tier,
                kind = "governance.eval",
                payload_hash = payloadHash,
                tick = tick,
                notes = result.note // Propagation of diagnostic note is acceptable
            };
        }
    }
}
