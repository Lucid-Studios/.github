using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oan.Core;
using Oan.Core.Governance;
using Oan.SoulFrame;
using Oan.Runtime;
using Oan.Ledger;
using Oan.CradleTek;
using Oan.SoulFrame.Services;
using Oan.Core.Meaning;
using Oan.Core.Engrams;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSingleton<WorldState>();
builder.Services.AddSingleton<EventLog>();

// Engram MVP Services
builder.Services.AddSingleton<Oan.AgentiCore.Engrams.EngramStore>();
builder.Services.AddSingleton<Oan.AgentiCore.Engrams.EngramRouter>();
builder.Services.AddSingleton<Oan.AgentiCore.Engrams.EngramFormationService>();
builder.Services.AddSingleton<Oan.AgentiCore.Engrams.EngramQueryService>();
builder.Services.AddSingleton<Oan.AgentiCore.Engrams.EngramProjectionService>();

// SLI MVP Services
builder.Services.AddSingleton<Oan.SoulFrame.SLI.SliGateService>();

// Setup default session for prototype
var defaultSession = new SoulFrameSession("session-1", "operator-1");
defaultSession.AddToRoster("agent-profile-1"); // Default allowed agent
defaultSession.Mounts.TryAddMount(new Oan.Core.Governance.MountEntry { 
    Address = new Oan.Core.Governance.SliAddress(Oan.Core.Governance.SliChannel.Public, Oan.Core.Governance.SliPartition.OAN, Oan.Core.Governance.SliMirror.Standard),
    MountId = "seed-oan", PolicyVersion = "sli.policy.v0.1", SatCeiling = Oan.Core.Governance.SatMode.Standard, RequiresHitlForElevation = false, CreatedTick = 0
});
defaultSession.Mounts.TryAddMount(new Oan.Core.Governance.MountEntry { 
    Address = new Oan.Core.Governance.SliAddress(Oan.Core.Governance.SliChannel.Public, Oan.Core.Governance.SliPartition.GEL, Oan.Core.Governance.SliMirror.Standard),
    MountId = "seed-gel", PolicyVersion = "sli.policy.v0.1", SatCeiling = Oan.Core.Governance.SatMode.Standard, RequiresHitlForElevation = false, CreatedTick = 0
});
builder.Services.AddSingleton(defaultSession);

builder.Services.AddTransient<IntentProcessor>();
builder.Services.AddTransient<SessionOrchestrator>();
builder.Services.AddSingleton<MeaningLatticeService>(sp => 
    new MeaningLatticeService(
        (type, payload, tick) => sp.GetRequiredService<EventLog>().Append(type, payload, tick), 
        id => sp.GetRequiredService<SoulFrameSession>()
    ));

// Host Registry & Modules
var registry = new Oan.CradleTek.HostRegistry();
// In a real app, use DI or scanning. Here we manually load.
await registry.LoadModuleAsync(new Oan.Place.GEL.Service.GelServiceModule());
await registry.LoadModuleAsync(new Oan.Place.GEL.Self.GelSelfModule());
await registry.LoadModuleAsync(new Oan.Place.OAN.Service.OanServiceModule());
await registry.LoadModuleAsync(new Oan.Place.OAN.Self.OanSelfModule());
await registry.LoadModuleAsync(new Oan.Place.GOA.Service.GoaServiceModule());
await registry.LoadModuleAsync(new Oan.Place.GOA.Self.GoaSelfModule());

builder.Services.AddSingleton<Oan.Place.Abstractions.IHostRegistry>(registry);

var app = builder.Build();

// Endpoints

app.MapGet("/v1/snapshot", (WorldState world, EventLog ledger, SoulFrameSession session) =>
{
    return Results.Ok(new 
    { 
        Tick = world.Tick, 
        Entities = world.Entities.Count,
        Events = ledger.GetEvents().Count(),
        ActiveAgent = session.ActiveAgentProfileId
    });
});

