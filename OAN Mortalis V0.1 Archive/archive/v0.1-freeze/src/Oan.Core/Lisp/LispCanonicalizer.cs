using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

namespace Oan.Core.Lisp
{
    public static class LispCanonicalizer
    {
        /// <summary>
        /// Serializes a LispForm into a bit-stable canonical JSON string.
        /// </summary>
        public static string SerializeForm(LispForm form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var sorted = new SortedDictionary<string, object?>(StringComparer.Ordinal);
            
            // op is mandatory per schema
            sorted["op"] = form.op;

            // args: omit if null or empty
            if (form.args != null && form.args.Count > 0)
            {
                sorted["args"] = CanonicalizeNode(form.args);
            }

            // meta: omit if null or empty
            if (form.meta != null && form.meta.Count > 0)
            {
                sorted["meta"] = CanonicalizeNode(form.meta);
            }

            return JsonSerializer.Serialize(sorted);
        }

        /// <summary>
        /// Recursively transforms input into a structure ready for canonical serialization.
        /// </summary>
        internal static object? CanonicalizeNode(object? node)
        {
            if (node == null) return null;

            // Numeric check: Prohibit floats
            if (node is float || node is double || node is decimal)
            {
                throw new InvalidOperationException("FLOATS_PROHIBITED: Deterministic forms must use integers or fixed-point objects.");
            }

            if (node is IDictionary dict)
            {
                // Check for Fixed-Point pattern: {"v": <int64>, "s": <int64>}
                if (dict.Contains("s") && dict.Count <= 3) // Typically just v and s, maybe a type marker
                {
                    object? sVal = dict["s"];
                    if (sVal != null)
                    {
                        long s = Convert.ToInt64(sVal);
                        if (s != 1000000)
                        {
                            throw new InvalidOperationException($"BAD_SCALE: Fixed-point scale must be 1,000,000. Found: {s}");
                        }
                    }
                }

                var sortedDict = new SortedDictionary<string, object?>(StringComparer.Ordinal);
                foreach (DictionaryEntry entry in dict)
                {
                    string key = entry.Key?.ToString() ?? string.Empty;
                    object? val = entry.Value;
                    
                    // Absence over Null rule
                    if (val == null) continue;

                    sortedDict[key] = CanonicalizeNode(val);
                }
                return sortedDict;
            }

            if (node is IEnumerable list && !(node is string))
            {
                var canonicalList = new List<object?>();
                foreach (var item in list)
                {
                    canonicalList.Add(CanonicalizeNode(item));
                }
                return canonicalList;
            }

            return node;
        }
    }
}
