# Corpus Graph Analysis

Structural analysis utility for the derived Engram graph.

## Input

- `corpus_index/engram_graph.graphml`
- `corpus_index/engram_index.json`

## Output

- `corpus_index/graph_analysis_report.md`
- `corpus_index/graph_backbone.graphml`
- `corpus_index/graph_clusters.json`

## Run

```powershell
dotnet run --project tools/CorpusGraphAnalysis
```

This tool does not access `OAN_REFERENCE_CORPUS`.
