using System;
using System.Security.Cryptography;
using System.Text;

namespace Oan.Core.Engrams
{
    public static class SgelFingerprintService
    {
        // Does NOT expose SGEL contents.
        public static string ComputeSelfPolicyFingerprint(
            string oeId,
            string mosShadowCopyFingerprint,
            string crypticHandshakeFingerprint,
            string sgelPolicyVersion)
        {
            var input = oeId + mosShadowCopyFingerprint + crypticHandshakeFingerprint + sgelPolicyVersion;
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
