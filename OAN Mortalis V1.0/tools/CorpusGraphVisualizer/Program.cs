using CorpusGraphVisualizer;

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var corpusIndexDir = Path.Combine(root, "corpus_index");
var engramIndexPath = Path.Combine(corpusIndexDir, "engram_index.json");

if (!File.Exists(engramIndexPath))
{
    Console.Error.WriteLine("Missing corpus_index/engram_index.json. Run CorpusIndexer first.");
    return 1;
}

var loader = new GraphLoader();
var builder = new GraphBuilder();
var metricsAnalyzer = new MetricsAnalyzer();
var layoutEngine = new LayoutEngine();
var exporter = new GraphExporter();

var (nodes, edges) = loader.Load(engramIndexPath);
var graph = builder.Build(nodes, edges);
var metrics = metricsAnalyzer.Analyze(graph);
var positioned = layoutEngine.ComputeLayout(graph);

var graphMlPath = Path.Combine(corpusIndexDir, "engram_graph.graphml");
var svgPath = Path.Combine(corpusIndexDir, "engram_graph.svg");
exporter.ExportGraphMl(graphMlPath, graph);
exporter.ExportSvg(svgPath, graph, positioned);

Console.WriteLine($"Graph nodes: {graph.Nodes.Count}");
Console.WriteLine($"Graph edges: {graph.Edges.Count}");
Console.WriteLine($"Connected components: {metrics.ConnectedComponents.Count}");
Console.WriteLine($"Hub nodes (top {metrics.HubNodes.Count}): {string.Join(", ", metrics.HubNodes)}");
Console.WriteLine($"Bridge nodes: {metrics.BridgeNodes.Count}");
Console.WriteLine("GraphML file generated");
Console.WriteLine("SVG visualization generated");

return 0;
