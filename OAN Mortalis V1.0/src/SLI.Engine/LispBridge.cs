using System.Security.Cryptography;
using System.Text;
using CradleTek.Memory.Interfaces;
using SLI.Engine.Cognition;
using SLI.Engine.Models;
using SLI.Engine.Parser;
using SLI.Engine.Runtime;
using SLI.Lisp;
using SoulFrame.Host;

namespace SLI.Engine;

public sealed class LispBridge
{
    private static readonly string[] RequiredModules =
    [
        "core.lisp",
        "parser.lisp",
        "reasoning.lisp",
        "engram.lisp",
        "compass.lisp"
    ];

    private readonly IEngramResolver _resolver;
    private readonly ISoulFrameSemanticDevice _semanticDevice;
    private readonly SliParser _parser = new();
    private readonly SliInterpreter _interpreter = new(new SliSymbolTable());
    private bool _initialized;

    public LispBridge(IEngramResolver resolver, ISoulFrameSemanticDevice? semanticDevice = null)
    {
        _resolver = resolver;
        _semanticDevice = semanticDevice ?? NullSoulFrameSemanticDevice.Instance;
    }

    public IReadOnlyDictionary<string, string> LoadedModules { get; private set; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var modules = LispModuleCatalog.LoadModules();
        foreach (var required in RequiredModules)
        {
            if (!modules.ContainsKey(required))
            {
                throw new InvalidOperationException($"Required Lisp module '{required}' is not available.");
            }
        }

        LoadedModules = modules;
        _initialized = true;
        return Task.CompletedTask;
    }

    public async Task<LispExecutionResult> ExecuteProgramAsync(
        IReadOnlyList<string> symbolicProgram,
        ContextFrame frame,
        CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("LispBridge has not been initialized.");
        }

        ArgumentNullException.ThrowIfNull(symbolicProgram);
        ArgumentNullException.ThrowIfNull(frame);

        var program = _parser.ParseProgram(symbolicProgram);
        var context = new SliExecutionContext(frame, _resolver, _semanticDevice);
        await _interpreter.ExecuteProgramAsync(program, context, cancellationToken).ConfigureAwait(false);
        EnforceCanonicalOrdering(context.TraceLines);

        var cleaveResidue = context.PrunedBranches.Count == 0
            ? "[]"
            : $"[{string.Join(", ", context.PrunedBranches)}]";
        var traceHash = HashHex(string.Join("\n", context.TraceLines));
        var trace = context.TraceLines
            .Select((line, index) => $"{index + 1}. {line}")
            .ToList();

