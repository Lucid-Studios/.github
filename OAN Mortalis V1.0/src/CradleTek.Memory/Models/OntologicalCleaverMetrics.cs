namespace CradleTek.Memory.Models;

public sealed class OntologicalCleaverMetrics
{
    public required double KnownRatio { get; init; }
    public required double PartiallyKnownRatio { get; init; }
    public required double UnknownRatio { get; init; }
    public required string ConceptDensity { get; init; }
    public required string ContextStability { get; init; }
}
