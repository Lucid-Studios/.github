namespace EngramResolver;

internal sealed record ResolverNode(
    string NodeId,
    string Concept,
    string Domain,
    string Source);

internal sealed record ResolverEdge(
    string FromNode,
    string ToNode,
    string RelationType);

internal sealed class ResolverGraph
{
    public required IReadOnlyList<ResolverNode> Nodes { get; init; }
    public required IReadOnlyList<ResolverEdge> Edges { get; init; }
    public required IReadOnlyDictionary<string, ResolverNode> NodeById { get; init; }
    public required IReadOnlyDictionary<string, IReadOnlyList<ResolverEdge>> Adjacency { get; init; }
    public required IReadOnlyDictionary<string, string> ClusterByNode { get; init; }
}

internal sealed record ResolveResult(
    ResolverNode? Node,
    IReadOnlyList<ResolverNode> ConnectedNodes);

internal sealed record ConceptResult(
    ResolverNode? Node,
    IReadOnlyList<ResolverNode> RelatedNodes);

internal sealed record PathResult(
    string ConceptA,
    string ConceptB,
    bool Found,
    IReadOnlyList<ResolverNode> NodesInPath);

internal sealed record ClusterResult(
    string Concept,
    string? ClusterId,
    IReadOnlyList<ResolverNode> Members);
