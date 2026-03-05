namespace EngramGovernance.Models;

public sealed class EngramBootstrapResult
{
    public required IReadOnlyList<EngramTokenClassification> TokenClassifications { get; init; }
    public required IReadOnlyList<RootEngramRecord> RootEngramsCreated { get; init; }
    public required IReadOnlyList<ConstructorEngramRecord> ConstructorEngramsCreated { get; init; }
}
