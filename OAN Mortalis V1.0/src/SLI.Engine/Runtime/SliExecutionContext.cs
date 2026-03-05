using CradleTek.Memory.Interfaces;
using CradleTek.Memory.Models;
using SLI.Engine.Models;
using SoulFrame.Host;

namespace SLI.Engine.Runtime;

public sealed class SliExecutionContext
{
    private readonly List<EngramReference> _activeEngrams;

    public SliExecutionContext(
        ContextFrame frame,
        IEngramResolver resolver,
        ISoulFrameSemanticDevice? semanticDevice = null)
    {
        Frame = frame;
        Resolver = resolver;
        SemanticDevice = semanticDevice ?? NullSoulFrameSemanticDevice.Instance;
        OpalConstraints = BuildConstraints(frame.TaskObjective);
        _activeEngrams = frame.Engrams.ToList();
    }

    public ContextFrame Frame { get; }
    public IEngramResolver Resolver { get; }
    public ISoulFrameSemanticDevice SemanticDevice { get; }
    public SoulFrameInferenceConstraints OpalConstraints { get; }
    public IReadOnlyList<EngramReference> ActiveEngrams => _activeEngrams;
    public List<string> TraceLines { get; } = [];
    public List<string> CandidateBranches { get; } = [];
    public List<string> PrunedBranches { get; } = [];
    public SliExecutionGraph ExecutionGraph { get; } = new();
    public string FinalDecision { get; set; } = "defer";

    public void AddTrace(string trace)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trace);
        TraceLines.Add(trace);
    }

    public void AddResolvedEngrams(IEnumerable<EngramSummary> summaries)
    {
        foreach (var summary in summaries)
        {
            _activeEngrams.Add(new EngramReference
            {
                EngramId = summary.EngramId,
                ConceptTag = summary.ConceptTag,
                SummaryText = summary.SummaryText,
                DecisionSpline = summary.DecisionSpline,
                ConfidenceWeight = summary.ConfidenceWeight
            });
        }
    }

    private static SoulFrameInferenceConstraints BuildConstraints(string objective)
    {
        var domain = objective.Contains("equation", StringComparison.OrdinalIgnoreCase) ||
                     objective.Contains("arithmetic", StringComparison.OrdinalIgnoreCase)
            ? "arithmetic"
            : "general";

        return new SoulFrameInferenceConstraints
        {
            Domain = domain,
            DriftLimit = 0.02,
            MaxTokens = 128
        };
    }
}
