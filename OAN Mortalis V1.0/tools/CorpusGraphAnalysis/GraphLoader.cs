using System.Text.Json;
using System.Xml.Linq;

namespace CorpusGraphAnalysis;

internal sealed record AnalysisNode(
    string NodeId,
    string Concept,
    string Domain,
    string Source);

internal sealed record AnalysisEdge(
    string FromNode,
    string ToNode,
    string RelationType);

internal sealed class AnalysisGraph
{
    public required IReadOnlyList<AnalysisNode> Nodes { get; init; }
    public required IReadOnlyList<AnalysisEdge> Edges { get; init; }
    public required IReadOnlyDictionary<string, AnalysisNode> NodeById { get; init; }
    public required IReadOnlyDictionary<string, IReadOnlyList<AnalysisEdge>> Adjacency { get; init; }
}

internal sealed record GraphMetadata(
    string Source,
    int NodeCount,
    int LinkCount);

internal sealed class GraphLoader
{
    public AnalysisGraph LoadGraphMl(string graphMlPath)
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
                return new AnalysisNode(
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
                return new AnalysisEdge(
                    source,
                    target,
                    attributes.TryGetValue("relation", out var relation) ? relation : "associates-with");
            })
            .Where(e => nodeSet.Contains(e.FromNode) && nodeSet.Contains(e.ToNode))
            .OrderBy(e => e.FromNode, StringComparer.Ordinal)
            .ThenBy(e => e.ToNode, StringComparer.Ordinal)
            .ThenBy(e => e.RelationType, StringComparer.Ordinal)
            .ToList();

        var adjacencyMutable = nodes.ToDictionary(
            n => n.NodeId,
            _ => new List<AnalysisEdge>(),
            StringComparer.Ordinal);

        foreach (var edge in edges)
        {
            adjacencyMutable[edge.FromNode].Add(edge);
            adjacencyMutable[edge.ToNode].Add(new AnalysisEdge(edge.ToNode, edge.FromNode, edge.RelationType));
        }

        var adjacency = adjacencyMutable.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<AnalysisEdge>)kvp.Value
                .OrderBy(e => e.ToNode, StringComparer.Ordinal)
                .ThenBy(e => e.RelationType, StringComparer.Ordinal)
                .ToList(),
            StringComparer.Ordinal);

        return new AnalysisGraph
        {
            Nodes = nodes,
            Edges = edges,
            NodeById = nodes.ToDictionary(n => n.NodeId, StringComparer.Ordinal),
            Adjacency = adjacency
        };
    }

    public GraphMetadata LoadMetadata(string engramIndexPath)
    {
        if (!File.Exists(engramIndexPath))
        {
            return new GraphMetadata("Lucid Research Corpus", 0, 0);
        }

        var json = File.ReadAllText(engramIndexPath);
        var model = JsonSerializer.Deserialize<EngramIndexMetadata>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (model is null)
        {
            return new GraphMetadata("Lucid Research Corpus", 0, 0);
        }

        return new GraphMetadata(
            Source: string.IsNullOrWhiteSpace(model.Source) ? "Lucid Research Corpus" : model.Source,
            NodeCount: model.NodeCount,
            LinkCount: model.LinkCount);
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

    private sealed record EngramIndexMetadata(
        string? Source,
        int NodeCount,
        int LinkCount);
}
