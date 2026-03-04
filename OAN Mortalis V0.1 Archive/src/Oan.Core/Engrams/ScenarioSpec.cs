using System;
using System.Collections.Generic;

namespace Oan.Core.Engrams
{
    public sealed class ScenarioSpec
    {
        public required string ScenarioName { get; init; }
        public required string SessionId { get; init; }
        public required string OperatorId { get; init; }
        public required long GenesisTick { get; init; }

        // Initial environment
        public required string CradleId { get; init; }
        public required string ContextId { get; init; }
        public required string FormationLevel { get; init; } 
        public required string TheaterMode { get; init; }       
        public required string TheaterId { get; init; }              

        // Flags
        public bool IsOePrivileged { get; init; }           
        public bool OptedInResearch { get; init; }          
        public List<ScenarioStep> Steps { get; init; } = new();
    }

    public sealed class ScenarioStep
    {
        public required long Tick { get; init; }                     
        public required string StepName { get; init; }
        public required object Intent { get; init; } // Using object to allow various intent types
        public required ExpectedOutcome Expected { get; init; }      
    }

    public sealed class ExpectedOutcome
    {
        public bool ExpectDenied { get; init; }
        public string? ExpectedReasonCode { get; init; }    

        public bool ExpectGovernanceDecision { get; init; } = true;
        public bool ExpectCleaveRecord { get; init; }       
        public bool ExpectThetaSeal { get; init; }          

        public bool ExpectNoBindingEngrams { get; init; }   
        public bool ExpectNoTierPollution { get; init; }    
    }

    public sealed class ScenarioRunResult
    {
        public required string RunId { get; init; }
        public required string ScenarioName { get; init; }
        public List<StepRunResult> Steps { get; set; } = new();
        public IReadOnlyList<GovernanceDecision> GovernanceDecisions { get; set; } = new List<GovernanceDecision>();
        public IReadOnlyList<CleaveRecord> CleaveRecords { get; set; } = new List<CleaveRecord>();
        public IReadOnlyList<UptakePlan> UptakePlans { get; set; } = new List<UptakePlan>();
        public IReadOnlyList<ResidueSet> ResidueSets { get; set; } = new List<ResidueSet>();
        public IReadOnlyList<EngramBlock> Engrams { get; set; } = new List<EngramBlock>(); 
    }

    public sealed class StepRunResult
    {
        public required string StepName { get; init; }
        public required long Tick { get; init; }
        public bool WasDenied { get; set; }
        public string? ActualReasonCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
