namespace SLI.Engine.Runtime;

public sealed class SliExecutionGraph
{
    private readonly List<string> _nodes = [];
    private readonly List<(string From, string To)> _edges = [];

    public IReadOnlyList<string> Nodes => _nodes;
    public IReadOnlyList<(string From, string To)> Edges => _edges;

    public void AddNode(string node)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(node);
        _nodes.Add(node);
    }

    public void AddEdge(string from, string to)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(from);
        ArgumentException.ThrowIfNullOrWhiteSpace(to);
        _edges.Add((from, to));
    }
}
