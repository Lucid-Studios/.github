namespace EngramGovernance.Models;

public sealed class RootEngramRecord
{
    public required string SymbolicId { get; init; }
    public required string LexicalKey { get; init; }
    public required string OntologicalClass { get; init; }
    public required string GelDictionaryIndexPointer { get; init; }
    public required string DiscoveryContext { get; init; }
    public required DateTime Timestamp { get; init; }
}
