namespace Oan.Core.Lisp
{
    /// <summary>
    /// Deterministic symbolic transform. Must be pure: same input => same output.
    /// used to emit verifiable evidence (TransformReceipt) in the governance pipeline.
    /// </summary>
    public interface IFormTransform
    {
        string id { get; }          // stable token, e.g. "NOP", "OP_REWRITE"
        string version { get; }     // stable token, e.g. "1"
        string rationale_code { get; } // stable token, e.g. "SAFE_NORMALIZE"

        /// <summary>
        /// Applies the transform to a Lisp form. Should return a news instance or the same instance if no change.
        /// Must NOT mutate the input instance if it is shared.
        /// </summary>
        LispForm Apply(LispForm input);
    }
}
