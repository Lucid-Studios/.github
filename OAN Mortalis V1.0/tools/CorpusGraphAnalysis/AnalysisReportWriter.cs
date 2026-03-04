using System.Text;
using System.Text.Json;
using System.Xml;

namespace CorpusGraphAnalysis;

internal sealed class AnalysisReportWriter
{
    public void WriteReport(
        string reportPath,
        AnalysisGraph graph,
        GraphMetadata metadata,
        int connectedComponents,
        IReadOnlyList<(string NodeId, double Score)> topBetweenness,
        IReadOnlyList<ArticulationResult> articulations,
        CommunitySummary communities,
        IReadOnlyList<ConceptPathResult> paths)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Engram Graph Structural Analysis");
        builder.AppendLine();
        builder.AppendLine($"Source: {metadata.Source}");
        builder.AppendLine();
        builder.AppendLine("## Graph Summary");
        builder.AppendLine();
        builder.AppendLine($"- Node count: {graph.Nodes.Count}");
        builder.AppendLine($"- Edge count: {graph.Edges.Count}");
        builder.AppendLine($"- Connected components: {connectedComponents}");
        builder.AppendLine();
        builder.AppendLine("## Top 20 Betweenness Centrality Nodes");
        builder.AppendLine();

        var rank = 1;
        foreach (var (nodeId, score) in topBetweenness)
        {
            var node = graph.NodeById[nodeId];
            builder.AppendLine($"1. `{nodeId}` ({node.Concept}, {node.Domain}) - {score:F4}");
            rank++;
            if (rank > 20)
            {
                break;
            }
        }

        builder.AppendLine();
        builder.AppendLine("## Articulation Nodes");
        builder.AppendLine();
        if (articulations.Count == 0)
        {
            builder.AppendLine("- None detected.");
        }
        else
        {
            foreach (var articulation in articulations)
            {
                var node = graph.NodeById[articulation.NodeId];
                builder.AppendLine(
                    $"- `{articulation.NodeId}` ({node.Concept}) impact={articulation.StructuralImpact}, components-after-removal={articulation.ComponentsAfterRemoval}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## Community Statistics");
        builder.AppendLine();
        builder.AppendLine($"- Cluster count: {communities.MembersByCluster.Count}");
        foreach (var cluster in communities.MembersByCluster.OrderByDescending(kvp => kvp.Value.Count).ThenBy(kvp => kvp.Key, StringComparer.Ordinal))
        {
            builder.AppendLine($"- {cluster.Key}: {cluster.Value.Count} nodes");
        }

        builder.AppendLine();
        builder.AppendLine("## Example Concept Paths");
        builder.AppendLine();
        foreach (var path in paths)
        {
            if (!path.Found)
            {
                builder.AppendLine($"- {path.FromConcept} -> {path.ToConcept}: no path found");
                continue;
            }

            var chain = string.Join(" -> ", path.ConceptPath);
            builder.AppendLine($"- {path.FromConcept} -> {path.ToConcept}: {chain}");
            builder.AppendLine($"  - Node chain: {string.Join(" -> ", path.NodePath)}");
        }

        File.WriteAllText(reportPath, builder.ToString());
    }

    public void WriteClustersJson(
        string path,
        CommunitySummary communities)
    {
        var payload = new
        {
            Source = "Lucid Research Corpus",
            ClusterCount = communities.MembersByCluster.Count,
            Clusters = communities.MembersByCluster
                .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
                .Select(kvp => new
                {
                    ClusterId = kvp.Key,
                    Size = kvp.Value.Count,
                    Members = kvp.Value
                })
                .ToList()
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json + Environment.NewLine);
    }

    public void WriteBackboneGraphMl(
        string path,
        AnalysisGraph graph,
        IReadOnlyCollection<string> backboneNodes)
    {
        var nodeSet = backboneNodes.ToHashSet(StringComparer.Ordinal);
        var nodes = graph.Nodes.Where(n => nodeSet.Contains(n.NodeId)).OrderBy(n => n.NodeId, StringComparer.Ordinal).ToList();
        var edges = graph.Edges
            .Where(e => nodeSet.Contains(e.FromNode) && nodeSet.Contains(e.ToNode))
            .OrderBy(e => e.FromNode, StringComparer.Ordinal)
            .ThenBy(e => e.ToNode, StringComparer.Ordinal)
            .ToList();

        using var writer = XmlWriter.Create(path, new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = false
        });

        writer.WriteStartDocument();
        writer.WriteStartElement("graphml", "http://graphml.graphdrawing.org/xmlns");
        writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
        writer.WriteAttributeString(
            "xsi",
            "schemaLocation",
            "http://www.w3.org/2001/XMLSchema-instance",
            "http://graphml.graphdrawing.org/xmlns http://graphml.graphdrawing.org/xmlns/1.0/graphml.xsd");

        WriteKey(writer, "d0", "node", "concept", "string");
        WriteKey(writer, "d1", "node", "domain", "string");
        WriteKey(writer, "d2", "node", "source", "string");
        WriteKey(writer, "d3", "edge", "relation", "string");

        writer.WriteStartElement("graph");
        writer.WriteAttributeString("id", "EngramBackbone");
        writer.WriteAttributeString("edgedefault", "undirected");

        foreach (var node in nodes)
        {
            writer.WriteStartElement("node");
            writer.WriteAttributeString("id", node.NodeId);
            WriteData(writer, "d0", node.Concept);
            WriteData(writer, "d1", node.Domain);
            WriteData(writer, "d2", node.Source);
            writer.WriteEndElement();
        }

        var index = 0;
        foreach (var edge in edges)
        {
            writer.WriteStartElement("edge");
            writer.WriteAttributeString("id", $"b{index++:D5}");
            writer.WriteAttributeString("source", edge.FromNode);
            writer.WriteAttributeString("target", edge.ToNode);
            WriteData(writer, "d3", edge.RelationType);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    private static void WriteKey(XmlWriter writer, string id, string @for, string name, string type)
    {
        writer.WriteStartElement("key");
        writer.WriteAttributeString("id", id);
        writer.WriteAttributeString("for", @for);
        writer.WriteAttributeString("attr.name", name);
        writer.WriteAttributeString("attr.type", type);
        writer.WriteEndElement();
    }

    private static void WriteData(XmlWriter writer, string key, string value)
    {
        writer.WriteStartElement("data");
        writer.WriteAttributeString("key", key);
        writer.WriteString(value);
        writer.WriteEndElement();
    }
}
