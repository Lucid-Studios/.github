using CorpusGraphAnalysis;

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var corpusIndexDir = Path.Combine(root, "corpus_index");
var graphMlPath = Path.Combine(corpusIndexDir, "engram_graph.graphml");
var engramIndexPath = Path.Combine(corpusIndexDir, "engram_index.json");

if (!File.Exists(graphMlPath))
{
    Console.Error.WriteLine("Missing corpus_index/engram_graph.graphml. Run CorpusGraphVisualizer first.");
    return 1;
}

var loader = new GraphLoader();
var centrality = new CentralityAnalyzer();
var articulation = new ArticulationAnalyzer();
var communitiesDetector = new CommunityDetector();
var paths = new PathExplorer();
var writer = new AnalysisReportWriter();

var graph = loader.LoadGraphMl(graphMlPath);
var metadata = loader.LoadMetadata(engramIndexPath);
Console.WriteLine("Graph loaded successfully");

var betweenness = centrality.ComputeBetweenness(graph);
var top20 = centrality.TopBetweenness(betweenness, 20);
Console.WriteLine("Betweenness centrality computed");

var articulationNodes = articulation.Detect(graph);
var connectedComponents = articulation.CountComponents(graph, excludedNode: null);
Console.WriteLine("Articulation nodes detected");

var communities = communitiesDetector.DetectLouvain(graph);
Console.WriteLine("Communities detected");

var conceptPaths = new List<ConceptPathResult>
{
    paths.FindShortestConceptPath(graph, "SLI", "GEL"),
    paths.FindShortestConceptPath(graph, "CME", "Cryptic"),
    paths.FindShortestConceptPath(graph, "IdentityContinuity", "SoulFrame"),
    paths.FindShortestConceptPath(graph, "AgentiCore", "Engram")
};
Console.WriteLine("Concept paths generated");

var topDegreeNodes = graph.Adjacency
    .OrderByDescending(kvp => kvp.Value.Count)
    .ThenBy(kvp => kvp.Key, StringComparer.Ordinal)
    .Take(20)
    .Select(kvp => kvp.Key)
    .ToHashSet(StringComparer.Ordinal);

var topBetweennessNodes = top20
    .Select(item => item.NodeId)
    .ToHashSet(StringComparer.Ordinal);

var articulationSet = articulationNodes
    .Select(a => a.NodeId)
    .ToHashSet(StringComparer.Ordinal);

var backboneNodes = topDegreeNodes
    .Union(topBetweennessNodes, StringComparer.Ordinal)
    .Union(articulationSet, StringComparer.Ordinal)
    .OrderBy(id => id, StringComparer.Ordinal)
    .ToList();

writer.WriteReport(
    reportPath: Path.Combine(corpusIndexDir, "graph_analysis_report.md"),
    graph: graph,
    metadata: metadata,
    connectedComponents: connectedComponents,
    topBetweenness: top20,
    articulations: articulationNodes,
    communities: communities,
    paths: conceptPaths);

writer.WriteBackboneGraphMl(
    path: Path.Combine(corpusIndexDir, "graph_backbone.graphml"),
    graph: graph,
    backboneNodes: backboneNodes);

writer.WriteClustersJson(
    path: Path.Combine(corpusIndexDir, "graph_clusters.json"),
    communities: communities);

Console.WriteLine("Analysis report written");
Console.WriteLine("Artifacts produced:");
Console.WriteLine(" - graph_analysis_report.md");
Console.WriteLine(" - graph_backbone.graphml");
Console.WriteLine(" - graph_clusters.json");

return 0;
