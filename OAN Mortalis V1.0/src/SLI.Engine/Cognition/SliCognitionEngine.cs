using System.Security.Cryptography;
using System.Text;
using CradleTek.CognitionHost.Interfaces;
using CradleTek.CognitionHost.Models;
using CradleTek.Memory.Interfaces;
using GEL.Runtime;
using SLI.Engine.Models;
using SLI.Engine.Parser;
using SLI.Engine.Telemetry;
using SoulFrame.Host;

namespace SLI.Engine.Cognition;

public sealed class SliCognitionEngine : ICognitionEngine
{
    private readonly LispBridge _lispBridge;
    private readonly SliParser _parser = new();
    private readonly SheafMasterEngramService _sheafMasterEngrams;
    private readonly ICognitionEngine? _optionalLowMindEngine;
    private readonly IReadOnlyList<ICognitionObserver> _observers;
    private bool _initialized;

    public SliCognitionEngine(
        IEngramResolver resolver,
        ICognitionEngine? optionalLowMindEngine = null,
        SheafMasterEngramService? sheafMasterEngrams = null,
        ISoulFrameSemanticDevice? semanticDevice = null,
        IEnumerable<ICognitionObserver>? observers = null)
    {
        _lispBridge = new LispBridge(resolver, semanticDevice);
        _optionalLowMindEngine = optionalLowMindEngine;
        _sheafMasterEngrams = sheafMasterEngrams ?? new SheafMasterEngramService();
        _observers = observers?.ToList() ?? [NullCognitionObserver.Instance];
    }

    public DecisionSpline? LastDecisionSpline { get; private set; }
    public SliTraceEvent? LastTraceEvent { get; private set; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _lispBridge.InitializeAsync(cancellationToken).ConfigureAwait(false);
        if (_optionalLowMindEngine is not null)
        {
            await _optionalLowMindEngine.InitializeAsync(cancellationToken).ConfigureAwait(false);
        }

        _initialized = true;
    }

