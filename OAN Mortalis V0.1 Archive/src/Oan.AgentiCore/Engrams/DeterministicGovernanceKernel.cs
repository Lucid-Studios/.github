using System;
using System.Security.Cryptography;
using System.Text;
using Oan.Core.Engrams;

namespace Oan.AgentiCore.Engrams
{
    public interface IGovernanceKernel
    {
        GovernanceDecision Evaluate(GovernanceRequest req);
    }

    public sealed class DeterministicGovernanceKernel : IGovernanceKernel
    {
        public string IssuerId => "ApiKernel-v0.1";
        public string PolicyVersion => "1.0";

        public GovernanceDecision Evaluate(GovernanceRequest req)
        {
            if (req == null) throw new ArgumentNullException(nameof(req));

            var verdict = GovernanceVerdict.Allow;
            var reasonCode = "OK";
            IdentityScope? requiredScope = null;
            ArchiveTier? requiredTier = null;

            bool hasOePrivilege = !string.IsNullOrEmpty(req.SgelFingerprint) || 
                                 !string.IsNullOrEmpty(req.CrypticHandshakeFingerprint);

            // 1. Universal binding guard
            if ((req.IsBindingAttempt || req.IsPromotion) && !hasOePrivilege)
            {
                Console.WriteLine($"[Kernel] Bind Guard REASON: SrcMode={req.SourceTheaterMode}, SrcForm={req.SourceFormationLevel}, Target={req.TargetArchiveTier}");
                if (req.SourceTheaterMode != "Prime" || req.SourceFormationLevel != "HigherFormation")
                {
                    verdict = GovernanceVerdict.Deny;
                    reasonCode = "BIND_GUARD_FAIL";
                }
            }

            // 2. Tier guards
            if (verdict == GovernanceVerdict.Allow && (req.TargetArchiveTier == ArchiveTier.GOA || req.TargetArchiveTier == ArchiveTier.CGEL))
            {
                
                if (!hasOePrivilege)
                {
                    verdict = GovernanceVerdict.Deny;
                    reasonCode = "OE_REQUIRED";
                }
            }

            if (verdict == GovernanceVerdict.Allow && req.SourceArchiveTier == ArchiveTier.CGEL && req.TargetArchiveTier == ArchiveTier.GEL)
            {
                verdict = GovernanceVerdict.Deny;
                reasonCode = "CGEL_TO_GEL_FORBIDDEN";
            }

            // 3. ThetaSeal
            if (verdict == GovernanceVerdict.Allow && req.Kind == GovernanceOpKind.ThetaSeal)
            {
                if (req.TargetArchiveTier != ArchiveTier.GEL)
                {
                    verdict = GovernanceVerdict.Deny;
                    reasonCode = "THETA_TIER_INVALID";
                }
                else if (string.IsNullOrEmpty(req.UptakePlanId))
                {
                    verdict = GovernanceVerdict.Deny;
                    reasonCode = "UPTAKE_REQUIRED";
                }
            }

            // 4. CrossCradleGlue
            if (verdict == GovernanceVerdict.Allow && req.Kind == GovernanceOpKind.CrossCradleGlue)
            {
                if (string.IsNullOrEmpty(req.MorphismId))
                {
                    verdict = GovernanceVerdict.Deny;
                    reasonCode = "MORPHISM_MISSING";
                }
            }

            // Deterministic IDs
            var reqCanonical = GovernanceRequestCanonicalizer.Serialize(req);
            var reqFingerprint = GovernanceRequestCanonicalizer.ComputeFingerprint(reqCanonical);

            var decisionInput = reqFingerprint 
                               + verdict.ToString() 
                               + reasonCode 
                               + (requiredScope?.ToString() ?? "") 
                               + (requiredTier?.ToString() ?? "") 
                               + PolicyVersion;

            string decisionId;
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(decisionInput);
                var hashBytes = sha.ComputeHash(bytes);
                var sb = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes) sb.Append(b.ToString("x2"));
                decisionId = sb.ToString();
            }

            var policyFingerprintInput = PolicyVersion + decisionId;
            string policyFingerprint;
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(policyFingerprintInput);
                var hashBytes = sha.ComputeHash(bytes);
                var sb = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes) sb.Append(b.ToString("x2"));
                policyFingerprint = sb.ToString();
            }

            return new GovernanceDecision
            {
                DecisionId = decisionId,
                IssuerKind = GovernanceIssuerKind.ApiKernel,
                IssuerId = IssuerId,
                Verdict = verdict,
                ReasonCode = reasonCode,
                RequiredScope = requiredScope,
                RequiredTier = requiredTier,
                PolicyVersion = PolicyVersion,
                PolicyFingerprint = policyFingerprint,
                RequestFingerprint = reqFingerprint,
                Tick = req.Tick
            };
        }
    }
}
