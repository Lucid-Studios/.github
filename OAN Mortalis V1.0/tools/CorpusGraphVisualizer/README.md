# Corpus Engram Graph Visualizer

Builds structural topology views from derived corpus index artifacts.

## Input

- `corpus_index/engram_index.json`

## Output

- `corpus_index/engram_graph.graphml`
- `corpus_index/engram_graph.svg`

## Run

```powershell
dotnet run --project tools/CorpusGraphVisualizer
```

The tool does not read `OAN_REFERENCE_CORPUS`; it operates only on derived index files.
