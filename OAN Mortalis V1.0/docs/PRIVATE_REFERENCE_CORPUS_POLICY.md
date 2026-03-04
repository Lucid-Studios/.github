# Private Reference Corpus Policy

## Scope

This repository may use a private local reference corpus for architecture reasoning through the `OAN_REFERENCE_CORPUS` environment variable.

The corpus is referenced as:

`Lucid Research Corpus`

The resolved filesystem location is confidential.

## Hard Rules

1. Do not print the resolved corpus path in terminal output.
2. Do not commit the resolved corpus path into source, docs, tests, or configs.
3. Do not embed the resolved corpus path in runtime code.
4. Do not write the resolved corpus path to logs.
5. Do not quote large passages of corpus documents verbatim.

## Allowed Usage

1. Read corpus files as reference-only material.
2. Derive architecture and interface patterns.
3. Cite source concepts with abstract labels only.

## Citation Format

When a citation is needed, use:

`Reference: Lucid Research Corpus — <document/topic label>`

Never include absolute or relative corpus filesystem locations in citations.

## Portability Constraint

Build outputs and repository artifacts must remain independent of private local paths.

## Verification

Use:

`powershell -ExecutionPolicy Bypass -File tools/verify-private-corpus.ps1`

This check fails if tracked files contain the resolved `OAN_REFERENCE_CORPUS` path.
