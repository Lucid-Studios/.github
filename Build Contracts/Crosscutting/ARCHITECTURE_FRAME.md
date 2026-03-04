# Architecture Frame: OAN Mortalis v1.0

## Spine Architecture
The v1.0 solution is a "Discipline-First" reconstruction focused on a deterministic engram spine.

### Projects
- **Oan.Spinal**: The "base" layer. Contains deterministic primitives, engram definitions, and store interfaces. No project references.
- **Oan.Sli**: The "control" layer. Contains engrams validation, routing (Public/Cryptic/SpineNative), and authoritative enums.
- **Oan.Cradle**: Host orchestration and registry.
- **Oan.SoulFrame**: Session management and governance.
- **Oan.AgentiCore**: Identity and Engram management.
- **Oan.Place**: Module boundaries for external logic.
- **Oan.Storage**: Persistence implementations (NDJSON).
- **Oan.Runtime.Headless**: The single composition root.

## Hard Rules
- **No Unity**: Strictly net8.0 console/library.
- **No Globals**: All components injected or passed.
- **No Duplicate Enums**: Enums live in Sli/Spinal.
- **Deterministic Time**: Tick-based only.
