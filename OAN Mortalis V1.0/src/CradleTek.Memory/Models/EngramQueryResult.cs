namespace CradleTek.Memory.Models;

public sealed class EngramQueryResult
{
    public required string Source { get; init; }
    public required IReadOnlyList<EngramSummary> Summaries { get; init; }
}
