namespace SLI.Engine.Models;

public sealed class DecisionSpline
{
    public required string CMEId { get; init; }
    public required Guid SoulFrameId { get; init; }
    public required Guid ContextId { get; init; }
    public required string DecisionHash { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string SymbolicTraceHash { get; init; }
}
