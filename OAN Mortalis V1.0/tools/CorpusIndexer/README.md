# Corpus Engram Indexer

Tooling pipeline that derives a symbolic concept topology from the Lucid Research Corpus.

## Run

```powershell
dotnet run --project tools/CorpusIndexer
```

## Input

- Environment variable: `OAN_REFERENCE_CORPUS`
- Corpus is treated as read-only.

## Output

- `corpus_index/engram_nodes.lisp`
- `corpus_index/engram_links.lisp`
- `corpus_index/engram_index.json`

Outputs contain derived symbolic structures only: concept identifiers, relations, and document hashes.
