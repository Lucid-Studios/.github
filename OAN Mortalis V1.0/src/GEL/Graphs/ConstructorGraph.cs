namespace GEL.Graphs;

/// <summary>
/// Legacy graph name retained for compatibility.
/// This graph now represents propositional engram relationships.
/// </summary>
public sealed class ConstructorGraph
{
    public required IReadOnlyList<ConstructorEdge> Edges { get; init; }

    public IReadOnlyList<ConstructorEdge> Outgoing(string source)
    {
        return Edges
            .Where(edge => string.Equals(edge.Source, source, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}

/// <summary>
/// Directed relation between propositional engram concepts.
/// </summary>
public sealed class ConstructorEdge
{
    public required string Source { get; init; }
    public required string Target { get; init; }
    public required string Relation { get; init; }
}
