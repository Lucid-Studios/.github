namespace CradleTek.Host.Models;

public sealed record SoulFrameSnapshotRequest(
    Guid SoulFrameId,
    string CMEId,
    string OpalEngramId,
    DateTime Timestamp);
