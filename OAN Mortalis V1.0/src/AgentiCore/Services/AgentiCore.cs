using AgentiCore.Models;
using CradleTek.CognitionHost.Interfaces;
using CradleTek.CognitionHost.Models;
using CradleTek.Host.Interfaces;
using CradleTek.Memory.Interfaces;
using CradleTek.Memory.Models;
using CradleTek.Memory.Services;
using GEL.Runtime;
using OAN.Core.Telemetry;
using SLI.Ingestion;
using SoulFrame.Host;
using SoulFrame.Identity.Models;
using System.Globalization;
using System.Text.Json;
using Telemetry.GEL;
using SoulFrameModel = SoulFrame.Identity.Models.SoulFrame;

namespace AgentiCore.Services;

public sealed class AgentiCore
{
    private readonly ICognitionEngine _sliCognitionEngine;
    private readonly IEngramResolver _engramResolver;
    private readonly ContextAssembler _contextAssembler;
    private readonly EngramCommitService _engramCommit;
    private readonly IPublicStore _publicStore;
    private readonly ICrypticStore _crypticStore;
    private readonly GelTelemetryAdapter _telemetry;
    private readonly IRootOntologicalCleaver _rootOntologicalCleaver;
    private readonly SheafMasterEngramService _sheafMasterEngrams;
    private readonly SliIngestionEngine _sliIngestionEngine;
    private readonly SoulFrameHostClient _soulFrameHostClient;

    public AgentiCore(
        ICognitionEngine sliCognitionEngine,
        IEngramResolver engramResolver,
        ContextAssembler contextAssembler,
        EngramCommitService engramCommit,
        IPublicStore publicStore,
        ICrypticStore crypticStore,
        GelTelemetryAdapter telemetry,
        IRootOntologicalCleaver? rootOntologicalCleaver = null,
        SheafMasterEngramService? sheafMasterEngrams = null,
        SliIngestionEngine? sliIngestionEngine = null,
        SoulFrameHostClient? soulFrameHostClient = null)
    {
        _sliCognitionEngine = sliCognitionEngine;
        _engramResolver = engramResolver;
        _contextAssembler = contextAssembler;
        _engramCommit = engramCommit;
        _publicStore = publicStore;
        _crypticStore = crypticStore;
        _telemetry = telemetry;
        _rootOntologicalCleaver = rootOntologicalCleaver ?? new RootAtlasOntologicalCleaver();
        _sheafMasterEngrams = sheafMasterEngrams ?? new SheafMasterEngramService();
        _sliIngestionEngine = sliIngestionEngine ?? new SliIngestionEngine();
        _soulFrameHostClient = soulFrameHostClient ?? new SoulFrameHostClient(
            telemetry: new SoulFrameTelemetryAdapter(telemetry));
    }

    public AgentiContext InitializeContext(SoulFrameModel soulFrame, IEnumerable<string>? activeConcepts = null)
    {
        ArgumentNullException.ThrowIfNull(soulFrame);

        var context = new AgentiContext
        {
            CMEId = soulFrame.CMEId,
            SoulFrameId = soulFrame.SoulFrameId,
            ContextId = Guid.NewGuid(),
            ActiveConcepts = activeConcepts?.Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? ["Engram", "SLI", "SoulFrame"],
            WorkingMemory = new Dictionary<string, string>(StringComparer.Ordinal),
            ExecutionTimestamp = DateTime.UtcNow
        };

        EmitTelemetry("cognition-cycle-start", context.ContextId, context.CMEId);
        return context;
    }

    public Task<AgentiResult> ExecuteCognitionCycleAsync(
        AgentiContext context,
        CancellationToken cancellationToken = default)
    {
        return ExecuteCognitionCycleAsync(context, null, cancellationToken);
    }

