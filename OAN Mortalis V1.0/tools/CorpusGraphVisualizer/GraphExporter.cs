using System.Text;
using System.Xml;

namespace CorpusGraphVisualizer;

internal sealed class GraphExporter
{
    public void ExportGraphMl(string path, EngramGraph graph)
    {
        using var writer = XmlWriter.Create(path, new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = false,
            Encoding = Encoding.UTF8
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
        writer.WriteAttributeString("id", "EngramGraph");
        writer.WriteAttributeString("edgedefault", "undirected");

        foreach (var node in graph.Nodes.OrderBy(n => n.NodeId, StringComparer.Ordinal))
        {
            writer.WriteStartElement("node");
            writer.WriteAttributeString("id", node.NodeId);
            WriteData(writer, "d0", node.Concept);
            WriteData(writer, "d1", node.Domain);
            WriteData(writer, "d2", node.Source);
            writer.WriteEndElement();
        }

        var edgeIndex = 0;
        foreach (var edge in graph.Edges.OrderBy(e => e.FromNode, StringComparer.Ordinal).ThenBy(e => e.ToNode, StringComparer.Ordinal))
        {
            writer.WriteStartElement("edge");
            writer.WriteAttributeString("id", $"e{edgeIndex++:D5}");
            writer.WriteAttributeString("source", edge.FromNode);
            writer.WriteAttributeString("target", edge.ToNode);
            WriteData(writer, "d3", edge.RelationType);
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    public void ExportSvg(string path, EngramGraph graph, IReadOnlyList<PositionedNode> positioned)
    {
        const int width = 1600;
        const int height = 1200;
        var map = positioned.ToDictionary(p => p.Node.NodeId, StringComparer.Ordinal);

        var builder = new StringBuilder();
        builder.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        builder.AppendLine($"""<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}">""");
        builder.AppendLine("""  <rect x="0" y="0" width="100%" height="100%" fill="#0b1220" />""");

        foreach (var edge in graph.Edges)
        {
            if (!map.TryGetValue(edge.FromNode, out var from) || !map.TryGetValue(edge.ToNode, out var to))
            {
                continue;
            }

            builder.AppendLine(
                $"""  <line x1="{from.X:F2}" y1="{from.Y:F2}" x2="{to.X:F2}" y2="{to.Y:F2}" stroke="#5f6b85" stroke-opacity="0.55" stroke-width="1.2" />""");
        }

        foreach (var p in positioned.OrderBy(p => p.Node.NodeId, StringComparer.Ordinal))
        {
            var color = ColorForDomain(p.Node.Domain);
            builder.AppendLine($"""  <circle cx="{p.X:F2}" cy="{p.Y:F2}" r="5.5" fill="{color}" />""");
            builder.AppendLine(
                $"""  <text x="{p.X + 7:F2}" y="{p.Y - 7:F2}" fill="#d8e0ff" font-size="10" font-family="Consolas, 'Courier New', monospace">{EscapeXml($"{p.Node.NodeId}:{p.Node.Concept}")}</text>""");
        }

        builder.AppendLine("</svg>");
        File.WriteAllText(path, builder.ToString());
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

    private static string ColorForDomain(string domain)
    {
        return domain switch
        {
            "symbolic-language" => "#64b5f6",
            "orchestration" => "#ffb74d",
            "ledger-governance" => "#81c784",
            "private-plane" => "#ba68c8",
            "protocol-routing" => "#4dd0e1",
            "identity-architecture" => "#f48fb1",
            "governance-runtime" => "#ffd54f",
            "runtime-cognition" => "#90caf9",
            "runtime-host" => "#ff8a65",
            _ => "#cfd8dc"
        };
    }

    private static string EscapeXml(string value)
    {
        return value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("'", "&apos;", StringComparison.Ordinal);
    }
}
