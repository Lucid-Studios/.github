using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Oan.Core.Engrams;
using Oan.AgentiCore.Engrams;

namespace Oan.Tests.Engrams
{
    public class RailsCompletionTests
    {
        private readonly EngramStore _store;
        private readonly IGovernanceKernel _kernel;
        private readonly ThetaSealService _thetaService;

        public RailsCompletionTests()
        {
            _store = new EngramStore();
            _kernel = new DeterministicGovernanceKernel();
            _thetaService = new ThetaSealService(_store, _kernel);
        }

        [Fact]
        public void ResidueSet_Id_Is_Deterministic()
        {
            var rs1 = new ResidueSet
            {
                OmegaCandidateId = "omega1",
                CradleId = "c1",
                ContextId = "ctx1",
                TheaterId = "t1",
                TheaterMode = "Prime",
                FormationLevel = "HigherFormation",
                ResidueHashes = new List<string> { "h1", "h2" },
                PolicyVersion = "1"
            };

            var rs2 = new ResidueSet
            {
                OmegaCandidateId = "omega1",
                CradleId = "c1",
                ContextId = "ctx1",
                TheaterId = "t1",
                TheaterMode = "Prime",
                FormationLevel = "HigherFormation",
                ResidueHashes = new List<string> { "h2", "h1" }, // Reordered
                PolicyVersion = "1"
            };

            Assert.Equal(rs1.ResidueSetId, rs2.ResidueSetId);
            Assert.NotEmpty(rs1.ResidueSetId);
        }

        [Fact]
        public void UptakePlan_Id_Is_Deterministic()
        {
            var plan1 = new UptakePlan
            {
                OmegaCandidateId = "omega1",
                PsiPlus = new List<UptakeRef> { new UptakeRef { Origin = "GEL", RefId = "e1" }, new UptakeRef { Origin = "GOA", RefId = "e2" } },
                PsiMinus = new List<UptakeRef>(),
                PolicyVersion = "1"
            };

            var plan2 = new UptakePlan
            {
                OmegaCandidateId = "omega1",
                PsiPlus = new List<UptakeRef> { new UptakeRef { Origin = "GOA", RefId = "e2" }, new UptakeRef { Origin = "GEL", RefId = "e1" } }, // Reordered
                PsiMinus = new List<UptakeRef>(),
                PolicyVersion = "1"
            };

            Assert.Equal(plan1.UptakePlanId, plan2.UptakePlanId);
        }

        [Fact]
        public void ThetaSeal_Does_Not_Change_EngramHash()
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
                EngramId = "hash-pre", // Placeholder
                Hash = "hash-pre"
            };

            // 1. Initial Hash
            var hashValueBefore = EngramCanonicalizer.ComputeHash(EngramCanonicalizer.Serialize(block));

            // 2. Seal
            _thetaService.SealTheta(block, "plan1", "residue1", "op1", "s1", 101);

            // 3. Hash After
            var hashValueAfter = EngramCanonicalizer.ComputeHash(EngramCanonicalizer.Serialize(block));

            Assert.Equal(hashValueBefore, hashValueAfter);
            Assert.True(block.Header.IsThetaSealed);
        }

        [Fact]
        public void ThetaSeal_Rejects_InvalidTier()
        {
            var header = new EngramBlockHeader
            {
                PolicyVersion = "1",
                Tick = 100,
                SessionId = "s1",
                OperatorId = "op1",
                ArchiveTier = ArchiveTier.GOA, // NOT GEL
                Channel = EngramChannel.GOA,
                RoutingReason = "Test",
                RootId = "root",
                ConstructionTier = ConstructionTier.Root,
                OpalRootId = "opal",
                TheaterMode = "Prime",
                FormationLevel = "HigherFormation"
            };

            var block = new EngramBlock
            {
                Header = header,
                Factors = new List<EngramFactor>(),
                Refs = new List<string>(),
                EngramId = "h",
                Hash = "h"
            };

            Assert.Throws<InvalidOperationException>(() => _thetaService.SealTheta(block, "plan1", null, "op1", "s1", 101));
        }

        [Fact]
        public void SgelFingerprint_Is_Deterministic()
        {
            var f1 = SgelFingerprintService.ComputeSelfPolicyFingerprint("oe1", "mos1", "hand1", "p1");
            var f2 = SgelFingerprintService.ComputeSelfPolicyFingerprint("oe1", "mos1", "hand1", "p1");

            Assert.Equal(f1, f2);
            Assert.NotEmpty(f1);
        }

        [Fact]
        public void EngramStore_GOA_Sidecars_Work()
        {
            var rs = new ResidueSet
            {
                OmegaCandidateId = "omega1",
                CradleId = "c1",
                ContextId = "ctx1",
                TheaterId = "t1",
                TheaterMode = "Prime",
                FormationLevel = "HigherFormation",
                ResidueHashes = new List<string> { "h1" },
                PolicyVersion = "1"
            };
            
            _store.AppendResidueSet(rs);
            var retrieved = _store.GetResidueSet(rs.ResidueSetId);
            
            Assert.NotNull(retrieved);
            Assert.Equal(rs.OmegaCandidateId, retrieved.OmegaCandidateId);
        }
    }
}
