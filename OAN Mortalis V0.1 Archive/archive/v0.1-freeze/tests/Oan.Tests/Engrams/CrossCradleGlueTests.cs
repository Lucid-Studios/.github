using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Oan.Core.Engrams;
using Oan.AgentiCore.Engrams;

namespace Oan.Tests.Engrams
{
    public class CrossCradleGlueTests
    {
        private readonly EngramStore _store;
        private readonly IGovernanceKernel _kernel;
        private readonly CrossCradleGlueService _glue;

        public CrossCradleGlueTests()
        {
            _store = new EngramStore();
            _kernel = new DeterministicGovernanceKernel();
            _glue = new CrossCradleGlueService(_store, _kernel);
        }

        private EngramBlock CreateSourceEngram(string engramId, string cradleId, ArchiveTier tier)
        {
            var header = new EngramBlockHeader
            {
                PolicyVersion = "1",
                Tick = 100,
                SessionId = "s1",
                OperatorId = "op1",
                CradleId = cradleId,
                ArchiveTier = tier,
                Channel = EngramChannel.SelfGEL,
                RoutingReason = "Initial",
                RootId = "root1",
                ConstructionTier = ConstructionTier.Root,
                OpalRootId = "opal1",
                FormationLevel = "HigherFormation",
                TheaterMode = "Prime"
            };

            var block = new EngramBlock
            {
                Header = header,
                Factors = new List<EngramFactor>(),
                Refs = new List<string>(),
                EngramId = engramId,
                Hash = "hash1"
            };

            // We need canonical bytes for store. Append dummy.
            _store.Append(block, System.Text.Encoding.UTF8.GetBytes("canonical-data"));
            return block;
        }

        [Fact]
        public void Glue_CrossCradle_EmitsCleaveRecord()
        {
            var source = CreateSourceEngram("src1", "cradle-a", ArchiveTier.GEL);
            var morphism = new MorphismDescriptor { Kind = "CrossCradleGlue", PolicyVersion = "1" };
            
            var req = new GlueRequest
            {
                SourceEngramId = "src1",
                SourceCradleId = "cradle-a",
                SourceTier = ArchiveTier.GEL,
                TargetCradleId = "cradle-b",
                TargetTier = ArchiveTier.GEL,
                TargetTheaterMode = "Prime",
                TargetFormationLevel = "HigherFormation",
                PreferredScope = IdentityScope.Intrinsic,
                Morphism = morphism,
                OperatorId = "op1",
                SessionId = "s1",
                Tick = 101
            };

            var result = _glue.ApplyGlue(req);

            Assert.NotNull(result.CleaveId);
            var records = _store.GetCleaveRecords().ToList();
            Assert.Single(records);
            Assert.Equal(result.CleaveId, records[0].CleaveId);
            Assert.Equal("src1", records[0].SourceEngramId);
            Assert.Equal(result.ResultEngramId, records[0].ResultEngramId);
            Assert.True(records[0].IsCrossCradle);
        }

        [Fact]
        public void Glue_DoesNotMutate_SourceEngram()
        {
            var source = CreateSourceEngram("src1", "cradle-a", ArchiveTier.GEL);
            var morphism = new MorphismDescriptor { Kind = "CrossCradleGlue", PolicyVersion = "1" };

            var req = new GlueRequest
            {
                SourceEngramId = "src1",
                SourceCradleId = "cradle-a",
                SourceTier = ArchiveTier.GEL,
                TargetCradleId = "cradle-b",
                TargetTier = ArchiveTier.GEL,
                TargetTheaterMode = "Prime",
                TargetFormationLevel = "HigherFormation",
                PreferredScope = IdentityScope.Intrinsic,
                Morphism = morphism,
                OperatorId = "op1",
                SessionId = "s1",
                Tick = 101
            };

            _glue.ApplyGlue(req);

            var storedSource = _store.GetById("src1");
            Assert.Equal("cradle-a", storedSource.Header.CradleId);
            Assert.Equal(ArchiveTier.GEL, storedSource.Header.ArchiveTier);
        }

        [Fact]
        public void Glue_Produces_NewEngramId_In_Target()
        {
            var source = CreateSourceEngram("src1", "cradle-a", ArchiveTier.GEL);
            var morphism = new MorphismDescriptor { Kind = "CrossCradleGlue", PolicyVersion = "1" };

            var req = new GlueRequest
            {
                SourceEngramId = "src1",
                SourceCradleId = "cradle-a",
                SourceTier = ArchiveTier.GEL,
                TargetCradleId = "cradle-b",
                TargetTier = ArchiveTier.GEL,
                TargetTheaterMode = "Prime",
                TargetFormationLevel = "HigherFormation",
                PreferredScope = IdentityScope.Intrinsic,
                Morphism = morphism,
                OperatorId = "op1",
                SessionId = "s1",
                Tick = 101
            };

            var result = _glue.ApplyGlue(req);

            Assert.NotEqual("src1", result.ResultEngramId);
            var targetBlock = _store.GetById(result.ResultEngramId);
            Assert.NotNull(targetBlock);
            Assert.Equal("cradle-b", targetBlock.Header.CradleId);
            Assert.Contains("src1", targetBlock.Header.ParentEngramIds);
        }

