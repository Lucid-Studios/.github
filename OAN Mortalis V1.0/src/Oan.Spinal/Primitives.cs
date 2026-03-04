using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Oan.Spinal
{
    public static class Primitives
    {
        public static string ComputeHash(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            
            using var sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(bytes);
            
            var sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }

        public static string ToCanonicalJson(object obj)
        {
            // Simple deterministic serialization for v1.0 initial milestone
            // Use System.Text.Json with sorted property names if possible,
            // or manual for internal types.
            return System.Text.Json.JsonSerializer.Serialize(obj, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }
    }
}
