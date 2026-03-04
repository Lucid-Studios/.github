using System;
using Oan.Core.Engrams;

namespace Oan.AgentiCore.Engrams
{
    public sealed class ThetaSealService
    {
        private readonly EngramStore _store;
        private readonly IGovernanceKernel _kernel;

        public ThetaSealService(EngramStore store, IGovernanceKernel kernel)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        }

        public void SealTheta(EngramBlock theta, string uptakePlanId, string? residueSetId, string operatorId, string sessionId, long tick)
        {
            if (theta == null) throw new ArgumentNullException(nameof(theta));
            if (string.IsNullOrEmpty(uptakePlanId)) throw new ArgumentException("UptakePlanId is mandatory for ThetaSeal.", nameof(uptakePlanId));

            // --- GOVERNANCE KERNEL EVALUATION ---
            var govReq = new GovernanceRequest
            {
                Kind = GovernanceOpKind.ThetaSeal,
                SessionId = sessionId,
                ScenarioName = "ThetaSeal",
                OperatorId = operatorId,
                Tick = tick,

                SourceCradleId = theta.Header.CradleId ?? "Unknown",
                SourceContextId = theta.Header.ContextId ?? "Unknown",
                SourceTheaterId = theta.Header.TheaterId ?? "Unknown",
                SourceTheaterMode = theta.Header.TheaterMode ?? "Idle",
                SourceFormationLevel = theta.Header.FormationLevel ?? "HigherFormation",
                SourceArchiveTier = theta.Header.ArchiveTier,

                TargetCradleId = theta.Header.CradleId ?? "Unknown",
                TargetContextId = theta.Header.ContextId ?? "Unknown",
                TargetTheaterId = theta.Header.TheaterId ?? "Unknown",
                TargetTheaterMode = theta.Header.TheaterMode ?? "Idle",
                TargetFormationLevel = theta.Header.FormationLevel ?? "HigherFormation",
                TargetArchiveTier = theta.Header.ArchiveTier,

                UptakePlanId = uptakePlanId,
                ResidueSetId = residueSetId,
                ThetaCandidateEngramId = theta.EngramId,
                PolicyVersion = theta.Header.PolicyVersion
            };

            var decision = _kernel.Evaluate(govReq);
            _store.AppendGovernanceDecision(decision);

            if (decision.Verdict == GovernanceVerdict.Deny)
            {
                throw new InvalidOperationException($"Governance Denied: {decision.ReasonCode}");
            }
            // ------------------------------------

            // Enforce tier rules: Final paid record must be in GEL
            if (theta.Header.ArchiveTier != ArchiveTier.GEL)
            {
                throw new InvalidOperationException($"ThetaSeal requires GEL tier. Current tier: {theta.Header.ArchiveTier}");
            }

            // Universal binding/promotion guard check (implicitly verified during formation, but re-assert here for safety)
            if (theta.Header.TheaterMode != "Prime" || theta.Header.FormationLevel != "HigherFormation")
            {
                throw new InvalidOperationException("ThetaSeal identifies a 'paid record' which must be formed in Prime/HigherFormation.");
            }

            // Seal the block
            theta.Header.UptakePlanId = uptakePlanId;
            theta.Header.ResidueSetId = residueSetId;
            theta.Header.IsThetaSealed = true;
        }
    }
}
