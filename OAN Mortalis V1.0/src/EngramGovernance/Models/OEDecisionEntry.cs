namespace EngramGovernance.Models;

public sealed class OEDecisionEntry
{
    public required Guid DecisionId { get; init; }
    public required string CMEId { get; init; }
    public required Guid SoulFrameId { get; init; }
    public required Guid ContextId { get; init; }
    public required EngramClassification Classification { get; init; }
    public required string BodyHash { get; init; }
    public required DateTime Timestamp { get; init; }
}
