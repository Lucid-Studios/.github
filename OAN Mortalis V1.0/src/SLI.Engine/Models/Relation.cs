namespace SLI.Engine.Models;

public sealed class Relation
{
    public required Symbol Source { get; init; }
    public required Symbol Target { get; init; }
    public required string RelationType { get; init; }
}
