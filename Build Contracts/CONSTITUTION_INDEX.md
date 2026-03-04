# Constitution Index v0.1 (CradleTek / OAN Mortalis)

This index defines the precedence order, cross-document invariants, conflict resolution rules, and registration requirements for all constitutions governing the OAN Mortalis stack.

**Purpose:** prevent drift.  
**Rule:** If it isn’t registered here, it is not constitutional.

---

## 0. Canonical Terms (Glossary Lock)

### Planes
- **Standard (Public plane):** Admissible, user-facing plane. Minimal disclosure. Pointer-forward by default.
- **Cryptic plane:** Pre-admissibility cognition plane. Research-visible material. Append-only, lineage-scoped.

### Core Constructs
- **Spine:** Sovereign deterministic governance substrate. First and last authority pre/post every stack run.
- **SoulFrame:** Final enforcement authority for state transitions and legality checks (under Spine authority).
- **AgentiCore:** Produces inert intents only. No routing. No storage writes. No promotion.
- **RoutingEngine:** Enforces plane-scoped lineage and promotion graph. Orchestrates writes under SoulFrame checks.
- **PromotionReceipt:** The only lawful cross-plane admissibility certificate (Cryptic -> Standard).
- **Duplexing:** Standard -> Cryptic consultation protocol. Pointer-only returns by default.

### Storage / Ledgers
- **GEL:** Governance Event Ledger (authoritative trace of high-integrity decisions).  
  (Distinct from any “Golden Engram Library” template source.)
- **GoA:** Global of Action (current Standard world/context).
- **cGoA:** Cryptic Global of Action (cryptic ledger for research/audit).
- **cVault:** Escrow / cold storage for sensitive cryptic artifacts.
- **IncidentLog:** Frozen-state forensic channel (incident-only).

---

## 0.1 The 3-Layer Stack Taxonomy

All OAN Mortalis primitives must be registered in one of three layers:

1.  **Layer 0 (Identity):** Crystallized identity storage (GEL). Inert database. Identity-bearing only.
2.  **Layer 1 (Symbolic):** Routing, tensorization, and governance (SLI). No identity anchors; no runtime cognition.
3.  **Layer 2 (Runtime):** Ephemeral engineered cognition (IUTT). Salience and perspective-shifted state.

**Rule:** Layer 2 state must never be stored in Layer 0 except via crystallization (6-Phase Pipeline).

---

## 1. Precedence Order (LOCKED: Highest to Lowest)

If two documents conflict, the higher-precedence constitution governs.

1) **WORKSPACE_RULES.md** (Project boundary + root-of-truth constraints)
2) **SPINE_CONSTITUTION.md** (Sovereign governance substrate; fail-closed authority)
3) **AUTHORITY_CONTRACTS.md** (SoulFrame/AgentiCore authority + C2 posture)
4) **ROUTING_CONSTITUTION.md** (Plane invariants + promotion graph + lineage rules)
5) **DUPLEXING_CONSTITUTION.md** (Pointer-only consult + deterministic harness rules)
6) **CRYPTIC_BLOOM_CONSTITUTION.md** (Cryptic-first emission + visibility tiers)
7) **PUBLIC_BRAIDING_CONSTITUTION.md** (IUTT gluing/braiding: functorial export to Standard)
8) Other registered policies (e.g., MIGRATION_POLICY.md)

**Interpretation Rule:** higher-precedence documents constrain lower-precedence ones; lower-precedence documents may specialize but never weaken invariants.

---

## 2. Conflict Resolution Rules (LOCKED)

When a conflict is detected:

1) **Do not improvise at runtime.**  
2) The system MUST **fail closed** via SoulFrame (Frozen or Halt depending on severity).
3) A constitutional revision is required.

### Conflict Severity → Safe-Fail Mapping
- **Policy ambiguity / missing authority binding:** Frozen
- **Lineage integrity violation / hash mismatch:** Frozen (promotion subsystem), or Halt if integrity compromise is global
- **Attempted cross-plane mutation without receipt:** Frozen + audit record
- **Determinism breach (non-replayable artifacts):** Halt

---

## 3. Registration Rule (Adding or Revising Constitutions)

