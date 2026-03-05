namespace GEL.Models;

public sealed class ConsistencyRule
{
    public required string DomainA { get; init; }
    public required string DomainB { get; init; }
    public required string SharedSymbol { get; init; }
}

public sealed class ConsistencyRules
{
    public required IReadOnlyList<ConsistencyRule> Rules { get; init; }

    public bool HasRule(string domainA, string domainB, string sharedSymbol)
    {
        return Rules.Any(rule =>
            string.Equals(rule.SharedSymbol, sharedSymbol, StringComparison.OrdinalIgnoreCase) &&
            ((string.Equals(rule.DomainA, domainA, StringComparison.OrdinalIgnoreCase) &&
              string.Equals(rule.DomainB, domainB, StringComparison.OrdinalIgnoreCase)) ||
             (string.Equals(rule.DomainA, domainB, StringComparison.OrdinalIgnoreCase) &&
              string.Equals(rule.DomainB, domainA, StringComparison.OrdinalIgnoreCase))));
    }
}
