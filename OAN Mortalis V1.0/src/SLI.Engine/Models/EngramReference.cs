namespace SLI.Engine.Models;

public sealed class EngramReference
{
    public required string EngramId { get; init; }
    public required string ConceptTag { get; init; }
    public required string SummaryText { get; init; }
    public required string DecisionSpline { get; init; }
    public required double ConfidenceWeight { get; init; }
}
