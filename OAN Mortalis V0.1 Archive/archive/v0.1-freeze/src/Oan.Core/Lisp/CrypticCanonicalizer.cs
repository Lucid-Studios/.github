using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Oan.Core.Lisp
{
    public static class CrypticCanonicalizer
    {
        private static readonly JsonSerializerOptions _deterministicOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IgnoreReadOnlyProperties = false
        };

        public static string SerializePointer(CrypticPointer p)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            if (string.IsNullOrEmpty(p.pointer)) throw new ArgumentException("MANDATORY_FIELD_MISSING: pointer");

            var sorted = new SortedDictionary<string, object?>(StringComparer.Ordinal);
            sorted["pointer"] = p.pointer;
            sorted["tier"] = SerializeTier(p.tier);
            
            if (!string.IsNullOrEmpty(p.hint))
            {
                sorted["hint"] = p.hint;
            }

            return JsonSerializer.Serialize(sorted, _deterministicOptions);
        }

        public static string SerializeEmission(CrypticEmission e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (string.IsNullOrEmpty(e.kind)) throw new ArgumentException("MANDATORY_FIELD_MISSING: kind");
            if (string.IsNullOrEmpty(e.payload_hash)) throw new ArgumentException("MANDATORY_FIELD_MISSING: payload_hash");

            var sorted = new SortedDictionary<string, object?>(StringComparer.Ordinal);
            sorted["kind"] = e.kind;
            sorted["payload_hash"] = e.payload_hash;
            sorted["tier"] = SerializeTier(e.tier);

            if (e.pointers != null && e.pointers.Count > 0)
            {
                var canonicalPointers = new List<object>();
                foreach (var p in e.pointers)
                {
                    // Recursively canonicalize pointer nodes
                    var pDict = new SortedDictionary<string, object?>(StringComparer.Ordinal);
                    pDict["pointer"] = p.pointer;
                    pDict["tier"] = SerializeTier(p.tier);
                    if (!string.IsNullOrEmpty(p.hint)) pDict["hint"] = p.hint;
                    canonicalPointers.Add(pDict);
                }
                sorted["pointers"] = canonicalPointers;
            }

            if (!string.IsNullOrEmpty(e.notes))
            {
                sorted["notes"] = e.notes;
            }

            if (e.tick.HasValue)
            {
                sorted["tick"] = e.tick.Value;
            }

            return JsonSerializer.Serialize(sorted, _deterministicOptions);
        }

        public static string SerializeAccessLog(AccessLogEvent e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (string.IsNullOrEmpty(e.operator_id)) throw new ArgumentException("MANDATORY_FIELD_MISSING: operator_id");
            if (string.IsNullOrEmpty(e.action)) throw new ArgumentException("MANDATORY_FIELD_MISSING: action");
            if (string.IsNullOrEmpty(e.target)) throw new ArgumentException("MANDATORY_FIELD_MISSING: target");

            var sorted = new SortedDictionary<string, object?>(StringComparer.Ordinal);
            sorted["action"] = e.action;
            sorted["operator_id"] = e.operator_id;
            sorted["target"] = e.target;
            sorted["tier"] = SerializeTier(e.tier);
            sorted["tick"] = e.tick;

            return JsonSerializer.Serialize(sorted, _deterministicOptions);
        }

        public static string SerializeFreeze(FreezeDirective d)
        {
            if (d == null) throw new ArgumentNullException(nameof(d));
            if (string.IsNullOrEmpty(d.reason_code)) throw new ArgumentException("MANDATORY_FIELD_MISSING: reason_code");

            var sorted = new SortedDictionary<string, object?>(StringComparer.Ordinal);
            sorted["is_frozen"] = d.is_frozen;
            sorted["reason_code"] = d.reason_code;
            sorted["tick"] = d.tick;

            return JsonSerializer.Serialize(sorted, _deterministicOptions);
        }

        private static string SerializeTier(CrypticTier tier) => tier switch
        {
            CrypticTier.GEL => "GEL",
            CrypticTier.GoA => "GoA",
            CrypticTier.SGEL => "SelfGEL",
            CrypticTier.CGEL => "cGEL",
            CrypticTier.CGoA => "cGoA",
            CrypticTier.cSGEL => "cSelfGEL",
            _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, null)
        };
    }
}
