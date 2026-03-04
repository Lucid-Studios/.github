using System.Text.Json;
using System.Xml.Linq;

namespace EngramResolver;

internal sealed class GraphLoader
{
    public ResolverGraph Load(string graphMlPath, string clustersJsonPath)
    {
        var graph = LoadGraphMl(graphMlPath);
        var clusterByNode = LoadClusters(clustersJsonPath);

        return new ResolverGraph
        {
            Nodes = graph.Nodes,
            Edges = graph.Edges,
            NodeById = graph.NodeById,
            Adjacency = graph.Adjacency,
            ClusterByNode = clusterByNode
        };
    }

    private static ResolverGraph LoadGraphMl(string graphMlPath)
    {
        if (!File.Exists(graphMlPath))
        {
            throw new FileNotFoundException("GraphML file not found.", graphMlPath);
        }

        var doc = XDocument.Load(graphMlPath);
        XNamespace ns = "http://graphml.graphdrawing.org/xmlns";

        var keyLookup = doc.Root?
            .Elements(ns + "key")
            .ToDictionary(
                k => (string?)k.Attribute("id") ?? string.Empty,
                k => (string?)k.Attribute("attr.name") ?? string.Empty,
                StringComparer.Ordinal)
            ?? new Dictionary<string, string>(StringComparer.Ordinal);

        var graphElement = doc.Root?.Element(ns + "graph")
            ?? throw new InvalidOperationException("GraphML graph element is missing.");

        var nodes = graphElement
            .Elements(ns + "node")
            .Select(nodeEl =>
            {
                var id = (string?)nodeEl.Attribute("id") ?? string.Empty;
                var attributes = ReadDataAttributes(nodeEl, ns, keyLookup);
                return new ResolverNode(
                    id,
                    attributes.TryGetValue("concept", out var concept) ? concept : string.Empty,
                    attributes.TryGetValue("domain", out var domain) ? domain : string.Empty,
                    attributes.TryGetValue("source", out var source) ? source : "Lucid Research Corpus");
            })
            .Where(n => !string.IsNullOrWhiteSpace(n.NodeId))
            .OrderBy(n => n.NodeId, StringComparer.Ordinal)
            .ToList();

        var nodeSet = nodes.Select(n => n.NodeId).ToHashSet(StringComparer.Ordinal);
        var edges = graphElement
            .Elements(ns + "edge")
            .Select(edgeEl =>
            {
                var source = (string?)edgeEl.Attribute("source") ?? string.Empty;
                var target = (string?)edgeEl.Attribute("target") ?? string.Empty;
                var attributes = ReadDataAttributes(edgeEl, ns, keyLookup);
                return new ResolverEdge(
                    source,
                    target,
                    attributes.TryGetValue("relation", out var relation) ? relation : "associates-with");
            })
            .Where(e => nodeSet.Contains(e.FromNode) && nodeSet.Contains(e.ToNode))
            .OrderBy(e => e.FromNode, StringComparer.Ordinal)
            .ThenBy(e => e.ToNode, StringComparer.Ordinal)
            .ThenBy(e => e.RelationType, StringComparer.Ordinal)
            .ToList();

        var adjacency = nodes.ToDictionary(
            n => n.NodeId,
            _ => new List<ResolverEdge>(),
            StringComparer.Ordinal);

        foreach (var edge in edges)
        {
            adjacency[edge.FromNode].Add(edge);
            adjacency[edge.ToNode].Add(new ResolverEdge(edge.ToNode, edge.FromNode, edge.RelationType));
        }

        var readonlyAdjacency = adjacency.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<ResolverEdge>)kvp.Value
                .OrderBy(e => e.ToNode, StringComparer.Ordinal)
                .ThenBy(e => e.RelationType, StringComparer.Ordinal)
                .ToList(),
            StringComparer.Ordinal);

        return new ResolverGraph
        {
            Nodes = nodes,
            Edges = edges,
            NodeById = nodes.ToDictionary(n => n.NodeId, StringComparer.Ordinal),
            Adjacency = readonlyAdjacency,
            ClusterByNode = new Dictionary<string, string>(StringComparer.Ordinal)
        };
    }

    private static IReadOnlyDictionary<string, string> LoadClusters(string clustersJsonPath)
    {
        if (!File.Exists(clustersJsonPath))
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        var json = File.ReadAllText(clustersJsonPath);
        var model = JsonSerializer.Deserialize<ClusterEnvelope>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (model?.Clusters is null)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        var mapping = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var cluster in model.Clusters)
        {
            if (cluster?.Members is null || string.IsNullOrWhiteSpace(cluster.ClusterId))
            {
                continue;
            }

            foreach (var member in cluster.Members.Where(m => !string.IsNullOrWhiteSpace(m)))
            {
                mapping[member] = cluster.ClusterId;
            }
        }

        return mapping;
    }

    private static Dictionary<string, string> ReadDataAttributes(
        XElement element,
        XNamespace ns,
        IReadOnlyDictionary<string, string> keyLookup)
    {
        var data = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var dataEl in element.Elements(ns + "data"))
        {
            var keyId = (string?)dataEl.Attribute("key") ?? string.Empty;
            if (keyLookup.TryGetValue(keyId, out var attrName) && !string.IsNullOrWhiteSpace(attrName))
            {
                data[attrName] = dataEl.Value.Trim();
            }
        }

        return data;
    }

    private sealed record ClusterEnvelope(List<ClusterItem>? Clusters);

    private sealed record ClusterItem(string ClusterId, List<string>? Members);
}
