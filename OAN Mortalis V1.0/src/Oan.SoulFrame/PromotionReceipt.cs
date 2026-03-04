using System;
using System.Text.Json.Serialization;
using Oan.Spinal;

namespace Oan.SoulFrame
{
    /// <summary>
    /// A deterministic receipt binding a cryptic emission to a standard plane commit.
    /// Used to authorize promotion under historical policy.
    /// </summary>
    public sealed class PromotionReceipt
    {
        [JsonPropertyName("v")]
        public string Version { get; set; } = "1.0";

        [JsonPropertyName("policy_v")]
        public string PolicyVersion { get; set; } = "0";

        [JsonPropertyName("handle")]
        public string InvokingHandle { get; set; } = "none";

        [JsonPropertyName("sat_mode")]
        public string SatMode { get; set; } = "Standard";

        [JsonPropertyName("c_hash")]
        public string SourceCrypticHash { get; set; } = "none";

        [JsonPropertyName("c_tip_hash")]
        public string SourceCrypticTipHash { get; set; } = "none";

        [JsonPropertyName("s_hash")]
        public string ResultingStandardHash { get; set; } = "none";

        /// <summary>
        /// Computes a deterministic identity for this receipt.
        /// </summary>
        public string ComputeId()
        {
            // Use Spinal Primitives for deterministic JSON and hashing
            string json = Primitives.ToCanonicalJson(this);
            return Primitives.ComputeHash(json);
        }
    }
}
