namespace CorpusGraphAnalysis;

internal sealed record ArticulationResult(
    string NodeId,
    int StructuralImpact,
    int ComponentsAfterRemoval);

internal sealed class ArticulationAnalyzer
{
    public IReadOnlyList<ArticulationResult> Detect(AnalysisGraph graph)
    {
        var articulation = FindArticulationPoints(graph);
        var baseline = CountComponents(graph, excludedNode: null);

        var results = new List<ArticulationResult>();
        foreach (var nodeId in articulation.OrderBy(id => id, StringComparer.Ordinal))
        {
            var components = CountComponents(graph, excludedNode: nodeId);
            var impact = Math.Max(0, components - baseline);
            results.Add(new ArticulationResult(nodeId, impact, components));
        }

        return results
            .OrderByDescending(r => r.StructuralImpact)
            .ThenByDescending(r => r.ComponentsAfterRemoval)
            .ThenBy(r => r.NodeId, StringComparer.Ordinal)
            .ToList();
    }

    private static HashSet<string> FindArticulationPoints(AnalysisGraph graph)
    {
        var time = 0;
        var disc = new Dictionary<string, int>(StringComparer.Ordinal);
        var low = new Dictionary<string, int>(StringComparer.Ordinal);
        var parent = new Dictionary<string, string?>(StringComparer.Ordinal);
        var points = new HashSet<string>(StringComparer.Ordinal);

        foreach (var nodeId in graph.Nodes.Select(n => n.NodeId).OrderBy(id => id, StringComparer.Ordinal))
        {
            if (!disc.ContainsKey(nodeId))
            {
                Dfs(nodeId);
            }
        }

        return points;

        void Dfs(string u)
        {
            disc[u] = time;
            low[u] = time;
            time++;

            var children = 0;
            foreach (var edge in graph.Adjacency[u])
            {
                var v = edge.ToNode;
                if (!disc.ContainsKey(v))
                {
                    children++;
                    parent[v] = u;
                    Dfs(v);
                    low[u] = Math.Min(low[u], low[v]);

                    var isRoot = !parent.ContainsKey(u);
                    if (isRoot && children > 1)
                    {
                        points.Add(u);
                    }

                    if (!isRoot && low[v] >= disc[u])
                    {
                        points.Add(u);
                    }
                }
                else if (parent.TryGetValue(u, out var p) && !string.Equals(v, p, StringComparison.Ordinal))
                {
                    low[u] = Math.Min(low[u], disc[v]);
                }
                else if (!parent.ContainsKey(u))
                {
                    low[u] = Math.Min(low[u], disc[v]);
                }
            }
        }
    }

    public int CountComponents(AnalysisGraph graph, string? excludedNode)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        if (!string.IsNullOrWhiteSpace(excludedNode))
        {
            visited.Add(excludedNode);
        }

        var components = 0;
        foreach (var nodeId in graph.Nodes.Select(n => n.NodeId).OrderBy(id => id, StringComparer.Ordinal))
        {
            if (visited.Contains(nodeId))
            {
                continue;
            }

            components++;
            var queue = new Queue<string>();
            queue.Enqueue(nodeId);
            visited.Add(nodeId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var edge in graph.Adjacency[current])
                {
                    if (visited.Add(edge.ToNode))
                    {
                        queue.Enqueue(edge.ToNode);
                    }
                }
            }
        }

        return components;
    }
}
