namespace CorpusGraphAnalysis;

internal sealed record CommunitySummary(
    IReadOnlyDictionary<string, string> ClusterByNode,
    IReadOnlyDictionary<string, IReadOnlyList<string>> MembersByCluster);

internal sealed class CommunityDetector
{
    public CommunitySummary DetectLouvain(AnalysisGraph graph)
    {
        var originalNodes = graph.Nodes.Select(n => n.NodeId).OrderBy(id => id, StringComparer.Ordinal).ToList();
        var currentGraph = BuildWeightedGraph(originalNodes, graph.Edges);
        var currentToOriginal = originalNodes.ToDictionary(id => id, id => new List<string> { id }, StringComparer.Ordinal);

        for (var level = 0; level < 10; level++)
        {
            var assignment = LocalMoveOptimization(currentGraph);
            var grouped = assignment
                .GroupBy(kvp => kvp.Value, kvp => kvp.Key)
                .OrderBy(g => g.Key, StringComparer.Ordinal)
                .ToList();

            if (grouped.Count == currentGraph.Nodes.Count)
            {
                break;
            }

            var nextNodes = new List<string>();
            var nextToOriginal = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (var group in grouped)
            {
                var newNodeId = $"C{nextNodes.Count + 1:D4}";
                nextNodes.Add(newNodeId);

                var originals = group
                    .SelectMany(node => currentToOriginal[node])
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .ToList();
                nextToOriginal[newNodeId] = originals;
            }

            var commNodeByOldNode = grouped
                .Select((g, i) => new { Group = g, NewId = nextNodes[i] })
                .SelectMany(
                    x => x.Group.Select(oldNode => new KeyValuePair<string, string>(oldNode, x.NewId)))
                .ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);

            currentGraph = AggregateGraph(currentGraph, commNodeByOldNode, nextNodes);
            currentToOriginal = nextToOriginal;
        }

        var clusters = currentToOriginal
            .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
            .Select((kvp, index) => new { kvp.Value, ClusterId = $"CLUSTER-{index + 1:D3}" })
            .ToList();

        var clusterByNode = new Dictionary<string, string>(StringComparer.Ordinal);
        var membersByCluster = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        foreach (var cluster in clusters)
        {
            var members = cluster.Value.OrderBy(id => id, StringComparer.Ordinal).ToList();
            membersByCluster[cluster.ClusterId] = members;
            foreach (var member in members)
            {
                clusterByNode[member] = cluster.ClusterId;
            }
        }

