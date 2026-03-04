using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Oan.Core.Governance;

namespace Oan.Core.Lisp
{
    /// <summary>
    /// Host boundary wrapper that orchestrates pure evaluation with cryptic persistence.
    /// Ensures every decision is committed to storage and anchored via a FormHeader.
    /// </summary>
    public sealed class PipelineEvaluatorUnified
    {
        private readonly PipelineEvaluator _core;
        private readonly ICrypticStore _store;
        private readonly IPromotionPolicy _promotion;

        public PipelineEvaluatorUnified(PipelineEvaluator core, ICrypticStore store)
            : this(core, store, new MinimalPromotionPolicy())
        {
        }

        public PipelineEvaluatorUnified(
            PipelineEvaluator core, 
            ICrypticStore store, 
            IPromotionPolicy promotion)
        {
            _core = core ?? throw new ArgumentNullException(nameof(core));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _promotion = promotion ?? throw new ArgumentNullException(nameof(promotion));
        }

        /// <summary>
        /// Executes evaluation, persists results to cryptic storage, and binds an audit header.
        /// Supports multi-tier routing via IPromotionPolicy.
        /// </summary>
        public async Task<EvaluateEnvelope> EvaluateAsync(
            LispForm form,
            EvalContext ctx,
            SatFrame sat,
            CancellationToken ct = default)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (sat == null) throw new ArgumentNullException(nameof(sat));
            
            // Deterministic tick source requirement
            if (ctx.tick <= 0) throw new ArgumentException("MANDATORY_FIELD_MISSING: tick", nameof(ctx));

            // 1) Pure evaluation (Policy + Pipeline)
            EvalResult r = _core.Evaluate(form, ctx, sat);

            // 2) Decide promotion plan (multi-tier routing)
            PromotionPlan plan = _promotion.Decide(r);

            // 3) Process tier routing based on plan
            var pointers = new List<string>();
            string? primaryPtr = null;

            foreach (var item in plan.items)
            {
                // Build tier-specific emission
                CrypticEmission e = CrypticEmissionBuilders.BuildEvalBoundaryEmission(
                    r, 
                    item.tier,
                    item.rationale_code, 
                    ctx.tick);

                // Persist line (NDJSON) -> return canonical pointer
                string ptr = await _store.AppendAsync(e, ct).ConfigureAwait(false);
                pointers.Add(ptr);

                // Convenience: the first (baseline) pointer is our primary echo
                if (primaryPtr == null) primaryPtr = ptr;
            }

            // 4) Bind v0.1 Header with all resulting pointers (preserves order)
            FormHeader h = FormHeaderBinder.Bind(r, pointers);

            // 5) Produce hermetic envelope
            return new EvaluateEnvelope
            {
                result = r,
                header = h,
                cryptic_ptr = primaryPtr ?? string.Empty
            };
        }
    }
}
