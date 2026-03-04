# WORKSPACE RULES (MANDATORY)

## 1) Build Contracts are the root of truth
**Folder:** `Build Contracts/`  
**Rule:** Any task that affects architecture, interfaces, naming, layering, persistence rules, determinism, or governance **must first be checked against Build Contracts**. If a model proposes anything that conflicts, it must **stop and ask** or **revise to comply**.

> "Before making any changes, read and obey Build Contracts. If a requested change conflicts with contracts, do not implement—report the conflict."

---

## 2) v0.1 archive is read-only reference
**Folder:** `OAN Mortalis V0.1 Archive/`  
**Rule:** This is a **reference dataset** only. No edits, no builds, no "quick fixes," no refactors.  
It exists for concept/prototype mining, migration mapping, and provenance checks.

When info is needed from v0.1:
- Quote file paths
- Extract minimal relevant snippets
- Propose a v1.0-native reimplementation

---

## 3) v1.0 is the only active build target
**Folder:** `OAN Mortalis V1.0/`  
**Rule:** All implementation work happens here. All build/test commands run here. All new code lands here.

---

## Path Allowlist
- **Allowed write paths:** `OAN Mortalis V1.0/**`
- **Allowed read paths:** `Build Contracts/**`, `OAN Mortalis V0.1 Archive/**`

## Forbidden write paths
- `Build Contracts/**`
- `OAN Mortalis V0.1 Archive/**`
- Anything else in `Unity Projects/**` outside v1.0
