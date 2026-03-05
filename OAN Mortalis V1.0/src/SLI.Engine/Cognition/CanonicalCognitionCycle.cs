namespace SLI.Engine.Cognition;

public static class CanonicalCognitionCycle
{
    public static readonly IReadOnlyList<string> Steps =
    [
        "signal-intake",
        "pre-sli-translation",
        "sli-packetization",
        "engram-context-expansion",
        "symbolic-reasoning",
        "compass-orientation-update",
        "decision-branch-resolution",
        "ontological-cleave",
        "engram-commit-proposal",
        "steward-governance-pipeline"
    ];

    public static void ValidateProgramOrder(IReadOnlyList<string> symbolicProgram)
    {
        ArgumentNullException.ThrowIfNull(symbolicProgram);

        var reasoningIndex = IndexOf(symbolicProgram, "(decision-evaluate");
        var compassIndex = IndexOf(symbolicProgram, "(compass-update");
        var decisionIndex = IndexOf(symbolicProgram, "(decision-branch");
        var cleaveIndex = IndexOf(symbolicProgram, "(cleave");
        var commitIndex = IndexOf(symbolicProgram, "(commit");

        if (reasoningIndex < 0 || compassIndex < 0 || decisionIndex < 0 || cleaveIndex < 0 || commitIndex < 0)
        {
            throw new InvalidOperationException("Canonical cognition cycle requires reasoning, compass, decision, cleave, and commit steps.");
        }

        if (!(reasoningIndex < compassIndex && compassIndex < decisionIndex && decisionIndex < cleaveIndex && cleaveIndex < commitIndex))
        {
            throw new InvalidOperationException("Canonical cognition cycle ordering violation.");
        }
    }

    private static int IndexOf(IReadOnlyList<string> symbolicProgram, string fragment)
    {
        for (var index = 0; index < symbolicProgram.Count; index++)
        {
            if (symbolicProgram[index].Contains(fragment, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }
}
