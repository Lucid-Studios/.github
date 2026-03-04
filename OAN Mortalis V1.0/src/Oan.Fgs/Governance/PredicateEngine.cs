using System;
using System.Collections.Generic;

namespace Oan.Fgs.Governance
{
    public enum EvalResult
    {
        ALLOW,
        DENY,
        HITL_REQUIRED,
        QUARANTINE,
        FREEZE
    }

    public class EvalContext
    {
        public string CradleId { get; set; } = string.Empty;
        public string RoleBinding { get; set; } = string.Empty;

        /// <summary>
        /// Evidence objects (e.g., Capability Evidence Credentials).
        /// Evidence is NOT permission by itself.
        /// </summary>
        public List<object> CapabilityEvidence { get; set; } = new();

        /// <summary>
        /// Output of SIL.Normalize(...). Stage 2 keeps this as opaque.
        /// </summary>
        public object? SemanticNormalForm { get; set; }

        /// <summary>
        /// Emergency mode triggers dual-ratification semantics in higher layers.
        /// In Stage 2, CA can force FREEZE/HITL_REQUIRED on sensitive families.
        /// </summary>
        public bool EmergencyMode { get; set; } = false;
    }

    public class PredicateMetadata
    {
        public string PredicateId { get; set; } = string.Empty;  // e.g., "fgs.core.lb.006"
        public string DisplayCode { get; set; } = string.Empty;  // e.g., "LB-006"
        public string Name { get; set; } = string.Empty;         // e.g., "PermitAction"
        public bool RequiresHITL { get; set; } = false;
    }

    public class PredicateEngine
    {
        private readonly Dictionary<string, PredicateMetadata> _registry = new(StringComparer.Ordinal);

        public void RegisterPredicate(PredicateMetadata metadata)
        {
            if (string.IsNullOrWhiteSpace(metadata.PredicateId))
                throw new ArgumentException("PredicateId is required.");

            _registry[metadata.PredicateId] = metadata;
        }

        public EvalResult Evaluate(string predicateId, EvalContext context)
        {
            // Fail-closed: unknown predicate
            if (!_registry.TryGetValue(predicateId, out var meta))
                return EvalResult.DENY;

            // Emergency mode: conservative posture (local policy can refine later)
            if (context.EmergencyMode)
            {
                if (predicateId.StartsWith("fgs.core.id.", StringComparison.Ordinal) ||
                    predicateId.StartsWith("fgs.core.sc.", StringComparison.Ordinal))
                    return EvalResult.FREEZE;
            }

            // Stage 2 rule: Identity/Authority family always requires HITL (Lab/Home posture)
            if (predicateId.StartsWith("fgs.core.id.", StringComparison.Ordinal))
                return EvalResult.HITL_REQUIRED;

            // Stage 2 metadata override
            if (meta.RequiresHITL)
                return EvalResult.HITL_REQUIRED;

            // Prototype locality rule (placeholder for real policy handlers):
            // Researchers can PermitAction in the Lab node.
            if (predicateId == "fgs.core.lb.006" && context.RoleBinding == "Researcher")
                return EvalResult.ALLOW;

            return EvalResult.DENY;
        }
    }
}
