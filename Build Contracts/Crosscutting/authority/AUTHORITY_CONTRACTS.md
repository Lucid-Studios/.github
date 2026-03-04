# Authority Contracts (v0.1a) — Phase D0 Specification

This document defines the authoritative contracts for `SoulFrame` and `AgentiCore` to ensure enforceable plane boundaries and deterministic governance.

---

## 1. Telemetry Levels (Explicit)

- **ALLOW**: Full governance telemetry + required storage telemetry + deterministic receipt records.
- **LIMITED**: State transitions + invariant violations + receipt hashes only (no payload detail).
- **MINIMAL**: Heartbeat + current state only.

## 2. State Allow/Deny Matrix (v0.1a)

| Mode / Capability | Evaluate Commits | Governance Telemetry | Cryptic Writes | Public Writes | Promotion Ops |
| :--- | :---: | :---: | :---: | :---: | :---: |
| **Operational** | ALLOW | ALLOW | ALLOW | ALLOW | ALLOW |
| **Frozen** | BLOCK | ALLOW | **INCIDENT-ONLY** | BLOCK | BLOCK |
| **Quarantined** | BLOCK | LIMITED | BLOCK | BLOCK | BLOCK |
| **Halt** | BLOCK | MINIMAL | BLOCK | BLOCK | BLOCK |

---

## 3. Forensic Freeze Definition (C2)

Under **Frozen**:
- Evaluation commits are blocked.
- Public plane writes are blocked.
- Promotion operations are blocked.
- Governance telemetry continues.
- **Cryptic writes are allowed ONLY to**: `cVault/IncidentLog` or `cGoA/Incident`.

### Constraints on Incident Channel:
- Append-only, deterministic, and hash-bound.
- Explicitly labeled as `IncidentTelemetry`.
- Cannot reference Standard plane outputs.
- Cannot trigger routing or promotion.
- **Intent**: Forensic logging only. Prevents Frozen from becoming a shadow-compute mode.

---

## 4. Recovery Protocol (Explicit Authority)

**Frozen/Quarantined → Operational** requires a **RecoveryReceipt**:
- `policyVersion`
- `triggeringFailureHash`
- `invariantRecheckHash`
- `administrativeAuthorization`
- `deterministic receipt chain hash`

### Recovery Authority:
- May ONLY be initiated by `CradleTekHost` or an external administrative authority.
- **Never** by `AgentRuntime`, `AgentiCore`, or SLI handle invocation.
- There is no self-thaw path.

---

## 5. SoulFrame Authority Lock

SoulFrame is the final arbiter of:
- Mount legality (which partitions exist/active).
- Plane invariants and promotion legality.
- State transitions and enforcement choke points.
- **Constraint**: All store writes must pass through a single SoulFrame verification boundary.
- **Layer Disclosure Rule**: SoulFrame enforcement must remain Layer-2 ignorant. It verifies *that* a write is authorized for a GEL/GoA handle, but does not parse the salience or engineered cognition within the payload.

---

## 6. AgentiCore Boundary (Locked)

`AgentiCore`:
- Produces **inert intent DTOs**.
- Does not route, write stores, promote, or thaw.
- Cannot bypass SLI.
- All outputs must be explicitly tagged as `StandardIntent` or `CrypticIntent`.
- **Constraint**: Routing remains strictly outside AgentiCore.

---

## 7. Acceptance Criteria (Verification)
- [x] Every store write has a single enforcement choke-point with SoulFrame authority.
- [x] Plane boundary rules are enforceable without relying on "developer discipline."
- [x] State transitions have explicit allow/deny matrices using v0.1a definitions.
- [x] Forensic trails (C2) are preserved during Frozen states via INCIDENT-ONLY writes.
