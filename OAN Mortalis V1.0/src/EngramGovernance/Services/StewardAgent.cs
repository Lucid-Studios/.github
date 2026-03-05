using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using CradleTek.Host.Interfaces;
using EngramGovernance.Models;
using OAN.Core.Telemetry;
using Telemetry.GEL;

namespace EngramGovernance.Services;

public sealed class StewardAgent
{
    private readonly OntologicalCleaver _cleaver;
    private readonly EncryptionService _encryptionService;
    private readonly LedgerWriter _ledgerWriter;
    private readonly EngramBootstrapService _engramBootstrap;
    private readonly SymbolicConstructorGuidanceService _constructorGuidance;
    private readonly IPublicStore _publicStore;
    private readonly ICrypticStore _crypticStore;
    private readonly GelTelemetryAdapter _telemetry;

    public StewardAgent(
        OntologicalCleaver cleaver,
        EncryptionService encryptionService,
        LedgerWriter ledgerWriter,
        EngramBootstrapService? engramBootstrap,
        SymbolicConstructorGuidanceService? constructorGuidance,
        IPublicStore publicStore,
        ICrypticStore crypticStore,
        GelTelemetryAdapter telemetry)
    {
        _cleaver = cleaver;
        _encryptionService = encryptionService;
        _ledgerWriter = ledgerWriter;
        _engramBootstrap = engramBootstrap ?? new EngramBootstrapService(ledgerWriter, publicStore, telemetry);
        _constructorGuidance = constructorGuidance ?? new SymbolicConstructorGuidanceService();
        _publicStore = publicStore;
        _crypticStore = crypticStore;
        _telemetry = telemetry;
    }

    public async Task<EngramRecord?> ProcessCandidateAsync(
        EngramCandidate candidate,
        CancellationToken cancellationToken = default)
    {
        Validate(candidate);
        await EmitTelemetryAsync("engram-candidate-received", candidate, cancellationToken).ConfigureAwait(false);
        var sliTokens = ExtractSliTokens(candidate.Metadata);
        var bootstrapResult = await _engramBootstrap.BootstrapAsync(candidate, sliTokens, cancellationToken).ConfigureAwait(false);
        await EmitTelemetryAsync("engram-bootstrap-complete", candidate, cancellationToken).ConfigureAwait(false);

        var decision = _cleaver.Classify(candidate);
        await EmitTelemetryAsync($"classification-{decision.Classification.ToString().ToLowerInvariant()}", candidate, cancellationToken)
            .ConfigureAwait(false);

        if (decision.Classification == EngramClassification.Discard)
        {
            await EmitTelemetryAsync("governance-rejection", candidate, cancellationToken).ConfigureAwait(false);
            return null;
        }

        var split = SplitCandidate(candidate);
        var bodyHash = EncryptionService.ComputeBodyHash(split.CognitionBody);
        var selfGelPointer = await StoreCognitionBodyAsync(candidate, split.CognitionBody, decision, cancellationToken)
            .ConfigureAwait(false);

        var decisionEntry = new OEDecisionEntry
        {
            DecisionId = CreateDeterministicDecisionId(candidate, decision.Classification, bodyHash),
            CMEId = candidate.CMEId,
            SoulFrameId = candidate.SoulFrameId,
            ContextId = candidate.ContextId,
            Classification = decision.Classification,
            BodyHash = bodyHash,
            Timestamp = DateTime.UtcNow
        };

        var symbolicTrace = ExtractMetadata(candidate.Metadata, "symbolic_trace", "[]");
        var guidanceEvaluation = _constructorGuidance.Evaluate(symbolicTrace);
        symbolicTrace = guidanceEvaluation.NormalizedTrace;

        var decisionSpline =
            $"decision:{decisionEntry.DecisionId:D}|class:{decisionEntry.Classification}|context:{decisionEntry.ContextId:D}" +
            $"|constructor:{guidanceEvaluation.ConstructorTag}|roots:{bootstrapResult.RootEngramsCreated.Count}|constructors:{bootstrapResult.ConstructorEngramsCreated.Count}";

        if (guidanceEvaluation.UsedFallback || guidanceEvaluation.WasTruncated || guidanceEvaluation.ReservedCollisionCount > 0)
        {
            await EmitTelemetryAsync("constructor-guidance-applied", candidate, cancellationToken).ConfigureAwait(false);
        }

        var compassState = ExtractCompassState(candidate.Metadata);

        var record = await _ledgerWriter
            .AppendDecisionAsync(decisionEntry, selfGelPointer, decisionSpline, symbolicTrace, compassState, cancellationToken)
            .ConfigureAwait(false);

        await RouteResidueAsync(split.CleavedResidue, record.DecisionEntry, decision.ResidueTarget, cancellationToken).ConfigureAwait(false);
        await EmitTelemetryAsync("governance-commit-success", candidate, cancellationToken).ConfigureAwait(false);
        return record;
    }

    private async Task<string> StoreCognitionBodyAsync(
        EngramCandidate candidate,
        string cognitionBody,
        CleavingDecision decision,
        CancellationToken cancellationToken)
    {
        var payload = _encryptionService.PrepareSelfGelPayload(
            candidate.CMEId,
            candidate.SoulFrameId,
            candidate.ContextId,
            cognitionBody,
            decision.RequiresEncryption);

        if (payload.EncryptForCrypticLayer)
        {
            var crypticPointer = await _crypticStore
                .StorePointerAsync($"cselfgel:body:{payload.StoragePointer}:{payload.BodyHash}", cancellationToken)
                .ConfigureAwait(false);
            await EmitTelemetryAsync("encryption-performed", candidate, cancellationToken).ConfigureAwait(false);
            return crypticPointer;
        }

        await _publicStore.PublishPointerAsync($"selfgel:body:{payload.StoragePointer}:{payload.BodyHash}", cancellationToken)
            .ConfigureAwait(false);
        return payload.StoragePointer;
    }

