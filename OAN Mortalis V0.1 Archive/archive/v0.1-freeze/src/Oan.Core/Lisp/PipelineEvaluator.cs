using System;
using Oan.Core.Governance;

namespace Oan.Core.Lisp
{
    /// <summary>
    /// Evaluator that runs a deterministic transform pipeline and anchors decisions 
    /// via a pluggable IPolicyMembrane. Binds Intent + SAT hashes into the result.
    /// </summary>
    public sealed class PipelineEvaluator : ILispEvaluator
    {
        private readonly TransformPipeline _pipeline;
        private readonly IPolicyMembrane _policy;

        public PipelineEvaluator(TransformPipeline pipeline)
            : this(pipeline, new MinimalPolicy())
        {
        }

        public PipelineEvaluator(TransformPipeline pipeline, IPolicyMembrane policy)
        {
            _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        public EvalResult Evaluate(LispForm form, EvalContext ctx, SatFrame sat)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (sat == null) throw new ArgumentNullException(nameof(sat));
            if (ctx.intent == null) throw new ArgumentException("MANDATORY_FIELD_MISSING: intent", nameof(ctx));

            // 0) Policy decision (deterministic)
            var p = _policy.Decide(new PolicyInput { intent = ctx.intent, sat = sat, ctx = ctx, form = form });

            // 1) Run deterministic pipeline (still runs; decision semantics only for result)
            TransformPipelineResult piped = _pipeline.Run(form);

            // 2) Bind context hashes
            string intentHash = IntentCanonicalizer.HashIntent(ctx.intent);
            string satHash = SatCanonicalizer.HashSatFrame(sat);

            // 3) Return fully-bound EvalResult
            return new EvalResult
            {
                decision = p.decision,

                sealed_form = piped.sealed_form,
                receipts = piped.receipts,
                receipt_hashes = piped.receipt_hashes,

                chain_hash = piped.chain_hash,
                form_hash = piped.final_form_hash,

                intent_hash = intentHash,
                sat_hash = satHash,

                cryptic_emissions = Array.Empty<CrypticEmission>(),

                note = p.note
            };
        }
    }
}
