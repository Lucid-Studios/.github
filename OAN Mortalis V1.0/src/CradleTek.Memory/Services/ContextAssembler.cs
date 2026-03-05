using CradleTek.CognitionHost.Models;
using CradleTek.Memory.Models;

namespace CradleTek.Memory.Services;

public sealed class ContextAssembler
{
    private const int MaxEngramEntries = 8;
    private const int MaxSummaryLength = 220;
    private const int MaxDecisionSplineLength = 140;
    private const int MaxSymbolicProgramLines = 6;
    private const int MaxSymbolicExpressionLength = 180;

    public CognitionRequest BuildRequest(
        CognitionContext baseContext,
        IEnumerable<EngramSummary> summaries,
        string prompt)
    {
        ArgumentNullException.ThrowIfNull(baseContext);
        ArgumentNullException.ThrowIfNull(summaries);
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        var boundedEngrams = summaries
            .OrderByDescending(s => s.ConfidenceWeight)
            .ThenBy(s => s.EngramId, StringComparer.Ordinal)
            .Take(MaxEngramEntries)
            .Select(s => new CognitionEngramEntry
            {
                EngramId = s.EngramId,
                SummaryText = TrimToLength(s.SummaryText, MaxSummaryLength),
                DecisionSpline = TrimToLength(s.DecisionSpline, MaxDecisionSplineLength)
            })
            .ToList();

        var context = new CognitionContext
        {
            CMEId = baseContext.CMEId,
            SoulFrameId = baseContext.SoulFrameId,
            ContextId = baseContext.ContextId,
            TaskObjective = TrimToLength(baseContext.TaskObjective, 180),
            RelevantEngrams = boundedEngrams,
            SymbolicProgram = TrimProgram(baseContext.SymbolicProgram)
        };

        return new CognitionRequest
        {
            Context = context,
            Prompt = TrimToLength(prompt, 320)
        };
    }

    private static string TrimToLength(string input, int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        return input.Length <= maxLength ? input : input[..maxLength];
    }

    private static IReadOnlyList<string>? TrimProgram(IReadOnlyList<string>? program)
    {
        if (program is null || program.Count == 0)
        {
            return null;
        }

        return program
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Take(MaxSymbolicProgramLines)
            .Select(line => TrimToLength(line.Trim(), MaxSymbolicExpressionLength))
            .ToList();
    }
}
