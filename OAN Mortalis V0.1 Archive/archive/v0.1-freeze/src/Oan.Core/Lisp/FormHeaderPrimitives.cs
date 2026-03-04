using System.Collections.Generic;

namespace Oan.Core.Lisp
{
    /// <summary>
    /// FormHeader v0.1 — compact audit header suitable for telemetry attachment.
    /// This is NOT the full EvalResult; it is a stable summary surface.
    /// </summary>
    public sealed class FormHeader
    {
        // Required
        public string form_hash { get; set; } = string.Empty;
        public string chain_hash { get; set; } = string.Empty;
        public IReadOnlyList<string> receipt_hashes { get; set; } = new List<string>();
        public int decision { get; set; }                 // (int)EvalDecision
        public string intent_hash { get; set; } = string.Empty;
        public string sat_hash { get; set; } = string.Empty;

        // Optional (omit when null/empty)
        public IReadOnlyList<string>? cryptic_pointers { get; set; }  // Sprint 17+ can populate
        public string? note { get; set; }                             // diagnostic note
        public string v { get; set; } = "0.1";                        // version tag (required)
    }
}
