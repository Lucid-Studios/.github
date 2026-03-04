# Engram Resolver

Symbolic resolver utility for navigating the derived Engram graph.

## Commands

- `resolve <engram_id>`
- `concept <concept_name>`
- `path <conceptA> <conceptB>`
- `cluster <concept>`

## Run

```powershell
dotnet run --project tools/EngramResolver resolve E-118
```

## Data Inputs

- `corpus_index/engram_index.json`
- `corpus_index/engram_graph.graphml`
- `corpus_index/graph_clusters.json`

The resolver never reads `OAN_REFERENCE_CORPUS`.
