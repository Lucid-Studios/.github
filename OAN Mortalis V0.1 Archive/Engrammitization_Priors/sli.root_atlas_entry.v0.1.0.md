Perfect — `sli.root_atlas_entry.v0.1.0` is the keystone that makes **SLI Gate determinism** actually enforceable, because the Gate can only be “pure capability routing” if the **Root Atlas is static, canonical, and hashable**. 

Below is a freeze-grade spec you can drop straight into `Build Contracts/`.

---

# sli.root_atlas_entry.v0.1.0

## Root Atlas Handle Registry Entry (Layer 1, Static, Deterministic)

## 0. Purpose

`**sli.root_atlas_entry.v0.1.0**` defines the immutable registry entry for a single SLI handle.

Each entry is the **authoritative declared capability** that maps:

`Handle → IntentKind + AllowedAddress + Required SAT/Governance + HITL requirement + PolicyVersion`

SLI Gate evaluation must rely **only** on:

* the packet’s declared handles (`sli.packet.v0.1.0`)
* session mounts
* the Root Atlas entries
* policy versioning

No inference. No semantic heuristics. No dynamic mutation. 

---

## 1. Non-Goals (Hard Boundary)

Root Atlas MUST NOT:

* be modified at runtime (v0.1.0)
* be influenced by natural language parsing
* infer permissions or generate new handles automatically
* contain runtime context or salience metrics
* contain implementation code; it is data only
* silently promote capabilities across mirrors

---

## 2. Static Registry Rule (MVP Freeze)

**For v0.1.0:**

* Root Atlas is **static** (loaded at boot).
* No runtime writes.
* No runtime merges.
* No drift.

If you need evolution, it happens via **new policy versions** and a new boot artifact, not a hot edit.

---

## 3. Schema ID

`**sli.root_atlas_entry.v0.1.0**`

---

## 4. Canonical Record (C#)

```csharp
public record SliRootAtlasEntry_v0_1_0(
    // --- Schema ---
    string Schema,                        // "sli.root_atlas_entry.v0.1.0"

    // --- Handle identity ---
    string Handle,                        // e.g. "engram.construct.propositional"
    string IntentKind,                    // e.g. "EngramConstruct" | "TelemetryWrite" | "ExecuteIntent" | ...

    // --- Allowed address constraints (capability target space) ---
    AddressConstraint AllowedAddress,     // see §4.1

    // --- Governance constraints ---
    string[] RequiredSatModes,            // sorted; e.g. ["SAT_Gate", "HITL"]
    bool RequiresHITL,                    // explicit human-in-the-loop requirement
    string PolicyVersion,                 // e.g. "POLICY-0.1.0"

    // --- Cryptic constraints (explicit) ---
    CrypticConstraint Cryptic,            // see §4.2

    // --- Audit metadata (static) ---
    string Description,                   // human-readable summary
    string Owner,                         // e.g. "AgentiCore" | "CradleTek" | "Lab-D"
    string EntryHash,                     // sha256(canonicalBytes(entry without EntryHash))
    string CanonicalSeal                  // sha256(full canonical serialization)
);
```

---

### 4.1 AddressConstraint

This is the **declared capability envelope**. Gate checks the packet’s requested/declared env against this.

```csharp
public record AddressConstraint(
    string AllowedChannel,                // "Public" | "Private" | "Any"
    string AllowedPartition,              // "GEL" | "GOA" | "OAN" | "Any"
    string AllowedMirror                  // "Standard" | "Cryptic" | "Any"
);
```

**Freeze rule:** these are exact strings, case-fixed, no enum ToString drift.

---

### 4.2 CrypticConstraint

Cryptic mirrors require explicit governance.

```csharp
public record CrypticConstraint(
    bool IsCrypticCapable,                // true if AllowedMirror includes Cryptic
    string MinimumSatModeForCryptic,      // e.g. "SAT_Gate" (required if IsCrypticCapable)
    bool ApplyMaskingPolicy,              // whether masking flag must be set/logged
    string MaskingPolicyId                // e.g. "MASK-0.1" or "none"
);
```

This makes Cryptic rules explicit and auditable. 

---

## 5. Canonicalization Contract (Mandatory)

Entry hashing must obey the same canonical builder rules:

1. Fixed field order per schema version
2. Arrays sorted lexicographically (`RequiredSatModes`)
3. Dictionaries (if any future fields) keys sorted lexicographically
4. null → literal `"null"`
5. UTF-8 only
6. Lowercase hex for hashes
7. Manual builder, no serializer auto-ordering

Compute:

* `EntryHash = sha256(canonicalBytes(entry without EntryHash))`
* `CanonicalSeal = sha256(full canonical serialization)`

---

## 6. Root Atlas Container (Recommended, but separate schema)

The Root Atlas itself should be a container mapping handles → entries.

If you want to freeze the container too (recommended), define:

`**sli.root_atlas.v0.1.0**`:

* `AtlasVersion`
* `PolicyVersion`
* `Entries[]` (sorted by Handle)
* `AtlasHash` (sha256 over canonical bytes)

But the critical unit is the entry spec above.

---

## 7. Gate Evaluation Dependencies (How this is used)

For each handle in `sli.packet.Handles`:

1. lookup RootAtlasEntry by exact handle string
2. enforce `AllowedAddress` constraints against requested/declared env
3. enforce `RequiredSatModes` against packet.Mode.SatMode
4. enforce `RequiresHITL` against runtime governance state
5. if cryptic access, enforce `CrypticConstraint` rules + masking requirements
6. log resolution and policyVersion in `GateTrace` 

No heuristics. No fallback.

---

## 8. Required Tests (Freeze-Grade)

### Test RA1 — Deterministic hashing

Same entry fields → same `EntryHash` across machines.

### Test RA2 — Exact handle lookup

Handle must resolve by exact match; no case folding or fuzzy matching.

### Test RA3 — Cryptic capability rules

If `AllowedMirror == "Cryptic"` (or Any), then:

* `IsCrypticCapable == true`
* `MinimumSatModeForCryptic` must be non-null and valid
* Gate must require that minimum sat mode

### Test RA4 — No runtime fields contamination

Serialized entry must not include forbidden substrings:
`salience`, `gradient`, `relevance`, `queryContext`, `perspective`, `basin`.

### Test RA5 — Static immutability (MVP)

Attempt runtime modification must be rejected or ignored (depending on your host policy), but never silently accepted.

---

## 9. Practical Naming Notes (Handle Format)

Freeze a handle naming convention now to avoid entropy:

Recommended format:

* `domain.action.object[:variant]`
* lowercase
* dot-separated

Examples:

* `engram.construct.propositional`
* `engram.commit`
* `telemetry.write.sli_gate`
* `bootstrap.rootatlas.load`

This keeps handles stable and grep-able.

---

## Next lock (if you want the full SLI chain sealed)

After Root Atlas entries, the next most “contract-critical” objects are typically:

1. `sli.gate_decision.v0.1.0` (the exact allow/deny record + evidence snapshot format)
2. `sli.resolved_address.v0.1.0` (Channel×Partition×Mirror normalization object)
3. `sli.session_mounts.v0.1.0` (what partitions/mirrors are mounted for a session)

If you tell me which direction you want next, I’ll freeze the next schema in the same style.
