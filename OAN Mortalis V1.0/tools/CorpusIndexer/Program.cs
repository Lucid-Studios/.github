using CorpusIndexer;

var corpusRoot = Environment.GetEnvironmentVariable("OAN_REFERENCE_CORPUS");
if (string.IsNullOrWhiteSpace(corpusRoot))
{
    Console.Error.WriteLine("OAN_REFERENCE_CORPUS is not set. Configure it and rerun.");
    return 1;
}

if (!Directory.Exists(corpusRoot))
{
    Console.Error.WriteLine("OAN_REFERENCE_CORPUS does not resolve to a readable directory.");
    return 1;
}

var scanner = new CorpusScanner();
var extractor = new ConceptExtractor();
var builder = new EngramBuilder();
var linker = new GraphLinker();
var writer = new IndexWriter();

var documents = scanner.Scan(corpusRoot);
var hits = extractor.Extract(documents);
var nodes = builder.Build(hits);
var links = linker.Link(nodes);

var outputDirectory = Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "corpus_index"));

writer.Write(outputDirectory, nodes, links);

Console.WriteLine("Corpus Engram Index generated from Lucid Research Corpus.");
Console.WriteLine($"Documents scanned: {documents.Count}");
Console.WriteLine($"Engram nodes: {nodes.Count}");
Console.WriteLine($"Engram links: {links.Count}");
Console.WriteLine("Artifacts: corpus_index/engram_nodes.lisp, corpus_index/engram_links.lisp, corpus_index/engram_index.json");
return 0;
