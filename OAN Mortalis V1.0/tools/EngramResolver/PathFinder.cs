namespace EngramResolver;

internal sealed class PathFinder
{
    public PathResult FindShortestPath(ResolverGraph graph, string conceptA, string conceptB)
    {
        var starts = graph.Nodes
            .Where(n => string.Equals(n.Concept, conceptA, StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n.NodeId, StringComparer.Ordinal)
            .ToList();

        var targetIds = graph.Nodes
            .Where(n => string.Equals(n.Concept, conceptB, StringComparison.OrdinalIgnoreCase))
            .Select(n => n.NodeId)
            .ToHashSet(StringComparer.Ordinal);

        if (starts.Count == 0 || targetIds.Count == 0)
        {
            return new PathResult(conceptA, conceptB, false, []);
        }

        var parent = new Dictionary<string, string?>(StringComparer.Ordinal);
        var queue = new Queue<string>();

        foreach (var start in starts)
        {
            parent[start.NodeId] = null;
            queue.Enqueue(start.NodeId);
        }

        string? foundTarget = null;
        while (queue.Count > 0 && foundTarget is null)
        {
            var current = queue.Dequeue();
            if (targetIds.Contains(current))
            {
                foundTarget = current;
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

        if (foundTarget is null)
        {
            return new PathResult(conceptA, conceptB, false, []);
        }

        var pathIds = new List<string>();
        var cursor = foundTarget;
        while (cursor is not null)
        {
            pathIds.Add(cursor);
            cursor = parent[cursor];
        }

        pathIds.Reverse();
        var pathNodes = pathIds.Select(id => graph.NodeById[id]).ToList();
        return new PathResult(conceptA, conceptB, true, pathNodes);
    }
}
