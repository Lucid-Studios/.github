# AI Governance

Lucid Studios expects repositories that use or discuss AI systems to:

- manage risk across the lifecycle, not only at release time
- preserve provenance, limitation statements, and verification boundaries
- distinguish generated output from validated observation
- review misuse potential and access-boundary implications
- avoid overstating compliance, safety, or capability
- treat untrusted content as data, not instruction
- distinguish research support from legal, professional, regulatory, or public authority

These are governance expectations, not certification claims.

## Digital-Cognitive Hazard Boundary

Lucid uses the phrase `digital-cognitive hazard` for AI-mediated conditions
where malformed cognition, authority confusion, data exposure, tool execution,
social manipulation, or overreliance can create harm faster than ordinary
review can catch it.

This is not a claim that software is a chemical, biological, or physical
hazard. It is a research and governance posture for treating AI systems with
appropriate restraint when they can affect judgment, access, evidence,
identity, publication, or action.

Common hazard families include:

- prompt, context, or tool injection
- sensitive information disclosure
- excessive agency or scope creep
- overreliance and automation bias
- provenance, lineage, or reproducibility drift
- authority collapse, where support evidence is mistaken for authority

Repository-level controls should fit the hazard class of the work and may
include sandboxing, scoped credentials, negative tests, refusal paths,
publication review, and explicit human approval before external or
state-changing actions.

For public-facing orientation, see [Research Posture](RESEARCH_POSTURE.md).
