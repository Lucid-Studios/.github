using CradleTek.Memory.Interfaces;
using CradleTek.Memory.Models;
using CradleTek.Memory.Services;
using System.Text.RegularExpressions;

namespace SLI.Ingestion;

public sealed partial class RootEngramMatcher
{
    private readonly IRootOntologicalCleaver _rootCleaver;

    public RootEngramMatcher(IRootOntologicalCleaver? rootCleaver = null)
    {
        _rootCleaver = rootCleaver ?? new RootAtlasOntologicalCleaver();
    }

    public async Task<EngramMatchResult> MatchAsync(
        CleavedOntology cleavedOntology,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cleavedOntology);

        var lookupInput = string.Join(" ", cleavedOntology.Tokens.Where(IsLookupToken));
        var lookup = await _rootCleaver.CleaveAsync(
                string.IsNullOrWhiteSpace(lookupInput) ? "identity" : lookupInput,
                cancellationToken)
            .ConfigureAwait(false);

        var knownById = new Dictionary<string, RootEngram>(StringComparer.OrdinalIgnoreCase);
        foreach (var known in lookup.Known.Concat(lookup.PartiallyKnown))
        {
            knownById[known.SymbolicId] = known;
        }

        foreach (var token in cleavedOntology.Tokens)
        {
            if (OperatorRoots.TryGetValue(token, out var root))
            {
                knownById[root.SymbolicId] = root;
            }
        }

        var knownTerms = new HashSet<string>(
            knownById.Values.SelectMany(entry => entry.VariantForms.Append(entry.RootTerm)),
            StringComparer.OrdinalIgnoreCase);

        var candidates = new List<EngramCandidate>();
        foreach (var token in cleavedOntology.Tokens)
        {
            if (!IsCandidateToken(token))
            {
                continue;
            }

            if (knownTerms.Contains(token) || OperatorRoots.ContainsKey(token))
            {
                continue;
            }

            candidates.Add(new EngramCandidate
            {
                Token = token,
                Context = string.Join(" ", cleavedOntology.Expressions.DefaultIfEmpty("natural-language")),
                DomainGuess = GuessDomain(cleavedOntology, token)
            });
        }

        foreach (var unknown in lookup.Unknown.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (candidates.Any(c => string.Equals(c.Token, unknown, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            candidates.Add(new EngramCandidate
            {
                Token = unknown,
                Context = string.Join(" ", cleavedOntology.Expressions.DefaultIfEmpty("natural-language")),
                DomainGuess = GuessDomain(cleavedOntology, unknown)
            });
        }

        return new EngramMatchResult
        {
            KnownEngrams = knownById.Values.OrderBy(v => v.SymbolicId, StringComparer.Ordinal).ToList(),
            EngramCandidates = candidates
                .OrderBy(c => c.Token, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.DomainGuess, StringComparer.OrdinalIgnoreCase)
                .ToList()
        };
    }

    private static string GuessDomain(CleavedOntology cleavedOntology, string token)
    {
        if (cleavedOntology.Expressions.Count > 0 ||
            MathTokenRegex().IsMatch(token) ||
            MathDomainTokens.Contains(token))
        {
            return "mathematics";
        }

        return "general";
    }

    private static bool IsLookupToken(string token)
    {
        return !string.IsNullOrWhiteSpace(token) &&
               !OperatorRoots.ContainsKey(token) &&
               !MathTokenRegex().IsMatch(token);
    }

    private static bool IsCandidateToken(string token)
    {
        return !string.IsNullOrWhiteSpace(token) &&
               !IgnoredTokens.Contains(token) &&
               !token.Contains('=', StringComparison.Ordinal) &&
               !MathTokenRegex().IsMatch(token);
    }

    private static readonly HashSet<string> IgnoredTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        "for"
    };

    private static readonly HashSet<string> MathDomainTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        "solve",
        "equation",
        "variable",
        "addition",
        "subtraction",
        "multiplication",
        "division"
    };

    private static readonly IReadOnlyDictionary<string, RootEngram> OperatorRoots =
        new Dictionary<string, RootEngram>(StringComparer.OrdinalIgnoreCase)
        {
            ["addition"] = BuildOperatorRoot("addition", "ADDITION", "arithmetic"),
            ["subtraction"] = BuildOperatorRoot("subtraction", "SUBTRACTION", "arithmetic"),
            ["multiplication"] = BuildOperatorRoot("multiplication", "MULTIPLICATION", "arithmetic"),
            ["division"] = BuildOperatorRoot("division", "DIVISION", "arithmetic"),
            ["equation"] = BuildOperatorRoot("equation", "EQUATION", "algebra"),
            ["variable"] = BuildOperatorRoot("variable", "VARIABLE", "algebra"),
            ["solve"] = BuildOperatorRoot("solve", "SOLVE", "algebra")
        };

    private static RootEngram BuildOperatorRoot(string token, string symbol, string domain)
    {
        return new RootEngram
        {
            SymbolicId = $"ATLAS.SYM.{symbol}",
            AtlasDomain = $"atlas.{domain}",
            RootTerm = token,
            VariantForms = [token],
            FrequencyWeight = 1,
            DictionaryPointer = $"atlas://operator/{token}"
        };
    }

    [GeneratedRegex(@"^[-+]?\d+([.]\d+)?$|^[-+]?\d+[a-zA-Z]$", RegexOptions.Compiled)]
    private static partial Regex MathTokenRegex();
}

public sealed class EngramMatchResult
{
    public required IReadOnlyList<RootEngram> KnownEngrams { get; init; }
    public required IReadOnlyList<EngramCandidate> EngramCandidates { get; init; }
}

public sealed class EngramCandidate
{
    public required string Token { get; init; }
    public required string Context { get; init; }
    public required string DomainGuess { get; init; }
}
