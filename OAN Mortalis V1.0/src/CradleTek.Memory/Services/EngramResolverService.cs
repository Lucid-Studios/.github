using System.Text.Json;
using System.Xml.Linq;
using CradleTek.CognitionHost.Models;
using CradleTek.Memory.Interfaces;
using CradleTek.Memory.Models;

namespace CradleTek.Memory.Services;

public sealed class EngramResolverService : IEngramResolver
{
    private readonly object _loadGate = new();
    private bool _loaded;
    private string _source = "Lucid Research Corpus";
    private Dictionary<string, IndexNode> _nodesById = new(StringComparer.Ordinal);
    private Dictionary<string, List<string>> _nodeIdsByConcept = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, string> _clusterByNodeId = new(StringComparer.Ordinal);
    private Dictionary<string, int> _degreeByNodeId = new(StringComparer.Ordinal);

    public Task<EngramQueryResult> ResolveRelevantAsync(CognitionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var tokens = Tokenize(context.TaskObjective)
            .Concat(Tokenize(string.Join(" ", context.RelevantEngrams.Select(e => e.SummaryText))))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var query = new EngramQuery
        {
            MaxResults = 8,
            HintTokens = tokens
        };

        return ResolveByQueryAsync(query, cancellationToken);
    }

    public Task<EngramQueryResult> ResolveConceptAsync(string concept, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(concept);
        var query = new EngramQuery
        {
            Concept = concept,
            MaxResults = 8,
            HintTokens = Tokenize(concept).ToArray()
        };

        return ResolveByQueryAsync(query, cancellationToken);
    }

    public Task<EngramQueryResult> ResolveClusterAsync(string clusterId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clusterId);
        var query = new EngramQuery
        {
            ClusterId = clusterId,
            MaxResults = 16
        };

