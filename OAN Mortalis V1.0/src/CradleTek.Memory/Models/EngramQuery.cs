namespace CradleTek.Memory.Models;

public sealed class EngramQuery
{
    public string? Concept { get; init; }
    public string? ClusterId { get; init; }
    public int MaxResults { get; init; } = 8;
    public IReadOnlyCollection<string> HintTokens { get; init; } = Array.Empty<string>();
}
