using CradleTek.CognitionHost.Models;
using CradleTek.Memory.Services;
using SLI.Engine.Cognition;
using SLI.Ingestion;

namespace Oan.Sli.Tests;

public sealed class SliIngestionTests
{
    [Fact]
    public async Task Ingestion_TransformsLinearEquation_ToCanonicalSliStructure()
    {
        var engine = new SliIngestionEngine();

        var result = await engine.IngestAsync("Solve for x: 3x + 7 = 25");

        Assert.Contains(
            result.SliExpression.ProgramExpressions,
            expression => string.Equals(expression, "(≡ (= (+ (⊗ 3 x) 7) 25))", StringComparison.Ordinal));
        Assert.Contains("(= x 6)", result.SliExpression.SymbolTree, StringComparison.Ordinal);
        Assert.NotEmpty(result.MatchResult.KnownEngrams);
    }

    [Fact]
    public async Task Ingestion_DetectsUnknownTokens_AsEngramCandidates()
    {
        var engine = new SliIngestionEngine();

        var result = await engine.IngestAsync("Analyze hypernumeration resonance field");

        Assert.Contains(
            result.MatchResult.EngramCandidates,
            candidate => string.Equals(candidate.Token, "hypernumeration", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task IngestionOutput_IsConsumableBySliCognitionEngine()
    {
        var resolver = new EngramResolverService();
        var ingestionEngine = new SliIngestionEngine();
        var ingestion = await ingestionEngine.IngestAsync("Solve 3x + 7 = 25");

        var cognition = new SliCognitionEngine(resolver);
        await cognition.InitializeAsync();

        var request = new CognitionRequest
        {
            Context = new CognitionContext
            {
                CMEId = "cme-ingestion",
                SoulFrameId = Guid.NewGuid(),
                ContextId = Guid.NewGuid(),
                TaskObjective = "solve equation",
                RelevantEngrams = [],
                SymbolicProgram = ingestion.SliExpression.ProgramExpressions
            },
            Prompt = "execute symbolic cognition"
        };

        var result = await cognition.ExecuteAsync(request);

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Decision));
        Assert.NotEmpty(result.SymbolicTrace);
    }
}
