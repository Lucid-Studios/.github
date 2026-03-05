namespace EngramGovernance.Models;

public sealed class EngramRecord
{
    public required OEDecisionEntry DecisionEntry { get; init; }
    public required string StoragePointer { get; init; }
    public required long LedgerIndex { get; init; }
    public required string DecisionSpline { get; init; }
    public required string SymbolicTrace { get; init; }
    public required EngramCompassState CompassState { get; init; }
}
