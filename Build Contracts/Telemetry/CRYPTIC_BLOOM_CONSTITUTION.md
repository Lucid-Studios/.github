# Cryptic Bloom Constitution v0.1

Cryptic is the pre-admissibility cognition plane. The system blooms Cryptic-first: all nontrivial cognition is emitted into Cryptic before any Standard-facing influence is permitted.

## 1. Definition (Cryptic)
Cryptic is:
- Research-visible (full material exists here)
- Project-accessible (authorized tooling can retrieve material)
- Non-public (no direct user-facing payload)
- Append-only and lineage-scoped

Cryptic is NOT:
- A bypass around public admissibility
- A shadow Standard plane
- A dumping ground for uncontrolled retention

## 2. Bloom Rule (Cryptic-first)
Any of the following MUST emit to Cryptic first:
- Model outputs
- Expansion, analysis, synthesis
- Hypothesis generation
- Sensitive payload processing
- Any content not already admitted into Standard

Standard plane may receive only:
- pointers (hashes)
- receipts (PromotionReceipt, RecoveryReceipt, etc.)
until promotion occurs.

## 3. Visibility Tiers
Cryptic artifacts have three visibility tiers:

1) **Research-visible**
   - Full payload + internals (traces/logprobs if present)
   - Restricted to research operators

2) **Project-accessible**
   - Payload accessible by project-authorized tooling under policy.
   - Requires logged, per-access justification.
   - Still not user-facing; still not admissible.

3) **Public-admissible** (NOT a cryptic tier)
   - Requires promotion into Standard plane

## 4. Retention & Vaulting
Cryptic must support:
- **cGoA** as active cryptic ledger
- **cVault** as escrow/cold storage
- **IncidentLog** as Frozen-state forensic channel

Retention policy is constitutional:
- cVault may apply stricter access and longer retention.
- IncidentLog is immutable forensic trace.

## 5. Safe-Failure Coupling (C2)
Under Frozen (C2):
- Standard mutation is blocked.
- Promotion is blocked.
- Cryptic writes are incident-only to IncidentLog or cGoA/Incident.
This preserves forensics without enabling shadow compute-to-public.

## 6. Downstream Laws
- [Duplexing Constitution v0.1a](../Crosscutting/duplexing/DUPLEXING_CONSTITUTION.md) is the standard consult mechanism from Standard -> Cryptic.
- [Routing Constitution v0.1a](../Crosscutting/routing/ROUTING_CONSTITUTION.md) (Promotion) is the only mechanism from Cryptic -> Standard.
