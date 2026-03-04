# Audit Boundary Definition v0.1

This document defines the constitutional boundary between **governance logic** and **audit evidence** within the OAN Mortalis stack.

Audit artifacts are evidence.  
They are NOT law.  
They are NOT admissible inputs to governance decisions.

---

## 1. Definition (Audit Artifacts)

Audit artifacts include:

- `audit_results.txt`
- `audit_results_f1.txt`
- `*.binlog`
- `storage.ndjson` snapshots used for verification
- Replay identity comparison logs
- DeterminismAuditTests output
- Any exported trace or verification file

These artifacts are generated **after** governance decisions and are used for:

- Verification
- Regression detection
- Human review
- Research reporting

They are never used to decide allow/deny logic.

---

## 2. Governance vs Audit Boundary

### Governance Layer
Governance decisions may depend ONLY on:

- Canonical IR (LispForm)
- PolicyVersion
- InvokingHandle
- SAT mode
- Plane-scoped TipHash
- PromotionReceipt
- RecoveryReceipt
- Governance telemetry (hash-bound, admissible surface)

### Audit Layer
Audit artifacts may contain:

- Diff outputs
- Byte comparisons
- Test logs
- Performance metrics
- Harness traces
- Diagnostic state

But these are:

- Non-authoritative
- Non-admissible
- Non-binding

---

## 3. Hard Invariant: No Audit Feedback Loop

Governance logic MUST NOT:

- Read audit result files
- Parse build logs
- Depend on test results
- Depend on replay comparison artifacts
- Depend on performance metrics
- Depend on research telemetry

If governance requires information from an audit artifact,
that information must be:

1. Formalized as a constitutional field
2. Bound via canonical IR
3. Hash-addressed
4. Versioned
5. Approved through constitutional revision

No shortcuts.

---

## 4. Storage Separation Rule

Audit artifacts must:

- Live outside governance stores (GEL, GoA, cGoA)
- Not be mounted as partitions
- Not be included in routing resolution
- Not be referenced by PromotionReceipt
- Not be hashed into governance receipt chains

They are external evidence only.

---

## 5. Determinism Protection Clause

Replay identity tests are required to prove determinism.

However:

- The existence or failure of a replay test must not change runtime behavior.
- Runtime must remain deterministic regardless of audit environment.

If a determinism failure is detected:
- SoulFrame transitions state (Frozen or Halt) based on constitutional rules.
- It does not inspect the audit file to decide.

---

## 6. Research Visibility Clarification

Audit artifacts are:

- Research-visible
- Project-accessible
- Never Public-admissible
- Never CME personification inputs

They are observational surfaces only.

---

## 7. Constitutional Status

This document is subordinate to:

- [SPINE_CONSTITUTION.md](../SPINE_CONSTITUTION.md)
- [AUTHORITY_CONTRACTS.md](../AUTHORITY_CONTRACTS.md)

If an audit artifact is ever used as a governance input,
the system is in constitutional violation and must fail closed.
