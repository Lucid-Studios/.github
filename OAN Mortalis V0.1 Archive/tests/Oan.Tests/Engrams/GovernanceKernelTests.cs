using System;
using System.Collections.Generic;
using Xunit;
using Oan.Core.Engrams;
using Oan.AgentiCore.Engrams;

namespace Oan.Tests.Engrams
{
    public class GovernanceKernelTests
    {
        private readonly IGovernanceKernel _kernel;

        public GovernanceKernelTests()
        {
            _kernel = new DeterministicGovernanceKernel();
        }

        private GovernanceRequest CreateBaseRequest(GovernanceOpKind kind)
        {
            return new GovernanceRequest
            {
                Kind = kind,
                SessionId = "s1",
                ScenarioName = "sc1",
                OperatorId = "op1",
                Tick = 100,
                SourceCradleId = "c1",
                SourceContextId = "ctx1",
                SourceTheaterId = "t1",
                SourceTheaterMode = "Prime",
                SourceFormationLevel = "HigherFormation",
                SourceArchiveTier = ArchiveTier.GEL,
                TargetCradleId = "c1",
                TargetContextId = "ctx1",
                TargetTheaterId = "t1",
                TargetTheaterMode = "Prime",
                TargetFormationLevel = "HigherFormation",
                TargetArchiveTier = ArchiveTier.GEL,
                PolicyVersion = "1"
            };
        }

        [Fact]
        public void Deny_BindingAttempt_When_Not_Prime()
        {
            var req = CreateBaseRequest(GovernanceOpKind.BindAttempt) with
            {
                IsBindingAttempt = true,
                SourceTheaterMode = "Idle"
            };

            var decision = _kernel.Evaluate(req);
            Assert.Equal(GovernanceVerdict.Deny, decision.Verdict);
            Assert.Equal("BIND_GUARD_FAIL", decision.ReasonCode);
        }

        [Fact]
        public void Deny_Goa_Target_When_No_OE_Privilege()
        {
            var req = CreateBaseRequest(GovernanceOpKind.ResidueCast) with
            {
                TargetArchiveTier = ArchiveTier.GOA,
                SgelFingerprint = null,
                CrypticHandshakeFingerprint = null
            };

            var decision = _kernel.Evaluate(req);
            Assert.Equal(GovernanceVerdict.Deny, decision.Verdict);
            Assert.Equal("OE_REQUIRED", decision.ReasonCode);
        }

        [Fact]
        public void Allow_Goa_Target_With_OE_Privilege()
        {
            var req = CreateBaseRequest(GovernanceOpKind.ResidueCast) with
            {
                TargetArchiveTier = ArchiveTier.GOA,
                SgelFingerprint = "fingerprint-abc"
            };

            var decision = _kernel.Evaluate(req);
            Assert.Equal(GovernanceVerdict.Allow, decision.Verdict);
        }

        [Fact]
        public void Deny_Cgel_To_Gel_Direct()
        {
            var req = CreateBaseRequest(GovernanceOpKind.CrossCradleGlue) with
            {
                SourceArchiveTier = ArchiveTier.CGEL,
                TargetArchiveTier = ArchiveTier.GEL,
                MorphismId = "m1"
            };

            var decision = _kernel.Evaluate(req);
            Assert.Equal(GovernanceVerdict.Deny, decision.Verdict);
            Assert.Equal("CGEL_TO_GEL_FORBIDDEN", decision.ReasonCode);
        }

        [Fact]
        public void Deny_ThetaSeal_When_Not_GEL()
        {
            var req = CreateBaseRequest(GovernanceOpKind.ThetaSeal) with
            {
                TargetArchiveTier = ArchiveTier.GOA,
                SgelFingerprint = "oe-priv"
            };

            var decision = _kernel.Evaluate(req);
            Assert.Equal(GovernanceVerdict.Deny, decision.Verdict);
            Assert.Equal("THETA_TIER_INVALID", decision.ReasonCode);
        }

        [Fact]
        public void Deny_ThetaSeal_When_UptakePlan_Missing()
        {
            var req = CreateBaseRequest(GovernanceOpKind.ThetaSeal) with
            {
                TargetArchiveTier = ArchiveTier.GEL,
                UptakePlanId = null
            };

            var decision = _kernel.Evaluate(req);
            Assert.Equal(GovernanceVerdict.Deny, decision.Verdict);
            Assert.Equal("UPTAKE_REQUIRED", decision.ReasonCode);
        }
    }
}
