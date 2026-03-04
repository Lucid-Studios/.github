using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Oan.Core.Engrams
{
    public sealed record UptakeRef
    {
        public required string Origin { get; init; } // "GEL" | "GOA" | "CGEL" | "CGOA" | "SGEL"
        public required string RefId { get; init; }  // EngramId or sidecar id, OR hash fingerprint for SGEL
    }

    public sealed record UptakePlan
    {
        public required string OmegaCandidateId { get; init; }
        public string? ResidueSetId { get; init; }
        public required IReadOnlyList<UptakeRef> PsiPlus { get; init; }
        public required IReadOnlyList<UptakeRef> PsiMinus { get; init; }
        public string? SelfPolicyFingerprint { get; init; }
        public required string PolicyVersion { get; init; }

        public string UptakePlanId
        {
            get
            {
                var sortedPlus = PsiPlus.OrderBy(r => r.Origin, StringComparer.Ordinal).ThenBy(r => r.RefId, StringComparer.Ordinal).ToList();
                var sortedMinus = PsiMinus.OrderBy(r => r.Origin, StringComparer.Ordinal).ThenBy(r => r.RefId, StringComparer.Ordinal).ToList();

                var sb = new StringBuilder();
                sb.Append(OmegaCandidateId);
                sb.Append(ResidueSetId ?? "");
                foreach (var r in sortedPlus) { sb.Append(r.Origin); sb.Append(r.RefId); }
                foreach (var r in sortedMinus) { sb.Append(r.Origin); sb.Append(r.RefId); }
                sb.Append(SelfPolicyFingerprint ?? "");
                sb.Append(PolicyVersion);

                using var sha = SHA256.Create();
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                var res = new StringBuilder(hash.Length * 2);
                foreach (var b in hash) res.Append(b.ToString("x2"));
                return res.ToString();
            }
        }
    }
}
