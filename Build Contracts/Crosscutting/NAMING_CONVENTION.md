Good. We’ll formalize this properly so future work cannot silently collapse layers.

Below is a **repository-ready document** you can drop in as:

```
/docs/NAMING_CONVENTION_v1.0.md
```

or at repo root as:

```
NAMING_CONVENTION.md
```

---

# OAN Mortalis

# Naming Convention & Layer Constitution

## v1.0 — Phase A Locked

---

# 1. Purpose

This document defines the authoritative naming and layering constitution for the OAN Mortalis governance stack.

It exists to prevent:

* Concept collapse (SLI == Lisp == Spine confusion)
* Hidden coupling (cryptic IO required for survival)
* Reboot-hook dependencies
* Architectural drift during Phase B and beyond

This specification freezes terminology at the end of Phase A.

---

# 2. Layer Model (Authoritative)

The system is divided into **four conceptual layers**:

```
Public Layer
    ↓
Cryptic Layer
    ↓
Spine (Deterministic Governance Kernel)
    ↓
SoulFrame (Final Authority Boundary)
```

These are not interchangeable.

---

# 3. Conceptual Naming (What Things ARE)

## 3.1 Spine

**Definition:**
The minimum deterministic governance kernel that must operate without:

* Embedded LLM
* Public layer
* Cryptic storage
* Duplex routing

The Spine must be capable of:

* Deterministic evaluation
* Canonical hashing
* Receipt chain formation
* Policy enforcement
* Safe failure

### Concrete Spine Components (Current)

* `LispForm`
* `TransformPipeline`
* `IaNormalizeFormTransform`
* `PipelineEvaluator`
* `IPolicyMembrane`
* `MinimalPolicy`
* `EvalResult`
* `IntentCanonicalizer`
* `SatCanonicalizer`
* `LispHasher`
* `FormHeaderBinder`

If cryptic store disappears, Spine still functions.

If unified boundary disappears, Spine still functions.

Spine must never depend on duplexing to exist.

---

## 3.2 SLI (Symbolic Language Interconnect)

**Definition:**
The protocol specification for deterministic symbolic governance.

SLI defines:

* Canonicalization rules
* Transform trace discipline
* Hash binding
* Receipt chaining
* Evaluation membrane
* Header binding

SLI is **a protocol**, not a syntax.

### Current Implementation

SLI is currently implemented using a Lisp-shaped AST:

```
SLI protocol
    ↓ implemented as
LispForm + TransformPipeline
```

SLI ≠ Lisp.

Future symbolic representations may replace Lisp without replacing SLI.

---

## 3.3 Lisp

**Definition:**
The current symbolic AST representation of SLI.

Namespace: `Oan.Core.Lisp`

Lisp is:

* A structural form
* A transform input/output surface
* A canonicalizable entity

Lisp is not the governance authority.

---

## 3.4 Cryptic Layer

**Definition:**
Append-only fingerprint persistence.

Responsibilities:

* Store deterministic emissions
* Provide stable `cGoA/<hash>` pointers
* Maintain NDJSON ledger integrity

Components:

* `CrypticEmission`
* `CrypticCanonicalizer`
* `CrypticEmissionBuilders`
* `CrypticPointerHelper`
* `CrypticNdjsonStore`
* `ICrypticStore`

Cryptic layer is operational, not identity.

---

## 3.5 Public Layer

**Definition:**
Presentation and host integration surface.

Includes:

* `FormHeader`
* `EvaluateEnvelope`
* Telemetry outputs
* API surfaces

Public may degrade without compromising Spine.

---

## 3.6 SoulFrame

**Definition:**
The final enforcement authority before critical failure.

SoulFrame must:

* Monitor rebuild attempts
* Enforce tier transitions
* Gate promotion logic
* Control safe-fail states

SoulFrame is the last boundary before critical halt.

If SoulFrame cannot enforce, system must halt safely.

---

# 4. Safe-Fail Ladder (Mandatory Behavior)

The system must degrade in this order:

1. Public failure → degrade public output.
2. Cryptic failure → operate Spine-only (no persistence).
3. Spine degradation → SoulFrame safe state.
4. SoulFrame failure → critical halt.

The system must never silently bypass SoulFrame via reboot hooks.

---

# 5. Code Naming Rules

## 5.1 Type Naming

* PascalCase for all types.
* Interfaces prefixed with `I`.
* `Unified` suffix indicates IO boundary.
* `Envelope` indicates host transport surface.
* `Canonicalizer` indicates bit-stable JSON surface.
* `Hasher` indicates SHA-256 computation.
* `Builder` indicates fingerprint DTO construction.
* `Binder` indicates surface binding without logic.
* `Store` indicates IO.

---

## 5.2 Serialized Property Naming

All canonical JSON surfaces use **snake_case**.

Examples:

```
form_hash
chain_hash
intent_hash
sat_hash
receipt_hashes
payload_hash
cryptic_pointers
policy_rationale_code
```

Never camelCase in canonical surfaces.

---

## 5.3 Hashing Rules

All hashes:

* SHA-256
* Lowercase hex
* 64 characters
* Never base64
* Never uppercase
* Never truncated

---

## 5.4 Enum Serialization

* Enums serialize as integers.
* Tier strings normalized via canonicalizer only.

Example:

```
CrypticTier.CGoA → "cGoA"
```

Never serialize enum name directly.

---

# 6. Canonical Wire Surfaces

## 6.1 FormHeader v0.1

Must include:

```
v
form_hash
chain_hash
receipt_hashes
decision
intent_hash
sat_hash
```

Optional:

```
cryptic_pointers
note
```

Optional fields omitted (never null).

---

## 6.2 Cryptic NDJSON Record

One emission per line:

* Minified JSON
* Ordinally sorted keys
* UTF-8
* `\n` newline only
* Append-only

---

# 7. Unified Boundary Rules

`PipelineEvaluatorUnified`:

* Must not alter pure evaluation semantics.
* Must persist emission.
* Must bind pointer into header.
* Must not mutate Spine state.

Core evaluator must remain usable without Unified wrapper.

---

# 8. Prohibited Collapses

The following are forbidden architectural collapses:

* SLI == Lisp
* Spine == Cryptic layer
* Spine requires cryptic persistence
* Public layer required for survival
* SoulFrame treated as optional
* Reboot hook bypass of promotion enforcement

---

# 9. Versioning

This document defines:

**Naming Convention v1.0 — Phase A Complete**

Changes to naming or layer semantics require explicit version bump.

---

# 10. Summary

The governance stack is now defined as:

```
SLI (protocol)
    implemented as
Lisp AST
    executed by
Spine (deterministic kernel)
    optionally wrapped by
Unified Boundary (IO + cryptic persistence)
    monitored by
SoulFrame (final authority)
```

Spine must survive without duplexing.

SoulFrame must enforce rebuild access.

Public and Cryptic are operational surfaces — not identity.

---