        [Fact]
        public void Glue_ResolvesScope_UsingMatrix()
        {
            var source = CreateSourceEngram("src1", "cradle-a", ArchiveTier.GEL);
            var morphism = new MorphismDescriptor { Kind = "CrossCradleGlue", PolicyVersion = "1" };

            // 1. Cross-cradle transport to GOA (non-binding-class) -> Intrinsic
            var reqIntrinsic = new GlueRequest
            {
                SourceEngramId = "src1",
                SourceCradleId = "cradle-a",
                SourceTier = ArchiveTier.GEL,
                TargetCradleId = "cradle-b",
                TargetTier = ArchiveTier.GOA, 
                TargetTheaterMode = "Prime",
                TargetFormationLevel = "HigherFormation", 
                PreferredScope = IdentityScope.Contextual, 
                Morphism = morphism,
                IsOePrivileged = true,
                OperatorId = "op1",
                SessionId = "s1",
                Tick = 101
            };

            var resultIntrinsic = _glue.ApplyGlue(reqIntrinsic);
            Assert.Equal(IdentityScope.Intrinsic, resultIntrinsic.ScopeUsed);

            // 2. Cross-cradle transport to GEL (binding-class) -> Contextual
            var reqContextual = new GlueRequest
            {
                SourceEngramId = "src1",
                SourceCradleId = "cradle-a",
                SourceTier = ArchiveTier.GEL,
                TargetCradleId = "cradle-b",
                TargetTier = ArchiveTier.GEL, 
                TargetTheaterMode = "Prime",
                TargetFormationLevel = "HigherFormation", 
                PreferredScope = IdentityScope.Intrinsic, 
                Morphism = morphism,
                IsOePrivileged = true,
                OperatorId = "op1",
                SessionId = "s1",
                Tick = 102
            };

            var resultContextual = _glue.ApplyGlue(reqContextual);
            Assert.Equal(IdentityScope.Contextual, resultContextual.ScopeUsed);
        }

        [Fact]
        public void Glue_Forbids_CGEL_To_GEL_Direct()
        {
            var source = CreateSourceEngram("src1", "cradle-a", ArchiveTier.CGEL);
            var morphism = new MorphismDescriptor { Kind = "CrossCradleGlue", PolicyVersion = "1" };

            var req = new GlueRequest
            {
                SourceEngramId = "src1",
                SourceCradleId = "cradle-a",
                SourceTier = ArchiveTier.CGEL,
                TargetCradleId = "cradle-b",
                TargetTier = ArchiveTier.GEL,
                TargetTheaterMode = "Prime",
                TargetFormationLevel = "HigherFormation",
                PreferredScope = IdentityScope.Intrinsic,
                Morphism = morphism,
                OperatorId = "op1",
                SessionId = "s1",
                Tick = 101,
                IsOePrivileged = true
            };

            var ex = Assert.Throws<InvalidOperationException>(() => _glue.ApplyGlue(req));
            Assert.Contains("CGEL_TO_GEL_FORBIDDEN", ex.Message);
        }

        [Fact]
        public void Glue_Requires_OE_For_GOA_And_CGEL_Target()
        {
            var source = CreateSourceEngram("src1", "cradle-a", ArchiveTier.GEL);
            var morphism = new MorphismDescriptor { Kind = "CrossCradleGlue", PolicyVersion = "1" };

            var req = new GlueRequest
            {
                SourceEngramId = "src1",
                SourceCradleId = "cradle-a",
                SourceTier = ArchiveTier.GEL,
                TargetCradleId = "cradle-b",
                TargetTier = ArchiveTier.GOA,
                TargetTheaterMode = "Prime",
                TargetFormationLevel = "HigherFormation",
                PreferredScope = IdentityScope.Intrinsic,
                Morphism = morphism,
                IsOePrivileged = false, // NO OE privilege
                OperatorId = "op1",
                SessionId = "s1",
                Tick = 101
            };

            var ex = Assert.Throws<InvalidOperationException>(() => _glue.ApplyGlue(req));
            Assert.Contains("OE_REQUIRED", ex.Message);
        }

        [Fact]
        public void Glue_Is_Deterministic_Replay()
        {
            var source = CreateSourceEngram("src1", "cradle-a", ArchiveTier.GEL);
            var morphism = new MorphismDescriptor { Kind = "CrossCradleGlue", PolicyVersion = "1" };

            var req = new GlueRequest
            {
                SourceEngramId = "src1",
                SourceCradleId = "cradle-a",
                SourceTier = ArchiveTier.GEL,
                TargetCradleId = "cradle-b",
                TargetTier = ArchiveTier.GEL,
                TargetTheaterMode = "Prime",
                TargetFormationLevel = "HigherFormation",
                PreferredScope = IdentityScope.Intrinsic,
                Morphism = morphism,
                OperatorId = "op1",
                SessionId = "s1",
                Tick = 101
            };

            var result1 = _glue.ApplyGlue(req);
            
            // Replay SAME request
            var result2 = _glue.ApplyGlue(req);

            Assert.Equal(result1.ResultEngramId, result2.ResultEngramId);
            Assert.Equal(result1.CleaveId, result2.CleaveId);
        }
    }
}
