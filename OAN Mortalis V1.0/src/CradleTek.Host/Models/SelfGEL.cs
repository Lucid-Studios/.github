namespace CradleTek.Host.Models;

public sealed record SelfGEL(
    string BlockHash,
    DateTimeOffset CreatedAtUtc,
    string PayloadPointer);
