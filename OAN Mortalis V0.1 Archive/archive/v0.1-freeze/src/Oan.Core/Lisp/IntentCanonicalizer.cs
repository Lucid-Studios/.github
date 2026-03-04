using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Oan.Core.Lisp
{
    public static class IntentCanonicalizer
    {
        private static readonly JsonSerializerOptions _deterministicOptions = new JsonSerializerOptions
        {
            WriteIndented = false
        };

        /// <summary>
        /// Serializes an IntentForm into a bit-stable canonical JSON string.
        /// Rules: ordinal keys, absence-over-null, enums as integers, minified JSON.
        /// </summary>
        public static string SerializeIntent(IntentForm i)
        {
            if (i == null) throw new ArgumentNullException(nameof(i));
            if (string.IsNullOrEmpty(i.verb)) throw new ArgumentException("MANDATORY_FIELD_MISSING: verb");
            if (string.IsNullOrEmpty(i.scope)) throw new ArgumentException("MANDATORY_FIELD_MISSING: scope");

            var sorted = new SortedDictionary<string, object?>(StringComparer.Ordinal);

            // Required keys
            sorted["kind"] = (int)i.kind;
            sorted["scope"] = i.scope;
            sorted["tick"] = i.tick;
            sorted["verb"] = i.verb;

            // Optional keys (omit when null/empty)
            if (!string.IsNullOrEmpty(i.note))
            {
                sorted["note"] = i.note;
            }
            if (!string.IsNullOrEmpty(i.object_ref))
            {
                sorted["object_ref"] = i.object_ref;
            }
            if (!string.IsNullOrEmpty(i.subject))
            {
                sorted["subject"] = i.subject;
            }

            return JsonSerializer.Serialize(sorted, _deterministicOptions);
        }

        /// <summary>
        /// SHA-256 lowercase hex of canonical IntentForm JSON (UTF-8, no BOM).
        /// </summary>
        public static string HashIntent(IntentForm i)
        {
            string canonical = SerializeIntent(i);

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(canonical);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToHexString(hashBytes).ToLowerInvariant();
            }
        }
    }
}
