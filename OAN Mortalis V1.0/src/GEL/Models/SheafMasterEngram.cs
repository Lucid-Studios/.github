using GEL.Graphs;
using GEL.Telemetry;

namespace GEL.Models;

/// <summary>
/// Legacy compatibility shape for the "Sheaf Master Engram" container.
/// Semantically this holds a procedural engram surface (functor pipeline)
/// paired with propositional engram topology (root set + constructor graph).
/// </summary>
public sealed class SheafMasterEngram
{
    public required string SymbolicID { get; init; }
    public required string DomainName { get; init; }
    public required IReadOnlyList<string> RootSet { get; init; }
    public required ConstructorGraph ConstructorGraph { get; init; }
    public required ProceduralFunctorGraph ProceduralFunctors { get; init; }
    public required LocalSymbolAtlas LocalAtlas { get; init; }
    public required IReadOnlyList<DomainMorphism> Morphisms { get; init; }
    public required ConsistencyRules Consistency { get; init; }
    public required SheafCohomologyState CohomologyState { get; init; }
}
