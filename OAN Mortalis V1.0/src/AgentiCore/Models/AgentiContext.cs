namespace AgentiCore.Models;

public sealed class AgentiContext
{
    public required string CMEId { get; init; }
    public required Guid SoulFrameId { get; init; }
    public required Guid ContextId { get; init; }
    public required List<string> ActiveConcepts { get; init; }
    public required Dictionary<string, string> WorkingMemory { get; init; }
    public required DateTime ExecutionTimestamp { get; set; }
}
