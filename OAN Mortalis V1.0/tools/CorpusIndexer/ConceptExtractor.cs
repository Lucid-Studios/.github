namespace CorpusIndexer;

internal sealed class ConceptExtractor
{
    private static readonly IReadOnlyList<ConceptRule> Rules =
    [
        new("SLI", "symbolic-language", ["sli", "symbolic language", "symbolic packet"]),
        new("Engram", "identity-architecture", ["engram", "engrams", "engrammatic"]),
        new("CME", "orchestration", ["cme", "cradle management engine"]),
        new("GEL", "ledger-governance", ["gel", "governance event ledger"]),
        new("Cryptic", "private-plane", ["cryptic", "cgoa", "cvault"]),
        new("IdentityContinuity", "identity-architecture", ["identity continuity", "continuity", "identity lifecycle"]),
        new("SymbolicPacketRouting", "protocol-routing", ["symbolic packet routing", "packet routing", "routing constitution"]),
        new("SoulFrame", "governance-runtime", ["soulframe", "state boundary"]),
        new("AgentiCore", "runtime-cognition", ["agenticore", "cognition runtime"]),
        new("CradleTek", "runtime-host", ["cradletek", "host orchestration"])
    ];

    public IReadOnlyList<ConceptHit> Extract(IReadOnlyList<ScannedDocument> documents)
    {
        var hits = new HashSet<ConceptHit>();

        foreach (var document in documents)
        {
            var lowered = document.Content.ToLowerInvariant();

            foreach (var rule in Rules)
            {
                var matched = rule.Tokens.Any(token => lowered.Contains(token, StringComparison.Ordinal))
                              || document.Keywords.Any(k => rule.Tokens.Any(token => k.Contains(token.Replace(' ', '-'), StringComparison.Ordinal)));

                if (!matched)
                {
                    continue;
                }

                hits.Add(new ConceptHit(document.Hash, rule.Concept, rule.Domain));
            }
        }

        return hits
            .OrderBy(h => h.Concept, StringComparer.Ordinal)
            .ThenBy(h => h.Domain, StringComparer.Ordinal)
            .ThenBy(h => h.DocumentHash, StringComparer.Ordinal)
            .ToList();
    }

    private sealed record ConceptRule(string Concept, string Domain, IReadOnlyList<string> Tokens);
}
