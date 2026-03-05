namespace GEL.Models;

public sealed class LocalSymbolAtlas
{
    public required string DomainName { get; init; }
    public required IReadOnlyDictionary<string, string> SymbolMap { get; init; }
}
