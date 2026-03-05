using CradleTek.CognitionHost.Interfaces;
using CradleTek.CognitionHost.Models;

namespace CradleTek.CognitionHost.Services;

public sealed class CognitionHostService : ICognitionEngine
{
    private RuntimePathConfiguration? _paths;
    private bool _initialized;

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return Task.CompletedTask;
        }

        _paths = RuntimePathConfiguration.FromEnvironment();
        _paths.ValidateOutsideRepository(Directory.GetCurrentDirectory());
        _paths.EnsureDirectories();
        _initialized = true;
        return Task.CompletedTask;
    }

    public Task<CognitionResult> ExecuteAsync(CognitionRequest request, CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Cognition host is not initialized.");
        }

        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Context);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Prompt);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Context.TaskObjective);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Context.CMEId);

        var engramCount = request.Context.RelevantEngrams.Count;
        var confidence = Math.Clamp(0.45 + (engramCount * 0.08), 0.0, 0.98);
        var decision = engramCount == 0 ? "collect-more-context" : "proceed-with-objective";
        var engramCandidate = confidence >= 0.5;

        var reasoning = $"Seed LLM evaluated objective '{request.Context.TaskObjective}' using {engramCount} relevant engrams.";
        var result = new CognitionResult
        {
            Reasoning = reasoning,
            Decision = decision,
            EngramCandidate = engramCandidate,
            CleaveResidue = "[]",
            TraceId = Guid.NewGuid().ToString("D"),
            SymbolicTrace = ["1. lowmind-evaluate(seed-llm)"],
            SliTokens = [],
            DecisionBranch = decision,
            CompassState = new CognitionCompassTelemetry
            {
                IdForce = 0.35,
                SuperegoConstraint = 0.25,
                EgoStability = 0.6,
                ValueElevation = CognitionValueElevation.Neutral,
                SymbolicDepth = 1,
                BranchingFactor = 1,
                DecisionEntropy = 0.5,
                Timestamp = DateTime.UtcNow
            },
            Confidence = confidence
        };

        return Task.FromResult(result);
    }

    public Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        _initialized = false;
        return Task.CompletedTask;
    }
}

internal sealed class RuntimePathConfiguration
{
    private RuntimePathConfiguration(
        string runtimeRoot,
        string modelPath,
        string selfGelPath,
        string cSelfGelPath,
        string goaPath,
        string cgoaPath)
    {
        RuntimeRoot = runtimeRoot;
        ModelPath = modelPath;
        SelfGelPath = selfGelPath;
        CSelfGelPath = cSelfGelPath;
        GoaPath = goaPath;
        CgoaPath = cgoaPath;
    }

    public string RuntimeRoot { get; }
    public string ModelPath { get; }
    public string SelfGelPath { get; }
    public string CSelfGelPath { get; }
    public string GoaPath { get; }
    public string CgoaPath { get; }

    public static RuntimePathConfiguration FromEnvironment()
    {
        var runtimeRoot = ReadRequiredPath("OAN_RUNTIME_ROOT");
        var modelPath = ReadRequiredPath("OAN_MODEL_PATH");
        var selfGelPath = ReadRequiredPath("OAN_SELF_GEL");
        var cSelfGelPath = ReadRequiredPath("OAN_CSELF_GEL");
        var goaPath = ReadRequiredPath("OAN_GOA");
        var cgoaPath = ReadRequiredPath("OAN_CGOA");

        return new RuntimePathConfiguration(runtimeRoot, modelPath, selfGelPath, cSelfGelPath, goaPath, cgoaPath);
    }

    public void ValidateOutsideRepository(string repositoryRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);
        var normalizedRepositoryRoot = NormalizeDir(repositoryRoot);

        EnsureOutsideRepo(RuntimeRoot, normalizedRepositoryRoot, "OAN_RUNTIME_ROOT");
        EnsureOutsideRepo(ModelPath, normalizedRepositoryRoot, "OAN_MODEL_PATH");
        EnsureOutsideRepo(SelfGelPath, normalizedRepositoryRoot, "OAN_SELF_GEL");
        EnsureOutsideRepo(CSelfGelPath, normalizedRepositoryRoot, "OAN_CSELF_GEL");
        EnsureOutsideRepo(GoaPath, normalizedRepositoryRoot, "OAN_GOA");
        EnsureOutsideRepo(CgoaPath, normalizedRepositoryRoot, "OAN_CGOA");
    }

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(RuntimeRoot);
        Directory.CreateDirectory(ModelPath);
        Directory.CreateDirectory(SelfGelPath);
        Directory.CreateDirectory(CSelfGelPath);
        Directory.CreateDirectory(GoaPath);
        Directory.CreateDirectory(CgoaPath);
    }

    private static string ReadRequiredPath(string environmentVariable)
    {
        var raw = Environment.GetEnvironmentVariable(environmentVariable);
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new InvalidOperationException($"Missing required environment variable '{environmentVariable}'.");
        }

        return Path.GetFullPath(raw);
    }

    private static void EnsureOutsideRepo(string candidatePath, string repositoryRoot, string variableName)
    {
        var normalizedCandidate = NormalizeDir(candidatePath);
        if (normalizedCandidate.StartsWith(repositoryRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Environment variable '{variableName}' points inside the repository.");
        }
    }

    private static string NormalizeDir(string path)
    {
        var fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return $"{fullPath}{Path.DirectorySeparatorChar}";
    }
}