app.MapGet("/v1/engram/{id}", (string id, Oan.AgentiCore.Engrams.EngramQueryService query, Oan.AgentiCore.Engrams.EngramProjectionService projection) =>
{
    var block = query.GetById(id);
    return block != null ? Results.Ok(projection.ToDto(block)) : Results.NotFound();
});

app.MapGet("/v1/engram/query", (
    string? rootId, 
    string? opalRootId, 
    string? sessionId, 
    EngramChannel? channel,
    KnowingMode? knowingMode,
    MetabolicRegime? metabolicRegime,
    ResolutionMode? resolutionMode,
    long? afterTick,
    int? limit,
    Oan.AgentiCore.Engrams.EngramQueryService query,
    Oan.AgentiCore.Engrams.EngramProjectionService projection) =>
{
    int max = limit ?? 100;
    IEnumerable<Oan.Core.Engrams.EngramBlock> results = Array.Empty<Oan.Core.Engrams.EngramBlock>();
    
    // Priority: Root -> Opal -> Session -> Channel -> Stance
    if (!string.IsNullOrEmpty(rootId)) results = query.QueryByRootId(rootId, max, afterTick);
    else if (!string.IsNullOrEmpty(opalRootId)) results = query.QueryByOpalRootId(opalRootId, max, afterTick);
    else if (!string.IsNullOrEmpty(sessionId)) results = query.QueryBySessionId(sessionId, max);
    else if (channel.HasValue) results = query.QueryByChannel(channel.Value, max);
    else if (knowingMode.HasValue || metabolicRegime.HasValue || resolutionMode.HasValue)
    {
        results = query.QueryByStance(knowingMode, metabolicRegime, resolutionMode, max);
    }
    else
    {
        return Results.BadRequest(new { error = "No query criteria provided." });
    }

    return Results.Ok(projection.ToDto(results));
});

app.MapPost("/v1/soulframe/{sessionId}/activate-agent", (string sessionId, ActivationRequest request, IntentProcessor processor) =>
{
    // In a real app, retrieve session by ID. Here we use the singleton.
    var result = processor.ActivateAgent(request.AgentProfileId, request.Reason);
    return Results.Ok(result);
});

app.MapPost("/v1/soulframe/{sessionId}/closeout", (string sessionId, [Microsoft.AspNetCore.Mvc.FromBody] CloseoutRequest request, SessionOrchestrator orchestrator) =>
{
    try
    {
        var receipt = orchestrator.CloseoutSession(sessionId, request.OperatorId, request.RequestId);
        return Results.Ok(receipt);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = ex.Message });
    }
});

app.MapPost("/v1/intent/evaluate", (Intent intent, IntentProcessor processor) =>
{
    // True Evaluation (ReadOnly)
    var result = processor.EvaluateIntent(intent);
    return Results.Ok(result);
});

app.MapPost("/v1/intent/commit", (Intent intent, IntentProcessor processor, EventLog ledger) =>
{
    // Commit (Mutation)
    var result = processor.CommitIntent(intent);
    
    if (result.Status == IntentStatus.Committed)
    {
        ledger.Append("IntentCommitted", result);
    }
    else
    {
        ledger.Append("IntentRefused", result);
    }

    return Results.Ok(result);
});

app.Run();

public class ActivationRequest
{
    public required string AgentProfileId { get; set; }
    public required string Reason { get; set; }
}

public class CloseoutRequest
{
    public required string OperatorId { get; set; }
    public required string RequestId { get; set; }
}

public class ProposeSpansRequest
{
    public required string NaturalLanguage { get; set; }
    public required string ContextSnapshotId { get; set; }
    public required string OperatorId { get; set; }
}

public class UpdateSpanRequest
{
    public required string SpanId { get; set; }
    public string? UserGloss { get; set; }
    public MeaningStatus Status { get; set; }
    public required string OperatorId { get; set; }
}

public class FrameLockRequest
{
    public required string Goal { get; set; }
    public FrameMode Mode { get; set; }
    public List<string>? Constraints { get; set; }
    public List<string>? Assumptions { get; set; }
    public required string OperatorId { get; set; }
}

