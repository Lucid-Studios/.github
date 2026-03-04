namespace CorpusGraphVisualizer;

internal sealed class GraphBuilder
{
    public EngramGraph Build(IReadOnlyList<GraphNode> nodes, IReadOnlyList<GraphEdge> edges)
    {
        var nodeById = nodes.ToDictionary(n => n.NodeId, StringComparer.Ordinal);
        var filteredEdges = edges
            .Where(e => nodeById.ContainsKey(e.FromNode) && nodeById.ContainsKey(e.ToNode))
            .ToList();

        var adjacency = nodes.ToDictionary(
            n => n.NodeId,
            _ => (IReadOnlyList<GraphEdge>)new List<GraphEdge>(),
            StringComparer.Ordinal);

        var mutable = nodes.ToDictionary(n => n.NodeId, _ => new List<GraphEdge>(), StringComparer.Ordinal);

        foreach (var edge in filteredEdges)
        {
            mutable[edge.FromNode].Add(edge);
            // Treat topology as undirected for structural analysis and layout.
            mutable[edge.ToNode].Add(new GraphEdge(edge.ToNode, edge.FromNode, edge.RelationType));
        }

        foreach (var entry in mutable)
        {
            adjacency[entry.Key] = entry.Value
                .OrderBy(e => e.ToNode, StringComparer.Ordinal)
                .ThenBy(e => e.RelationType, StringComparer.Ordinal)
                .ToList();
        }

        return new EngramGraph
        {
            Nodes = nodes,
            Edges = filteredEdges,
            NodeById = nodeById,
            Adjacency = adjacency
        };
    }
}
