namespace EngramGovernance.Models;

public sealed class EngramCandidate
{
    public required Guid CandidateId { get; init; }
    public required string CMEId { get; init; }
    public required Guid SoulFrameId { get; init; }
    public required Guid ContextId { get; init; }
    public required string CognitionBody { get; init; }
    public required IReadOnlyDictionary<string, string> Metadata { get; init; }
    public required DateTime Timestamp { get; init; }
}
