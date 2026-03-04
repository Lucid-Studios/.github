using System.Text.Json;
using System.Text.Json.Serialization;

namespace CorpusIndexer;

internal sealed class IndexWriter
{
    public void Write(string outputDirectory, IReadOnlyList<EngramNode> nodes, IReadOnlyList<EngramLink> links)
    {
        Directory.CreateDirectory(outputDirectory);

        var nodesPath = Path.Combine(outputDirectory, "engram_nodes.lisp");
        var linksPath = Path.Combine(outputDirectory, "engram_links.lisp");
        var jsonPath = Path.Combine(outputDirectory, "engram_index.json");

        File.WriteAllText(nodesPath, BuildNodesLisp(nodes));
        File.WriteAllText(linksPath, BuildLinksLisp(links));
        File.WriteAllText(jsonPath, BuildJson(nodes, links));
    }

    private static string BuildNodesLisp(IReadOnlyList<EngramNode> nodes)
    {
        var lines = new List<string>
        {
            ";;; Auto-generated symbolic engram nodes.",
            ";;; Source: Lucid Research Corpus",
            "(defparameter *engram-nodes*",
            "  '("
        };

        foreach (var node in nodes)
        {
            lines.Add(
                $"    (engram-node :id \"{node.Id}\" :concept {ToSymbol(node.Concept)} :domain {ToSymbol(node.Domain)} :source \"{node.Source}\" :hash \"{node.Hash}\")");
        }

        lines.Add("   ))");
        lines.Add(string.Empty);
        lines.Add("(defun find-engram (&key concept)");
        lines.Add("  (let ((target (if (symbolp concept) (symbol-name concept) concept)))");
        lines.Add("    (remove-if-not");
        lines.Add("      (lambda (node)");
        lines.Add("        (let ((value (symbol-name (getf (cdr node) :concept))))");
        lines.Add("          (or (null target) (string-equal value target))))");
        lines.Add("      *engram-nodes*)))");

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string BuildLinksLisp(IReadOnlyList<EngramLink> links)
    {
        var lines = new List<string>
        {
            ";;; Auto-generated symbolic engram links.",
            "(defparameter *engram-links*",
            "  '("
        };

        foreach (var link in links)
        {
            lines.Add(
                $"    (engram-link :from \"{link.From}\" :to \"{link.To}\" :relation {ToSymbol(link.Relation)})");
        }

        lines.Add("   ))");
        lines.Add(string.Empty);
        lines.Add("(defun find-relations (&key node)");
        lines.Add("  (let ((target (if (symbolp node) (symbol-name node) node)))");
        lines.Add("    (remove-if-not");
        lines.Add("      (lambda (edge)");
        lines.Add("        (let ((from (getf (cdr edge) :from))");
        lines.Add("              (to (getf (cdr edge) :to)))");
        lines.Add("          (or (null target)");
        lines.Add("              (string-equal from target)");
        lines.Add("              (string-equal to target))))");
        lines.Add("      *engram-links*)))");

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string BuildJson(IReadOnlyList<EngramNode> nodes, IReadOnlyList<EngramLink> links)
    {
        var payload = new EngramIndexPayload(
            Source: "Lucid Research Corpus",
            NodeCount: nodes.Count,
            LinkCount: links.Count,
            Nodes: nodes,
            Links: links);

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        }) + Environment.NewLine;
    }

    private static string ToSymbol(string value)
    {
        return value
            .Replace("-", "_", StringComparison.Ordinal)
            .Replace(" ", "_", StringComparison.Ordinal)
            .ToUpperInvariant();
    }

    private sealed record EngramIndexPayload(
        string Source,
        int NodeCount,
        int LinkCount,
        IReadOnlyList<EngramNode> Nodes,
        IReadOnlyList<EngramLink> Links);
}
