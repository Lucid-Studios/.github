# Public Braiding Constitution v0.1 (IUTT Gluing)

Public (Standard plane) is the admissible, user-facing plane used for CME personification and development. Public is formed by functorial export from Cryptic through promotion, not by direct ingestion.

## 1. Definition (Public / Standard)
Public is:
- Admissible
- Minimal disclosure
- Pointer-forward (references instead of payload by default)
- Governance-auditable

Public is NOT:
- A mirror of cryptic material
- A raw transcript of cognition
- A place where sensitive payloads “accidentally appear”

## 2. Functor Rule (Cryptic -> Public)
Export from Cryptic to Public must be functorial:
- Structure-preserving
- Deterministic
- Audit-bound

Meaning:
- The export preserves identity via hashes and receipts.
- The export does not import cryptic payload unless explicitly permitted.
- The export preserves lineage continuity on both sides without tip contamination.

## 3. Gluing/Braiding (IUTT-style)
Define a promotion as a “gluing morphism”:

`CrypticArtifact --(PromotionReceipt)--> PublicCommit`

Where:
- **PromotionReceipt** is the gluing certificate
- **PublicCommit** is the glued object in Standard lineage
- The glue preserves references, not material, unless explicitly allowed

Braiding means:
- Multiple cryptic sources may be braided into a single PublicCommit only via:
  - explicit multi-parent lineage references
  - deterministic ordering
  - single PromotionReceipt binding the braid set

## 4. Default Public Form: Pointer + Summary (No Payload)
Default user-facing surface must be:
- pointer(s) to cryptic artifacts
- plus Standard-admissible summaries produced within Standard constraints

If the system requires payload material, it must be:
- promoted explicitly
- redacted/quarantined/vaulted as policy requires
- logged in GEL as admissibility event

## 5. CME Personification Boundary
CME personification and development occurs only in Public:
- identity formation, continuity narrative, and user relationship are Public-plane phenomena
- Cryptic may propose, but cannot personify
- Any “self” claims must be Standard-admissible artifacts or pointer-only references

## 6. Lisp IR Integration
All governance-relevant objects that participate in routing/promotion/braiding must have:
- a **LispForm** representation (canonical IR)
- deterministic serialization
- stable hashing

This ensures:
- the symbolic governance spine remains the source of truth
- [Duplexing](../Crosscutting/duplexing/DUPLEXING_CONSTITUTION.md) and [Routing](../Crosscutting/routing/ROUTING_CONSTITUTION.md) can be reasoned about as transformations over IR
