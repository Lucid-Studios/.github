namespace GEL.Telemetry;

public sealed class SheafCohomologyState
{
    public required IReadOnlyList<string> MissingMorphisms { get; init; }
    public required IReadOnlyList<string> InconsistentSymbols { get; init; }
    public required IReadOnlyList<string> DisconnectedFunctorChains { get; init; }
}
