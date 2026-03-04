using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Oan.Core.Engrams
{
    public static class EngramCanonicalizer
    {
        public static string Serialize(EngramBlock block, CanonicalProfile profile = CanonicalProfile.Intrinsic)
        {
            var sb = new StringBuilder();

            // 1. Header Fields (Sorted by Key)
            // keys: AgentProfileId, CanonicalVersion, Channel, ConstructionTier, OpalRootId, OperatorId, PolicyVersion, PreviousOpalEngramId, RootId, RoutingReason, SessionId, Tick
            
            // Allow nulls to be empty string, but key must exist.
            
            // AgentProfileId (Intrinsic)
            sb.Append("AgentProfileId:");
            sb.Append(block.Header.AgentProfileId ?? "");
            sb.Append('\n');

            // ArchiveTier (Contextual)
            if (profile == CanonicalProfile.Contextual)
            {
                sb.Append("ArchiveTier:");
                sb.Append(block.Header.ArchiveTier.ToString());
                sb.Append('\n');
            }

            // CanonicalVersion (Intrinsic)
            sb.Append("CanonicalVersion:");
            sb.Append(block.Header.CanonicalVersion ?? "1");
            sb.Append('\n');

            // Channel (Intrinsic)
            sb.Append("Channel:");
            sb.Append(block.Header.Channel.ToString());
            sb.Append('\n');
            
            // ContextId (Contextual)
            if (profile == CanonicalProfile.Contextual)
            {
                sb.Append("ContextId:");
                sb.Append(block.Header.ContextId ?? "");
                sb.Append('\n');
            }

            // CradleId (Contextual)
            if (profile == CanonicalProfile.Contextual)
            {
                sb.Append("CradleId:");
                sb.Append(block.Header.CradleId ?? "");
                sb.Append('\n');
            }

            // FormationLevel (Contextual)
            if (profile == CanonicalProfile.Contextual)
            {
                sb.Append("FormationLevel:");
                sb.Append(block.Header.FormationLevel ?? "");
                sb.Append('\n');
            }
            
            // ConstructionTier (Intrinsic)
            sb.Append("ConstructionTier:");
            sb.Append(block.Header.ConstructionTier.ToString());
            sb.Append('\n');

            // OpalRootId (Intrinsic)
            sb.Append("OpalRootId:");
            sb.Append(block.Header.OpalRootId);
            sb.Append('\n');

            // OperatorId (Contextual)
            if (profile == CanonicalProfile.Contextual)
            {
                sb.Append("OperatorId:");
                sb.Append(block.Header.OperatorId);
                sb.Append('\n');
            }

            // PolicyVersion (Intrinsic)
            sb.Append("PolicyVersion:");
            sb.Append(block.Header.PolicyVersion);
            sb.Append('\n');

            // PreviousOpalEngramId (Intrinsic)
            sb.Append("PreviousOpalEngramId:");
            sb.Append(block.Header.PreviousOpalEngramId ?? "");
            sb.Append('\n');

            // RootId (Intrinsic)
            sb.Append("RootId:");
            sb.Append(block.Header.RootId);
            sb.Append('\n');

            // RoutingReason (Intrinsic)
            sb.Append("RoutingReason:");
            sb.Append(block.Header.RoutingReason);
            sb.Append('\n');

            // SessionId (Contextual)
            if (profile == CanonicalProfile.Contextual)
            {
                sb.Append("SessionId:");
                sb.Append(block.Header.SessionId);
                sb.Append('\n');
            }

            // TheaterId (Contextual)
            if (profile == CanonicalProfile.Contextual)
            {
                sb.Append("TheaterId:");
                sb.Append(block.Header.TheaterId ?? "");
                sb.Append('\n');
            }

            // TheaterMode (Contextual)
            if (profile == CanonicalProfile.Contextual)
            {
                sb.Append("TheaterMode:");
                sb.Append(block.Header.TheaterMode ?? "");
                sb.Append('\n');
            }

            // Tick (Intrinsic)
            sb.Append("Tick:");
            sb.Append(block.Header.Tick.ToString(System.Globalization.CultureInfo.InvariantCulture));
            sb.Append('\n');

            // 2. Parents (Sorted)
            sb.Append("Parents:\n");
            if (block.Header.ParentEngramIds != null)
            {
                foreach (var paramId in block.Header.ParentEngramIds.OrderBy(x => x, StringComparer.Ordinal))
                {
                    sb.Append(paramId);
                    sb.Append('\n');
                }
            }

            // 3. Factors (Sorted by Tier -> Order -> Kind -> Key -> Value)
            sb.Append("Factors:\n");
            var sortedFactors = block.Factors
                .OrderBy(f => f.Tier)
                .ThenBy(f => f.Order)
                .ThenBy(f => f.Kind)
                .ThenBy(f => f.Key, StringComparer.Ordinal)
                .ThenBy(f => f.Value, StringComparer.Ordinal); // Value should also be ordinal

            foreach (var f in sortedFactors)
            {
                // Format: Tier|Order|Kind|Key|Value|Weight
                sb.Append(f.Tier);
                sb.Append('|');
                sb.Append(f.Order);
                sb.Append('|');
                sb.Append(f.Kind);
                sb.Append('|');
                sb.Append(NormalizeString(f.Key));
                sb.Append('|');
                sb.Append(NormalizeString(f.Value));
                sb.Append('|');
                // Normalized decimal string
                sb.Append(f.Weight.ToString("G29", System.Globalization.CultureInfo.InvariantCulture)); 
                sb.Append('\n');
            }

            // 4. Refs (Sorted)
            sb.Append("Refs:\n");
            foreach (var r in block.Refs.OrderBy(x => x, StringComparer.Ordinal))
            {
                sb.Append(NormalizeString(r));
                sb.Append('\n');
            }

            return sb.ToString();
        }

        public static string ComputeHash(string canonicalString)
        {
            var bytes = Encoding.UTF8.GetBytes(canonicalString);
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(bytes);
                return ToHexString(hashBytes);
            }
        }

        private static string NormalizeString(string input)
        {
            // Replace newlines and pipes to avoid breaking the format
            // MVP: Replace \n with space, | with space.
            if (input == null) return "";
            return input.Replace('\n', ' ').Replace('|', ' ').Trim();
        }

        private static string ToHexString(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
