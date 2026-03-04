# Routing Constitution (v0.1a) — Phase D Specification

This document defines the rules for data movement, lineage preservation, and plane transitions within the OAN Mortalis stack.

---

## 1. Plane Invariants

### 1.1 The One-Way Valve
- **Standard -> Cryptic**: Data may flow freely but is **OBSERVATIONAL ONLY**. It does not alter Standard lineage state and cannot retroactively modify Standard history.
- **Cryptic -> Standard**: Data may ONLY flow via an authorized **PromotionReceipt**.

### 1.2 Lineage Continuity (TipHash / ParentTip)
- **Scoped Continuity**: Every event must reference the `ParentTip` (or `TipHash`) of the previous event *in its specific plane lineage*.
- **Independence**: GEL, GoA, cGoA, and IncidentLog lineages are strictly independent.
- **Cross-Plane Referencing**: Standard entries must NEVER inherit Cryptic `ParentTip` directly.
    - `StandardCommit.ParentTip` -> References previous Standard `TipHash` only.
    - `StandardCommit.Metadata` -> References `PromotionReceiptHash`.

---

## 2. Store Constitution (Naming Lock)

### 2.1 Public (Standard) Plane
- **GEL (Governance Event Ledger / Golden Engram Library)**: The authoritative trace of all high-integrity decisions and crystallized identity units.
- **GoA (Global of Action)**: The current public world state/context.

### 2.2 Cryptic Plane
- **cGoA (Cryptic Global of Action)**: Forensic-grade append-only storage for research and audit.
- **cVault**: Long-term cold storage for sensitive engrams.
- **IncidentLog**: Forensic channel active during Frozen states (INCIDENT-ONLY).

---

## 3. Promotion Graph (The 6-Phase Pipeline)

The high-level promotion of data follows the mechanical steps defined in the Engrammitization Constitution:

1.  **Phase 1-3: Intent & Packetization**: AgentiCore emits a `CrypticIntent`, which is packetized into an `SLIPacket`.
2.  **Phase 4: Gate Evaluation**: SoulFrame verifies the packet against current policy and `RootAtlas` registry.
3.  **Phase 5: Commit Crystallization**: If authorized, the intent is crystallized into GEL.
    - **Promotion Receipt**: Generation of a deterministic receipt binding `policyVersion`, `invokingHandle`, `SAT mode`, and `CasTip`.
    - **Standard Commit**: Data is committed as a `GoldenEngram` to `GEL`.
4.  **Phase 6: Runtime Recovery**: The Standard plane state (`GoA`) is updated or reconstructed from the new engram.

---

## 4. Telemetry Split & Non-Influence

### 4.1 Governance Telemetry (High-Integrity)
- Focus: Intent, Decision, Hashes, Lineage.
- Criticality: Failure triggers SoulFrame Freeze.
- **Admissibility**: This is the primary input for governance and routing decisions.

### 4.2 Research/Storage Telemetry (Diagnostic)
- Focus: Store operations, performance, offsets.
- **Observational Only**: Research telemetry is strictly diagnostic. 
- **Non-Influence**: It is NOT admissible input to governance decisions and must never influence routing logic or be read by AgentiCore.

---

## 5. SoulFrame Enforcement Hooks

SoulFrame must hook into the Routing layer at:
- **Write Authorization**: Is the current state permissive for this store?
- **Lineage Verification**: Verify plane-scoped `TipHash` continuity.
- **Promotion Authorization**: Validate `PromotionReceipt` against historical policy version.

---

## 6. Acceptance Criteria (Verification)
- [x] Standard -> Cryptic flow is observational only.
- [x] TipHash lineages are plane-scoped and independent.
- [x] Promotion receipts bind `policyVersion`, `handle`, and `SAT mode`.
- [x] Research telemetry is explicitly non-influential.
