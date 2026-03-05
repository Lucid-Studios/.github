namespace CradleTek.CognitionHost.Models;

public sealed class CognitionEngramEntry
{
    public required string EngramId { get; init; }
    public required string SummaryText { get; init; }
    public required string DecisionSpline { get; init; }
}
