using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Oan.SoulFrame.Identity
{
    public class OpalTheaterTip
    {
        [JsonPropertyName("theaterId")]
        public string TheaterId { get; set; } = string.Empty;

        [JsonPropertyName("tip")]
        public string Tip { get; set; } = string.Empty;
    }

    public class OpalTipSnapshot
    {
        [JsonPropertyName("schema")]
        public string Schema { get; set; } = "opal.tips.v0.2";

        [JsonPropertyName("policyVersion")]
        public string PolicyVersion { get; set; } = string.Empty;

        [JsonPropertyName("rootAtlasHash")]
        public string RootAtlasHash { get; set; } = string.Empty;

        [JsonPropertyName("runId")]
        public string RunId { get; set; } = string.Empty;

        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; } = string.Empty;

        [JsonPropertyName("theaters")]
        public List<OpalTheaterTip> Theaters { get; set; } = new List<OpalTheaterTip>();

        /// <summary>
        /// Serializes the snapshot to deterministic JSON.
        /// Enforces: Sorted keys (implicit by consistent struct), Sorted theaters by ID, UTF8, No Indentation (for compact hash stability).
        /// </summary>
        public string ToDeterministicJson()
        {
            // Sort theaters by ID for determinism
            Theaters.Sort((a, b) => StringComparer.Ordinal.Compare(a.TheaterId, b.TheaterId));

            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // Prevent escaping of standard chars if needed, or default is fine for hex
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Serialize(this, options);
        }

        public static OpalTipSnapshot? FromJson(string json)
        {
            try 
            {
                return JsonSerializer.Deserialize<OpalTipSnapshot>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
