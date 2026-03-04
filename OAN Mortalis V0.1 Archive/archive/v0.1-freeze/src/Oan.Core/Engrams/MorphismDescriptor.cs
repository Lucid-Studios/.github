using System;
using System.Security.Cryptography;
using System.Text;

namespace Oan.Core.Engrams
{
    public sealed record MorphismDescriptor
    {
        public required string Kind { get; init; }           // e.g. "CrossCradleGlue"
        public required string PolicyVersion { get; init; }  // ties to theater policy / transport policy

        // Deterministic ID: SHA256(Kind + "|" + PolicyVersion)
        public string MorphismId 
        {
            get
            {
                var input = $"{Kind}|{PolicyVersion}";
                using var sha = SHA256.Create();
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