A document is constitutional only if:

- It lives in `Build Contracts/` (root or sub-buckets)
- It is named `*_CONSTITUTION.md` or explicitly registered as a policy here
- It declares a version header (e.g., v0.1, v0.1a)
- It does not contradict higher-precedence constitutions

### Required front-matter section for new constitutions
Every new constitution MUST include:

- Scope
- Definitions
- Hard invariants
- Enforcement hooks
- Safe-fail mapping
- Acceptance criteria

---

---

## 4. Layer Boundary Interaction Rules (LOCKED)

1. **Layer 0 (GEL)**: Strictly inert. Must not contain logic, salience, or ephemeral context.
2. **Layer 1 (SLI)**: The Policy Membrane. Only Layer 1 may invoke Layer 0 commits or promote to Public.
3. **Layer 2 (IUTT)**: Ephemeral. Must remain blind to Layer 0/1 implementation details. Communicates via SLI Packet only.

---

## 5. The Crosscutting Bucket
The `Crosscutting/` directory contains foundation infrastructure used by all component buckets:
- **spine**: Deterministic kernel authority.
- **authority**: SoulFrame/AgentiCore boundary contracts.
- **routing**: Plane transition and promotion logic.
- **duplexing**: Standard-to-Cryptic consultation protocol.
- **migration**: Staged porting policies.

---

## 6. Canonical Link Policy (Portability)

All links MUST be relative paths.

✅ Use: `./ROUTING_CONSTITUTION.md`  
❌ Do not use: `file:///D:/...`

---

## 5. Constitution Catalog (Registered Documents)

### Root / Operational Constraints
- [WORKSPACE_RULES.md](./Crosscutting/WORKSPACE_RULES.md)
- [MIGRATION_POLICY.md](./Crosscutting/migration/MIGRATION_POLICY.md)
- [NAMING_CONVENTION.md](./Crosscutting/NAMING_CONVENTION.md)

### Sovereign Governance Layer (Highest Precedence)
- [SPINE_CONSTITUTION.md](./Crosscutting/spine/SPINE_CONSTITUTION.md)
- [AUTHORITY_CONTRACTS.md](./Crosscutting/authority/AUTHORITY_CONTRACTS.md)
- [Audit Boundary Definition](../OAN%20Mortalis%20V1.0/docs/audits/README.md)

### Plane Governance & Component Buckets
- [ROUTING_CONSTITUTION.md](./Crosscutting/routing/ROUTING_CONSTITUTION.md)
- [DUPLEXING_CONSTITUTION.md](./Crosscutting/duplexing/DUPLEXING_CONSTITUTION.md)
- [CRYPTIC_BLOOM_CONSTITUTION.md](./Telemetry/CRYPTIC_BLOOM_CONSTITUTION.md)
- [PUBLIC_BRAIDING_CONSTITUTION.md](./SoulFrame/PUBLIC_BRAIDING_CONSTITUTION.md)
- [ENGRAMMITIZATION_CONSTITUTION.md](./SLI/ENGRAMMITIZATION_CONSTITUTION.md)
- [CRADLETEK_FGS_CONSTITUTION.md](./CradleTek/CRADLETEK_FGS_CONSTITUTION.md)

---

## 6. Global Invariants (Cross-Cutting)

These must hold across the entire stack:

1) **Determinism:** same inputs → byte-identical artifacts.
2) **Append-only:** no overwrites; all mutations are new commits.
3) **Plane separation:** Cryptic cannot influence Standard except via PromotionReceipt.
4) **Pointer-only default:** Standard receives pointers unless promoted.
5) **Telemetry non-influence:** research telemetry is non-admissible to governance.
8) **6-Phase Engrammitization:** all Standard plane identity writes must follow the Intake -> SLI Packet -> Gate -> Commit pipeline.
9) **Layer Secrecy:** Layer 1 and Layer 0 must remain blind to Layer 2 salience/perspective.

---

## 7. Audit Attachments (Non-Constitutional Evidence)

Audit outputs are evidence, not law. Store them in root or `docs/audits/`:

- `audit_results.txt`
- `audit_results_f1.txt`
- build logs (`*.binlog`)

They must never become inputs to governance logic.
