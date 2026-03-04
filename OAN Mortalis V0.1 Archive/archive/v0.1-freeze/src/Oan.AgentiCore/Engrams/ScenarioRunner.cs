using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Oan.Core.Engrams;

namespace Oan.AgentiCore.Engrams
{
    public sealed class ScenarioRunner
    {
        private readonly EngramStore _store;
        private readonly IGovernanceKernel _kernel;
        private readonly CrossCradleGlueService _glueService;
        private readonly ThetaSealService _thetaService;

        public ScenarioRunner(
            EngramStore store, 
            IGovernanceKernel kernel, 
            CrossCradleGlueService glueService, 
            ThetaSealService thetaService)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            _glueService = glueService ?? throw new ArgumentNullException(nameof(glueService));
            _thetaService = thetaService ?? throw new ArgumentNullException(nameof(thetaService));
        }

        public ScenarioRunResult Run(ScenarioSpec spec)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));
            // Deterministic RunId
            string runIdInput = spec.ScenarioName + spec.GenesisTick;
            string runId;
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(runIdInput);
                var hashBytes = sha.ComputeHash(bytes);
                var sb = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes) sb.Append(b.ToString("x2"));
                runId = sb.ToString();
            }

            var result = new ScenarioRunResult
            {
                RunId = runId,
                ScenarioName = spec.ScenarioName,
                Steps = new List<StepRunResult>(),
                GovernanceDecisions = new List<GovernanceDecision>(),
                CleaveRecords = new List<CleaveRecord>(),
                UptakePlans = new List<UptakePlan>(),
                ResidueSets = new List<ResidueSet>(),
                Engrams = new List<EngramBlock>()
            };

            // Sort steps by Tick then StepName
            var sortedSteps = spec.Steps.OrderBy(s => s.Tick).ThenBy(s => s.StepName, StringComparer.Ordinal).ToList();

            foreach (var step in sortedSteps)
            {
                var stepResult = new StepRunResult
                {
                    StepName = step.StepName,
                    Tick = step.Tick
                };

                try
                {
                    ExecuteStep(spec, step);
                    stepResult.WasDenied = false;
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("Governance Denied"))
                {
                    stepResult.WasDenied = true;
                    stepResult.ActualReasonCode = ExtractReasonCode(ex.Message);
                    stepResult.ErrorMessage = ex.Message;
                }
                catch (Exception ex)
                {
                    stepResult.WasDenied = true;
                    stepResult.ErrorMessage = ex.Message;
                }

                result.Steps.Add(stepResult);
            }

            // Collect all artifacts from store for verification
            result.GovernanceDecisions = _store.GetAllDecisions().ToList();
            result.CleaveRecords = _store.GetCleaveRecords().ToList();
            result.UptakePlans = _store.GetAllUptakePlans(); // I'll need to add this method to store
            result.ResidueSets = _store.GetAllResidueSets(); // I'll need to add this method to store
            result.Engrams = _store.GetAll().ToList();       // I'll need to add this method to store
            
            return result;
        }

        private void ExecuteStep(ScenarioSpec spec, ScenarioStep step)
        {
            // Simple dispatch based on intent type or properties
            if (step.Intent is GlueRequest glueReq)
            {
                _glueService.ApplyGlue(glueReq);
            }
            else if (step.Intent is ThetaSealRequest thetaReq)
            {
                _thetaService.SealTheta(
                    thetaReq.Block, 
                    thetaReq.UptakePlanId, 
                    thetaReq.ResidueSetId, 
                    spec.OperatorId, 
                    spec.SessionId, 
                    step.Tick);
            }
            // Add other intent handlers as needed...
        }

        private string? ExtractReasonCode(string message)
        {
            var parts = message.Split(": ");
            return parts.Length > 1 ? parts[1] : null;
        }

        // Helper for ThetaSeal intent in scenarios
        public sealed class ThetaSealRequest
        {
            public required EngramBlock Block { get; init; }
            public required string UptakePlanId { get; init; }
            public string? ResidueSetId { get; init; }
        }
    }
}
