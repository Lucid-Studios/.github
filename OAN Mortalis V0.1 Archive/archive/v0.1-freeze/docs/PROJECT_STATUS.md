# OAN Mortalis Headless Spine: Project Status

## Purpose
- **Headless Runtime**: A standalone .NET 8 runtime for the OAN Mortalis governance and identity systems, decoupled from Unity.
- **Deterministic Core**: Provides a verifiable, ledger-driven state machine for agent interactions and world state evolution.
- **Governance Enforcer**: Strictly enforces structure, resource limits, and anti-swarm invariants via the SoulFrame session layer.
- **Identity Kernel**: Manages AgentiCore identities (Engrams) and their ephemeral projections (Profiles) securely.

## Architectural Spine
- **CradleTek** (authority host): The authoritative host and lifecycle manager. Handles module registration, startup/shutdown, and session orchestration (Closeout).
- **SoulFrame** (session channel): The session channel and governance layer. Enforces "Anti-Swarm" (one active agent), Cryptic Masking, and manages the Atlas/SLI surface.
- **AgentiCore** (identity kernel): The identity kernel. Manages immutable Engrams, mutable Profiles, and Energy checks.
- **Place Modules**: Feature sets defined as "Places".
    - **Self**: Offline, DLL-embedded logic (e.g., local physics or math).
    - **Service**: Connected, API-driven logic (e.g., external economy or database).

## Hard Invariants
- **EvaluateIntent read-only; CommitIntent is only mutation**: All evaluation logic is strictly non-mutating. State changes only occur during commit.
- **Ledger-driven state projection (no public mutation)**: World and Session state are purely projections of the event log.
- **Anti-swarm: one active AgentiCore per SoulFrame at a time**: Strict singular activation of agent profiles within a given session tick.
- **Deterministic time (no DateTime.Now in state transitions)**: All logic depends on the injected `WorldTick` or event timestamps, never wall-clock time.
- **Refusals always include reasonCode + policyVersion**: Every rejection must be auditable and traceable to a specific policy version.
- **Closeout lifecycle (Quiesce→Seal→Fold→Clear) must be auditable**: The session termination sequence is atomic and deterministically verifiable via ledger hashes.
- **Manifest Compliance**: All milestones must adhere to [Corridor Manifest](corridor_manifest.json).

## Repo Layout (Current)
```
src/
├── Oan.Core/           # Core domain types (Entity, Intent, Events, WorldState)
├── Oan.AgentiCore/     # Identity subsystem (Engram, Profile)
├── Oan.SoulFrame/      # Session & Governance subsystem (Session, Anti-Swarm, Atlas)
├── Oan.Runtime/        # Execution logic (IntentProcessor)
├── Oan.Ledger/         # Event log & append-only storage
├── Oan.CradleTek/      # Host orchestration & Registry
├── Oan.Place/          # Module abstractions & Implementations (GEL, GOA, OAN)
├── Oan.Host.Api/       # ASP.NET Core API entry point
└── Oan.Host.Cli/       # Command-line interface entry point

tests/
└── Oan.Tests/          # xUnit test suite (Unit, Governance, Architecture, Closeout)

docs/                   # Project Status, Manifest, Walkthroughs, Build Logs
artifacts/              # Git-ignored build/test/audit logs (Do not commit)
```

## Build & Run
**Build:**
`dotnet build`

**Test:**
`dotnet test`

**Run CLI:**
`dotnet run --project src/Oan.Host.Cli -- [command]`
Commands: 
- `run` (Interactive mode)
- `test` (Run a test intent scenario)
- `activate <agentId>` (Activate an agent profile)
- `closeout` (Seal the current session)

**Run API:**
`dotnet run --project src/Oan.Host.Api`
Endpoints:
- `GET /v1/snapshot`
- `POST /v1/soulframe/{id}/activate-agent`
- `POST /v1/soulframe/{id}/closeout`
- `POST /v1/intent/evaluate`
- `POST /v1/intent/commit`

Environment Variables: None currently required (defaults used).

> **Log Hygiene**: Do not place logs in repo root; use `/artifacts/*`.

## Current Milestone State
- **Implemented & Verified**:
    - Core Architecture (Triad Spine: CradleTek, SoulFrame, AgentiCore).
    - Intent Processing Pipeline (Evaluate -> Commit logic).
    - Strict Governance Guardrails (Anti-Swarm, Cooldowns, Strict Build Props).
    - CME Closeout Orchestration (Quiesce/Seal/Fold/Clear logic with event ledger).
    - Basic CLI and API surfaces.
    - 29 Passing Tests covering Unit, Governance, Architecture, Closeout, and Meaning Lattice verification (`Oan.Tests`).
- **Stubbed / Prototype**:
    - **Persistence**: EventLog is in-memory only. Ledger lost on restart.
    - **Place Modules**: GEL/GOA/OAN modules exist as skeletal structures but have no business logic implementation.
    - **Atlas/SLI**: Basic structure exists (`LexicalLookup`, `Morpheme`), but linguistic parsing/masking logic is minimal.

## Next Milestones (3)

### Milestone 1: Persistence & State Recovery
- [ ] Implement file-system or SQLite backing for `EventLog`.
- [ ] Implement `Snapshot` serialization/deserialization for `WorldState` and `SoulFrameSession`.
- [ ] Implement `Hydrate` logic to restore state from Ledger/Snapshot on startup.

### Milestone 2: Place Module Logic (GEL/GOA)
- [ ] Implement GEL (Global Economic Layer) logic for transaction verification.
- [ ] Implement GOA (Goals/Objectives) logic for quest/objective tracking.
- [ ] Expose Module capabilities via `HostRegistry` to `IntentProcessor`.

### Milestone 3: Atlas & SLI Deepening
- [ ] Implement real `LexicalLookup` and `Morpheme` parsing.
- [ ] Enforce "Cryptic Masking" (intent action must match SLI handle).
- [ ] Expand `RootAtlas` data loader to consume actual lexicon data.

## Build Log History
- [2026-02-15 Meaning Lattice Implementation](build_logs/2026-02-15_meaning_lattice.md)
- [2026-02-15 Engram MVP Implementation](build_logs/2026-02-15_engram_mvp.md)
- [2026-02-15 Vervaeke Stance Integration](build_logs/2026-02-15_stance_integration.md)
- [2026-02-15 FormationContext v1](build_logs/2026-02-15_formation_context_v1.md)

> **Doc Update Rule**: Any milestone update must create a new dated file in `docs/build_logs/` and update `docs/walkthrough_latest.md`.

