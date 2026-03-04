# Symbolic Duplexing Constitution v0.1

This constitution defines the protocol for **Symbolic Duplexing**, allowing Standard-facing agents to consult Cryptic computations without violating plane invariants, authority boundaries, or determinism.

## 1. Duplexing Definition
**Symbolic Duplexing** is a two-plane consultation protocol where the Standard plane "queries" the Cryptic plane for expanded analysis.

- **Duplexing IS**: A request for secondary, isolated computation.
- **Duplexing IS NOT**: A policy bypass, an auto-promotion mechanism, or a direct data pipe from Cryptic to Standard outputs.

## 2. Duplexing Artifacts

### 2.1 DuplexQuery (Standard Plane)
The query originates in the Standard plane and is bound to the current governance context.
- `policyVersion`: Binding to the governing policy.
- `invokingHandle`: The specific SLI handle/function initiating the query.
- `satMode`: Current SAT mode (Standard, Audit, etc.).
- `queryHash`: Canonical content hash of the query intent.
- `standardTipHash`: Reference to the latest Standard plane tip for lineage binding.

> [!IMPORTANT]
> **Refinement: Standard Tip Binding**
> DuplexQuery validation requires that `standardTipHash` must match the current Standard plane tip at the time of acceptance by the Routing Engine. If a mismatch is detected, the query must be denied as stale/divergent.

### 2.2 DuplexResponse (Cryptic Plane)
The response is generated within the Cryptic plane (cGoA) and is strictly isolated.
- `queryHash`: Reference to the originating query.
- `crypticResultHash`: Content-addressed hash of the cryptic compute result.
- `crypticTipHash`: Reference to the latest Cryptic plane tip for lineage binding.
- `classificationTags`: Metadata suggesting safety levels (e.g., `Standard-safe`, `Promotion-Required`).
- **NO PAYLOAD**: The response object itself contains only hashes/pointers. Material content remains in Cryptic storage.

> [!CAUTION]
> **Advisory-Only Tags**
> `classificationTags` are non-authoritative hints and provide zero authority to bypass the Promotion Graph. Only a valid `PromotionReceipt` grants Standard-plane admissibility.

## 3. The "Pointer-Only" Default Rule
By default, any result returned to the Standard-facing agent from a duplexed computation MUST be a **pointer** (hash) to the cryptic artifact.
- The agent may know *that* a result exists and its *identity*.
- The agent may NOT read the material content of the result within the Standard plane context.

## 4. Duplexing + Promotion Coupling
If a duplexed output's material content is required to influence Standard plane behavior or state:
1. It must be emitted to the **Cryptic Governance Ledger (cGoA)**.
2. It must be processed through a **PromotionRequest**.
3. It must pass **SoulFrameValidation** against current policy.
4. It must receive a **PromotionReceipt**.
5. It is only then committed to the **Standard Plane (GEL/GoA)**.

## 5. Determinism Constraints
Duplexing must be 100% deterministic to ensure replay identity.
- No time-based sampling or non-monotonic state consultation.
- **DeterministicHarness mandatory**: All nondeterministic engines (LLMs) must be wrapped in a `DeterministicHarness` layer that canonicalizes input, enforces fixed sampling parameters (seed, temp=0), and strips time-based variables before emitting a hash.
- All model outputs are treated as **Cryptic Emissions** first and stored in cGoA.

## 6. Telemetry Constraints
- **Governance Telemetry**: Records metadata only (`DuplexQueryHash`, `DuplexResponseHash`, `PromotionReceiptHash`).
- **Research Telemetry**: May record rich model internals (traces, logprobs) but is strictly **non-admissible** for governance.
- **AgentiCore Boundary**: AgentiCore MUST be prohibited from reading research telemetry at the routing layer.

## 7. Acceptance Criteria (Verification)
1. **Mutation Guard**: No duplexing call can trigger a Standard-plane write without an accompanying `PromotionReceipt`.
2. **Payload Isolation**: Cryptic text payloads cannot manifest in Standard outputs without promotion.
3. **Replay Identity**: Replaying a `DuplexQuery` results in identical `DuplexResponse` hashes and identical cryptic storage artifacts.
4. **C2 Compliance**: When in `Frozen` state, duplexing results are blocked from entering the Standard plane.
