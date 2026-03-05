namespace CradleTek.Memory.Models;

public sealed class OntologicalTokenResolution
{
    public required string Token { get; init; }
    public required string NormalizedToken { get; init; }
    public required OntologicalCleaverClassification Classification { get; init; }
    public RootEngram? RootEngram { get; init; }
    public required string ResolutionReason { get; init; }
}
