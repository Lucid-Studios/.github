namespace CradleTek.Memory.Models;

public sealed class OntologicalCleaverResult
{
    public required string InputText { get; init; }
    public required IReadOnlyList<OntologicalTokenResolution> Resolutions { get; init; }
    public required IReadOnlyList<RootEngram> Known { get; init; }
    public required IReadOnlyList<RootEngram> PartiallyKnown { get; init; }
    public required IReadOnlyList<string> Unknown { get; init; }
    public required OntologicalCleaverMetrics Metrics { get; init; }
}