    private async Task RouteResidueAsync(
        string residue,
        OEDecisionEntry decisionEntry,
        string residueTarget,
        CancellationToken cancellationToken)
    {
        var residueHash = HashHex(residue);
        var residuePointer = $"residue:{decisionEntry.DecisionId:D}:{residueHash[..16]}";

        if (residueTarget == ResidueTargets.GoA)
        {
            await _publicStore.PublishPointerAsync($"goa:{residuePointer}", cancellationToken).ConfigureAwait(false);
            return;
        }

        if (residueTarget == ResidueTargets.cGoA)
        {
            var crypticPointer = await _crypticStore.StorePointerAsync($"cgoa:{residuePointer}", cancellationToken).ConfigureAwait(false);
            await _publicStore.PublishPointerAsync($"goa:pointer:{decisionEntry.DecisionId:D}:{crypticPointer}", cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        if (residueTarget == ResidueTargets.Discard)
        {
            return;
        }

        throw new InvalidOperationException($"Unsupported residue target '{residueTarget}'.");
    }

    private async Task EmitTelemetryAsync(string stage, EngramCandidate candidate, CancellationToken cancellationToken)
    {
        ITelemetryEvent telemetryEvent = new GovernanceTelemetryEvent
        {
            EventHash = HashHex($"{stage}|{candidate.CandidateId:D}|{candidate.CMEId}|{candidate.ContextId:D}"),
            Timestamp = DateTime.UtcNow
        };

        await _telemetry.AppendAsync(telemetryEvent, stage, cancellationToken).ConfigureAwait(false);
    }

    private static CandidateSplit SplitCandidate(EngramCandidate candidate)
    {
        var body = candidate.CognitionBody.Trim();
        var metadataProjection = candidate.Metadata
            .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
            .Select(kvp => $"{kvp.Key}={kvp.Value}")
            .ToArray();

        var residue = metadataProjection.Length == 0
            ? $"context={candidate.ContextId:D};source=governance"
            : string.Join(";", metadataProjection);

        return new CandidateSplit(body, residue);
    }

    private static Guid CreateDeterministicDecisionId(
        EngramCandidate candidate,
        EngramClassification classification,
        string bodyHash)
    {
        var source = $"{candidate.CMEId}|{candidate.SoulFrameId:D}|{candidate.ContextId:D}|{classification}|{bodyHash}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        var guidBytes = new byte[16];
        Buffer.BlockCopy(bytes, 0, guidBytes, 0, 16);
        return new Guid(guidBytes);
    }

    private static void Validate(EngramCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentException.ThrowIfNullOrWhiteSpace(candidate.CMEId);
        ArgumentException.ThrowIfNullOrWhiteSpace(candidate.CognitionBody);
        ArgumentNullException.ThrowIfNull(candidate.Metadata);
    }

    private static string HashHex(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static EngramCompassState ExtractCompassState(IReadOnlyDictionary<string, string> metadata)
    {
        var valueElevationRaw = ExtractMetadata(metadata, "compass_value_elevation", "Neutral");
        var valueElevation = Enum.TryParse<EngramValueElevation>(valueElevationRaw, ignoreCase: true, out var parsedElevation)
            ? parsedElevation
            : EngramValueElevation.Neutral;

        return new EngramCompassState
        {
            IdForce = ExtractDouble(metadata, "compass_id_force"),
            SuperegoConstraint = ExtractDouble(metadata, "compass_superego_constraint"),
            EgoStability = ExtractDouble(metadata, "compass_ego_stability"),
            ValueElevation = valueElevation,
            SymbolicDepth = (int)Math.Round(ExtractDouble(metadata, "compass_symbolic_depth")),
            BranchingFactor = (int)Math.Round(ExtractDouble(metadata, "compass_branching_factor")),
            DecisionEntropy = ExtractDouble(metadata, "compass_decision_entropy"),
            Timestamp = ExtractDate(metadata, "compass_timestamp")
        };
    }

    private static string ExtractMetadata(IReadOnlyDictionary<string, string> metadata, string key, string fallback)
    {
        if (metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return fallback;
    }

    private static IReadOnlyList<string> ExtractSliTokens(IReadOnlyDictionary<string, string> metadata)
    {
        if (!metadata.TryGetValue("sli_tokens", out var raw) || string.IsNullOrWhiteSpace(raw))
        {
            if (!metadata.TryGetValue("symbolic_trace", out raw) || string.IsNullOrWhiteSpace(raw))
            {
                return [];
            }
        }

        var separators = new[] { '|', ',', ';', ' ' };
        return raw
            .Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static double ExtractDouble(IReadOnlyDictionary<string, string> metadata, string key)
    {
        if (metadata.TryGetValue(key, out var value) &&
            double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return 0.0;
    }

    private static DateTime ExtractDate(IReadOnlyDictionary<string, string> metadata, string key)
    {
        if (metadata.TryGetValue(key, out var value) &&
            DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
        {
            return parsed;
        }

        return DateTime.UtcNow;
    }
}

internal sealed record CandidateSplit(string CognitionBody, string CleavedResidue);
