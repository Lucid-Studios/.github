namespace GEL.Models;

public sealed class DomainMorphism
{
    public required string SourceDomain { get; init; }
    public required string TargetDomain { get; init; }
    public required string TranslationFunctor { get; init; }
}