    public async Task<AgentiResult> ExecuteCognitionCycleAsync(
        AgentiContext context,
        string? operatorInput,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.ExecutionTimestamp = DateTime.UtcNow;
        var publicPointers = await _publicStore.ListPublishedPointersAsync(cancellationToken).ConfigureAwait(false);
        var crypticPointers = await _crypticStore.ListPointersAsync(cancellationToken).ConfigureAwait(false);
        context.WorkingMemory["public_pointer_count"] = publicPointers.Count.ToString();
        context.WorkingMemory["cryptic_pointer_count"] = crypticPointers.Count.ToString();
        EmitTelemetry("memory-retrieval", context.ContextId, context.CMEId);

        var runtimeInput = string.IsNullOrWhiteSpace(operatorInput)
            ? "maintain identity continuity and resolve runtime objective"
            : operatorInput.Trim();

        var ingestionResult = await _sliIngestionEngine.IngestAsync(runtimeInput, cancellationToken).ConfigureAwait(false);
        EmitTelemetry("sli-ingestion", context.ContextId, context.CMEId);

        context.WorkingMemory["ingestion_token_count"] = ingestionResult.CleavedOntology.Tokens.Count.ToString(CultureInfo.InvariantCulture);
        context.WorkingMemory["ingestion_expression_count"] = ingestionResult.CleavedOntology.Expressions.Count.ToString(CultureInfo.InvariantCulture);
        context.WorkingMemory["ingestion_candidate_count"] = ingestionResult.MatchResult.EngramCandidates.Count.ToString(CultureInfo.InvariantCulture);
        context.WorkingMemory["ingestion_trace_seed"] = ingestionResult.SliExpression.TraceSeed;
        context.WorkingMemory["ingestion_sheaf_domain"] = ingestionResult.SheafDomain;

        var baseCognitionContext = new CognitionContext
        {
            CMEId = context.CMEId,
            SoulFrameId = context.SoulFrameId,
            ContextId = context.ContextId,
            TaskObjective = runtimeInput,
            RelevantEngrams = [],
            SymbolicProgram = ingestionResult.SliExpression.ProgramExpressions
        };

        // Legacy SheafMasterEngram resolution provides the procedural engram path for execution.
        var sheafPlan = _sheafMasterEngrams.BuildExecutionPlan(baseCognitionContext.TaskObjective);
        context.WorkingMemory["sheaf_domain"] = sheafPlan.Domain;
        context.WorkingMemory["sheaf_functor_path"] = string.Join(" -> ", sheafPlan.FunctorPath);
        context.WorkingMemory["sheaf_lisp_compose"] = sheafPlan.LispComposition;
        context.WorkingMemory["sheaf_missing_morphisms"] = sheafPlan.Cohomology.MissingMorphisms.Count.ToString(CultureInfo.InvariantCulture);
        context.WorkingMemory["sheaf_inconsistent_symbols"] = sheafPlan.Cohomology.InconsistentSymbols.Count.ToString(CultureInfo.InvariantCulture);
        context.WorkingMemory["sheaf_disconnected_functors"] = sheafPlan.Cohomology.DisconnectedFunctorChains.Count.ToString(CultureInfo.InvariantCulture);
        var selectedSheaf = _sheafMasterEngrams.ResolveForObjective(baseCognitionContext.TaskObjective);
        await _sheafMasterEngrams.PersistSheafRecordAsync(selectedSheaf, _publicStore, _telemetry, cancellationToken).ConfigureAwait(false);
        EmitTelemetry("sheaf-master-engram-selected", context.ContextId, context.CMEId);

        var hostedRequestContext = ingestionResult.MatchResult.EngramCandidates.Count == 0
            ? runtimeInput
            : string.Join(" ", ingestionResult.MatchResult.EngramCandidates.Select(candidate => candidate.Token).Take(8));
        var hostedSemanticResponse = await _soulFrameHostClient.ClassifyAsync(
                new SoulFrameInferenceRequest
                {
                    Task = "classify",
                    Context = hostedRequestContext,
                    OpalConstraints = BuildOpalConstraints(sheafPlan.Domain),
                    SoulFrameId = context.SoulFrameId,
                    ContextId = context.ContextId
                },
                cancellationToken)
            .ConfigureAwait(false);
        context.WorkingMemory["hosted_semantic_decision"] = hostedSemanticResponse.Decision;
        context.WorkingMemory["hosted_semantic_confidence"] = hostedSemanticResponse.Confidence.ToString("F4", CultureInfo.InvariantCulture);
        context.WorkingMemory["hosted_semantic_accepted"] = hostedSemanticResponse.Accepted ? "true" : "false";
        EmitTelemetry("soulframe-host-classify", context.ContextId, context.CMEId);

        var cleaverInput = $"{baseCognitionContext.TaskObjective}. concepts: {string.Join(" ", context.ActiveConcepts)}";
        var cleaverResult = await _rootOntologicalCleaver.CleaveAsync(cleaverInput, cancellationToken).ConfigureAwait(false);
        context.WorkingMemory["known_ratio"] = cleaverResult.Metrics.KnownRatio.ToString("F4", CultureInfo.InvariantCulture);
        context.WorkingMemory["unknown_ratio"] = cleaverResult.Metrics.UnknownRatio.ToString("F4", CultureInfo.InvariantCulture);
        context.WorkingMemory["concept_density"] = cleaverResult.Metrics.ConceptDensity;
        context.WorkingMemory["context_stability"] = cleaverResult.Metrics.ContextStability;
        context.WorkingMemory["unknown_tokens"] = cleaverResult.Unknown.Count == 0
            ? "none"
            : string.Join(",", cleaverResult.Unknown);
        EmitTelemetry("ontological-cleave", context.ContextId, context.CMEId);

        var resolvedEngrams = await _engramResolver.ResolveRelevantAsync(baseCognitionContext, cancellationToken).ConfigureAwait(false);
        context.WorkingMemory["resolved_engram_count"] = resolvedEngrams.Summaries.Count.ToString();
        EmitTelemetry("engram-resolve", context.ContextId, context.CMEId);

        var cognitionRequest = _contextAssembler.BuildRequest(
            baseCognitionContext,
            resolvedEngrams.Summaries,
            BuildCognitionPrompt(context, cleaverResult, sheafPlan, ingestionResult));

        var cognitionResult = await _sliCognitionEngine.ExecuteAsync(cognitionRequest, cancellationToken).ConfigureAwait(false);
        EmitTelemetry("cognition-inference", context.ContextId, context.CMEId);

        var requiresCommit = cognitionResult.EngramCandidate;

        var cognitionPayload = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["reasoning"] = cognitionResult.Reasoning,
            ["decision"] = cognitionResult.Decision,
            ["engram_candidate"] = cognitionResult.EngramCandidate,
            ["cleave_residue"] = cognitionResult.CleaveResidue,
            ["trace_id"] = cognitionResult.TraceId,
            ["symbolic_trace"] = cognitionResult.SymbolicTrace,
            ["sli_tokens"] = cognitionResult.SliTokens,
            ["decision_branch"] = cognitionResult.DecisionBranch,
            ["sheaf_domain"] = sheafPlan.Domain,
            ["sheaf_functor_path"] = sheafPlan.FunctorPath,
            ["sheaf_lisp_compose"] = sheafPlan.LispComposition,
            ["sheaf_cohomology"] = new Dictionary<string, object?>
            {
                ["missing_morphisms"] = sheafPlan.Cohomology.MissingMorphisms.ToArray(),
                ["inconsistent_symbols"] = sheafPlan.Cohomology.InconsistentSymbols.ToArray(),
                ["disconnected_functor_chains"] = sheafPlan.Cohomology.DisconnectedFunctorChains.ToArray()
            },
            ["root_cleaver"] = new Dictionary<string, object?>
            {
                ["known"] = cleaverResult.Known.Select(k => k.SymbolicId).ToArray(),
                ["partially_known"] = cleaverResult.PartiallyKnown.Select(k => k.SymbolicId).ToArray(),
                ["unknown"] = cleaverResult.Unknown.ToArray(),
                ["known_ratio"] = cleaverResult.Metrics.KnownRatio,
                ["unknown_ratio"] = cleaverResult.Metrics.UnknownRatio,
                ["concept_density"] = cleaverResult.Metrics.ConceptDensity,
                ["context_stability"] = cleaverResult.Metrics.ContextStability
            },
            ["sli_ingestion"] = new Dictionary<string, object?>
            {
                ["symbol_tree"] = ingestionResult.SliExpression.SymbolTree,
                ["engram_references"] = ingestionResult.SliExpression.EngramReferences.ToArray(),
                ["trace_seed"] = ingestionResult.SliExpression.TraceSeed,
                ["sheaf_domain"] = ingestionResult.SheafDomain,
                ["semantic_hints"] = ingestionResult.SemanticHints.ToArray(),
                ["constructor_graph_edges"] = ingestionResult.ConstructorGraph.Edges.Select(edge => new Dictionary<string, object?>
                {
                    ["source"] = edge.Source,
                    ["target"] = edge.Target,
                    ["relation"] = edge.Relation
                }).ToArray(),
                ["constructor_records"] = ingestionResult.ConstructorEngrams.Select(record => new Dictionary<string, object?>
                {
                    ["domain"] = record.Domain,
                    ["level"] = record.Level.ToString(),
                    ["symbolic_structure"] = record.SymbolicStructure,
                    ["root_references"] = record.RootReferences.ToArray()
                }).ToArray(),
                ["engram_candidates"] = ingestionResult.MatchResult.EngramCandidates.Select(candidate => new Dictionary<string, object?>
                {
                    ["token"] = candidate.Token,
                    ["context"] = candidate.Context,
                    ["domain_guess"] = candidate.DomainGuess
                }).ToArray()
            },
            ["soulframe_host"] = new Dictionary<string, object?>
            {
                ["accepted"] = hostedSemanticResponse.Accepted,
                ["decision"] = hostedSemanticResponse.Decision,
                ["payload"] = hostedSemanticResponse.Payload,
                ["confidence"] = hostedSemanticResponse.Confidence
            },
            ["compass_state"] = new Dictionary<string, object?>
            {
                ["id_force"] = cognitionResult.CompassState.IdForce,
                ["superego_constraint"] = cognitionResult.CompassState.SuperegoConstraint,
                ["ego_stability"] = cognitionResult.CompassState.EgoStability,
                ["value_elevation"] = cognitionResult.CompassState.ValueElevation.ToString(),
                ["symbolic_depth"] = cognitionResult.CompassState.SymbolicDepth,
                ["branching_factor"] = cognitionResult.CompassState.BranchingFactor,
                ["decision_entropy"] = cognitionResult.CompassState.DecisionEntropy,
                ["timestamp"] = cognitionResult.CompassState.Timestamp
            },
            ["confidence"] = cognitionResult.Confidence
        });

        return new AgentiResult
        {
            ContextId = context.ContextId,
            ResultType = requiresCommit ? "cognition-accepted" : "cognition-rejected",
            ResultPayload = cognitionPayload,
            EngramCommitRequired = requiresCommit
        };
    }

    public async Task ProcessCognitionResult(
        AgentiContext context,
        AgentiResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(result);

        if (!result.EngramCommitRequired)
        {
            return;
        }

        var metadata = BuildCommitMetadata(result);
        await _engramCommit.CommitAsync(context.CMEId, context.SoulFrameId, context.ContextId, result.ResultPayload, metadata, cancellationToken)
            .ConfigureAwait(false);
        EmitTelemetry("engram-commit", context.ContextId, context.CMEId);
    }

    private static IReadOnlyDictionary<string, string> BuildCommitMetadata(AgentiResult result)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["result_type"] = result.ResultType
        };

        try
        {
            using var document = JsonDocument.Parse(result.ResultPayload);
            var root = document.RootElement;
            CopyString(root, "decision", metadata, "decision_branch");
            CopyString(root, "cleave_residue", metadata, "cleave_residue");
            CopyString(root, "trace_id", metadata, "trace_id");
            CopyString(root, "sheaf_domain", metadata, "sheaf_domain");
            CopyString(root, "sheaf_lisp_compose", metadata, "sheaf_lisp_compose");

            if (root.TryGetProperty("symbolic_trace", out var symbolicTrace))
            {
                metadata["symbolic_trace"] = symbolicTrace.ValueKind == JsonValueKind.Array
                    ? string.Join(" | ", symbolicTrace.EnumerateArray().Select(item => item.GetString()).Where(v => !string.IsNullOrWhiteSpace(v)))
                    : symbolicTrace.ToString();
            }

            if (root.TryGetProperty("sli_tokens", out var sliTokens))
            {
                metadata["sli_tokens"] = sliTokens.ValueKind == JsonValueKind.Array
                    ? string.Join(" | ", sliTokens.EnumerateArray().Select(item => item.GetString()).Where(v => !string.IsNullOrWhiteSpace(v)))
                    : sliTokens.ToString();
            }

            if (root.TryGetProperty("sheaf_functor_path", out var sheafFunctorPath))
            {
                metadata["sheaf_functor_path"] = sheafFunctorPath.ValueKind == JsonValueKind.Array
                    ? string.Join(" -> ", sheafFunctorPath.EnumerateArray().Select(item => item.GetString()).Where(v => !string.IsNullOrWhiteSpace(v)))
                    : sheafFunctorPath.ToString();
            }

            if (root.TryGetProperty("sli_ingestion", out var ingestion))
            {
                CopyString(ingestion, "trace_seed", metadata, "sli_trace_seed");
                if (ingestion.TryGetProperty("engram_candidates", out var candidates) && candidates.ValueKind == JsonValueKind.Array)
                {
                    var tokens = candidates
                        .EnumerateArray()
                        .Where(item => item.ValueKind == JsonValueKind.Object && item.TryGetProperty("token", out _))
                        .Select(item => item.TryGetProperty("token", out var token) ? token.GetString() : null)
                        .Where(token => !string.IsNullOrWhiteSpace(token));
                    metadata["ingestion_candidates"] = string.Join(" | ", tokens);
                }
            }

            if (root.TryGetProperty("soulframe_host", out var hosted))
            {
                CopyString(hosted, "decision", metadata, "hosted_semantic_decision");
                CopyDouble(hosted, "confidence", metadata, "hosted_semantic_confidence");
                CopyString(hosted, "accepted", metadata, "hosted_semantic_accepted");
            }

            if (root.TryGetProperty("confidence", out var confidence) && confidence.TryGetDouble(out var confidenceValue))
            {
                metadata["commit_confidence"] = confidenceValue.ToString("F6", CultureInfo.InvariantCulture);
            }

            if (root.TryGetProperty("compass_state", out var compass))
            {
                CopyDouble(compass, "id_force", metadata, "compass_id_force");
                CopyDouble(compass, "superego_constraint", metadata, "compass_superego_constraint");
                CopyDouble(compass, "ego_stability", metadata, "compass_ego_stability");
                CopyString(compass, "value_elevation", metadata, "compass_value_elevation");
                CopyDouble(compass, "decision_entropy", metadata, "compass_decision_entropy");
                CopyDouble(compass, "symbolic_depth", metadata, "compass_symbolic_depth");
                CopyDouble(compass, "branching_factor", metadata, "compass_branching_factor");
                CopyString(compass, "timestamp", metadata, "compass_timestamp");
            }
        }
        catch (JsonException)
        {
            metadata["symbolic_trace"] = "[]";
        }

        return metadata;
    }

    private static void CopyString(JsonElement source, string sourceProperty, IDictionary<string, string> destination, string targetProperty)
    {
        if (source.TryGetProperty(sourceProperty, out var value))
        {
            var stringValue = value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
            if (!string.IsNullOrWhiteSpace(stringValue))
            {
                destination[targetProperty] = stringValue;
            }
        }
    }

    private static void CopyDouble(JsonElement source, string sourceProperty, IDictionary<string, string> destination, string targetProperty)
    {
        if (source.TryGetProperty(sourceProperty, out var value))
        {
            if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var numeric))
            {
                destination[targetProperty] = numeric.ToString("F6", CultureInfo.InvariantCulture);
                return;
            }

            if (value.ValueKind == JsonValueKind.String &&
                double.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out numeric))
            {
                destination[targetProperty] = numeric.ToString("F6", CultureInfo.InvariantCulture);
            }
        }
    }

    private static string BuildCognitionPrompt(
        AgentiContext context,
        OntologicalCleaverResult cleaverResult,
        SheafExecutionPlan sheafPlan,
        SliIngestionResult ingestionResult)
    {
        var knownTerms = cleaverResult.Known
            .Select(k => k.RootTerm)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToArray();

        var unknownTerms = cleaverResult.Unknown.Take(8).ToArray();
        var knownSegment = knownTerms.Length == 0 ? "none" : string.Join("|", knownTerms);
        var unknownSegment = unknownTerms.Length == 0 ? "none" : string.Join("|", unknownTerms);

        return
            $"Cycle:{context.ContextId:D}; Concepts:{string.Join(",", context.ActiveConcepts)}; " +
            $"KnownRoots:{knownSegment}; UnknownRoots:{unknownSegment}; " +
            $"KnownRatio:{cleaverResult.Metrics.KnownRatio:F4}; UnknownRatio:{cleaverResult.Metrics.UnknownRatio:F4}; " +
            $"SheafDomain:{sheafPlan.Domain}; SheafFunctors:{string.Join(">", sheafPlan.FunctorPath)}; " +
            $"SLIExpr:{string.Join(" || ", ingestionResult.SliExpression.ProgramExpressions.Take(2))}";
    }

    private static SoulFrameInferenceConstraints BuildOpalConstraints(string domain)
    {
        return new SoulFrameInferenceConstraints
        {
            Domain = domain,
            DriftLimit = 0.02,
            MaxTokens = 128
        };
    }

    private void EmitTelemetry(string stage, Guid contextId, string cmeId)
    {
        var hashPayload = $"{stage}|{contextId:D}|{cmeId}";
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(hashPayload));
        ITelemetryEvent telemetryEvent = new AgentiTelemetryEvent
        {
            EventHash = Convert.ToHexString(bytes).ToLowerInvariant(),
            Timestamp = DateTime.UtcNow
        };
        _telemetry.AppendAsync(telemetryEvent, stage).GetAwaiter().GetResult();
    }
}
