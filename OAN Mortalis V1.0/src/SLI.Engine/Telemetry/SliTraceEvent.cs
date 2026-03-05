using OAN.Core.Telemetry;
using SLI.Engine.Cognition;

namespace SLI.Engine.Telemetry;

/// <summary>
/// Deterministic symbolic trace event.
/// Includes compass data that can be interpreted as perspectival engram telemetry.
/// </summary>
public sealed class SliTraceEvent : ITelemetryEvent
{
    public required string EventHash { get; init; }
    public required string TraceId { get; init; }
    public required IReadOnlyList<string> SymbolicTrace { get; init; }
    public required string DecisionBranch { get; init; }
    public required string CleaveResidue { get; init; }
    public required DateTime Timestamp { get; init; }
    public required string SymbolicTraceHash { get; init; }
    public required CognitiveCompassState CompassState { get; init; }
}
