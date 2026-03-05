# Git Workflow

## Branch Naming

Branches follow this structure:

codex/<component>-<feature>

Examples:

codex/hosted-llm-runtime
codex/soulframe-adapter
codex/cradletek-bootstrap
codex/sli-engine

Branch types:

feat/   new functionality
fix/    bug fixes
infra/  infrastructure/runtime changes
docs/   documentation updates
test/   test additions

## Commit Format

Use Conventional Commits:

<type>(<scope>): <summary>

Examples:

feat(hosted-llm): add mistral runtime
infra(runtime): compile optimized llama.cpp build
docs(workflow): add git workflow documentation

Commit body should include:

Context
Implementation
Testing
Impact

## Tagging Scheme

Tags represent architecture milestones.

Format:

cme-<stage>-<number>

Examples:

cme-seed-001
cme-runtime-online
cme-soulframe-integrated
cme-symbolic-engine-alpha

Tags must be created only from the main branch.
