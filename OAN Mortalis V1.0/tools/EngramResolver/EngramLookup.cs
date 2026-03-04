namespace EngramResolver;

internal sealed class EngramLookup
{
    public ResolveResult ResolveById(ResolverGraph graph, string rawId)
    {
        var normalized = NormalizeId(rawId);
        if (normalized is null || !graph.NodeById.TryGetValue(normalized, out var node))
        {
            return new ResolveResult(null, []);
        }

        var connected = graph.Adjacency[node.NodeId]
            .Select(edge => graph.NodeById[edge.ToNode])
            .DistinctBy(n => n.NodeId)
            .OrderBy(n => n.Concept, StringComparer.OrdinalIgnoreCase)
            .ThenBy(n => n.NodeId, StringComparer.Ordinal)
            .ToList();

        return new ResolveResult(node, connected);
    }

    public ConceptResult ResolveByConcept(ResolverGraph graph, string concept)
    {
        var candidates = graph.Nodes
            .Where(n => string.Equals(n.Concept, concept, StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n.NodeId, StringComparer.Ordinal)
            .ToList();

        if (candidates.Count == 0)
        {
            return new ConceptResult(null, []);
        }

        var primary = candidates[0];
        var related = graph.Adjacency[primary.NodeId]
            .Select(edge => graph.NodeById[edge.ToNode])
            .DistinctBy(n => n.Concept)
            .OrderBy(n => n.Concept, StringComparer.OrdinalIgnoreCase)
            .ThenBy(n => n.NodeId, StringComparer.Ordinal)
            .ToList();

        return new ConceptResult(primary, related);
    }

    public static string? NormalizeId(string rawId)
    {
        if (string.IsNullOrWhiteSpace(rawId))
        {
            return null;
        }

        var value = rawId.Trim().ToUpperInvariant();
        if (value.StartsWith("E-", StringComparison.Ordinal))
        {
            var suffix = value[2..];
            if (int.TryParse(suffix, out var parsed))
            {
                return $"E-{parsed:D5}";
            }
        }

        return int.TryParse(value, out var number) ? $"E-{number:D5}" : value;
    }
}
