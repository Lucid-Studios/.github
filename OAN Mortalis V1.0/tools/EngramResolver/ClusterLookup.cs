namespace EngramResolver;

internal sealed class ClusterLookup
{
    public ClusterResult LookupByConcept(ResolverGraph graph, string concept)
    {
        var node = graph.Nodes
            .Where(n => string.Equals(n.Concept, concept, StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n.NodeId, StringComparer.Ordinal)
            .FirstOrDefault();

        if (node is null)
        {
            return new ClusterResult(concept, null, []);
        }

        if (!graph.ClusterByNode.TryGetValue(node.NodeId, out var clusterId))
        {
            return new ClusterResult(concept, null, []);
        }

        var members = graph.ClusterByNode
            .Where(kvp => string.Equals(kvp.Value, clusterId, StringComparison.Ordinal))
            .Select(kvp => graph.NodeById[kvp.Key])
            .OrderBy(n => n.Concept, StringComparer.OrdinalIgnoreCase)
            .ThenBy(n => n.NodeId, StringComparer.Ordinal)
            .ToList();

        return new ClusterResult(concept, clusterId, members);
    }
}
