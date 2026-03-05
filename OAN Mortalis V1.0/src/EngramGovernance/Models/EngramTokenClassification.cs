namespace EngramGovernance.Models;

public sealed class EngramTokenClassification
{
    public required string Token { get; init; }
    public required string NormalizedToken { get; init; }
    public required EngramLookupClassification Classification { get; init; }
    public required string ResolutionReason { get; init; }
}
