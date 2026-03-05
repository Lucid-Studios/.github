using EngramGovernance.Models;

namespace EngramGovernance.Services;

public sealed class OntologicalCleaver
{
    private static readonly string[] ExploratoryMarkers =
    [
        ":mode explore",
        ":op probe",
        ":op discovery",
        "exploratory",
        "hypothesis",
        "draft"
    ];

    private static readonly string[] ProtectedMarkers =
    [
        ":route :data",
        ":classification cryptic",
        ":visibility protected",
        "cryptic",
        "confidential",
        "restricted",
        "private"
    ];

    public CleavingDecision Classify(EngramCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        if (string.IsNullOrWhiteSpace(candidate.CognitionBody))
        {
            return new CleavingDecision(EngramClassification.Discard, false, ResidueTargets.Discard);
        }

        var payload = candidate.CognitionBody.ToLowerInvariant();
        var exploratory = ContainsAny(payload, ExploratoryMarkers);
        var protectedData = ContainsAny(payload, ProtectedMarkers);

        if (exploratory && protectedData)
        {
            return new CleavingDecision(EngramClassification.Exploratory, true, ResidueTargets.cGoA);
        }

        if (exploratory)
        {
            return new CleavingDecision(EngramClassification.Exploratory, false, ResidueTargets.GoA);
        }

        if (protectedData)
        {
            return new CleavingDecision(EngramClassification.Cryptic, true, ResidueTargets.cGoA);
        }

        return new CleavingDecision(EngramClassification.Prime, false, ResidueTargets.GoA);
    }

    private static bool ContainsAny(string payload, IEnumerable<string> markers)
    {
        foreach (var marker in markers)
        {
            if (payload.Contains(marker, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}

public sealed record CleavingDecision(
    EngramClassification Classification,
    bool RequiresEncryption,
    string ResidueTarget);

public static class ResidueTargets
{
    public const string GoA = "GoA";
    public const string cGoA = "cGoA";
    public const string Discard = "Discard";
}
