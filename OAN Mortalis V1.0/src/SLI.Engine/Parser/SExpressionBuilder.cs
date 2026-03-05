using SLI.Engine.Models;

namespace SLI.Engine.Parser;

public static class SExpressionBuilder
{
    public static SExpression Build(TokenStream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var token = stream.Read();
        if (token == "(")
        {
            var children = new List<SExpression>();
            while (!stream.EndOfStream && stream.Peek() != ")")
            {
                children.Add(Build(stream));
            }

            if (stream.EndOfStream)
            {
                throw new FormatException("Unclosed S-expression.");
            }

            stream.Read(); // consume ')'
            return SExpression.ListNode(children);
        }

        if (token == ")")
        {
            throw new FormatException("Unexpected closing parenthesis.");
        }

        return SExpression.AtomNode(token);
    }
}
