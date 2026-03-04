namespace CorpusGraphAnalysis;

internal sealed record ConceptPathResult(
    string FromConcept,
    string ToConcept,
    IReadOnlyList<string> NodePath,
    IReadOnlyList<string> ConceptPath,
    bool Found);

internal sealed class PathExplorer
{
    public ConceptPathResult FindShortestConceptPath(AnalysisGraph graph, string fromConcept, string toConcept)
    {
        var starts = graph.Nodes
            .Where(n => string.Equals(n.Concept, fromConcept, StringComparison.OrdinalIgnoreCase))
            .Select(n => n.NodeId)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();

        var targets = graph.Nodes
            .Where(n => string.Equals(n.Concept, toConcept, StringComparison.OrdinalIgnoreCase))
            .Select(n => n.NodeId)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        if (starts.Count == 0 || targets.Count == 0)
        {
            return new ConceptPathResult(fromConcept, toConcept, [], [], false);
        }

        var parent = new Dictionary<string, string?>(StringComparer.Ordinal);
        var queue = new Queue<string>();
        foreach (var start in starts)
        {
            parent[start] = null;
            queue.Enqueue(start);
        }

        string? found = null;
        while (queue.Count > 0 && found is null)
        {
            var current = queue.Dequeue();
            if (targets.Contains(current))
            {
                found = current;
                break;
            }

            foreach (var edge in graph.Adjacency[current].OrderBy(e => e.ToNode, StringComparer.Ordinal))
            {
                if (parent.ContainsKey(edge.ToNode))
                {
                    continue;
                }

                parent[edge.ToNode] = current;
                queue.Enqueue(edge.ToNode);
            }
        }

        if (found is null)
        {
            return new ConceptPathResult(fromConcept, toConcept, [], [], false);
        }

        var nodePath = new List<string>();
        var cursor = found;
        while (cursor is not null)
        {
            nodePath.Add(cursor);
            cursor = parent[cursor];
        }

        nodePath.Reverse();
        var conceptPath = nodePath
            .Select(id => graph.NodeById[id].Concept)
            .ToList();

        return new ConceptPathResult(fromConcept, toConcept, nodePath, conceptPath, true);
    }
}
