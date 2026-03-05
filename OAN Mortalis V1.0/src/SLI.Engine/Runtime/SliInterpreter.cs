using SLI.Engine.Models;

namespace SLI.Engine.Runtime;

public sealed class SliInterpreter
{
    private readonly SliSymbolTable _symbolTable;

    public SliInterpreter(SliSymbolTable symbolTable)
    {
        _symbolTable = symbolTable;
    }

    public async Task ExecuteProgramAsync(
        IReadOnlyList<SExpression> program,
        SliExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(context);

        string? previousNode = null;
        foreach (var expression in program)
        {
            var result = await ExecuteAsync(expression, context, cancellationToken).ConfigureAwait(false);
            var currentNode = expression.ToCanonicalString();
            context.ExecutionGraph.AddNode(currentNode);
            if (previousNode is not null)
            {
                context.ExecutionGraph.AddEdge(previousNode, currentNode);
            }

            previousNode = currentNode;
            context.ExecutionGraph.AddNode(result.ToCanonicalString());
        }
    }

    public async Task<SExpression> ExecuteAsync(
        SExpression expression,
        SliExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(context);

        if (expression.IsAtom)
        {
            return expression;
        }

        if (expression.Children.Count == 0)
        {
            return SExpression.AtomNode("()");
        }

        var op = expression.Children[0].Atom;
        if (string.IsNullOrWhiteSpace(op))
        {
            return SExpression.AtomNode("invalid-op");
        }

        if (_symbolTable.TryResolve(op, out var handler))
        {
            return await handler(expression, context, cancellationToken).ConfigureAwait(false);
        }

        context.AddTrace($"unknown-op({op})");
        return SExpression.AtomNode("unknown-op");
    }
}
