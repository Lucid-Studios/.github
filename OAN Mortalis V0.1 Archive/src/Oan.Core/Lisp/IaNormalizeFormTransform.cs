using System;
using System.Collections.Generic;

namespace Oan.Core.Lisp
{
    /// <summary>
    /// Pure transform that normalizes the structural shape of a LispForm.
    /// - Normalizes 'op' (Trim, defaults to "nop").
    /// - Ensures 'args' is non-null.
    /// - Enforces absence-over-null for 'meta'.
    /// </summary>
    public sealed class IaNormalizeFormTransform : IFormTransform
    {
        public string id => "IA_NORMALIZE_FORM";
        public string version => "1";
        public string rationale_code => "IA_CANONICAL_SHAPE";

        public LispForm Apply(LispForm input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            string normalizedOp = (input.op ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(normalizedOp)) normalizedOp = "nop";

            bool needsNewArgs = input.args == null;
            bool needsMetaChange = (input.meta != null && input.meta.Count == 0);

            // If no changes needed, return the same reference
            if (input.op == normalizedOp && !needsNewArgs && !needsMetaChange)
            {
                return input;
            }

            // Create new instance (non-mutating, no aliasing)
            return new LispForm
            {
                op = normalizedOp,
                args = input.args != null ? new Dictionary<string, object>(input.args)
                                          : new Dictionary<string, object>(),
                meta = needsMetaChange ? null
                     : (input.meta != null ? new Dictionary<string, object>(input.meta) : null)
            };
        }
    }
}
