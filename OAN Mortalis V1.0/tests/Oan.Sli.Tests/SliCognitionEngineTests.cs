using CradleTek.CognitionHost.Models;
using CradleTek.Memory.Services;
using SLI.Engine.Cognition;

namespace Oan.Sli.Tests;

public sealed class SliCognitionEngineTests
{
    [Fact]
    public async Task LispRuntime_LoadsAndExecutes_WithCompassMetrics()
    {
        var resolver = new EngramResolverService();
        var engine = new SliCognitionEngine(resolver);
        await engine.InitializeAsync();

        var request = new CognitionRequest
        {
            Context = new CognitionContext
            {
                CMEId = "cme-test",
                SoulFrameId = Guid.NewGuid(),
                ContextId = Guid.NewGuid(),
                TaskObjective = "identity-continuity",
                RelevantEngrams = []
            },
            Prompt = "execute symbolic cognition"
        };

        var result = await engine.ExecuteAsync(request);

        Assert.False(string.IsNullOrWhiteSpace(result.Decision));
        Assert.False(string.IsNullOrWhiteSpace(result.CleaveResidue));
        Assert.InRange(result.Confidence, 0.1, 0.99);

        Assert.NotNull(engine.LastTraceEvent);
        Assert.NotNull(engine.LastDecisionSpline);
        Assert.NotNull(engine.LastTraceEvent!.CompassState);
        Assert.True(engine.LastTraceEvent.CompassState.SymbolicDepth > 0);
        Assert.True(engine.LastTraceEvent.SymbolicTrace.Count > 0);
        Assert.False(string.IsNullOrWhiteSpace(engine.LastTraceEvent.TraceId));
        Assert.Equal(result.Decision, engine.LastTraceEvent.DecisionBranch);
        Assert.Equal(result.CleaveResidue, engine.LastTraceEvent.CleaveResidue);
    }
}
