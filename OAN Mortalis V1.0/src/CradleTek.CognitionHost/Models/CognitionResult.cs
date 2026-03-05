using System.Text.Json.Serialization;

namespace CradleTek.CognitionHost.Models;

public sealed class CognitionResult
{
    [JsonPropertyName("reasoning")]
    public required string Reasoning { get; init; }

    [JsonPropertyName("decision")]
    public required string Decision { get; init; }

    [JsonPropertyName("engram_candidate")]
    public required bool EngramCandidate { get; init; }

    [JsonPropertyName("cleave_residue")]
    public required string CleaveResidue { get; init; }

    [JsonPropertyName("trace_id")]
    public required string TraceId { get; init; }

    [JsonPropertyName("symbolic_trace")]
    public required IReadOnlyList<string> SymbolicTrace { get; init; }

    [JsonPropertyName("sli_tokens")]
    public IReadOnlyList<string> SliTokens { get; init; } = Array.Empty<string>();

    [JsonPropertyName("decision_branch")]
    public required string DecisionBranch { get; init; }

    [JsonPropertyName("compass_state")]
    public required CognitionCompassTelemetry CompassState { get; init; }

    [JsonPropertyName("confidence")]
    public required double Confidence { get; init; }
}
