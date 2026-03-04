using System;
using System.Text.Json.Serialization;
using Oan.Spinal;

namespace Oan.SoulFrame
{
    /// <summary>
    /// A deterministic query from the Standard plane to the Cryptic plane.
    /// Bound to the current Standard plane tip for lineage integrity.
    /// </summary>
    public sealed class DuplexQuery
    {
        [JsonPropertyName("v")]
        public string Version { get; set; } = "1.0";

        [JsonPropertyName("policy_v")]
        public string PolicyVersion { get; set; } = "0";

        [JsonPropertyName("handle")]
        public string InvokingHandle { get; set; } = "none";

        [JsonPropertyName("sat_mode")]
        public string SatMode { get; set; } = "Standard";

        [JsonPropertyName("q_hash")]
        public string QueryHash { get; set; } = "none";

        [JsonPropertyName("s_tip_hash")]
        public string StandardTipHash { get; set; } = "none";

        /// <summary>
        /// Computes a deterministic identity for this query.
        /// </summary>
        public string ComputeId()
        {
            string json = Primitives.ToCanonicalJson(this);
            return Primitives.ComputeHash(json);
        }
    }
}
