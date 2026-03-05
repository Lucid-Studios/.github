using System.Text;

namespace SLI.Engine.Models;

public sealed class SExpression
{
    private SExpression(string? atom, IReadOnlyList<SExpression>? children)
    {
        Atom = atom;
        Children = children ?? Array.Empty<SExpression>();
    }

    public string? Atom { get; }
    public IReadOnlyList<SExpression> Children { get; }
    public bool IsAtom => Atom is not null;

    public static SExpression AtomNode(string atom)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(atom);
        return new SExpression(atom, null);
    }

    public static SExpression ListNode(IEnumerable<SExpression> children)
    {
        ArgumentNullException.ThrowIfNull(children);
        return new SExpression(null, children.ToList());
    }

    public string ToCanonicalString()
    {
        if (IsAtom)
        {
            return Atom!;
        }

        var builder = new StringBuilder();
        builder.Append('(');
        for (var index = 0; index < Children.Count; index++)
        {
            if (index > 0)
            {
                builder.Append(' ');
            }

            builder.Append(Children[index].ToCanonicalString());
        }

        builder.Append(')');
        return builder.ToString();
    }

    public override string ToString() => ToCanonicalString();
}
