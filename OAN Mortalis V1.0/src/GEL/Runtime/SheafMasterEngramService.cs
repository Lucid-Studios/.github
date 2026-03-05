using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CradleTek.Host.Interfaces;
using GEL.Analysis;
using GEL.Graphs;
using GEL.Models;
using GEL.Telemetry;
using Telemetry.GEL;

namespace GEL.Runtime;

public sealed class SheafMasterEngramService
{
    private const string LegacySheafType = "SheafMasterEngram";
    private const string ProceduralEngramClass = "ProceduralEngram";
    private const string PropositionalEngramClass = "PropositionalEngram";
    private readonly SheafCohomologyAnalyzer _cohomologyAnalyzer;
    private IReadOnlyList<SheafMasterEngram> _sheaves;

    public SheafMasterEngramService(SheafCohomologyAnalyzer? cohomologyAnalyzer = null)
    {
        _cohomologyAnalyzer = cohomologyAnalyzer ?? new SheafCohomologyAnalyzer();
        _sheaves = RefreshCohomologyStates(CreateDefaults());
    }

    public IReadOnlyList<SheafMasterEngram> GetAll() => _sheaves;

    public SheafExecutionPlan BuildExecutionPlan(string objective)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objective);

        var sheaf = ResolveForObjective(objective);
        return new SheafExecutionPlan
        {
            Domain = sheaf.DomainName,
            FunctorPath = sheaf.ProceduralFunctors.BuildFunctorPath(),
            LispComposition = sheaf.ProceduralFunctors.ToLispComposition(),
            Cohomology = sheaf.CohomologyState
        };
    }

    public SheafMasterEngram ResolveForObjective(string objective)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(objective);
        var normalized = objective.ToLowerInvariant();

        if (ContainsAny(normalized, "equation", "algebra", "variable", "solve", "x", "linear"))
        {
            return FindDomain("algebra");
        }

        if (ContainsAny(normalized, "add", "subtract", "multiply", "divide", "arithmetic", "number"))
        {
            return FindDomain("arithmetic");
        }

        if (ContainsAny(normalized, "proof", "logic", "predicate", "tautology", "implication"))
        {
            return FindDomain("logic");
        }

        if (ContainsAny(normalized, "probability", "distribution", "random", "stochastic", "bayes"))
        {
            return FindDomain("probability");
        }

        if (ContainsAny(normalized, "geometry", "triangle", "shape", "angle", "vector"))
        {
            return FindDomain("geometry");
        }

        return FindDomain("arithmetic");
    }

    public async Task PersistSheafRecordAsync(
        SheafMasterEngram sheaf,
        IPublicStore publicStore,
        GelTelemetryAdapter telemetry,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sheaf);
        ArgumentNullException.ThrowIfNull(publicStore);
        ArgumentNullException.ThrowIfNull(telemetry);

        var record = BuildGelRecord(sheaf);
        var hash = HashHex(record);
        await publicStore.PublishPointerAsync($"gel:sheaf:{sheaf.DomainName}:{hash}", cancellationToken).ConfigureAwait(false);

        var cohomologyEvent = await _cohomologyAnalyzer
            .AnalyzeAndEmitAsync(sheaf, _sheaves, telemetry, cancellationToken)
            .ConfigureAwait(false);

        await publicStore.PublishPointerAsync(
            $"gel:sheaf:cohomology:{cohomologyEvent.DomainName}:{cohomologyEvent.EventHash}",
            cancellationToken).ConfigureAwait(false);
    }

    public string BuildGelRecord(SheafMasterEngram sheaf)
    {
        ArgumentNullException.ThrowIfNull(sheaf);

        var payload = new
        {
            // Keep legacy type for compatibility with existing GEL readers.
            type = LegacySheafType,
            epistemic_class = ProceduralEngramClass,
            propositional_component_class = PropositionalEngramClass,
            domain = sheaf.DomainName,
            symbolic_id = sheaf.SymbolicID,
            root_set = sheaf.RootSet,
            constructor_graph = sheaf.ConstructorGraph.Edges.Select(edge => new { edge.Source, edge.Target, edge.Relation }).ToArray(),
            procedural_functors = sheaf.ProceduralFunctors.FunctorPipeline,
            morphisms = sheaf.Morphisms.Select(morphism => new
            {
                source = morphism.SourceDomain,
                target = morphism.TargetDomain,
                functor = morphism.TranslationFunctor
            }).ToArray(),
            cohomology = new
            {
                missing_morphisms = sheaf.CohomologyState.MissingMorphisms,
                inconsistent_symbols = sheaf.CohomologyState.InconsistentSymbols,
                disconnected_functor_chains = sheaf.CohomologyState.DisconnectedFunctorChains
            }
        };

        return JsonSerializer.Serialize(payload);
    }

    private SheafMasterEngram FindDomain(string domainName)
    {
        return _sheaves.First(sheaf => string.Equals(sheaf.DomainName, domainName, StringComparison.OrdinalIgnoreCase));
    }

    private IReadOnlyList<SheafMasterEngram> RefreshCohomologyStates(IReadOnlyList<SheafMasterEngram> sheaves)
    {
        var updated = new List<SheafMasterEngram>(sheaves.Count);
        foreach (var sheaf in sheaves)
        {
            var cohomology = _cohomologyAnalyzer.Analyze(sheaf, sheaves);
            updated.Add(new SheafMasterEngram
            {
                SymbolicID = sheaf.SymbolicID,
                DomainName = sheaf.DomainName,
                RootSet = sheaf.RootSet,
                ConstructorGraph = sheaf.ConstructorGraph,
                ProceduralFunctors = sheaf.ProceduralFunctors,
                LocalAtlas = sheaf.LocalAtlas,
                Morphisms = sheaf.Morphisms,
                Consistency = sheaf.Consistency,
                CohomologyState = cohomology
            });
        }

        return updated;
    }

    private static IReadOnlyList<SheafMasterEngram> CreateDefaults()
    {
        var emptyCohomology = new SheafCohomologyState
        {
            MissingMorphisms = [],
            InconsistentSymbols = [],
            DisconnectedFunctorChains = []
        };

        var commonRules = new ConsistencyRules
        {
            Rules =
            [
                new ConsistencyRule { DomainA = "arithmetic", DomainB = "algebra", SharedSymbol = "addition" },
                new ConsistencyRule { DomainA = "arithmetic", DomainB = "algebra", SharedSymbol = "multiplication" },
                new ConsistencyRule { DomainA = "logic", DomainB = "probability", SharedSymbol = "implication" }
            ]
        };

        var morphisms = new[]
        {
            new DomainMorphism { SourceDomain = "arithmetic", TargetDomain = "algebra", TranslationFunctor = "arith_to_alg" },
            new DomainMorphism { SourceDomain = "algebra", TargetDomain = "geometry", TranslationFunctor = "alg_to_geom" },
            new DomainMorphism { SourceDomain = "logic", TargetDomain = "probability", TranslationFunctor = "logic_to_prob" }
        };

        var arithmetic = new SheafMasterEngram
        {
            SymbolicID = "Arithmetic.Sheaf",
            DomainName = "arithmetic",
            RootSet = ["number", "addition", "subtraction", "multiplication", "division", "equation", "variable", "solution"],
            ConstructorGraph = new ConstructorGraph
            {
                Edges =
                [
                    new ConstructorEdge { Source = "addition", Target = "commutative", Relation = "satisfies" },
                    new ConstructorEdge { Source = "addition", Target = "associative", Relation = "satisfies" },
                    new ConstructorEdge { Source = "multiplication", Target = "distributive", Relation = "satisfies" },
                    new ConstructorEdge { Source = "division", Target = "inverse_of_multiplication", Relation = "maps-to" }
                ]
            },
            ProceduralFunctors = new ProceduralFunctorGraph
            {
                FunctorPipeline = ["solve_equation", "normalize_expression", "simplify_expression", "isolate_variable", "evaluate_solution"]
            },
            LocalAtlas = new LocalSymbolAtlas
            {
                DomainName = "arithmetic",
                SymbolMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["number"] = "ATLAS.ROOT.NUMBER",
                    ["addition"] = "ATLAS.ROOT.ADD",
                    ["subtraction"] = "ATLAS.ROOT.SUBTRACT",
                    ["multiplication"] = "ATLAS.ROOT.MULTIPLY",
                    ["division"] = "ATLAS.ROOT.DIVIDE",
                    ["equation"] = "ATLAS.ROOT.EQUATION",
                    ["variable"] = "ATLAS.ROOT.VARIABLE",
                    ["solution"] = "ATLAS.ROOT.SOLUTION"
                }
            },
            Morphisms = morphisms.Where(m => m.SourceDomain == "arithmetic").ToList(),
            Consistency = commonRules,
            CohomologyState = emptyCohomology
        };

        var algebra = new SheafMasterEngram
        {
            SymbolicID = "Algebra.Sheaf",
            DomainName = "algebra",
            RootSet = ["variable", "equation", "addition", "multiplication", "polynomial", "transformation"],
            ConstructorGraph = new ConstructorGraph
            {
                Edges =
                [
                    new ConstructorEdge { Source = "equation", Target = "balanced_transform", Relation = "requires" },
                    new ConstructorEdge { Source = "polynomial", Target = "factorization", Relation = "supports" }
                ]
            },
            ProceduralFunctors = new ProceduralFunctorGraph
            {
                FunctorPipeline = ["normalize_equation", "simplify_expression", "isolate_variable", "verify_solution"]
            },
            LocalAtlas = new LocalSymbolAtlas
            {
                DomainName = "algebra",
                SymbolMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["variable"] = "ATLAS.ROOT.VARIABLE",
                    ["equation"] = "ATLAS.ROOT.EQUATION",
                    ["polynomial"] = "ATLAS.ROOT.POLYNOMIAL"
                }
            },
            Morphisms = morphisms.Where(m => m.SourceDomain == "algebra").ToList(),
            Consistency = commonRules,
            CohomologyState = emptyCohomology
        };

        var geometry = new SheafMasterEngram
        {
            SymbolicID = "Geometry.Sheaf",
            DomainName = "geometry",
            RootSet = ["point", "line", "angle", "triangle", "vector", "proof"],
            ConstructorGraph = new ConstructorGraph
            {
                Edges =
                [
                    new ConstructorEdge { Source = "triangle", Target = "angle_sum", Relation = "implies" },
                    new ConstructorEdge { Source = "vector", Target = "transform", Relation = "supports" }
                ]
            },
            ProceduralFunctors = new ProceduralFunctorGraph
            {
                FunctorPipeline = ["parse_figure", "construct_relations", "derive_constraints", "validate_proof"]
            },
            LocalAtlas = new LocalSymbolAtlas
            {
                DomainName = "geometry",
                SymbolMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["point"] = "ATLAS.ROOT.POINT",
                    ["line"] = "ATLAS.ROOT.LINE",
                    ["angle"] = "ATLAS.ROOT.ANGLE"
                }
            },
            Morphisms = [],
            Consistency = commonRules,
            CohomologyState = emptyCohomology
        };

        var logic = new SheafMasterEngram
        {
            SymbolicID = "Logic.Sheaf",
            DomainName = "logic",
            RootSet = ["predicate", "implication", "quantifier", "proof", "consistency"],
            ConstructorGraph = new ConstructorGraph
            {
                Edges =
                [
                    new ConstructorEdge { Source = "predicate", Target = "entailment", Relation = "produces" },
                    new ConstructorEdge { Source = "implication", Target = "truth_table", Relation = "evaluates-with" }
                ]
            },
            ProceduralFunctors = new ProceduralFunctorGraph
            {
                FunctorPipeline = ["symbolize", "evaluate_predicates", "branch_proof", "check_consistency"]
            },
            LocalAtlas = new LocalSymbolAtlas
            {
                DomainName = "logic",
                SymbolMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["predicate"] = "ATLAS.ROOT.PREDICATE",
                    ["implication"] = "ATLAS.ROOT.IMPLICATION"
                }
            },
            Morphisms = morphisms.Where(m => m.SourceDomain == "logic").ToList(),
            Consistency = commonRules,
            CohomologyState = emptyCohomology
        };

        var probability = new SheafMasterEngram
        {
            SymbolicID = "Probability.Sheaf",
            DomainName = "probability",
            RootSet = ["distribution", "likelihood", "bayes", "prior", "posterior", "evidence", "implication"],
            ConstructorGraph = new ConstructorGraph
            {
                Edges =
                [
                    new ConstructorEdge { Source = "bayes", Target = "posterior", Relation = "updates" },
                    new ConstructorEdge { Source = "likelihood", Target = "evidence", Relation = "weights" }
                ]
            },
            ProceduralFunctors = new ProceduralFunctorGraph
            {
                FunctorPipeline = ["frame_hypothesis", "collect_evidence", "update_prior", "compute_posterior"]
            },
            LocalAtlas = new LocalSymbolAtlas
            {
                DomainName = "probability",
                SymbolMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["distribution"] = "ATLAS.ROOT.DISTRIBUTION",
                    ["likelihood"] = "ATLAS.ROOT.LIKELIHOOD",
                    ["bayes"] = "ATLAS.ROOT.BAYES"
                }
            },
            Morphisms = [],
            Consistency = commonRules,
            CohomologyState = emptyCohomology
        };

        return [arithmetic, algebra, geometry, logic, probability];
    }

    private static bool ContainsAny(string value, params string[] terms)
    {
        return terms.Any(term => value.Contains(term, StringComparison.Ordinal));
    }

    private static string HashHex(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }
}

public sealed class SheafExecutionPlan
{
    public required string Domain { get; init; }
    public required IReadOnlyList<string> FunctorPath { get; init; }
    public required string LispComposition { get; init; }
    public required SheafCohomologyState Cohomology { get; init; }
}
