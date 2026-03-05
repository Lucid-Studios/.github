namespace CradleTek.CognitionHost.Models;

public sealed class CognitionRequest
{
    public required CognitionContext Context { get; init; }
    public required string Prompt { get; init; }
}
