namespace CorpusIndexer;

internal sealed class GraphLinker
{
    private static readonly Dictionary<string, string> RelationMap = new(StringComparer.Ordinal)
    {
        ["CME|CradleTek"] = "implemented-by",
        ["CME|Engram"] = "orchestrates",
        ["Cryptic|GEL"] = "mirrors-to",
        ["Engram|GEL"] = "persists-in",
        ["Engram|IdentityContinuity"] = "stabilizes",
        ["SLI|SymbolicPacketRouting"] = "operationalizes",
        ["SLI|SoulFrame"] = "governs",
        ["AgentiCore|SLI"] = "communicates-through",
        ["CradleTek|SoulFrame"] = "hosts"
    };

    public IReadOnlyList<EngramLink> Link(IReadOnlyList<EngramNode> nodes)
    {
        var links = new HashSet<EngramLink>();

        var byHash = nodes
            .GroupBy(n => n.Hash, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal);

        foreach (var group in byHash)
        {
            var docNodes = group
                .OrderBy(n => n.Concept, StringComparer.Ordinal)
                .ThenBy(n => n.Id, StringComparer.Ordinal)
                .ToList();

            for (var i = 0; i < docNodes.Count; i++)
            {
                for (var j = i + 1; j < docNodes.Count; j++)
                {
                    var left = docNodes[i];
                    var right = docNodes[j];
                    var relation = ResolveRelation(left.Concept, right.Concept);
                    links.Add(new EngramLink(left.Id, right.Id, relation));
                }
            }
        }

        return links
            .OrderBy(l => l.From, StringComparer.Ordinal)
            .ThenBy(l => l.To, StringComparer.Ordinal)
            .ThenBy(l => l.Relation, StringComparer.Ordinal)
            .ToList();
    }

    private static string ResolveRelation(string a, string b)
    {
        var key = string.CompareOrdinal(a, b) <= 0 ? $"{a}|{b}" : $"{b}|{a}";
        return RelationMap.TryGetValue(key, out var relation)
            ? relation
            : "associates-with";
    }
}
