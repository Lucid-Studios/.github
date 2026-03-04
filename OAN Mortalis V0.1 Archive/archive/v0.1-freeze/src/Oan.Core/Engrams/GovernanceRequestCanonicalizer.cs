using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Oan.Core.Engrams
{
    public static class GovernanceRequestCanonicalizer
    {
        public static string Serialize(GovernanceRequest req)
        {
            var sb = new StringBuilder();
            
            // Strict alphabetical field ordering
            sb.Append("CrypticHandshakeFingerprint:").Append(req.CrypticHandshakeFingerprint ?? "").Append('\n');
            sb.Append("IsBindingAttempt:").Append(req.IsBindingAttempt.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()).Append('\n');
            sb.Append("IsCrossCradle:").Append(req.IsCrossCradle.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()).Append('\n');
            sb.Append("IsPromotion:").Append(req.IsPromotion.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()).Append('\n');
            sb.Append("Kind:").Append(req.Kind.ToString()).Append('\n');
            sb.Append("MorphismId:").Append(req.MorphismId ?? "").Append('\n');
            sb.Append("OperatorId:").Append(req.OperatorId).Append('\n');
            sb.Append("PolicyVersion:").Append(req.PolicyVersion).Append('\n');
            sb.Append("ResidueSetId:").Append(req.ResidueSetId ?? "").Append('\n');
            sb.Append("ScenarioName:").Append(req.ScenarioName).Append('\n');
            sb.Append("SgelFingerprint:").Append(req.SgelFingerprint ?? "").Append('\n');
            sb.Append("SessionId:").Append(req.SessionId).Append('\n');
            
            sb.Append("SourceArchiveTier:").Append(req.SourceArchiveTier.ToString()).Append('\n');
            sb.Append("SourceContextId:").Append(req.SourceContextId).Append('\n');
            sb.Append("SourceCradleId:").Append(req.SourceCradleId).Append('\n');
            sb.Append("SourceFormationLevel:").Append(req.SourceFormationLevel).Append('\n');
            sb.Append("SourceTheaterId:").Append(req.SourceTheaterId).Append('\n');
            sb.Append("SourceTheaterMode:").Append(req.SourceTheaterMode).Append('\n');

            sb.Append("TargetArchiveTier:").Append(req.TargetArchiveTier.ToString()).Append('\n');
            sb.Append("TargetContextId:").Append(req.TargetContextId).Append('\n');
            sb.Append("TargetCradleId:").Append(req.TargetCradleId).Append('\n');
            sb.Append("TargetFormationLevel:").Append(req.TargetFormationLevel).Append('\n');
            sb.Append("TargetTheaterId:").Append(req.TargetTheaterId).Append('\n');
            sb.Append("TargetTheaterMode:").Append(req.TargetTheaterMode).Append('\n');
            
            sb.Append("ThetaCandidateEngramId:").Append(req.ThetaCandidateEngramId ?? "").Append('\n');
            sb.Append("Tick:").Append(req.Tick.ToString(CultureInfo.InvariantCulture)).Append('\n');
            sb.Append("UptakePlanId:").Append(req.UptakePlanId ?? "").Append('\n');

            return sb.ToString();
        }

        public static string ComputeFingerprint(string canonicalString)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(canonicalString);
            var hashBytes = sha256.ComputeHash(bytes);
            return ToHexString(hashBytes);
        }

        private static string ToHexString(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
