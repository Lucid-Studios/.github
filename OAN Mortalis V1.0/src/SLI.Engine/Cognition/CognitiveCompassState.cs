namespace SLI.Engine.Cognition;

public enum ValueElevation
{
    Positive = 0,
    Neutral = 1,
    Negative = 2
}

public sealed class CognitiveCompassState
{
    public required double IdForce { get; init; }
    public required double SuperegoConstraint { get; init; }
    public required double EgoStability { get; init; }
    public required ValueElevation ValueElevation { get; init; }
    public required int SymbolicDepth { get; init; }
    public required int BranchingFactor { get; init; }
    public required double DecisionEntropy { get; init; }
    public required DateTime Timestamp { get; init; }

    public required double ContextExpansionRate { get; init; }
    public required double PredicateAlignment { get; init; }
    public required double CleaveRatio { get; init; }
    public required int GovernanceFlags { get; init; }
    public required double CommitConfidence { get; init; }
}
