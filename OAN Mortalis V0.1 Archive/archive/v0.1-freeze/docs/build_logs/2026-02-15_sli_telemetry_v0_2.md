# Build Log: SLI MVP v0.2 — Telemetry & Progress Instrumentation
**Date:** 2026-02-15
**Goal:** Add deterministic, append-only telemetry logging around the SLI Gate and session mounts to measure progress and guide development.

## Summary
Successfully implemented `SliTelemetryRecord` and a manual NDJSON telemetry sink. Instrumentation added to `SliGateService` captures all intermediate decision factors without altering runtime behavior. Verified via CLI scenario runners and golden sentinel tests.

## Key Changes
1.  **SliTelemetryRecord (Oan.Core):**
    -   Defines a deterministic container for SLI decisions.
    -   Includes logical identifiers (`RunId`, `Tick`) and intermediate gate state.
2.  **Telemetry Sinks (Oan.SoulFrame):**
    -   Added `ISliTelemetrySink` and its manual NDJSON formatter `FileSliTelemetrySink`.
    -   Ensured strict field ordering and `InvariantCulture` for stability.
    -   Added `NullSliTelemetrySink` as a default.
3.  **Gate Instrumentation (Oan.SoulFrame):**
    -   `SliGateService.Resolve` now accepts an optional `runId`.
    -   Emits a telemetry record after every resolution, capturing `PartitionMounted`, `SatSatisfied`, `CrypticRequested`, and `MaskingApplied` booleans.
4.  **CLI Scenario Runners (Oan.Host.Cli):**
    -   Added `sli telemetry <scenario>` command.
    -   Implemented 3 deterministic scenarios: `baseline_allow_move`, `deny_unmounted_partition`, `deny_sat_insufficient_private_or_cryptic`.
5.  **Golden Sentinel Tests (Oan.Tests):**
    -   Added `SliTelemetrySentinelTests.cs` to lock in the NDJSON formatting via exact hash match.

## Field List (SliTelemetryRecord)
| Field | Type | Description |
|---|---|---|
| `RunId` | `string` | SHA256 deterministic ID |
| `Tick` | `long` | Session tick |
| `ActiveSatMode` | `string` | Current SAT mode |
| `MountedPartitions` | `string[]` | Ordinal-sorted mounted partitions |
| `ResolvedAddress` | `string` | `{Visibility}/{Domain}/{Partition}` |
| `Allowed` | `bool` | Final gate decision |
| `ReasonCode` | `string` | Stable error code if denied |

## Example Log Line (NDJSON)
```json
{"RunId":"767c9c0b0213d2f281e80b853909796ea8434720937a09289260c670a4a82110","Tick":0,"SessionId":"telemetry-session","OperatorId":"telemetry-op","ActiveSatMode":"Baseline","MountedPartitions":["OAN"],"RequestedHandle":"public/oan/move.commit","RequestedKind":"MoveTo","ResolvedAddress":"Public/OAN/Standard","PartitionMounted":true,"SatSatisfied":true,"CrypticRequested":false,"MaskingApplied":false,"Allowed":true,"ReasonCode":"OK","PolicyVersion":"sli.policy.v0.1","Notes":null}
```

## CLI Usage
```powershell
dotnet run --project src/Oan.Host.Cli -- sli telemetry baseline_allow_move
```

## Verification Results
-   **Tests Passed:** 12/12 in `Oan.Tests.Llm` and `Oan.Tests.SLI`.
    -   `SliTelemetrySentinelTests`: Passed (Hash match confirmed).
    -   `SliTelemetryTests`: Passed (Deterministic ordering confirmed).
