# Meaning Lattice Walkthrough

This document outlines the implementation of the **Interactive Meaning Lattice**, a subsystem for `Oan.SoulFrame` that makes meaning addressable and governable *before* generation.

## 1. Core Models (`Oan.Core`)

We introduced the following foundational models in `Oan.Core.Meaning`:

*   **`MeaningSpan`**: Represents a segment of text with semantic attributes.
    *   `SpanId`: Stable hash of session, offset, and content.
    *   `Role`: Syntactic role (Goal, Constraint, etc.).
    *   `Status`: `Proposed`, `Confirmed`, `Edited`, `Rejected`.
    *   `AmbiguityScore`: Heuristic score (0.0 - 1.0).
*   **`FrameLock`**: Defines the "Anchored Context" for a session.
    *   `Goal`: The primary objective.
    *   `Mode`: `Clarify`, `Debate`, `Plan`, etc.
    *   `Constraints` & `Assumptions`: Lists of strings.
    *   `IsSet`: Boolean flag enforcing the lock.
*   **`RiskBandAssessment`**: A lightweight risk assessment.
    *   `Band`: `SAFE`, `AMBIGUOUS`, `HARD_STOP`.
*   **`DialecticTraceEvent`**: A generic event for meaning interactions (`DialecticEventType`).

## 2. Meaning Lattice Service (`Oan.SoulFrame`)

The `MeaningLatticeService` provides the core logic:

*   **Decoupled Design**: Uses an injected `Action<string, object, long>` for event appending, ensuring `Oan.SoulFrame` does not depend on `Oan.Ledger`.
*   **`ProposeSpans`**: Tokenizes natural language (whitespace/punctuation) and generates stable-ID spans.
*   **`UpdateSpan`**: Allows operators to confirm, reject, or edit spans (gloss).
*   **`SetFrameLock`**: Sets the FrameLock for the session, which is required for generation.
*   **`AssessRisk`**: Performs keyword-based heuristic risk assessment ("ambiguous", "forbidden").

## 3. Governance Gate (`FrameLockGate`)

*   **`FrameLockGate`**: A SoulFrame gate that checks `session.FrameLock.IsSet`.
    *   Returns `SOULFRAME.FRAMELOCK_REQUIRED` if the lock is not set, blocking AgentiCore generation.

## 4. API Endpoints (`Oan.Host.Api`)

New endpoints exposed under `/v1/soulframe/{sessionId}/meaning/`:

*   `POST spans/propose`: Propose spans from text.
*   `POST spans/update`: Update a span's status or gloss.
*   `POST framelock`: Set the valid frame lock.
*   `GET anchored`: Retrieve current FrameLock and confirmed spans.
*   `GET risk`: Perform and retrieve a risk assessment.

## 5. CLI Commands (`Oan.Host.Cli`)

The `meaning` subcommand enables interactive testing:

```bash
# Propose spans
dotnet run --project src/Oan.Host.Cli -- meaning propose --session cli-session --text "Manage the project."

# Update a span
dotnet run --project src/Oan.Host.Cli -- meaning update --session cli-session --span <ID> --status Confirmed

# Set FrameLock
dotnet run --project src/Oan.Host.Cli -- meaning framelock --session cli-session --goal "Project Management" --mode Plan

# Check Risk
dotnet run --project src/Oan.Host.Cli -- meaning risk --session cli-session
```

## 6. Verification

Unit tests in `Oan.Tests` cover:
*   **Determinism**: Ensuring `ProposeSpans` produces stable IDs.
*   **State Updates**: Verifying `UpdateSpan` updates session state correctly.
*   **Gate Logic**: Confirming `FrameLockGate` blocks when lock is unset and allows when set.
*   **Risk Assessment**: Verifying heuristic detection of risky terms.

### Test Results
All 29 tests passed (Exit Code 0).
*Verified via `dotnet test` on 2026-02-15.*
**Summary:** `Passed! - Failed: 0, Passed: 29, Skipped: 0, Total: 29`

## 7. Next Steps

*   Integrate `FrameLockGate` into the main `IntentProcessor` pipeline (currently implemented but not explicitly added to processor list in this task).
*   Enhance heuristics (tokenizer, risk keywords) with real models.
*   Implement `ClarifyInvited` event logic more fully in the session state.
