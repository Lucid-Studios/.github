namespace GEL.Models;

public enum EngramEpistemicClass
{
    Propositional,
    Procedural,
    Perspectival,
    Participatory
}

public enum PropositionalEngramLevel
{
    Basic,
    Intermediate,
    Advanced
}

public sealed class PropositionalEngram
{
    public required string Domain { get; init; }
    public required PropositionalEngramLevel Level { get; init; }
    public required IReadOnlyList<string> RootReferences { get; init; }
    public required string SymbolicStructure { get; init; }
}

public sealed class ProceduralEngram
{
    public required string Domain { get; init; }
    public required string InputType { get; init; }
    public required string OutputType { get; init; }
    public required IReadOnlyList<string> FunctorPipeline { get; init; }
}

public sealed class PerspectivalEngram
{
    public required string Domain { get; init; }
    public required IReadOnlyDictionary<string, double> OrientationVector { get; init; }
    public required IReadOnlyList<string> EthicalConstraints { get; init; }
    public required IReadOnlyDictionary<string, double> WeightFunctions { get; init; }
}

public sealed class ParticipatoryEngram
{
    public required string Role { get; init; }
    public required string OperationalMode { get; init; }
    public required IReadOnlyList<string> InteractionRules { get; init; }
    public required IReadOnlyList<string> CapabilitySet { get; init; }
}

