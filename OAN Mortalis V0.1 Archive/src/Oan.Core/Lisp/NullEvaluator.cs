using System;
using System.Collections.Generic;
using Oan.Core.Governance;

namespace Oan.Core.Lisp
{
    /// <summary>
    /// A deterministic placeholder evaluator that allows everything and computes correct hashes for empty chains.
    /// </summary>
    public sealed class NullEvaluator : ILispEvaluator
    {
        public EvalResult Evaluate(LispForm form, EvalContext ctx, SatFrame sat)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (sat == null) throw new ArgumentNullException(nameof(sat));

            // Hash the input form (no transforms in NullEvaluator)
            string formHash = LispHasher.HashForm(form);
            
            // Empty receipt chain handling
            var receiptHashes = Array.Empty<string>();
            string chainHash = LispHasher.HashReceiptChain(receiptHashes);

            return new EvalResult
            {
                decision = EvalDecision.Allow,
                sealed_form = form, // No mutation
                receipts = Array.Empty<TransformReceipt>(),
                cryptic_emissions = Array.Empty<CrypticEmission>(),
                form_hash = formHash,
                receipt_hashes = receiptHashes,
                chain_hash = chainHash,
                note = "NULL_EVALUATOR"
            };
        }
    }
}
