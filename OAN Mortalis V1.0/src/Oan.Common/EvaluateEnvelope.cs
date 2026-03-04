using System.Text.Json.Serialization;

namespace Oan.Common
{
    /// <summary>
    /// Host transport surface for evaluation results.
    /// </summary>
    public sealed class EvaluateEnvelope
    {
        [JsonPropertyName("v")]
        public string Version { get; set; } = "1.0";

        [JsonPropertyName("agent_id")]
        public string AgentId { get; set; } = string.Empty;

        [JsonPropertyName("theater_id")]
        public string TheaterId { get; set; } = string.Empty;

        [JsonPropertyName("decision")]
        public string Decision { get; set; } = string.Empty;

        [JsonPropertyName("note")]
        public string? Note { get; set; }

        [JsonPropertyName("duplex_ptr")]
        public string? DuplexResponseHash { get; set; }
    }
}
