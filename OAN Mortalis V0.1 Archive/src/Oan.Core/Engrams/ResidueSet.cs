using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Oan.Core.Engrams
{
    public sealed record ResidueSet
    {
        public string? SourceEngramId { get; init; }
        public required string OmegaCandidateId { get; init; }
        public required string CradleId { get; init; }
        public required string ContextId { get; init; }
        public required string TheaterId { get; init; }
        public required string TheaterMode { get; init; }
        public required string FormationLevel { get; init; }
        public required IReadOnlyList<string> ResidueHashes { get; init; }
        public required string PolicyVersion { get; init; }

        public string ResidueSetId
        {
            get
            {
                var sortedResidues = ResidueHashes.OrderBy(x => x, StringComparer.Ordinal).ToList();
                var input = (SourceEngramId ?? "")
                          + OmegaCandidateId
                          + CradleId
                          + ContextId
                          + TheaterId
                          + TheaterMode
                          + FormationLevel
                          + string.Join("", sortedResidues)
                          + PolicyVersion;

                using var sha = SHA256.Create();
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (var b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        public static string HashFragment(string fragment)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(fragment));
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
