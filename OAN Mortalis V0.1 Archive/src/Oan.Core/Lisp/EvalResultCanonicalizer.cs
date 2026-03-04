using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Oan.Core.Lisp
{
    public static class EvalResultCanonicalizer
    {
        private static readonly JsonSerializerOptions _deterministicOptions = new JsonSerializerOptions
        {
            WriteIndented = false
        };

        /// <summary>
        /// Serializes an EvalResult into a bit-stable canonical JSON string.
        /// Rules: binds fingerprint surface only, ordinal keys, absence-over-null, enums as integers.
        /// </summary>
        public static string SerializeEvalResult(EvalResult r)
        {
            if (r == null) throw new ArgumentNullException(nameof(r));
            if (string.IsNullOrEmpty(r.form_hash)) throw new ArgumentException("MANDATORY_FIELD_MISSING: form_hash");
            if (string.IsNullOrEmpty(r.chain_hash)) throw new ArgumentException("MANDATORY_FIELD_MISSING: chain_hash");
            if (string.IsNullOrEmpty(r.intent_hash)) throw new ArgumentException("MANDATORY_FIELD_MISSING: intent_hash");
            if (string.IsNullOrEmpty(r.sat_hash)) throw new ArgumentException("MANDATORY_FIELD_MISSING: sat_hash");

            var sorted = new SortedDictionary<string, object?>(StringComparer.Ordinal);

            // Required keys (fingerprint surface only)
            sorted["chain_hash"] = r.chain_hash;
            sorted["decision"] = (int)r.decision;
            sorted["form_hash"] = r.form_hash;
            sorted["intent_hash"] = r.intent_hash;
            sorted["sat_hash"] = r.sat_hash;

            // Preserve order; do not sort
            sorted["receipt_hashes"] = r.receipt_hashes ?? Array.Empty<string>();

            // Optional (absence-over-null/empty)
            if (!string.IsNullOrEmpty(r.note))
            {
                sorted["note"] = r.note;
            }

            return JsonSerializer.Serialize(sorted, _deterministicOptions);
        }

        /// <summary>
        /// Produces a SHA-256 lowercase hex hash of the canonicalized EvalResult.
        /// </summary>
        public static string HashEvalResult(EvalResult r)
        {
            string canonical = SerializeEvalResult(r);

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(canonical);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToHexString(hashBytes).ToLowerInvariant();
            }
        }
    }
}
