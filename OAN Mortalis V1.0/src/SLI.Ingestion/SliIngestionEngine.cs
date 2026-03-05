using GEL.Graphs;
using GEL.Runtime;
using SoulFrame.Host;

namespace SLI.Ingestion;

public sealed class SliIngestionEngine
{
    private readonly OntologicalCleaver _ontologicalCleaver;
    private readonly RootEngramMatcher _rootEngramMatcher;
    private readonly ConstructorEngramBuilder _constructorEngramBuilder;
    private readonly SymbolicAssembler _symbolicAssembler;
    private readonly SheafMasterEngramService _sheafMasterEngrams;
    private readonly ISoulFrameSemanticDevice _semanticDevice;

    public SliIngestionEngine(
        OntologicalCleaver? ontologicalCleaver = null,
        RootEngramMatcher? rootEngramMatcher = null,
        ConstructorEngramBuilder? constructorEngramBuilder = null,
        SymbolicAssembler? symbolicAssembler = null,
        SheafMasterEngramService? sheafMasterEngrams = null,
        ISoulFrameSemanticDevice? semanticDevice = null)
    {
        _ontologicalCleaver = ontologicalCleaver ?? new OntologicalCleaver();
        _rootEngramMatcher = rootEngramMatcher ?? new RootEngramMatcher();
        _constructorEngramBuilder = constructorEngramBuilder ?? new ConstructorEngramBuilder();
        _symbolicAssembler = symbolicAssembler ?? new SymbolicAssembler();
        _sheafMasterEngrams = sheafMasterEngrams ?? new SheafMasterEngramService();
        _semanticDevice = semanticDevice ?? NullSoulFrameSemanticDevice.Instance;
    }

    public async Task<SliIngestionResult> IngestAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        cancellationToken.ThrowIfCancellationRequested();

        var cleaved = _ontologicalCleaver.Cleave(input);
        var matched = await _rootEngramMatcher.MatchAsync(cleaved, cancellationToken).ConfigureAwait(false);
        var constructors = _constructorEngramBuilder.Build(cleaved, matched);
        var constructorGraph = _constructorEngramBuilder.BuildGraph(constructors);
        var symbolic = _symbolicAssembler.Assemble(constructors, matched, input);
        var sheafDomain = _sheafMasterEngrams.ResolveForObjective(input).DomainName;
        var semanticHints = await BuildSemanticHintsAsync(matched.EngramCandidates, sheafDomain, cancellationToken).ConfigureAwait(false);

        return new SliIngestionResult
        {
            CleavedOntology = cleaved,
            MatchResult = matched,
            ConstructorEngrams = constructors,
            SliExpression = symbolic,
            ConstructorGraph = constructorGraph,
            SheafDomain = sheafDomain,
            SemanticHints = semanticHints
        };
    }

    private async Task<IReadOnlyList<string>> BuildSemanticHintsAsync(
        IReadOnlyList<EngramCandidate> candidates,
        string sheafDomain,
        CancellationToken cancellationToken)
    {
        var hints = new List<string>();
        foreach (var candidate in candidates.Take(2))
        {
            var response = await _semanticDevice.SemanticExpandAsync(
                    new SoulFrameInferenceRequest
                    {
                        Task = "semantic_expand",
                        Context = candidate.Token,
                        OpalConstraints = new SoulFrameInferenceConstraints
                        {
                            Domain = sheafDomain,
                            DriftLimit = 0.02,
                            MaxTokens = 64
                        },
                        SoulFrameId = Guid.Empty,
                        ContextId = Guid.Empty
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            if (response.Accepted && !string.IsNullOrWhiteSpace(response.Payload))
            {
                hints.Add(response.Payload);
            }
        }

        return hints;
    }
}

public sealed class SliIngestionResult
{
    public required CleavedOntology CleavedOntology { get; init; }
    public required EngramMatchResult MatchResult { get; init; }
    public required IReadOnlyList<ConstructorEngramRecord> ConstructorEngrams { get; init; }
    public required SliExpression SliExpression { get; init; }
    public required ConstructorGraph ConstructorGraph { get; init; }
    public required string SheafDomain { get; init; }
    public required IReadOnlyList<string> SemanticHints { get; init; }
}
