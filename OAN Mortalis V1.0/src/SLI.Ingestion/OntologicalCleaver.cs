using System.Text.RegularExpressions;

namespace SLI.Ingestion;

public sealed partial class OntologicalCleaver
{
    public CleavedOntology Cleave(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var expressions = EquationRegex()
            .Matches(input)
            .Select(match => match.Value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var tokens = LexemeRegex()
            .Matches(input)
            .Select(match => NormalizeToken(match.Value))
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var expression in expressions)
        {
            tokens.Add(expression);
        }

        return new CleavedOntology
        {
            Tokens = tokens,
            Expressions = expressions
        };
    }

    private static string NormalizeToken(string token)
    {
        var value = token.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        if (VariableRegex().IsMatch(value))
        {
            return "variable";
        }

        if (OperatorLexemeMap.TryGetValue(value, out var mapped))
        {
            return mapped;
        }

        value = value.ToLowerInvariant();
        if (WordLexemeMap.TryGetValue(value, out mapped))
        {
            return mapped;
        }

        return value;
    }

    private static readonly IReadOnlyDictionary<string, string> OperatorLexemeMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["+"] = "addition",
            ["-"] = "subtraction",
            ["*"] = "multiplication",
            ["/"] = "division",
            ["="] = "equation"
        };

    private static readonly IReadOnlyDictionary<string, string> WordLexemeMap =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["solve"] = "solve",
            ["solving"] = "solve",
            ["compute"] = "solve",
            ["calculate"] = "solve",
            ["for"] = "for",
            ["equation"] = "equation",
            ["equals"] = "equation",
            ["plus"] = "addition",
            ["add"] = "addition",
            ["minus"] = "subtraction",
            ["subtract"] = "subtraction",
            ["times"] = "multiplication",
            ["multiply"] = "multiplication",
            ["divide"] = "division"
        };

    [GeneratedRegex(@"[-+]?\d+[a-zA-Z]\s*[+\-]\s*[-+]?\d+\s*=\s*[-+]?\d+", RegexOptions.Compiled)]
    private static partial Regex EquationRegex();

    [GeneratedRegex(@"[A-Za-z][A-Za-z\-']*|[-+]?\d+[A-Za-z]|[-+]?\d+|[+\-*/=]", RegexOptions.Compiled)]
    private static partial Regex LexemeRegex();

    [GeneratedRegex(@"^[-+]?\d+[a-zA-Z]$", RegexOptions.Compiled)]
    private static partial Regex VariableRegex();
}

public sealed class CleavedOntology
{
    public required IReadOnlyList<string> Tokens { get; init; }
    public required IReadOnlyList<string> Expressions { get; init; }
}

