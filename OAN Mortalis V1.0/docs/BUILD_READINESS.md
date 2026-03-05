# BUILD_READINESS

## Purpose

This document formalizes the path from the current `OAN Mortalis V1.0` solution state to a mature, repeatable, compile-ready and release-ready build posture.

It is intended to answer three questions:

1. Is the active build currently compile-ready?
2. What must be true for the project to be considered build-complete?
3. How should the solution structure mature to support stable development and operation?

## Scope

Active build:

- `D:\OAN Tech Stack\OAN Mortalis V1.0\Oan.sln`

Out of scope:

- `D:\OAN Tech Stack\OAN Mortalis V0.1 Archive`

Assessment date:

- March 5, 2026

## Current Verified State

The active solution is currently buildable on the local Windows environment using `.NET SDK 8.0.418`.

Verified commands:

```powershell
dotnet build Oan.sln -v minimal
dotnet test Oan.sln -v minimal --no-build
```

Verified result:

- Build succeeded
- `0` warnings
- `0` errors
- `26` tests passed across `5` discovered test assemblies

Observed repository conditions:

- The active solution currently contains `29` source projects
- The repository currently contains `6` test projects
- No GitHub Actions workflow exists under `.github/workflows`
- `build_error.txt` reflects an older failure and is not current truth
- `.THIS_IS_THE_ACTIVE_BUILD` references `docs/WORKSPACE_RULES.md`, but that file is not present
- Foundational research documents may be indexed from a local private corpus root outside the repository, but that path must remain local-only and never be committed into tracked files

## Ready State

### Compile-Ready

The active build is currently **compile-ready**.

Definition:

- A clean local restore, build, and test pass succeeds from the active solution directory
- Required SDK is available
- Project references resolve without manual edits

Current status:

- **Met locally**

### Build-Complete

The active build is **not yet build-complete**.

Definition:

- Compile-ready state is reproducible on a clean machine
- SDK and toolchain are pinned
- CI reproduces restore, build, and test results
- build metadata, status files, and active-build documentation are current
- runtime entrypoints and operating instructions are documented

Current status:

- **Partially met**

## Build Completion Criteria

The project should be considered build-complete only when all items below are true.

### 1. Environment Reproducibility

- `global.json` pins the required .NET SDK
- package restore works from a clean checkout
- no manual machine-specific path edits are required
- local build scripts exist for Windows-first execution

### 2. Canonical Build Commands

The repo should expose a single approved command surface:

```powershell
dotnet restore Oan.sln
dotnet build Oan.sln -c Release
dotnet test Oan.sln -c Release --no-build
```

Recommended wrappers:

- `build.ps1`
- `test.ps1`

### 3. Build Metadata Integrity

- stale artifacts such as old error logs are removed, regenerated, or clearly marked historical
- `.THIS_IS_THE_ACTIVE_BUILD` points only to files that exist
- active build rules are documented in the current `docs` tree
- private corpus roots and other out-of-scope absolute paths are kept in ignored local configuration only

### 4. CI Verification

- a GitHub Actions workflow builds the active solution
- CI publishes test results and fails on restore, compile, or test regressions
- CI uses the same SDK version as local development

### 5. Runtime Readiness

- one canonical host startup path is documented
- required environment variables and storage locations are documented
- runtime boot, shutdown, and failure states are defined

### 6. Governance Readiness

- architectural boundaries are documented and enforced
- release artifacts are versioned
- experimental projects are clearly separated from production runtime projects
- tracked managed files do not reveal absolute paths outside `D:\OAN Tech Stack`

### 7. Foundational Corpus Safety

- foundational documents may be sourced from a local external corpus root
- the local root is configured by `OAN_REFERENCE_CORPUS` or ignored local config
- tracked files must reference the corpus only by logical identifier, never by absolute path
- leak scanning is part of build hygiene before publish or commit

## Proposed Maturity Model

### Stage 1. Buildable

Meaning:

- the solution restores, compiles, and tests locally

Current status:

- **Achieved**

Primary risk:

- local success is not yet backed by pinned tooling or CI

### Stage 2. Composable

Meaning:

- projects have clear ownership, stable roles, and controlled dependency flow

Current status:

- **In progress**

Primary risk:

- the solution appears conceptually rich but assembly boundaries are still broad and overlapping

### Stage 3. Operable

Meaning:

- the system can be launched, configured, observed, and shut down without source modification

Current status:

- **Partial**

Primary risk:

- runtime documentation exists, but canonical execution paths are not yet formalized as build gates

### Stage 4. Governed

Meaning:

- build, architecture, and release checks are automated

Current status:

- **Not achieved**

Primary risk:

- no CI workflow currently enforces the verified local build state

### Stage 5. Mature

Meaning:

