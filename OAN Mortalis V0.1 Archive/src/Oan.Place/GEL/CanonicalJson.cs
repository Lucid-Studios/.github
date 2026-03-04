using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Encodings.Web;

namespace Oan.Place.GEL
{
    public static class CanonicalJson
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Prompt: "Use standard JSON escaping only". UnsafeRelaxed allows + and others to be literal, which is standard JSON. Default is overly strict. But prompt said "only (System.Text.Json)". I'll stick to strict default to be safe unless "No custom escaping" implies relaxed?
            // Prompt: "Use standard JSON escaping only (System.Text.Json). No custom escaping layer."
            // Default escapes '<', '>', '&', '+', '\'', etc.
            // If I use default, it is standard System.Text.Json.
        };

        private static readonly JsonSerializerOptions _strictOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public static string Serialize<T>(T value)
        {
            var node = JsonSerializer.SerializeToNode(value, _strictOptions);
            var sorted = SortKeys(node);
            return sorted?.ToJsonString(_strictOptions) ?? "null";
        }

        public static string Canon(string input)
        {
            // Just for strings if needed? No, usually we serialize objects.
            return Serialize(input);
        }

        private static JsonNode? SortKeys(JsonNode? node)
        {
            if (node is JsonObject obj)
            {
                var newObj = new JsonObject();
                // Extract properties and sort by key Ordinal
                var sortedProps = obj.ToList().OrderBy(kvp => kvp.Key, StringComparer.Ordinal);
                
                foreach (var kvp in sortedProps)
                {
                    // Recursively sort values
                    // Detach value from parent to attach to new parent?
                    // JsonNode can only have one parent. We need to clone or detach.
                    // Detach:
                    var val = kvp.Value;
                    obj.Remove(kvp.Key); // Remove from old
                    newObj.Add(kvp.Key, SortKeys(val));
                }
                return newObj;
            }
            else if (node is JsonArray arr)
            {
                // Process children recursively, but do NOT sort the array itself (handled by semantic logic)
                var newArr = new JsonArray();
                // Copy elements
                // Can't iterate strongly typed?
                // Pop elements?
                var elements = arr.ToList();
                arr.Clear();
                foreach (var elem in elements)
                {
                    // Detach handled by Clear?
                    // elem is detached if arr cleared? No, elem.Parent is null?
                    // Safe way:
                    if (elem != null) 
                    {
                        // SerializeToNode creates new nodes.
                        // But recursive call might fail if node attached.
                        // Let's rely on DeepClone if needed, or Detach.
                        // Since we are rebuilding, detach is fine.
                        // But we already retrieved list.
                         newArr.Add(SortKeys(elem));
                    }
                    else
                    {
                        newArr.Add(null);
                    }
                }
                return newArr;
            }
            return node;
        }
    }
}
