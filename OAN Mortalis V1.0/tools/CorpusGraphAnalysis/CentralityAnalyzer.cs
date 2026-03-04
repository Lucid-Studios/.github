namespace CorpusGraphAnalysis;

internal sealed class CentralityAnalyzer
{
    public IReadOnlyDictionary<string, double> ComputeBetweenness(AnalysisGraph graph)
    {
        var nodes = graph.Nodes.Select(n => n.NodeId).OrderBy(id => id, StringComparer.Ordinal).ToList();
        var centrality = nodes.ToDictionary(id => id, _ => 0.0, StringComparer.Ordinal);

        foreach (var source in nodes)
        {
            var stack = new Stack<string>();
            var predecessors = nodes.ToDictionary(id => id, _ => new List<string>(), StringComparer.Ordinal);
            var sigma = nodes.ToDictionary(id => id, _ => 0.0, StringComparer.Ordinal);
            var distance = nodes.ToDictionary(id => id, _ => -1, StringComparer.Ordinal);

            sigma[source] = 1.0;
            distance[source] = 0;

            var queue = new Queue<string>();
            queue.Enqueue(source);

            while (queue.Count > 0)
            {
                var v = queue.Dequeue();
                stack.Push(v);

                foreach (var edge in graph.Adjacency[v])
                {
                    var w = edge.ToNode;
                    if (distance[w] < 0)
                    {
                        queue.Enqueue(w);
                        distance[w] = distance[v] + 1;
                    }

                    if (distance[w] == distance[v] + 1)
                    {
                        sigma[w] += sigma[v];
                        predecessors[w].Add(v);
                    }
                }
            }

            var dependency = nodes.ToDictionary(id => id, _ => 0.0, StringComparer.Ordinal);
            while (stack.Count > 0)
            {
                var w = stack.Pop();
                foreach (var v in predecessors[w])
                {
                    if (sigma[w] > 0.0)
                    {
                        dependency[v] += (sigma[v] / sigma[w]) * (1.0 + dependency[w]);
                    }
                }

                if (!string.Equals(w, source, StringComparison.Ordinal))
                {
                    centrality[w] += dependency[w];
                }
            }
        }

        // Undirected graph: divide by 2.
        return centrality.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value / 2.0,
            StringComparer.Ordinal);
    }

    public IReadOnlyList<(string NodeId, double Score)> TopBetweenness(
        IReadOnlyDictionary<string, double> betweenness,
        int top = 20)
    {
        return betweenness
            .OrderByDescending(kvp => kvp.Value)
            .ThenBy(kvp => kvp.Key, StringComparer.Ordinal)
            .Take(top)
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToList();
    }
}
