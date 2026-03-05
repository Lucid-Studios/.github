namespace EngramGovernance.Models;

public sealed class ConstructorEngramRecord
{
    public required ConstructorEngramType ConstructorType { get; init; }
    public required string RootReference { get; init; }
    public required string SymbolicStructure { get; init; }
    public required IReadOnlyList<string> PredicateRules { get; init; }
    public required string ContextDomain { get; init; }
    public required string Provenance { get; init; }
    public required DateTime Timestamp { get; init; }
}
