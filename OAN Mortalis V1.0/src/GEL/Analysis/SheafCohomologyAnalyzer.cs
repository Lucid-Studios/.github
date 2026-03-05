using System.Security.Cryptography;
using System.Text;
using GEL.Models;
using GEL.Telemetry;
using Telemetry.GEL;

namespace GEL.Analysis;

public sealed class SheafCohomologyAnalyzer
{
    public SheafCohomologyState Analyze(
        SheafMasterEngram sheaf,
        IReadOnlyList<SheafMasterEngram> domainUniverse)
    {
        ArgumentNullException.ThrowIfNull(sheaf);
        ArgumentNullException.ThrowIfNull(domainUniverse);

        var missingMorphisms = new List<string>();
        var inconsistentSymbols = new List<string>();
        var disconnectedFunctorChains = new List<string>();

        foreach (var otherDomain in domainUniverse.Where(domain =>
                     !string.Equals(domain.DomainName, sheaf.DomainName, StringComparison.OrdinalIgnoreCase)))
        {
            var hasBridge = sheaf.Morphisms.Any(morphism =>
                string.Equals(morphism.SourceDomain, sheaf.DomainName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(morphism.TargetDomain, otherDomain.DomainName, StringComparison.OrdinalIgnoreCase));

            if (!hasBridge)
            {
                missingMorphisms.Add($"{sheaf.DomainName}->{otherDomain.DomainName}");
            }

            var sharedSymbols = sheaf.RootSet
                .Intersect(otherDomain.RootSet, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var sharedSymbol in sharedSymbols)
            {
                if (!sheaf.Consistency.HasRule(sheaf.DomainName, otherDomain.DomainName, sharedSymbol))
                {
                    inconsistentSymbols.Add($"{sheaf.DomainName}<->{otherDomain.DomainName}:{sharedSymbol}");
                }
            }
        }

        var functorPipeline = sheaf.ProceduralFunctors.FunctorPipeline;
        if (functorPipeline.Count == 0)
        {
            disconnectedFunctorChains.Add($"{sheaf.DomainName}:empty-functor-pipeline");
        }
        else
        {
            for (var index = 0; index < functorPipeline.Count; index++)
            {
                if (string.IsNullOrWhiteSpace(functorPipeline[index]))
                {
                    disconnectedFunctorChains.Add($"{sheaf.DomainName}:blank-functor-step:{index}");
                }
            }
        }

        return new SheafCohomologyState
        {
            MissingMorphisms = missingMorphisms,
            InconsistentSymbols = inconsistentSymbols,
            DisconnectedFunctorChains = disconnectedFunctorChains
        };
    }

    public async Task<SheafCohomologyEvent> AnalyzeAndEmitAsync(
        SheafMasterEngram sheaf,
        IReadOnlyList<SheafMasterEngram> domainUniverse,
        GelTelemetryAdapter telemetry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(telemetry);
        var state = Analyze(sheaf, domainUniverse);
        var timestamp = DateTime.UtcNow;
        var eventHash = HashHex(
            $"{sheaf.DomainName}|{string.Join(",", state.MissingMorphisms)}|{string.Join(",", state.InconsistentSymbols)}|{string.Join(",", state.DisconnectedFunctorChains)}");

        var cohomologyEvent = new SheafCohomologyEvent
        {
            DomainName = sheaf.DomainName,
            State = state,
            EventHash = eventHash,
            Timestamp = timestamp
        };

        await telemetry.AppendAsync(cohomologyEvent, "sheaf-cohomology", cancellationToken).ConfigureAwait(false);
        return cohomologyEvent;
    }

    private static string HashHex(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }
}
