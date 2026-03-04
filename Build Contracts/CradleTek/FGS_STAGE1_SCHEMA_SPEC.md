# FGS Stage 1 Technical Specification – Formalization
## Status: Stage 1 Draft / Formalization Phase

This document defines the core governance schemas for the Federated Governance Substrate (FGS), ensuring cross-Cradle interoperability and sovereign authority enforcement.

---

### 1. Predicate Taxonomy (Core 100)

Predicates are the atomic units of governance. They provide a deterministic framework for CradleTek without requiring high-level "agent" logic at the substrate level.

#### A. Canonical Structure
All predicates must follow this machine-readable tripartite structure:
- **Predicate ID**: `fgs.[scope].[category].[index]` (e.g., `fgs.core.lb.006`)
- **Display Code**: `[CATEGORY]-[INDEX]` (e.g., `LB-006`)
- **Name**: CamelCase descriptor (e.g., `PermitAction`)

#### B. Allocation & ID Ranges (Core vs. Extension)
1. **Core 100 (`fgs.core.*`)**: Indices 001-100. **Frozen semantics.** No silent redefinition; only Deprecate + Replace.
2. **Domain Extensions (`fgs.ext.*`)**: Indices 101+. Overlays for jurisdictions or specific research projects.

#### C. Initial Core Allocation
**Identity & Authority (ID-*)**
1.  `fgs.core.id.001`: `DeclareSovereignIdentity` - Root event for new identity creation.
2.  `fgs.core.id.002`: `AttestFederationLink` - Links identity across Cradles.
3.  `fgs.core.id.003`: `BindRole` - Assigns role within a local Cradle.
4.  `fgs.core.id.004`: `UnbindRole` - Terminating authority for a role.
5.  `fgs.core.id.005`: `IssueLocalAuthority` - Generates non-exportable local grant.
6.  `fgs.core.id.006`: `IssueCapabilityCredential` - Generates signed CEC (exportable evidence).

**Labor & Capability (LB-*)**
1.  `fgs.core.lb.001`: `ConscriptActivation` - Ephemeral, low-trust execution.
2.  `fgs.core.lb.002`: `ContractorEngagement` - Time-bound, scoped activation.
3.  `fgs.core.lb.003`: `FollowerIntegration` - Persistent activation.
4.  `fgs.core.lb.004`: `LegionOrchestration` - Multi-unit swarm coordination.
5.  `fgs.core.lb.005`: `CapabilityPromotion` - Tier elevation via ratification.
6.  `fgs.core.lb.006`: `PermitAction` - Explicit permission grant for a predicate.

**Semantic & Ontology (SM-*)**
1.  `fgs.core.sm.001`: `NormalizePrimitive` - Language-to-canonical mapping.
2.  `fgs.core.sm.002`: `ExtendOntology` - Domain-specific expansion.
3.  `fgs.core.sm.003`: `VersionAtlas` - Locking specific Language Atlas state.
4.  `fgs.core.sm.004`: `DeprecatePrimitive` - Formal removal from active set.

**Security & Enforcement (SC-*)**
1.  `fgs.core.sc.001`: `InitiateQuarantine` - Isolates a node/identity.
2.  `fgs.core.sc.002`: `FilterEgress` - Applying CA policies to outgoing data.
3.  `fgs.core.sc.003`: `InterceptRequest` - PEM-level interdiction.
4.  `fgs.core.sc.004`: `DeclareEmergency` - Dual Sovereign Ratification mode.
5.  `fgs.core.sc.005`: `RollbackState` - forensic recovery to last sealed state.

**Vitality & Telemetry (VT-*)**
1.  `fgs.core.vt.001`: `FlagLoop` - Detection of circular dependencies.
2.  `fgs.core.vt.002`: `LogDrift` - Identifying behavioral drift.
3.  `fgs.core.vt.003`: `ReportResourceAnomaly` - Compute/memory flags.
4.  `fgs.core.vt.004`: `AuditReset` - Logging system restart.

---

### 2. Capability Evidence Credential (CEC)

The CEC is a JSON-LD object signed by a Cradle's **Constraint Authority (CA)**. It serves as **evidence for evaluation**, not as authority itself.

```json
{
  "@context": [
    "https://www.w3.org/2018/credentials/v1",
    "https://cradletek.io/contexts/fgs/v1"
  ],
  "id": "urn:uuid:fgs-cec-123",
  "type": ["VerifiableCredential", "CapabilityEvidenceCredential"],
  "issuer": "did:fgs:[issuing_cradle]:ca",
  "issuanceDate": "2026-02-21T12:00:00Z",
  "credentialSubject": {
    "id": "did:fgs:[actor_identity]",
    "capability": {
      "predicate": "fgs.core.lb.006",
      "scope": "ExperimentalLab",
      "constraints": {
        "maxDuration": "3600",
        "hitlRequired": true,
        "non_transferable_authority": true,
        "requires_rebinding": true
      }
    },
    "locality_clause": {
      "issuing_cradle": "did:fgs:[cradle_A]",
      "valid_audience": ["did:fgs:[cradle_B]", "did:fgs:[cradle_C]"]
    }
  },
  "proof": {
    "type": "Ed25519Signature2020",
    "proofPurpose": "assertionMethod",
    "verificationMethod": "did:fgs:[issuing_cradle]:ca#key-1",
    "jws": "..."
  }
}
```

---

### 3. Role Rebinding Protocol (RRP)

The RRP ensures that while Identity history travels, Authority is always re-issued locally.

#### Phase 1: Identity Attestation (PEM)
- Identity `I` arrives at Cradle `B` with CEC `C` from Cradle `A`.
- `B.PEM` validates `C`'s signature and locality clause.

#### Phase 2: Role Proposal (IA)
- `I` requests `Role: Researcher` in `B`.
- `B.IA` (Integrator) evaluates `I`'s history and potential for promotion.

#### Phase 3: Authority Issuance (CA)
- If approved, `B.CA` issues a **Local Authority Token** (non-exportable).
- `B` appends the authoritative `fgs.core.id.003` (BindRole) event to the local ledger.

#### Phase 4: Verification Sync (Federation)
- `B` issues a signed **RebindReceipt**.
- `A` appends a `RebindReceiptObserved` event referencing `B`'s signed receipt.
- This ensures eventual consistency with signed proof of transit.

---

### 4. Constitutional Evaluation Pipeline

The order of execution for a governance request is a frozen invariant:

1. **SIL.Normalize(request)**: Transform freeform input → Canonical Intent Object.
2. **PEM.ValidateIngress(request)**: Zero-trust perimeter check.
3. **CA.PolicyEvaluate(intent)**: Core decision engine (using CECs + local policy).
4. **IA.RatifyLifecycle(intent)**: Growth/promotion approval (if required).
5. **EV.VerifyTransition()**: Dry-run verification of state change.
6. **IIM.AppendEvent()**: Commit to the immutable ledger.
7. **Telemetry.Log()**: Non-governing audit record.
