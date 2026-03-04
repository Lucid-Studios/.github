using System.Text.Json;

namespace CorpusGraphVisualizer;

internal sealed class GraphLoader
{
    public (IReadOnlyList<GraphNode> Nodes, IReadOnlyList<GraphEdge> Edges) Load(string engramIndexJsonPath)
    {
        if (!File.Exists(engramIndexJsonPath))
        {
            throw new FileNotFoundException("engram_index.json not found.", engramIndexJsonPath);
        }

        var json = File.ReadAllText(engramIndexJsonPath);
        var model = JsonSerializer.Deserialize<EngramIndexDocument>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (model is null)
        {
            throw new InvalidOperationException("Unable to deserialize engram_index.json.");
        }

        var nodes = (model.Nodes ?? [])
            .Select(n => new GraphNode(
                n.Id ?? string.Empty,
                n.Concept ?? string.Empty,
                n.Domain ?? string.Empty,
                n.Source ?? "Lucid Research Corpus",
                n.Hash ?? string.Empty))
            .Where(n => !string.IsNullOrWhiteSpace(n.NodeId))
            .OrderBy(n => n.NodeId, StringComparer.Ordinal)
            .ToList();

        var edges = (model.Links ?? [])
            .Select(e => new GraphEdge(
                e.From ?? string.Empty,
                e.To ?? string.Empty,
                e.Relation ?? "associates-with"))
            .Where(e => !string.IsNullOrWhiteSpace(e.FromNode) && !string.IsNullOrWhiteSpace(e.ToNode))
            .OrderBy(e => e.FromNode, StringComparer.Ordinal)
            .ThenBy(e => e.ToNode, StringComparer.Ordinal)
            .ThenBy(e => e.RelationType, StringComparer.Ordinal)
            .ToList();

        return (nodes, edges);
    }

    private sealed record EngramIndexDocument(
        string? Source,
        int NodeCount,
        int LinkCount,
        List<EngramNodeJson>? Nodes,
        List<EngramLinkJson>? Links);

    private sealed record EngramNodeJson(
        string? Id,
        string? Concept,
        string? Domain,
        string? Source,
        string? Hash);

    private sealed record EngramLinkJson(
        string? From,
        string? To,
        string? Relation);
}
