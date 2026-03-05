namespace SLI.Engine.Models;

public sealed class Predicate
{
    public required Symbol Name { get; init; }
    public required IReadOnlyList<string> Arguments { get; init; }
}