- the platform is reproducible, understandable, versionable, and supportable

Current status:

- **Not achieved**

## Recommended Structural Form

The solution appears best suited to a five-layer architecture.

### Layer 1. Core Contracts

Likely projects:

- `Oan.Common`
- `OAN.Core`

Role:

- deterministic primitives
- shared value contracts
- common abstractions

Rule:

- must not depend on runtime or host assemblies

### Layer 2. Cognition and Identity

Likely projects:

- `AgentiCore`
- `AgentiCore.Runtime`
- `Oan.AgentiCore`
- `Oan.SoulFrame`
- `SoulFrame.Identity`
- `SoulFrame.Host`

Role:

- identity continuity
- authority state
- cognition control surfaces

Rule:

- depends on core contracts, not host adapters

### Layer 3. Symbolic Engine

Likely projects:

- `Oan.Sli`
- `SLI.Engine`
- `SLI.Ingestion`
- `SLI.Lisp`
- `GEL`

Role:

- routing
- symbolic evaluation
- ingestion and transformation

Rule:

- symbolic engine contracts should remain deterministic and testable in isolation

### Layer 4. Runtime and Host

Likely projects:

- `Oan.Cradle`
- `Oan.Runtime.Headless`
- `CradleTek.Host`
- `CradleTek.Runtime`
- `CradleTek.CognitionHost`
- `CradleTek.Mantle`

Role:

- startup
- lifecycle management
- orchestration
- hosting and runtime state

Rule:

- composition root lives here

### Layer 5. Adapters, Storage, and Tools

Likely projects:

- `CradleTek.Public`
- `CradleTek.Cryptic`
- `CradleTek.Memory`
- `Data.Cryptic`
- `Oan.Storage`
- `Oan.Place`
- `Telemetry.GEL`
- `EngramGovernance`
- `Oan.Fgs`
- tool projects under `tools`

Role:

- storage
- telemetry
- governance utilities
- corpus and developer tooling

Rule:

- avoid placing core business contracts in tool assemblies

## Development Shape Needed For Maturity

### Standardize Naming

Current project naming mixes:

- `Oan.*`
- `OAN.*`
- `CradleTek.*`
- `SoulFrame.*`
- `SLI.*`

Recommended outcome:

- define a namespace and assembly naming policy
- reserve brand names for product surfaces
- reserve technical names for internal layering

### Reduce Overlap

Some project families appear to overlap in intent.

Recommended outcome:

- classify every project as one of:
  - core library
  - runtime library
  - host
  - adapter
  - tool
  - test
  - experimental

### Enforce Dependency Direction

Recommended rule:

- dependencies flow inward to core contracts and upward only through composition at the host layer

Avoid:

- host layers defining shared contracts
- tools acting as infrastructure dependencies for runtime code
- storage assemblies owning cognition logic

### Formalize Canonical Entrypoints

Recommended outcome:

- one canonical solution
- one canonical host startup path
- one canonical integration test harness

## Roadmap To Build Completion

### Phase 1. Build Hygiene

- add `global.json`
- add root `build.ps1`
- add root `test.ps1`
- refresh or remove stale build result files
- repair `.THIS_IS_THE_ACTIVE_BUILD` references
- enforce local-only handling for foundational corpus roots and external path leak checks

Exit:

- clean machine can build and test without ambiguity

### Phase 2. Solution Classification

- inventory all source and test projects
- classify each project role
- identify duplicate or overlapping project responsibilities
- document allowed dependency directions

Exit:

- solution graph is understandable and defensible

### Phase 3. Runtime Readiness

- document canonical startup path
- document runtime configuration model
- add startup validation and health checks
- verify runtime integration tests against expected host composition

Exit:

- compile success maps to usable runtime behavior

### Phase 4. CI and Release

- create `.github/workflows/build.yml`
- run restore, build, and test in CI
- publish test artifacts
- add release build configuration

Exit:

- build readiness is machine-verified, not manually asserted

### Phase 5. Architectural Maturity

- trim redundant assemblies
- stabilize public contracts
- separate production, experimental, and tooling concerns
- define release policy and support expectations

Exit:

- the platform is maintainable under continued growth

## Immediate Next Actions

The next practical steps are:

1. Add SDK pinning with `global.json`
2. Add CI for restore, build, and test
3. Correct active-build documentation references
4. Replace stale failure logs with current build status artifacts
5. Create a project classification matrix for all solution projects

## Decision Summary

As of March 5, 2026, the active `OAN Mortalis V1.0` solution is:

- **Compile-ready**
- **Locally test-green**
- **Not yet build-complete**
- **Not yet release-mature**

The primary work remaining is not basic compilation. It is reproducibility, governance, project boundary discipline, and operational standardization.
