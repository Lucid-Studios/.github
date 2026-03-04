namespace CradleTek.Host.Models;

public sealed record cSelfGEL(
    string BlockHash,
    DateTimeOffset CreatedAtUtc,
    string EncryptedPayloadPointer);
