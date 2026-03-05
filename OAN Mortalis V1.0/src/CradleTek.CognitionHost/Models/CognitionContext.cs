namespace CradleTek.CognitionHost.Models;

public sealed class CognitionContext
{
    public required string CMEId { get; init; }
    public required Guid SoulFrameId { get; init; }
    public required Guid ContextId { get; init; }
    public required string TaskObjective { get; init; }
    public required IReadOnlyList<CognitionEngramEntry> RelevantEngrams { get; init; }
    public IReadOnlyList<string>? SymbolicProgram { get; init; }
}
