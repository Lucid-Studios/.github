namespace CorpusIndexer;

internal sealed class EngramBuilder
{
    private const string SourceLabel = "Lucid Research Corpus";

    public IReadOnlyList<EngramNode> Build(IReadOnlyList<ConceptHit> hits)
    {
        var ordered = hits
            .Distinct()
            .OrderBy(h => h.Concept, StringComparer.Ordinal)
            .ThenBy(h => h.Domain, StringComparer.Ordinal)
            .ThenBy(h => h.DocumentHash, StringComparer.Ordinal)
            .ToList();

        var nodes = new List<EngramNode>(ordered.Count);
        for (var index = 0; index < ordered.Count; index++)
        {
            var hit = ordered[index];
            nodes.Add(new EngramNode(
                Id: $"E-{(index + 1):D5}",
                Concept: hit.Concept,
                Domain: hit.Domain,
                Source: SourceLabel,
                Hash: hit.DocumentHash));
        }

        return nodes;
    }
}
