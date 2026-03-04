namespace CorpusGraphVisualizer;

internal sealed record GraphNode(
    string NodeId,
    string Concept,
    string Domain,
    string Source,
    string Hash);

internal sealed record GraphEdge(
    string FromNode,
    string ToNode,
    string RelationType);

internal sealed class EngramGraph
{
    public required IReadOnlyList<GraphNode> Nodes { get; init; }
    public required IReadOnlyList<GraphEdge> Edges { get; init; }
    public required IReadOnlyDictionary<string, GraphNode> NodeById { get; init; }
    public required IReadOnlyDictionary<string, IReadOnlyList<GraphEdge>> Adjacency { get; init; }
}

internal sealed record GraphMetrics(
    IReadOnlyDictionary<string, int> DegreeByNode,
    IReadOnlyList<string> HubNodes,
    IReadOnlyList<string> BridgeNodes,
    IReadOnlyList<IReadOnlyList<string>> ConnectedComponents);

internal sealed record PositionedNode(
    GraphNode Node,
    double X,
    double Y);
