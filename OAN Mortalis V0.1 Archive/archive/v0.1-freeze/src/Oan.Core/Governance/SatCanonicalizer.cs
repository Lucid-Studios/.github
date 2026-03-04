using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Oan.Core.Governance
{
    public static class SatCanonicalizer
    {
        private static readonly JsonSerializerOptions _deterministicOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Serializes a SatFrame into a bit-stable canonical JSON string.
        /// </summary>
        public static string SerializeSatFrame(SatFrame f)
        {
            if (f == null) throw new ArgumentNullException(nameof(f));
            if (string.IsNullOrEmpty(f.scope)) throw new ArgumentException("MANDATORY_FIELD_MISSING: scope");

            // Build deterministic map with ordinal key ordering
            var sorted = new SortedDictionary<string, object?>(StringComparer.Ordinal);
            
            // Required: Encode enums as integers
            sorted["b"] = (int)f.b;
            sorted["dl"] = (int)f.dl;
            sorted["er"] = (int)f.er;
            sorted["et"] = (int)f.et;
            sorted["m"] = (int)f.m;
            sorted["scope"] = f.scope;
            sorted["tick"] = f.tick;

            // Optional: Absence-over-null
            if (!string.IsNullOrEmpty(f.note))
            {
                sorted["note"] = f.note;
            }
            if (!string.IsNullOrEmpty(f.operator_id))
            {
                sorted["operator_id"] = f.operator_id;
            }

            return JsonSerializer.Serialize(sorted, _deterministicOptions);
        }

        /// <summary>
        /// Produces a SHA-256 lowercase hex hash of the canonicalized SatFrame.
        /// </summary>
        public static string HashSatFrame(SatFrame f)
        {
            string canonical = SerializeSatFrame(f);
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(canonical);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToHexString(hashBytes).ToLowerInvariant();
            }
        }
    }
}
