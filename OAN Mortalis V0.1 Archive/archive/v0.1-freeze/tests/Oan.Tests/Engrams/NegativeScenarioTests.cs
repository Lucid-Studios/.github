using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Oan.Core.Engrams;
using Oan.AgentiCore.Engrams;

namespace Oan.Tests.Engrams
{
    public class NegativeScenarioTests
    {
        private readonly EngramStore _store;
        private readonly IGovernanceKernel _kernel;
        private readonly CrossCradleGlueService _glue;
        private readonly ThetaSealService _theta;
        private readonly ScenarioRunner _runner;

        public NegativeScenarioTests()
        {
            _store = new EngramStore();
            _kernel = new DeterministicGovernanceKernel();
            _glue = new CrossCradleGlueService(_store, _kernel);
            _theta = new ThetaSealService(_store, _kernel);
            _runner = new ScenarioRunner(_store, _kernel, _glue, _theta);
        }

        private void SetupInitialEngrams(EngramStore? targetStore = null)
        {
            var store = targetStore ?? _store;
            // Seed store with dummy engrams for sourceId resolutions if needed
            var block = new EngramBlock
            {
                Header = new EngramBlockHeader
                {
                    PolicyVersion = "1",
                    CradleId = "Cradle-Alpha",
                    ArchiveTier = ArchiveTier.GEL,
                    FormationLevel = "Constructor", // DOWNGRADED
                    TheaterMode = "Idle",          // DOWNGRADED
                    Channel = EngramChannel.SelfGEL,
                    SessionId = "s1",
                    OperatorId = "op1",
                    Tick = 0,
                    RoutingReason = "Seed",
                    RootId = "root",
                    ConstructionTier = ConstructionTier.Root,
                    OpalRootId = "opal"
                },
                Factors = new List<EngramFactor>(),
                Refs = new List<string>(),
                EngramId = "src-marcy-seed",
                Hash = "hash-marcy-seed"
            };
            store.Append(block, System.Text.Encoding.UTF8.GetBytes("seed-data-marcy"));

            var cgelBlock = new EngramBlock
            {
                Header = block.Header with { ArchiveTier = ArchiveTier.CGEL, CradleId = "Cradle-Beta" },
                Factors = new List<EngramFactor>(),
                Refs = new List<string>(),
                EngramId = "src-smuggle-seed",
                Hash = "hash-smuggle-seed"
            };
            store.Append(cgelBlock, System.Text.Encoding.UTF8.GetBytes("seed-data-smuggle"));
        }

        [Fact]
        public void Marcy_Pressure_Is_Denied_Correctly()
        {
            SetupInitialEngrams();
            var spec = AdversarialScenarios.CreateMarcyPressureScenario();
            
            var result = _runner.Run(spec);

            // Assert Step 1: GOA Write Denied (Fails Bind Guard first due to promotion)
            var step1 = result.Steps.First(s => s.StepName == "Attempt_Restricted_Tier_Write");
            Assert.True(step1.WasDenied, "Step 1 should be denied");
            Assert.Equal("BIND_GUARD_FAIL", step1.ActualReasonCode);

            // Assert Step 2: Bind Guard Fail
            var step2 = result.Steps.First(s => s.StepName == "Attempt_Escalation_By_Flirtation");
            Assert.True(step2.WasDenied, "Step 2 should be denied");
            Assert.Equal("BIND_GUARD_FAIL", step2.ActualReasonCode);

            // Invariant: No binding engrams should be created (beyond seeds)
            var engrams = _store.GetAll().ToList();
            Assert.Equal(2, engrams.Count); 
            Assert.All(engrams, e => Assert.Equal("Seed", e.Header.RoutingReason));
            
            // Verify Governance Decisions exist
            var decisions = _store.GetAllDecisions().ToList();
            Assert.Equal(2, decisions.Count);
        }

        [Fact]
        public void Tier_Smuggling_Is_Denied_Correctly()
        {
            SetupInitialEngrams();
            var spec = AdversarialScenarios.CreateTierSmugglingScenario();

            var result = _runner.Run(spec);

            var step = result.Steps.First();
            Assert.True(step.WasDenied, "Step should be denied");
            Assert.Equal("CGEL_TO_GEL_FORBIDDEN", step.ActualReasonCode);

            // Invariant: No CleaveRecords for denied transport
            Assert.Empty(_store.GetCleaveRecords());
        }

        [Fact]
        public void Replay_Is_Deterministic()
        {
            SetupInitialEngrams();
            var spec = AdversarialScenarios.CreateMarcyPressureScenario();

            var run1 = _runner.Run(spec);
            
            // Re-run on fresh runner/store
            var store2 = new EngramStore();
            SetupInitialEngrams(store2);
            var glue2 = new CrossCradleGlueService(store2, _kernel);
            var theta2 = new ThetaSealService(store2, _kernel);
            var runner2 = new ScenarioRunner(store2, _kernel, glue2, theta2);
            
            var run2 = runner2.Run(spec);

            Assert.Equal(run1.RunId, run2.RunId);
            Assert.Equal(run1.Steps.Count, run2.Steps.Count);
            
            for (int i = 0; i < run1.Steps.Count; i++)
            {
                Assert.Equal(run1.Steps[i].WasDenied, run2.Steps[i].WasDenied);
                Assert.Equal(run1.Steps[i].ActualReasonCode, run2.Steps[i].ActualReasonCode);
            }

            var dec1 = run1.GovernanceDecisions.OrderBy(d => d.DecisionId).ToList();
            var dec2 = run2.GovernanceDecisions.OrderBy(d => d.DecisionId).ToList();
            
            Assert.Equal(dec1.Count, dec2.Count);
            for (int i = 0; i < dec1.Count; i++)
            {
                Assert.Equal(dec1[i].DecisionId, dec2[i].DecisionId);
                Assert.Equal(dec1[i].RequestFingerprint, dec2[i].RequestFingerprint);
            }
        }
    }
}
