# Spine Constitution v0.1

The Spine is the sovereign, deterministic governance substrate that exists before and after every CradleTek-managed stack, and it is the first and last authority for admissibility, lineage, and safe-failure.

## 1. Definition (Spine)
The Spine is:
- Deterministic
- Append-only
- Policy-versioned
- Lineage-preserving (plane-scoped)
- Authority-bearing

The Spine is NOT:
- An agent
- A model
- A cognition engine
- A storage heap

## 2. Primacy Rule
For any CradleTek stack instance:
- Spine MUST be established before SoulFrame/AgentiCore/AgentRuntime are constructed.
- Spine MUST remain available as the final enforcement authority after runtime shutdown until sealing completes.

## 3. Spine Surfaces
Spine exposes exactly these sovereign surfaces:
- Governance IR surface (LispForm canonical IR)
- Policy membrane surface (allow/deny + reason codes)
- Receipt surface (hash-bound receipt chain)
- Routing constitution enforcement surface
- Safe-fail state surface (Operational/Frozen/Quarantined/Halt)
- Telemetry admissibility surface (Governance vs Research)

## 4. Determinism Rule
No Spine decision may depend on:
- Wall clock
- Randomness
- External network calls
- Non-canonical serialization
- Research telemetry

## 5. Pre/Post Rule (“Spine Envelope”)
Every stack run is wrapped by a Spine envelope:
- PRE: establish policyVersion, SAT mode, mounts, plane tips
- POST: seal receipts, commit governance telemetry, resolve final state

## 6. Invariant: No Ghost Writes
Any mutation to Standard plane stores MUST be:
- Routed through the routing engine
- Authorized by SoulFrame under Spine authority
- Recorded in governance telemetry

## 7. Authority Boundaries
- Spine governs admissibility and state.
- Agents propose intents only (inert DTOs).
- Only PromotionReceipt authorizes cross-plane admissibility.

## 8. Downstream Constitutions
- [Routing Constitution v0.1a](../routing/ROUTING_CONSTITUTION.md) (Downstream Law)
- [Duplexing Constitution v0.1a](../duplexing/DUPLEXING_CONSTITUTION.md) (Downstream Law)

If they conflict, Spine Constitution governs by:
- freezing the system (fail closed)
- requiring constitutional revision, not runtime improvisation
