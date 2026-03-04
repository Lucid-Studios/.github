namespace Oan.Core.Lisp
{
    /// <summary>
    /// Bundles evaluation results for host/telemetry surfaces.
    /// Provides a hermetic boundary for the operation's outcome and its audit anchors.
    /// </summary>
    public sealed class EvaluateEnvelope
    {
        public EvalResult result { get; set; } = new EvalResult();
        public FormHeader header { get; set; } = new FormHeader();
        public string cryptic_ptr { get; set; } = string.Empty; // convenience echo (must be "cGoA/<hash>")
    }
}