    public async Task<CognitionResult> ExecuteAsync(CognitionRequest request, CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("SLI cognition engine is not initialized.");
        }

        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Context);

        await NotifyStartAsync(request.Context, cancellationToken).ConfigureAwait(false);

        var contextFrame = new ContextFrame
        {
            CMEId = request.Context.CMEId,
            SoulFrameId = request.Context.SoulFrameId,
            ContextId = request.Context.ContextId,
            TaskObjective = request.Context.TaskObjective,
            Engrams = request.Context.RelevantEngrams
                .Select(entry => new EngramReference
                {
                    EngramId = entry.EngramId,
                    ConceptTag = ExtractConceptTag(entry),
                    SummaryText = entry.SummaryText,
                    DecisionSpline = entry.DecisionSpline,
                    ConfidenceWeight = 0.5
                })
                .ToList()
        };

        // Sheaf plan is the legacy container for procedural engram execution context.
        var sheafPlan = _sheafMasterEngrams.BuildExecutionPlan(request.Context.TaskObjective);

        var program = BuildProgram(request.Context.TaskObjective, request.Context.SymbolicProgram);
        var sliTokens = CollectProgramTokens(program);
        CanonicalCognitionCycle.ValidateProgramOrder(program);

        var lispResult = await _lispBridge
            .ExecuteProgramAsync(program, contextFrame, cancellationToken)
            .ConfigureAwait(false);

        CognitionResult? lowMindResult = null;
        if (_optionalLowMindEngine is not null)
        {
            lowMindResult = await _optionalLowMindEngine.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
        }

        var confidence = ComputeConfidence(lispResult.CompassState);
        if (lowMindResult is not null)
        {
            confidence = Math.Clamp((confidence + lowMindResult.Confidence) / 2.0, 0.1, 0.99);
        }

        var sheafTrace = sheafPlan.FunctorPath
            .Select((functor, index) => $"sheaf-functor({index + 1}:{functor})")
            .ToList();
        var symbolicTrace = lispResult.SymbolicTrace.Concat(sheafTrace).ToList();

        var reasoning = BuildReasoning(lispResult, lowMindResult, sheafPlan);
        var decisionHash = HashHex($"{request.Context.CMEId}|{request.Context.ContextId:D}|{lispResult.Decision}|{lispResult.CleaveResidue}");
        var decisionSpline = new DecisionSpline
        {
            CMEId = request.Context.CMEId,
            SoulFrameId = request.Context.SoulFrameId,
            ContextId = request.Context.ContextId,
            DecisionHash = decisionHash,
            Timestamp = DateTime.UtcNow,
            SymbolicTraceHash = lispResult.SymbolicTraceHash
        };
        LastDecisionSpline = decisionSpline;

        await NotifyCompassUpdateAsync(lispResult.CompassState, cancellationToken).ConfigureAwait(false);
        await NotifyDecisionCommitAsync(decisionSpline, cancellationToken).ConfigureAwait(false);

        LastTraceEvent = new SliTraceEvent
        {
            EventHash = HashHex($"{decisionHash}|trace"),
            TraceId = lispResult.TraceId,
            SymbolicTrace = symbolicTrace,
            DecisionBranch = lispResult.DecisionBranch,
            CleaveResidue = lispResult.CleaveResidue,
            Timestamp = DateTime.UtcNow,
            SymbolicTraceHash = lispResult.SymbolicTraceHash,
            CompassState = lispResult.CompassState
        };

        return new CognitionResult
        {
            Reasoning = reasoning,
            Decision = lispResult.Decision,
            EngramCandidate = !string.Equals(lispResult.Decision, "defer", StringComparison.OrdinalIgnoreCase),
            CleaveResidue = lispResult.CleaveResidue,
            TraceId = lispResult.TraceId,
            SymbolicTrace = symbolicTrace,
            SliTokens = sliTokens,
            DecisionBranch = lispResult.DecisionBranch,
            CompassState = new CognitionCompassTelemetry
            {
                IdForce = lispResult.CompassState.IdForce,
                SuperegoConstraint = lispResult.CompassState.SuperegoConstraint,
                EgoStability = lispResult.CompassState.EgoStability,
                ValueElevation = MapValueElevation(lispResult.CompassState.ValueElevation),
                SymbolicDepth = lispResult.CompassState.SymbolicDepth,
                BranchingFactor = lispResult.CompassState.BranchingFactor,
                DecisionEntropy = lispResult.CompassState.DecisionEntropy,
                Timestamp = lispResult.CompassState.Timestamp
            },
            Confidence = confidence
        };
    }

    public async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        _initialized = false;
        if (_optionalLowMindEngine is not null)
        {
            await _optionalLowMindEngine.ShutdownAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private IReadOnlyList<string> BuildProgram(string objective, IReadOnlyList<string>? symbolicProgram)
    {
        var escapedObjective = objective.Replace("\"", "\\\"", StringComparison.Ordinal);
        var program = new List<string>();
        if (symbolicProgram is not null)
        {
            foreach (var expression in symbolicProgram)
            {
                if (string.IsNullOrWhiteSpace(expression))
                {
                    continue;
                }

                try
                {
                    _parser.ParseSingle(expression);
                    program.Add(expression.Trim());
                }
                catch (FormatException)
                {
                    // Ignore malformed ingestion lines and continue with canonical runtime steps.
                }
            }
        }

        program.AddRange(
        [
            $"(engram-query \"{escapedObjective}\")",
            "(llm_classify \"task-objective\")",
            "(engram-ref \"identity-continuity\")",
            "(engram-ref \"context-preservation\")",
            "(llm_context_infer \"context-frame\")",
            "(predicate-evaluate identity-continuity)",
            "(context-expand engram-set)",
            "(decision-evaluate predicate-set)",
            "(compass-update context reasoning-state)",
            "(decision-branch cognition-state)",
            "(cleave branch-set)",
            "(commit decision)"
        ]);

        return program;
    }

    private static string ExtractConceptTag(CognitionEngramEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (!string.IsNullOrWhiteSpace(entry.DecisionSpline))
        {
            var prefix = "concept:";
            var segments = entry.DecisionSpline.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            foreach (var segment in segments)
            {
                if (segment.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return segment[prefix.Length..];
                }
            }
        }

        return entry.EngramId;
    }

    private static double ComputeConfidence(CognitiveCompassState compass)
    {
        var depthScore = Math.Min(0.30, compass.SymbolicDepth * 0.03);
        var idScore = Math.Min(0.2, compass.IdForce * 0.2);
        var superegoScore = Math.Min(0.15, compass.SuperegoConstraint * 0.15);
        var egoScore = Math.Min(0.2, compass.EgoStability * 0.2);
        var entropyScore = Math.Min(0.15, (1.0 - compass.DecisionEntropy) * 0.15);
        var cleavePenalty = Math.Min(0.12, compass.CleaveRatio * 0.08);
        var confidence = 0.3 + depthScore + idScore + superegoScore + egoScore + entropyScore - cleavePenalty;
        return Math.Clamp(Math.Round(confidence, 4), 0.1, 0.95);
    }

    private static string BuildReasoning(
        LispExecutionResult lispResult,
        CognitionResult? lowMindResult,
        SheafExecutionPlan sheafPlan)
    {
        var compass = lispResult.CompassState;
        var baseReasoning =
            $"SLI Lisp runtime executed {compass.SymbolicDepth} symbolic steps with Id={compass.IdForce:F3}, " +
            $"Superego={compass.SuperegoConstraint:F3}, Ego={compass.EgoStability:F3}, Value={compass.ValueElevation}. " +
            $"Sheaf={sheafPlan.Domain}; Functors={string.Join(">", sheafPlan.FunctorPath)}; " +
            $"Cohomology(missing={sheafPlan.Cohomology.MissingMorphisms.Count}, inconsistent={sheafPlan.Cohomology.InconsistentSymbols.Count}, disconnected={sheafPlan.Cohomology.DisconnectedFunctorChains.Count}).";

        if (lowMindResult is null)
        {
            return baseReasoning;
        }

        return $"{baseReasoning} LowMind augmentation: {lowMindResult.Reasoning}";
    }

    private static string HashHex(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private IReadOnlyList<string> CollectProgramTokens(IReadOnlyList<string> program)
    {
        ArgumentNullException.ThrowIfNull(program);

        var tokens = new List<string>();
        foreach (var line in program)
        {
            foreach (var token in _parser.TokenizeAtoms(line))
            {
                if (token == "(" || token == ")")
                {
                    continue;
                }

                tokens.Add(token);
            }
        }

        return tokens.Distinct(StringComparer.Ordinal).ToList();
    }

    private async Task NotifyStartAsync(CognitionContext context, CancellationToken cancellationToken)
    {
        foreach (var observer in _observers)
        {
            await observer.OnCognitionStartAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task NotifyCompassUpdateAsync(CognitiveCompassState compassState, CancellationToken cancellationToken)
    {
        foreach (var observer in _observers)
        {
            await observer.OnCompassUpdateAsync(compassState, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task NotifyDecisionCommitAsync(DecisionSpline decisionSpline, CancellationToken cancellationToken)
    {
        foreach (var observer in _observers)
        {
            await observer.OnDecisionCommitAsync(decisionSpline, cancellationToken).ConfigureAwait(false);
        }
    }

    private static CognitionValueElevation MapValueElevation(ValueElevation valueElevation)
    {
        return valueElevation switch
        {
            ValueElevation.Positive => CognitionValueElevation.Positive,
            ValueElevation.Negative => CognitionValueElevation.Negative,
            _ => CognitionValueElevation.Neutral
        };
    }
}