        var compass = BuildCompassState(context.TraceLines);
        var decisionBranch = context.CandidateBranches.FirstOrDefault() ?? context.FinalDecision;
        var traceId = CreateDeterministicTraceId(frame.CMEId, frame.ContextId, traceHash).ToString("D");
        return new LispExecutionResult
        {
            TraceId = traceId,
            Decision = context.FinalDecision,
            DecisionBranch = decisionBranch,
            CleaveResidue = cleaveResidue,
            SymbolicTrace = trace,
            SymbolicTraceHash = traceHash,
            CompassState = compass
        };
    }

    private static CognitiveCompassState BuildCompassState(IReadOnlyList<string> traceLines)
    {
        var symbolicDepth = traceLines.Count;
        var branchCount = traceLines.Count(line => line.StartsWith("decision-branch(", StringComparison.Ordinal));
        var cleaveCount = traceLines.Count(line => line.StartsWith("cleave(", StringComparison.Ordinal));
        var engramCount = traceLines.Count(line => line.Contains("engram", StringComparison.OrdinalIgnoreCase));
        var contextExpandCount = traceLines.Count(line => line.StartsWith("context-expand(", StringComparison.Ordinal));
        var predicateCount = traceLines.Count(line => line.StartsWith("predicate-evaluate(", StringComparison.Ordinal));
        var governanceFlags = traceLines.Count(line => line.Contains("rejected", StringComparison.OrdinalIgnoreCase));
        var commitCount = traceLines.Count(line => line.StartsWith("commit(", StringComparison.Ordinal));
        var cleaveRatio = branchCount == 0 ? 0.0 : (double)cleaveCount / branchCount;
        var contextExpansionRate = symbolicDepth == 0 ? 0.0 : (double)contextExpandCount / symbolicDepth;
        var predicateAlignment = symbolicDepth == 0 ? 0.0 : (double)predicateCount / symbolicDepth;
        var entropy = EntropyFromTrace(traceLines);
        var commitConfidence = commitCount > 0 ? Math.Clamp(1.0 - entropy, 0.0, 1.0) : 0.0;

        var idForce = Normalize(branchCount + symbolicDepth + contextExpandCount);
        var superegoConstraint = Normalize(cleaveRatio + predicateAlignment + (governanceFlags > 0 ? 0.5 : 0.0));
        var egoStability = Normalize((1.0 / Math.Max(0.01, entropy)) + commitConfidence);
        var valueElevation = EvaluateValueElevation(predicateAlignment, governanceFlags, contextExpansionRate);

        return new CognitiveCompassState
        {
            IdForce = idForce,
            SuperegoConstraint = superegoConstraint,
            EgoStability = egoStability,
            ValueElevation = valueElevation,
            SymbolicDepth = symbolicDepth,
            BranchingFactor = branchCount,
            DecisionEntropy = entropy,
            Timestamp = DateTime.UtcNow,
            ContextExpansionRate = Math.Round(contextExpansionRate, 4),
            PredicateAlignment = Math.Round(predicateAlignment, 4),
            CleaveRatio = Math.Round(cleaveRatio, 4),
            GovernanceFlags = governanceFlags,
            CommitConfidence = Math.Round(commitConfidence, 4)
        };
    }

    private static double EntropyFromTrace(IReadOnlyList<string> traceLines)
    {
        if (traceLines.Count == 0)
        {
            return 0.0;
        }

        var hash = HashHex(string.Join("|", traceLines));
        var sample = hash[..8];
        var number = Convert.ToUInt32(sample, 16);
        return Math.Round(number / (double)uint.MaxValue, 6);
    }

    private static string HashHex(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static Guid CreateDeterministicTraceId(string cmeId, Guid contextId, string traceHash)
    {
        var source = $"{cmeId}|{contextId:D}|{traceHash}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        var guidBytes = new byte[16];
        Buffer.BlockCopy(bytes, 0, guidBytes, 0, 16);
        return new Guid(guidBytes);
    }

    private static double Normalize(double value)
    {
        var normalized = value / (1.0 + Math.Abs(value));
        return Math.Round(Math.Clamp(normalized, 0.0, 1.0), 6);
    }

    private static ValueElevation EvaluateValueElevation(double predicateAlignment, int governanceFlags, double contextExpansionRate)
    {
        if (governanceFlags > 0)
        {
            return ValueElevation.Negative;
        }

        if (predicateAlignment >= 0.2 && contextExpansionRate > 0.0)
        {
            return ValueElevation.Positive;
        }

        return ValueElevation.Neutral;
    }

    private static void EnforceCanonicalOrdering(IReadOnlyList<string> traceLines)
    {
        var reasoningIndex = IndexOf(traceLines, "decision-evaluate(");
        var compassIndex = IndexOf(traceLines, "compass-update(");
        var decisionIndex = IndexOf(traceLines, "decision-branch(");
        var cleaveIndex = IndexOf(traceLines, "cleave(");
        var commitIndex = IndexOf(traceLines, "commit(");

        if (reasoningIndex < 0 || compassIndex < 0 || decisionIndex < 0 || cleaveIndex < 0 || commitIndex < 0)
        {
            throw new InvalidOperationException("SLI canonical cognition cycle is incomplete.");
        }

        var valid =
            reasoningIndex < compassIndex &&
            compassIndex < decisionIndex &&
            decisionIndex < cleaveIndex &&
            cleaveIndex < commitIndex;

        if (!valid)
        {
            throw new InvalidOperationException("SLI canonical cognition cycle ordering violation detected.");
        }
    }

    private static int IndexOf(IReadOnlyList<string> traceLines, string prefix)
    {
        for (var index = 0; index < traceLines.Count; index++)
        {
            if (traceLines[index].StartsWith(prefix, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }
}
