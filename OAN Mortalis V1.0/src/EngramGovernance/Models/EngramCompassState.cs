namespace EngramGovernance.Models;

public sealed class EngramCompassState
{
    public required double IdForce { get; init; }
    public required double SuperegoConstraint { get; init; }
    public required double EgoStability { get; init; }
    public required EngramValueElevation ValueElevation { get; init; }
    public required int SymbolicDepth { get; init; }
    public required int BranchingFactor { get; init; }
    public required double DecisionEntropy { get; init; }
    public required DateTime Timestamp { get; init; }
}
