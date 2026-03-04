using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Oan.Core.Lisp
{
    public static class LispHasher
    {
        private static readonly JsonSerializerOptions _deterministicOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Hashes canonical JSON string bytes (UTF-8, no BOM) to lowercase hex.
        /// </summary>
        public static string Sha256HexUtf8(string canonicalJson)
        {
            if (canonicalJson == null) throw new ArgumentNullException(nameof(canonicalJson));

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(canonicalJson);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToHexString(hash).ToLowerInvariant();
            }
        }

        /// <summary>
        /// Hashes a LispForm by canonicalizing it first.
        /// </summary>
        public static string HashForm(LispForm form)
        {
            string canonical = LispCanonicalizer.SerializeForm(form);
            return Sha256HexUtf8(canonical);
        }

        /// <summary>
        /// Hashes a set of receipts into their individual hashes.
        /// </summary>
        public static IReadOnlyList<string> HashReceiptSet(IReadOnlyList<TransformReceipt> receipts)
        {
            if (receipts == null) throw new ArgumentNullException(nameof(receipts));
            var hashes = new List<string>();
            foreach (var r in receipts)
            {
                hashes.Add(HashReceipt(r));
            }
            return hashes;
        }

        /// <summary>
        /// Hashes a TransformReceipt by canonicalizing it with ordinal keys and absence-over-null.
        /// </summary>
        public static string HashReceipt(TransformReceipt receipt)
        {
            if (receipt == null) throw new ArgumentNullException(nameof(receipt));

            // Mandatory Field Guards
            if (string.IsNullOrEmpty(receipt.id)) throw new InvalidOperationException("MANDATORY_FIELD_MISSING: id");
            if (string.IsNullOrEmpty(receipt.version)) throw new InvalidOperationException("MANDATORY_FIELD_MISSING: version");
            if (string.IsNullOrEmpty(receipt.in_hash)) throw new InvalidOperationException("MANDATORY_FIELD_MISSING: in_hash");
            if (string.IsNullOrEmpty(receipt.out_hash)) throw new InvalidOperationException("MANDATORY_FIELD_MISSING: out_hash");
            if (string.IsNullOrEmpty(receipt.rationale_code)) throw new InvalidOperationException("MANDATORY_FIELD_MISSING: rationale_code");

            var sorted = new SortedDictionary<string, object?>(StringComparer.Ordinal);
            sorted["id"] = receipt.id;
            sorted["version"] = receipt.version;
            sorted["in_hash"] = receipt.in_hash;
            sorted["out_hash"] = receipt.out_hash;
            sorted["rationale_code"] = receipt.rationale_code;

            // Absence over Null rule for optional notes
            if (!string.IsNullOrEmpty(receipt.notes))
            {
                sorted["notes"] = receipt.notes;
            }

            string json = JsonSerializer.Serialize(sorted, _deterministicOptions);
            return Sha256HexUtf8(json);
        }

        /// <summary>
        /// Alias for HashReceiptChain to match spec naming.
        /// </summary>
        public static string HashChain(IReadOnlyList<string> receiptHashesInOrder) => HashReceiptChain(receiptHashesInOrder);

        /// <summary>
        /// Chain hash: SHA256(Join(":", receipt_hashes_in_order))
        /// </summary>
        public static string HashReceiptChain(IReadOnlyList<string> receiptHashesInOrder)
        {
            if (receiptHashesInOrder == null) throw new ArgumentNullException(nameof(receiptHashesInOrder));

            string joined = string.Join(":", receiptHashesInOrder);
            return Sha256HexUtf8(joined);
        }
    }
}
