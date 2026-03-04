using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Oan.Spinal;

namespace Oan.SoulFrame
{
    /// <summary>
    /// A pointer-only response from the Cryptic plane back to the Standard plane.
    /// Contains NO material payload; only content-addressed hashes.
    /// </summary>
    public sealed class DuplexResponse
    {
        [JsonPropertyName("v")]
        public string Version { get; set; } = "1.0";

        [JsonPropertyName("q_hash")]
        public string QueryHash { get; set; } = "none";

        [JsonPropertyName("c_res_hash")]
        public string CrypticResultHash { get; set; } = "none";

        [JsonPropertyName("c_tip_hash")]
        public string CrypticTipHash { get; set; } = "none";

        [JsonPropertyName("tags")]
        public List<string> ClassificationTags { get; set; } = new();

        /// <summary>
        /// Computes a deterministic identity for this response.
        /// </summary>
        public string ComputeId()
        {
            string json = Primitives.ToCanonicalJson(this);
            return Primitives.ComputeHash(json);
        }
    }
}