        return new CommunitySummary(clusterByNode, membersByCluster);
    }

    private static WeightedGraph BuildWeightedGraph(IReadOnlyList<string> nodes, IReadOnlyList<AnalysisEdge> edges)
    {
        var weights = new Dictionary<(string A, string B), double>();
        foreach (var edge in edges)
        {
            var a = string.CompareOrdinal(edge.FromNode, edge.ToNode) <= 0 ? edge.FromNode : edge.ToNode;
            var b = string.CompareOrdinal(edge.FromNode, edge.ToNode) <= 0 ? edge.ToNode : edge.FromNode;
            var key = (a, b);
            weights[key] = weights.TryGetValue(key, out var weight) ? weight + 1.0 : 1.0;
        }

        return WeightedGraph.From(nodes, weights);
    }

    private static WeightedGraph AggregateGraph(
        WeightedGraph graph,
        IReadOnlyDictionary<string, string> communityByNode,
        IReadOnlyList<string> nextNodes)
    {
        var aggregated = new Dictionary<(string A, string B), double>();
        foreach (var edge in graph.Edges)
        {
            var ca = communityByNode[edge.A];
            var cb = communityByNode[edge.B];
            var a = string.CompareOrdinal(ca, cb) <= 0 ? ca : cb;
            var b = string.CompareOrdinal(ca, cb) <= 0 ? cb : ca;
            var key = (a, b);
            aggregated[key] = aggregated.TryGetValue(key, out var w) ? w + edge.Weight : edge.Weight;
        }

        return WeightedGraph.From(nextNodes, aggregated);
    }

    private static Dictionary<string, string> LocalMoveOptimization(WeightedGraph graph)
    {
        var assignment = graph.Nodes.ToDictionary(n => n, n => n, StringComparer.Ordinal);
        var improved = true;
        var iteration = 0;

        while (improved && iteration < 25)
        {
            iteration++;
            improved = false;

            foreach (var node in graph.Nodes.OrderBy(n => n, StringComparer.Ordinal))
            {
                var current = assignment[node];
                var candidates = graph.GetNeighborCommunities(node, assignment);
                candidates.Add(current);

                var bestCommunity = current;
                var bestScore = ComputeModularity(graph, assignment);

                foreach (var candidate in candidates.OrderBy(c => c, StringComparer.Ordinal))
                {
                    if (string.Equals(candidate, current, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    assignment[node] = candidate;
                    var score = ComputeModularity(graph, assignment);
                    if (score > bestScore + 1e-9)
                    {
                        bestScore = score;
                        bestCommunity = candidate;
                    }
                }

                assignment[node] = bestCommunity;
                if (!string.Equals(bestCommunity, current, StringComparison.Ordinal))
                {
                    improved = true;
                }
            }
        }

        return assignment;
    }

    private static double ComputeModularity(WeightedGraph graph, IReadOnlyDictionary<string, string> assignment)
    {
        var twoM = graph.TotalWeight * 2.0;
        if (twoM <= 0.0)
        {
            return 0.0;
        }

        var tot = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var node in graph.Nodes)
        {
            var comm = assignment[node];
            var degree = graph.Degree[node];
            tot[comm] = tot.TryGetValue(comm, out var value) ? value + degree : degree;
        }

        var @in = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var edge in graph.Edges)
        {
            var ca = assignment[edge.A];
            var cb = assignment[edge.B];
            if (string.Equals(ca, cb, StringComparison.Ordinal))
            {
                var contribution = edge.A == edge.B ? edge.Weight : (2.0 * edge.Weight);
                @in[ca] = @in.TryGetValue(ca, out var value) ? value + contribution : contribution;
            }
        }

        var communities = assignment.Values.Distinct(StringComparer.Ordinal);
        var q = 0.0;
        foreach (var community in communities)
        {
            var inWeight = @in.TryGetValue(community, out var i) ? i : 0.0;
            var totWeight = tot.TryGetValue(community, out var t) ? t : 0.0;
            q += (inWeight / twoM) - Math.Pow(totWeight / twoM, 2.0);
        }

        return q;
    }

    private sealed class WeightedGraph
    {
        public required IReadOnlyList<string> Nodes { get; init; }
        public required IReadOnlyList<WeightedEdge> Edges { get; init; }
        public required IReadOnlyDictionary<string, double> Degree { get; init; }
        public required IReadOnlyDictionary<string, IReadOnlyDictionary<string, double>> NeighborWeights { get; init; }
        public required double TotalWeight { get; init; }

        public static WeightedGraph From(
            IReadOnlyList<string> nodes,
            IReadOnlyDictionary<(string A, string B), double> weights)
        {
            var edgeList = new List<WeightedEdge>();
            var degree = nodes.ToDictionary(n => n, _ => 0.0, StringComparer.Ordinal);
            var neighbors = nodes.ToDictionary(
                n => n,
                _ => (IReadOnlyDictionary<string, double>)new Dictionary<string, double>(StringComparer.Ordinal),
                StringComparer.Ordinal);
            var mutable = nodes.ToDictionary(
                n => n,
                _ => new Dictionary<string, double>(StringComparer.Ordinal),
                StringComparer.Ordinal);

            var total = 0.0;
            foreach (var kvp in weights.OrderBy(k => k.Key.A, StringComparer.Ordinal).ThenBy(k => k.Key.B, StringComparer.Ordinal))
            {
                var a = kvp.Key.A;
                var b = kvp.Key.B;
                var w = kvp.Value;
                if (w <= 0.0)
                {
                    continue;
                }

                edgeList.Add(new WeightedEdge(a, b, w));
                total += w;

                degree[a] += w;
                if (!string.Equals(a, b, StringComparison.Ordinal))
                {
                    degree[b] += w;
                }

                mutable[a][b] = mutable[a].TryGetValue(b, out var aw) ? aw + w : w;
                if (!string.Equals(a, b, StringComparison.Ordinal))
                {
                    mutable[b][a] = mutable[b].TryGetValue(a, out var bw) ? bw + w : w;
                }
            }

            foreach (var node in nodes)
            {
                neighbors[node] = mutable[node];
            }

            return new WeightedGraph
            {
                Nodes = nodes.OrderBy(n => n, StringComparer.Ordinal).ToList(),
                Edges = edgeList,
                Degree = degree,
                NeighborWeights = neighbors,
                TotalWeight = total
            };
        }

        public HashSet<string> GetNeighborCommunities(string node, IReadOnlyDictionary<string, string> assignment)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            foreach (var neighbor in NeighborWeights[node].Keys)
            {
                set.Add(assignment[neighbor]);
            }

            return set;
        }
    }

    private sealed record WeightedEdge(string A, string B, double Weight);
}
