using System.Text;

namespace EngramResolver;

internal sealed class ResolverOutputFormatter
{
    public string FormatResolveResult(ResolveResult result)
    {
        if (result.Node is null)
        {
            return "ENGRAM not found.";
        }

        var concepts = result.ConnectedNodes
            .Select(n => n.Concept)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var builder = new StringBuilder();
        builder.AppendLine($"ENGRAM: {result.Node.NodeId}");
        builder.AppendLine($"Concept: {result.Node.Concept}");
        builder.AppendLine($"Domain: {ToTitleCase(result.Node.Domain)}");
        builder.AppendLine();
        builder.AppendLine("Connected Concepts:");
        if (concepts.Count == 0)
        {
            builder.AppendLine("  (none)");
        }
        else
        {
            foreach (var concept in concepts)
            {
                builder.AppendLine($"  {concept}");
            }
        }

        return builder.ToString().TrimEnd();
    }

    public string FormatConceptResult(ConceptResult result)
    {
        if (result.Node is null)
        {
            return "Concept not found.";
        }

        var builder = new StringBuilder();
        builder.AppendLine($"ENGRAM: {result.Node.NodeId}");
        builder.AppendLine($"Concept: {result.Node.Concept}");
        builder.AppendLine();
        builder.AppendLine("Relations:");
        var concepts = result.RelatedNodes.Select(n => n.Concept).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (concepts.Count == 0)
        {
            builder.AppendLine("  (none)");
        }
        else
        {
            foreach (var concept in concepts)
            {
                builder.AppendLine($"  {concept}");
            }
        }

        return builder.ToString().TrimEnd();
    }

    public string FormatPathResult(PathResult result)
    {
        if (!result.Found)
        {
            return $"No path found between {result.ConceptA} and {result.ConceptB}.";
        }

        var concepts = result.NodesInPath.Select(n => n.Concept).ToList();
        return string.Join(" -> ", concepts);
    }

    public string FormatClusterResult(ClusterResult result)
    {
        if (result.ClusterId is null || result.Members.Count == 0)
        {
            return $"No cluster found for concept {result.Concept}.";
        }

        var concepts = result.Members
            .Select(m => m.Concept)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var builder = new StringBuilder();
        builder.AppendLine("Community Cluster Members:");
        foreach (var concept in concepts)
        {
            builder.AppendLine($"  {concept}");
        }

        return builder.ToString().TrimEnd();
    }

    public string FormatExamplesMarkdown(IReadOnlyList<(string Query, string Output)> examples)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Engram Resolver Examples");
        builder.AppendLine();
        builder.AppendLine("Source: Lucid Research Corpus");
        builder.AppendLine();

        foreach (var example in examples)
        {
            builder.AppendLine($"## `{example.Query}`");
            builder.AppendLine();
            builder.AppendLine("```text");
            builder.AppendLine(example.Output.TrimEnd());
            builder.AppendLine("```");
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string ToTitleCase(string value)
    {
        var textInfo = System.Globalization.CultureInfo.InvariantCulture.TextInfo;
        return textInfo.ToTitleCase(value.Replace('-', ' '));
    }
}
