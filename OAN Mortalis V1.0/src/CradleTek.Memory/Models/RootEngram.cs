namespace CradleTek.Memory.Models;

public sealed class RootEngram
{
    public required string SymbolicId { get; init; }
    public required string AtlasDomain { get; init; }
    public required string RootTerm { get; init; }
    public required IReadOnlyList<string> VariantForms { get; init; }
    public required double FrequencyWeight { get; init; }
    public required string DictionaryPointer { get; init; }
}
