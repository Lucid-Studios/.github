namespace SoulFrame.Identity.Models;

public enum RuntimePolicy
{
    Default = 0
}

public sealed class SoulFrame
{
    public required Guid SoulFrameId { get; init; }
    public required string CMEId { get; init; }
    public required string OpalEngramId { get; init; }
    public required DateTime CreationTimestamp { get; init; }
    public required RuntimePolicy RuntimePolicy { get; init; }
    public required string OperatorBondReference { get; init; }
    public required string SelfGelReference { get; init; }
    public required string cSelfGelReference { get; init; }
}
