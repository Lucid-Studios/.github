namespace CorpusGraphVisualizer;

internal sealed class MetricsAnalyzer
{
    public GraphMetrics Analyze(EngramGraph graph)
    {
        var degreeByNode = graph.Adjacency.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Count,
            StringComparer.Ordinal);

        var orderedByDegree = degreeByNode
            .OrderByDescending(kvp => kvp.Value)
            .ThenBy(kvp => kvp.Key, StringComparer.Ordinal)
            .ToList();

        var hubCount = Math.Min(10, orderedByDegree.Count);
        var hubNodes = orderedByDegree.Take(hubCount).Select(kvp => kvp.Key).ToList();

        var bridgeNodes = FindArticulationPoints(graph);
        var components = FindConnectedComponents(graph);

        return new GraphMetrics(degreeByNode, hubNodes, bridgeNodes, components);
    }

    private static IReadOnlyList<IReadOnlyList<string>> FindConnectedComponents(EngramGraph graph)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var components = new List<IReadOnlyList<string>>();

        foreach (var node in graph.Nodes.OrderBy(n => n.NodeId, StringComparer.Ordinal))
        {
            if (visited.Contains(node.NodeId))
            {
                continue;
            }

            var queue = new Queue<string>();
            var component = new List<string>();
            queue.Enqueue(node.NodeId);
            visited.Add(node.NodeId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                component.Add(current);

                foreach (var edge in graph.Adjacency[current])
                {
                    if (visited.Add(edge.ToNode))
                    {
                        queue.Enqueue(edge.ToNode);
                    }
                }
            }

            component.Sort(StringComparer.Ordinal);
            components.Add(component);
        }

        return components
            .OrderByDescending(c => c.Count)
            .ThenBy(c => c.FirstOrDefault() ?? string.Empty, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<string> FindArticulationPoints(EngramGraph graph)
    {
        var index = 0;
        var indices = new Dictionary<string, int>(StringComparer.Ordinal);
        var low = new Dictionary<string, int>(StringComparer.Ordinal);
        var parent = new Dictionary<string, string?>(StringComparer.Ordinal);
        var articulation = new HashSet<string>(StringComparer.Ordinal);

        foreach (var node in graph.Nodes.Select(n => n.NodeId).OrderBy(id => id, StringComparer.Ordinal))
        {
            if (!indices.ContainsKey(node))
            {
                Dfs(node);
            }
        }

        return articulation.OrderBy(id => id, StringComparer.Ordinal).ToList();

        void Dfs(string u)
        {
            indices[u] = index;
            low[u] = index;
            index++;

            var children = 0;

            foreach (var edge in graph.Adjacency[u])
            {
                var v = edge.ToNode;

                if (!indices.ContainsKey(v))
                {
                    children++;
                    parent[v] = u;
                    Dfs(v);
                    low[u] = Math.Min(low[u], low[v]);

                    var isRoot = !parent.ContainsKey(u);
                    if (isRoot && children > 1)
                    {
                        articulation.Add(u);
                    }

                    if (!isRoot && low[v] >= indices[u])
                    {
                        articulation.Add(u);
                    }
                }
                else if (parent.TryGetValue(u, out var p) && !string.Equals(v, p, StringComparison.Ordinal))
                {
                    low[u] = Math.Min(low[u], indices[v]);
                }
                else if (!parent.ContainsKey(u))
                {
                    low[u] = Math.Min(low[u], indices[v]);
                }
            }
        }
    }
}
