namespace GEL.Graphs;

/// <summary>
/// Ordered procedural engram pipeline executed by symbolic runtime layers.
/// </summary>
public sealed class ProceduralFunctorGraph
{
    public required IReadOnlyList<string> FunctorPipeline { get; init; }

    public string ToLispComposition()
    {
        if (FunctorPipeline.Count == 0)
        {
            return "(compose)";
        }

        var segments = string.Join(" ", FunctorPipeline.Reverse());
        return $"(compose {segments})";
    }

    public IReadOnlyList<string> BuildFunctorPath() => FunctorPipeline.ToList();
}