        return ResolveByQueryAsync(query, cancellationToken);
    }

    private Task<EngramQueryResult> ResolveByQueryAsync(EngramQuery query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureLoaded();

        IEnumerable<IndexNode> candidates = _nodesById.Values;
        if (!string.IsNullOrWhiteSpace(query.Concept))
        {
            var concept = query.Concept.Trim();
            if (_nodeIdsByConcept.TryGetValue(concept, out var conceptNodeIds))
            {
                candidates = conceptNodeIds.Select(id => _nodesById[id]);
            }
            else
            {
                candidates = candidates.Where(n => n.Concept.Contains(concept, StringComparison.OrdinalIgnoreCase));
            }
        }

        if (!string.IsNullOrWhiteSpace(query.ClusterId))
        {
            candidates = candidates.Where(n =>
                _clusterByNodeId.TryGetValue(n.Id, out var cluster) &&
                cluster.Equals(query.ClusterId, StringComparison.OrdinalIgnoreCase));
        }

        var ranked = candidates
            .Select(node => new RankedNode(node, ComputeScore(node, query)))
            .Where(r => r.Score > 0.0 || string.IsNullOrWhiteSpace(query.Concept))
            .OrderByDescending(r => r.Score)
            .ThenByDescending(r => Degree(r.Node.Id))
            .ThenBy(r => r.Node.Concept, StringComparer.OrdinalIgnoreCase)
            .ThenBy(r => r.Node.Id, StringComparer.Ordinal)
            .Take(Math.Max(1, query.MaxResults))
            .ToList();

        if (ranked.Count == 0)
        {
            ranked = _nodesById.Values
                .OrderByDescending(n => Degree(n.Id))
                .ThenBy(n => n.Id, StringComparer.Ordinal)
                .Take(Math.Max(1, query.MaxResults))
                .Select(n => new RankedNode(n, 0.2))
                .ToList();
        }

        var summaries = ranked
            .Select(r => new EngramSummary
            {
                EngramId = r.Node.Id,
                ConceptTag = r.Node.Concept,
                DecisionSpline = BuildDecisionSpline(r.Node),
                SummaryText = BuildSummaryText(r.Node),
                ConfidenceWeight = Math.Clamp(r.Score, 0.1, 0.99)
            })
            .ToList();

        return Task.FromResult(new EngramQueryResult
        {
            Source = _source,
            Summaries = summaries
        });
    }

    private double ComputeScore(IndexNode node, EngramQuery query)
    {
        var score = 0.15;
        if (!string.IsNullOrWhiteSpace(query.Concept) &&
            node.Concept.Equals(query.Concept, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.5;
        }

        if (query.HintTokens.Count > 0)
        {
            foreach (var token in query.HintTokens)
            {
                if (node.Concept.Contains(token, StringComparison.OrdinalIgnoreCase))
                {
                    score += 0.18;
                }

                if (node.Domain.Contains(token, StringComparison.OrdinalIgnoreCase))
                {
                    score += 0.1;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(query.ClusterId) &&
            _clusterByNodeId.TryGetValue(node.Id, out var cluster) &&
            cluster.Equals(query.ClusterId, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.25;
        }

        score += Math.Min(0.35, Degree(node.Id) / 20.0);
        return score;
    }

    private int Degree(string nodeId) =>
        _degreeByNodeId.TryGetValue(nodeId, out var degree) ? degree : 0;

    private string BuildDecisionSpline(IndexNode node)
    {
        var cluster = _clusterByNodeId.TryGetValue(node.Id, out var clusterId) ? clusterId : "CLUSTER-UNKNOWN";
        return $"cluster:{cluster}|concept:{node.Concept}|domain:{node.Domain}";
    }

    private string BuildSummaryText(IndexNode node)
    {
        var degree = Degree(node.Id);
        return $"Concept '{node.Concept}' in domain '{node.Domain}' with structural degree {degree}.";
    }

    private void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        lock (_loadGate)
        {
            if (_loaded)
            {
                return;
            }

            LoadArtifacts();
            _loaded = true;
        }
    }

    private void LoadArtifacts()
    {
        var corpusIndexDirectory = ResolveCorpusIndexDirectory();
        var engramIndexPath = Path.Combine(corpusIndexDirectory, "engram_index.json");
        var clustersPath = Path.Combine(corpusIndexDirectory, "graph_clusters.json");
        var graphPath = Path.Combine(corpusIndexDirectory, "engram_graph.graphml");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var indexDocument = JsonSerializer.Deserialize<IndexDocument>(File.ReadAllText(engramIndexPath), options) ??
            throw new InvalidOperationException("Unable to parse engram index.");

        _source = string.IsNullOrWhiteSpace(indexDocument.Source) ? "Lucid Research Corpus" : indexDocument.Source;
        _nodesById = indexDocument.Nodes
            .Where(n => !string.IsNullOrWhiteSpace(n.Id))
            .GroupBy(n => n.Id, StringComparer.Ordinal)
            .Select(g => g.First())
            .ToDictionary(n => n.Id, StringComparer.Ordinal);

        _nodeIdsByConcept = _nodesById.Values
            .GroupBy(n => n.Concept, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(n => n.Id).OrderBy(id => id, StringComparer.Ordinal).ToList(), StringComparer.OrdinalIgnoreCase);

        _clusterByNodeId.Clear();
        if (File.Exists(clustersPath))
        {
            var clusterDocument = JsonSerializer.Deserialize<ClusterDocument>(File.ReadAllText(clustersPath), options);
            if (clusterDocument?.Clusters is not null)
            {
                foreach (var cluster in clusterDocument.Clusters)
                {
                    if (cluster.Members is null)
                    {
                        continue;
                    }

                    foreach (var nodeId in cluster.Members)
                    {
                        if (!string.IsNullOrWhiteSpace(nodeId))
                        {
                            _clusterByNodeId[nodeId] = cluster.ClusterId;
                        }
                    }
                }
            }
        }

        _degreeByNodeId = _nodesById.Keys.ToDictionary(id => id, _ => 0, StringComparer.Ordinal);
        if (File.Exists(graphPath))
        {
            var graphDoc = XDocument.Load(graphPath);
            var ns = graphDoc.Root?.Name.Namespace ?? XNamespace.None;
            var edgeElements = graphDoc.Descendants(ns + "edge");
            foreach (var edge in edgeElements)
            {
                var sourceId = edge.Attribute("source")?.Value;
                var targetId = edge.Attribute("target")?.Value;
                if (!string.IsNullOrWhiteSpace(sourceId) && _degreeByNodeId.ContainsKey(sourceId))
                {
                    _degreeByNodeId[sourceId] += 1;
                }

                if (!string.IsNullOrWhiteSpace(targetId) && _degreeByNodeId.ContainsKey(targetId))
                {
                    _degreeByNodeId[targetId] += 1;
                }
            }
        }
    }

    private static string ResolveCorpusIndexDirectory()
    {
        var candidates = new[]
        {
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory
        };

        foreach (var candidate in candidates)
        {
            var current = new DirectoryInfo(Path.GetFullPath(candidate));
            while (current is not null)
            {
                var corpusIndex = Path.Combine(current.FullName, "corpus_index");
                var engramIndexPath = Path.Combine(corpusIndex, "engram_index.json");
                if (Directory.Exists(corpusIndex) && File.Exists(engramIndexPath))
                {
                    return corpusIndex;
                }

                current = current.Parent;
            }
        }

        throw new DirectoryNotFoundException("Unable to locate corpus_index artifacts for engram resolution.");
    }

    private static IEnumerable<string> Tokenize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            yield break;
        }

        var separators = new[] { ' ', '\t', '\r', '\n', ',', ';', '.', ':', '-', '_', '/', '\\', '(', ')', '[', ']', '{', '}', '"' };
        foreach (var token in input.Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (token.Length >= 3)
            {
                yield return token.ToLowerInvariant();
            }
        }
    }
}

internal sealed record RankedNode(IndexNode Node, double Score);

internal sealed class IndexDocument
{
    public string Source { get; init; } = "Lucid Research Corpus";
    public List<IndexNode> Nodes { get; init; } = [];
}

internal sealed class IndexNode
{
    public string Id { get; init; } = string.Empty;
    public string Concept { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty;
    public string Source { get; init; } = "Lucid Research Corpus";
    public string Hash { get; init; } = string.Empty;
}

internal sealed class ClusterDocument
{
    public List<ClusterNodeSet> Clusters { get; init; } = [];
}

internal sealed class ClusterNodeSet
{
    public string ClusterId { get; init; } = string.Empty;
    public List<string> Members { get; init; } = [];
}
