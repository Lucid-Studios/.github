using System;

namespace Oan.Core.Lisp
{
    /// <summary>
    /// Builds FormHeader v0.1 from an EvalResult. Pure function.
    /// </summary>
    public static class FormHeaderBinder
    {
        public static FormHeader Bind(EvalResult r) => Bind(r, null);

        public static FormHeader Bind(EvalResult r, IReadOnlyList<string>? crypticPointers)
        {
            if (r == null) throw new ArgumentNullException(nameof(r));
            if (string.IsNullOrEmpty(r.form_hash)) throw new ArgumentException("MANDATORY_FIELD_MISSING: form_hash");
            if (string.IsNullOrEmpty(r.chain_hash)) throw new ArgumentException("MANDATORY_FIELD_MISSING: chain_hash");
            if (string.IsNullOrEmpty(r.intent_hash)) throw new ArgumentException("MANDATORY_FIELD_MISSING: intent_hash");
            if (string.IsNullOrEmpty(r.sat_hash)) throw new ArgumentException("MANDATORY_FIELD_MISSING: sat_hash");

            return new FormHeader
            {
                v = "0.1",
                form_hash = r.form_hash,
                chain_hash = r.chain_hash,
                receipt_hashes = r.receipt_hashes ?? Array.Empty<string>(),
                decision = (int)r.decision,
                intent_hash = r.intent_hash,
                sat_hash = r.sat_hash,
                // leave cryptic_pointers null for now (absence-over-null logic in consumers)
                cryptic_pointers = crypticPointers != null && crypticPointers.Count > 0 ? crypticPointers : null,
                note = string.IsNullOrEmpty(r.note) ? null : r.note
            };
        }
    }
}
