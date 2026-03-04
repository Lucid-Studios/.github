using AgentiCore.Models;
using CradleTek.Host.Interfaces;
using OAN.Core.Telemetry;
using SoulFrame.Identity.Models;
using Telemetry.GEL;
using SoulFrameModel = SoulFrame.Identity.Models.SoulFrame;

namespace AgentiCore.Services;

public sealed class AgentiCore
{
    private readonly SliExecutionService _sliExecution;
    private readonly EngramCommitService _engramCommit;
    private readonly IPublicStore _publicStore;
    private readonly ICrypticStore _crypticStore;
    private readonly GelTelemetryAdapter _telemetry;

    public AgentiCore(
        SliExecutionService sliExecution,
        EngramCommitService engramCommit,
        IPublicStore publicStore,
        ICrypticStore crypticStore,
        GelTelemetryAdapter telemetry)
    {
        _sliExecution = sliExecution;
        _engramCommit = engramCommit;
        _publicStore = publicStore;
        _crypticStore = crypticStore;
        _telemetry = telemetry;
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

    public async Task<AgentiResult> ExecuteCognitionCycleAsync(
        AgentiContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.ExecutionTimestamp = DateTime.UtcNow;
        var publicPointers = await _publicStore.ListPublishedPointersAsync(cancellationToken).ConfigureAwait(false);
        var crypticPointers = await _crypticStore.ListPointersAsync(cancellationToken).ConfigureAwait(false);
        context.WorkingMemory["public_pointer_count"] = publicPointers.Count.ToString();
        context.WorkingMemory["cryptic_pointer_count"] = crypticPointers.Count.ToString();
        EmitTelemetry("memory-retrieval", context.ContextId, context.CMEId);

        var sliResult = await _sliExecution
            .ExecuteAsync(context.CMEId, context.ContextId, context.ActiveConcepts, cancellationToken)
            .ConfigureAwait(false);
        EmitTelemetry("sli-execution", context.ContextId, context.CMEId);

        var requiresCommit = !sliResult.Contains(":status :rejected", StringComparison.OrdinalIgnoreCase);
        return new AgentiResult
        {
            ContextId = context.ContextId,
            ResultType = requiresCommit ? "symbolic-accepted" : "symbolic-rejected",
            ResultPayload = sliResult,
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

        var crypticClassification = result.ResultPayload.Contains(":route :data", StringComparison.OrdinalIgnoreCase);
        await _engramCommit.CommitAsync(context.ContextId, result.ResultPayload, crypticClassification, cancellationToken)
            .ConfigureAwait(false);
        EmitTelemetry("engram-commit", context.ContextId, context.CMEId);
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
