namespace SoulFrame.Identity.Models;

public enum RuntimeAttachmentState
{
    Detached = 0,
    Attached = 1
}

public sealed class SoulFrameState
{
    public required Guid SoulFrameId { get; init; }
    public required DateTime LastActiveTimestamp { get; set; }
    public required RuntimeAttachmentState RuntimeAttachmentState { get; set; }
    public required string IntegrityHash { get; set; }
}
