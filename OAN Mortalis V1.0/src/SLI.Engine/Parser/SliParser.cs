using System.Text;
using SLI.Engine.Models;

namespace SLI.Engine.Parser;

public sealed class SliParser
{
    public IReadOnlyList<string> TokenizeAtoms(string source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        return Tokenize(source);
    }

    public SExpression ParseSingle(string source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        var tokens = Tokenize(source);
        var stream = new TokenStream(tokens);
        var expression = SExpressionBuilder.Build(stream);

        if (!stream.EndOfStream)
        {
            throw new FormatException("Unexpected trailing tokens in S-expression.");
        }

        return expression;
    }

    public IReadOnlyList<SExpression> ParseProgram(IEnumerable<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        return lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(ParseSingle)
            .ToList();
    }

    private static IReadOnlyList<string> Tokenize(string source)
    {
        var tokens = new List<string>();
        var builder = new StringBuilder();
        var inString = false;

        foreach (var character in source)
        {
            if (character == '"')
            {
                builder.Append(character);
                if (inString)
                {
                    tokens.Add(builder.ToString());
                    builder.Clear();
                }

                inString = !inString;
                continue;
            }

            if (inString)
            {
                builder.Append(character);
                continue;
            }

            if (character == '(' || character == ')')
            {
                FlushAtom(builder, tokens);
                tokens.Add(character.ToString());
                continue;
            }

            if (char.IsWhiteSpace(character))
            {
                FlushAtom(builder, tokens);
                continue;
            }

            builder.Append(character);
        }

        if (inString)
        {
            throw new FormatException("Unclosed string literal in S-expression.");
        }

        FlushAtom(builder, tokens);
        return tokens;
    }

    private static void FlushAtom(StringBuilder builder, List<string> tokens)
    {
        if (builder.Length == 0)
        {
            return;
        }

        tokens.Add(builder.ToString());
        builder.Clear();
    }
}
