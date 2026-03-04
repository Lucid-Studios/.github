using EngramResolver;

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var corpusIndexDir = Path.Combine(root, "corpus_index");
var indexJsonPath = Path.Combine(corpusIndexDir, "engram_index.json");
var graphPath = Path.Combine(corpusIndexDir, "engram_graph.graphml");
var clusterPath = Path.Combine(corpusIndexDir, "graph_clusters.json");
var examplesPath = Path.Combine(corpusIndexDir, "engram_resolver_examples.md");

if (!File.Exists(indexJsonPath) || !File.Exists(graphPath) || !File.Exists(clusterPath))
{
    Console.Error.WriteLine("Required corpus_index artifacts are missing. Run indexer/visualizer/analysis first.");
    return 1;
}

var loader = new GraphLoader();
var graph = loader.Load(graphPath, clusterPath);
var engramLookup = new EngramLookup();
var pathFinder = new PathFinder();
var clusterLookup = new ClusterLookup();
var formatter = new ResolverOutputFormatter();

WriteExamples();

if (args.Length == 0)
{
    PrintUsage();
    return 1;
}

var command = args[0].Trim().ToLowerInvariant();
var output = command switch
{
    "resolve" => HandleResolve(args),
    "concept" => HandleConcept(args),
    "path" => HandlePath(args),
    "cluster" => HandleCluster(args),
    _ => "Unknown command."
};

Console.WriteLine(output);
return command is "resolve" or "concept" or "path" or "cluster" ? 0 : 1;

string HandleResolve(string[] argv)
{
    if (argv.Length < 2)
    {
        return "Usage: resolve <engram_id>";
    }

    var result = engramLookup.ResolveById(graph, argv[1]);
    return formatter.FormatResolveResult(result);
}

string HandleConcept(string[] argv)
{
    if (argv.Length < 2)
    {
        return "Usage: concept <concept_name>";
    }

    var concept = string.Join(" ", argv.Skip(1));
    var result = engramLookup.ResolveByConcept(graph, concept);
    return formatter.FormatConceptResult(result);
}

string HandlePath(string[] argv)
{
    if (argv.Length < 3)
    {
        return "Usage: path <conceptA> <conceptB>";
    }

    var result = pathFinder.FindShortestPath(graph, argv[1], argv[2]);
    return formatter.FormatPathResult(result);
}

string HandleCluster(string[] argv)
{
    if (argv.Length < 2)
    {
        return "Usage: cluster <concept>";
    }

    var concept = string.Join(" ", argv.Skip(1));
    var result = clusterLookup.LookupByConcept(graph, concept);
    return formatter.FormatClusterResult(result);
}

void WriteExamples()
{
    var examples = new List<(string Query, string Output)>
    {
        ("resolve E-001", formatter.FormatResolveResult(engramLookup.ResolveById(graph, "E-001"))),
        ("concept Engram", formatter.FormatConceptResult(engramLookup.ResolveByConcept(graph, "Engram"))),
        ("path SLI GEL", formatter.FormatPathResult(pathFinder.FindShortestPath(graph, "SLI", "GEL"))),
        ("path CME Engram", formatter.FormatPathResult(pathFinder.FindShortestPath(graph, "CME", "Engram"))),
        ("cluster IdentityContinuity", formatter.FormatClusterResult(clusterLookup.LookupByConcept(graph, "IdentityContinuity")))
    };

    var markdown = formatter.FormatExamplesMarkdown(examples);
    File.WriteAllText(examplesPath, markdown);
}

static void PrintUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project tools/EngramResolver resolve <engram_id>");
    Console.WriteLine("  dotnet run --project tools/EngramResolver concept <concept_name>");
    Console.WriteLine("  dotnet run --project tools/EngramResolver path <conceptA> <conceptB>");
    Console.WriteLine("  dotnet run --project tools/EngramResolver cluster <concept>");
}
