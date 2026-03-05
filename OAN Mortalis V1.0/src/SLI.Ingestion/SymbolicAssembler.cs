using System.Security.Cryptography;
using System.Text;

namespace SLI.Ingestion;

public sealed class SymbolicAssembler
{
    public SliExpression Assemble(
        IReadOnlyList<ConstructorEngramRecord> constructors,
        EngramMatchResult matchResult,
        string sourceInput)
    {
        ArgumentNullException.ThrowIfNull(constructors);
        ArgumentNullException.ThrowIfNull(matchResult);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceInput);

        if (constructors.Count == 0)
        {
            throw new InvalidOperationException("Cannot assemble SLI expression without constructor engrams.");
        }

        var primary = constructors[0];
        if (primary.EquationDescriptor is null)
        {
            var fallbackTree = $"⧉\n  {primary.SymbolicStructure}";
            return new SliExpression
            {
                SymbolTree = fallbackTree,
                EngramReferences = matchResult.KnownEngrams.Select(e => e.SymbolicId).Distinct(StringComparer.Ordinal).ToList(),
                TraceSeed = TraceSeed(sourceInput, fallbackTree),
                ProgramExpressions = [primary.SymbolicStructure]
            };
        }

        var equation = primary.EquationDescriptor;
        var intermediateValue = equation.Operator == '-'
            ? equation.RightHandSide + equation.Constant
            : equation.RightHandSide - equation.Constant;

        var solvedValue = equation.Coefficient == 0
            ? "undefined"
            : FormatDivision(intermediateValue, equation.Coefficient);

        var inverseOp = equation.Operator == '-' ? "⊕" : "⊖";
        var constructor = primary.SymbolicStructure;
        var derivation =
            $"(⊢ (→ ({inverseOp} {equation.Constant}) (= (⊗ {equation.Coefficient} {equation.Variable}) {intermediateValue})) " +
            $"(→ (÷ {equation.Coefficient}) (= {equation.Variable} {solvedValue})))";
        var conclusion =
            $"(∴ (∧ deterministic transparent_ops single_solution (= {equation.Variable} {solvedValue})))";

        var symbolTree =
            "⧉\n" +
            $"  {constructor}\n\n" +
            $"  {derivation}\n\n" +
            $"  {conclusion}";

        return new SliExpression
        {
            SymbolTree = symbolTree,
            EngramReferences = matchResult.KnownEngrams
                .Select(entry => entry.SymbolicId)
                .Distinct(StringComparer.Ordinal)
                .ToList(),
            TraceSeed = TraceSeed(sourceInput, symbolTree),
            ProgramExpressions =
            [
                constructor,
                derivation,
                conclusion
            ]
        };
    }

    private static string TraceSeed(string sourceInput, string symbolTree)
    {
        var payload = $"{sourceInput.Trim()}|{symbolTree}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string FormatDivision(int numerator, int denominator)
    {
        if (denominator == 0)
        {
            return "undefined";
        }

        if (numerator % denominator == 0)
        {
            return (numerator / denominator).ToString();
        }

        var quotient = numerator / (double)denominator;
        return quotient.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
    }
}

public sealed class SliExpression
{
    public required string SymbolTree { get; init; }
    public required IReadOnlyList<string> EngramReferences { get; init; }
    public required string TraceSeed { get; init; }
    public required IReadOnlyList<string> ProgramExpressions { get; init; }
}

