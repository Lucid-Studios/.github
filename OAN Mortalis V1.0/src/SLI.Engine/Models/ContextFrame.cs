namespace SLI.Engine.Models;

public sealed class ContextFrame
{
    public required string CMEId { get; init; }
    public required Guid SoulFrameId { get; init; }
    public required Guid ContextId { get; init; }
    public required string TaskObjective { get; init; }
    public required IReadOnlyList<EngramReference> Engrams { get; init; }
}
