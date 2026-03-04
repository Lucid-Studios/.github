namespace CorpusGraphVisualizer;

internal sealed class LayoutEngine
{
    public IReadOnlyList<PositionedNode> ComputeLayout(
        EngramGraph graph,
        int width = 1600,
        int height = 1200,
        int iterations = 250)
    {
        var nodes = graph.Nodes.OrderBy(n => n.NodeId, StringComparer.Ordinal).ToList();
        if (nodes.Count == 0)
        {
            return [];
        }

        var rng = new Random(42);
        var positions = new Dictionary<string, (double X, double Y)>(StringComparer.Ordinal);
        foreach (var node in nodes)
        {
            positions[node.NodeId] = (
                X: rng.NextDouble() * width,
                Y: rng.NextDouble() * height);
        }

        var area = (double)width * height;
        var k = Math.Sqrt(area / nodes.Count);
        var temperatureStart = Math.Min(width, height) / 8.0;

        for (var iter = 0; iter < iterations; iter++)
        {
            var temperature = temperatureStart * (1.0 - (double)iter / iterations);
            var displacement = nodes.ToDictionary(n => n.NodeId, _ => (DX: 0.0, DY: 0.0), StringComparer.Ordinal);

            for (var i = 0; i < nodes.Count; i++)
            {
                var v = nodes[i];
                for (var j = i + 1; j < nodes.Count; j++)
                {
                    var u = nodes[j];
                    var deltaX = positions[v.NodeId].X - positions[u.NodeId].X;
                    var deltaY = positions[v.NodeId].Y - positions[u.NodeId].Y;
                    var distance = Math.Max(0.01, Math.Sqrt(deltaX * deltaX + deltaY * deltaY));
                    var force = (k * k) / distance;
                    var rx = (deltaX / distance) * force;
                    var ry = (deltaY / distance) * force;

                    displacement[v.NodeId] = (displacement[v.NodeId].DX + rx, displacement[v.NodeId].DY + ry);
                    displacement[u.NodeId] = (displacement[u.NodeId].DX - rx, displacement[u.NodeId].DY - ry);
                }
            }

            foreach (var edge in graph.Edges)
            {
                var v = edge.FromNode;
                var u = edge.ToNode;
                var deltaX = positions[v].X - positions[u].X;
                var deltaY = positions[v].Y - positions[u].Y;
                var distance = Math.Max(0.01, Math.Sqrt(deltaX * deltaX + deltaY * deltaY));
                var force = (distance * distance) / k;
                var ax = (deltaX / distance) * force;
                var ay = (deltaY / distance) * force;

                displacement[v] = (displacement[v].DX - ax, displacement[v].DY - ay);
                displacement[u] = (displacement[u].DX + ax, displacement[u].DY + ay);
            }

            foreach (var node in nodes)
            {
                var (dx, dy) = displacement[node.NodeId];
                var displacementNorm = Math.Max(0.01, Math.Sqrt(dx * dx + dy * dy));
                var limited = Math.Min(displacementNorm, temperature);
                var newX = positions[node.NodeId].X + (dx / displacementNorm) * limited;
                var newY = positions[node.NodeId].Y + (dy / displacementNorm) * limited;

                positions[node.NodeId] = (
                    X: Math.Clamp(newX, 30, width - 30),
                    Y: Math.Clamp(newY, 30, height - 30));
            }
        }

        return nodes.Select(n => new PositionedNode(
            n,
            positions[n.NodeId].X,
            positions[n.NodeId].Y)).ToList();
    }
}
