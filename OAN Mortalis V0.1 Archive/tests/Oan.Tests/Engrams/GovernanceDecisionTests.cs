using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Oan.Core.Engrams;
using Oan.AgentiCore.Engrams;

namespace Oan.Tests.Engrams
{
    public class GovernanceDecisionTests
    {
        [Fact]
        public void GovernanceRequestFingerprint_Is_Deterministic()
        {
            var req1 = new GovernanceRequest
            {
                Kind = GovernanceOpKind.CrossCradleGlue,
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
                TargetCradleId = "c2",
                TargetContextId = "ctx2",
                TargetTheaterId = "t2",
                TargetTheaterMode = "Prime",
                TargetFormationLevel = "HigherFormation",
                TargetArchiveTier = ArchiveTier.GEL,
                PolicyVersion = "1"
            };

            var req2 = new GovernanceRequest
            {
                Kind = GovernanceOpKind.CrossCradleGlue,
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
                TargetCradleId = "c2",
                TargetContextId = "ctx2",
                TargetTheaterId = "t2",
                TargetTheaterMode = "Prime",
                TargetFormationLevel = "HigherFormation",
                TargetArchiveTier = ArchiveTier.GEL,
                PolicyVersion = "1"
            };

            var f1 = GovernanceRequestCanonicalizer.ComputeFingerprint(GovernanceRequestCanonicalizer.Serialize(req1));
            var f2 = GovernanceRequestCanonicalizer.ComputeFingerprint(GovernanceRequestCanonicalizer.Serialize(req2));

            Assert.Equal(f1, f2);
            Assert.NotEmpty(f1);
        }

        [Fact]
        public void GovernanceDecision_Does_Not_Affect_EngramHash()
        {
            var header = new EngramBlockHeader
            {
                PolicyVersion = "1",
                Tick = 100,
                SessionId = "s1",
                OperatorId = "op1",
                CradleId = "c1",
                TheaterId = "t1",
                TheaterMode = "Prime",
                FormationLevel = "HigherFormation",
                ArchiveTier = ArchiveTier.GEL,
                Channel = EngramChannel.SelfGEL,
                RoutingReason = "Test",
                RootId = "root",
                ConstructionTier = ConstructionTier.Root,
                OpalRootId = "opal"
            };

            var block = new EngramBlock
            {
                Header = header,
                Factors = new List<EngramFactor>(),
                Refs = new List<string>(),
                EngramId = "hash-pre", 
                Hash = "hash-pre"
            };

            var hashBefore = EngramCanonicalizer.ComputeHash(EngramCanonicalizer.Serialize(block));

            // Append decisions to store (not part of block)
            var store = new EngramStore();
            var kernel = new DeterministicGovernanceKernel();
            var req = new GovernanceRequest
            {
                Kind = GovernanceOpKind.ThetaSeal,
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
                PolicyVersion = "1",
                UptakePlanId = "up1",
                ThetaCandidateEngramId = block.EngramId
            };
            
            var decision = kernel.Evaluate(req);
            store.AppendGovernanceDecision(decision);

            var hashAfter = EngramCanonicalizer.ComputeHash(EngramCanonicalizer.Serialize(block));

            Assert.Equal(hashBefore, hashAfter);
        }
    }
}
