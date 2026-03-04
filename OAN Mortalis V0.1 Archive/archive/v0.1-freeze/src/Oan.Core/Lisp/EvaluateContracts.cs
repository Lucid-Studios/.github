using System.Collections.Generic;
using Oan.Core.Governance;

namespace Oan.Core.Lisp
{
    public enum EvalDecision
    {
        Allow = 0,
        Deny = 1,
        Transform = 2,
        Freeze = 3
    }

    /// <summary>
    /// The immutable result of a Lisp evaluation boundary.
    /// contains the decision, evidence (receipts), and cryptographic fingerprints.
    /// </summary>
    public sealed class EvalResult
    {
        // Required
        public EvalDecision decision { get; set; }
        public LispForm sealed_form { get; set; } = new LispForm(); // The final form (post-transform)
        public IReadOnlyList<TransformReceipt> receipts { get; set; } = new List<TransformReceipt>();
        public IReadOnlyList<CrypticEmission> cryptic_emissions { get; set; } = new List<CrypticEmission>();
        
        public string form_hash { get; set; } = string.Empty;     // SHA256(sealed_form)
        public string chain_hash { get; set; } = string.Empty;    // HashChain(receipt_hashes)
        public string intent_hash { get; set; } = string.Empty;
        public string sat_hash { get; set; } = string.Empty;
        public IReadOnlyList<string> receipt_hashes { get; set; } = new List<string>();

        // Optional
        public string? note { get; set; } // Omit when null/empty
    }

    /// <summary>
    /// Interface for the SLI governance interpreter.
    /// </summary>
    public interface ILispEvaluator
    {
        /// <summary>
        /// Evaluates a Lisp form against a context and topology snapshot.
        /// </summary>
        EvalResult Evaluate(LispForm form, EvalContext ctx, SatFrame sat);
    }
}
