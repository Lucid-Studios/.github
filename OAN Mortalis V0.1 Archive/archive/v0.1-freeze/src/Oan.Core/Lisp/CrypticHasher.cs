using System;
using System.Security.Cryptography;
using System.Text;

namespace Oan.Core.Lisp
{
    public static class CrypticHasher
    {
        public static string HashCanonicalJson(string canonicalJson)
        {
            if (canonicalJson == null) throw new ArgumentNullException(nameof(canonicalJson));

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(canonicalJson);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToHexString(hashBytes).ToLowerInvariant();
            }
        }

        public static string HashAccessLog(AccessLogEvent e)
        {
            string json = CrypticCanonicalizer.SerializeAccessLog(e);
            return HashCanonicalJson(json);
        }

        public static string HashFreeze(FreezeDirective d)
        {
            string json = CrypticCanonicalizer.SerializeFreeze(d);
            return HashCanonicalJson(json);
        }

        public static string HashPointer(CrypticPointer p)
        {
            string json = CrypticCanonicalizer.SerializePointer(p);
            return HashCanonicalJson(json);
        }

        public static string HashEmission(CrypticEmission e)
        {
            string json = CrypticCanonicalizer.SerializeEmission(e);
            return HashCanonicalJson(json);
        }
    }
}
