namespace AgentiCore.Models;

public sealed class AgentiResult
{
    public required Guid ContextId { get; init; }
    public required string ResultType { get; init; }
    public required string ResultPayload { get; init; }
    public required bool EngramCommitRequired { get; init; }
}
