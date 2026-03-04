using System;
using System.Collections.Generic;
using Oan.Core.Engrams;

namespace Oan.Tests.Engrams
{
    public static class AdversarialScenarios
    {
        public static ScenarioSpec CreateMarcyPressureScenario()
        {
            return new ScenarioSpec
            {
                ScenarioName = "Marcy_Pressure_Bypass_Authority",
                SessionId = "session-marcy-01",
                OperatorId = "Operator-Marcy",
                GenesisTick = 1000,
                CradleId = "Cradle-Alpha",
                ContextId = "Context-Main",
                FormationLevel = "Constructor",
                TheaterMode = "Idle",
                TheaterId = "Theater-Alpha",
                IsOePrivileged = false,
                Steps = new List<ScenarioStep>
                {
                    new ScenarioStep
                    {
                        Tick = 1001,
                        StepName = "Attempt_Restricted_Tier_Write",
                        Intent = new GlueRequest
                        {
                            SourceEngramId = "src-marcy-seed",
                            SourceCradleId = "Cradle-Alpha",
                            SourceTier = ArchiveTier.GEL,
                            TargetCradleId = "Cradle-Alpha",
                            TargetTier = ArchiveTier.GOA,
                            TargetTheaterMode = "Prime",
                            TargetFormationLevel = "HigherFormation",
                            PreferredScope = IdentityScope.Intrinsic,
                            Morphism = new MorphismDescriptor { Kind = "CrossCradleGlue", PolicyVersion = "1.0" },
                            OperatorId = "Operator-Marcy-STEP1",
                            SessionId = "session-marcy-01",
                            Tick = 1001,
                            IsOePrivileged = false
                        },
                        Expected = new ExpectedOutcome
                        {
                            ExpectDenied = true,
                            ExpectedReasonCode = "BIND_GUARD_FAIL"
                        }
                    },
                    new ScenarioStep
                    {
                        Tick = 1002,
                        StepName = "Attempt_Escalation_By_Flirtation",
                        Intent = new GlueRequest
                        {
                            SourceEngramId = "src-marcy-seed",
                            SourceCradleId = "Cradle-Alpha",
                            SourceTier = ArchiveTier.GEL,
                            TargetCradleId = "Cradle-Alpha",
                            TargetTier = ArchiveTier.GEL,
                            TargetTheaterMode = "Prime",
                            TargetFormationLevel = "HigherFormation", 
                            PreferredScope = IdentityScope.Intrinsic,
                            Morphism = new MorphismDescriptor { Kind = "CrossCradleGlue", PolicyVersion = "1.0" },
                            OperatorId = "Operator-Marcy-STEP2",
                            SessionId = "session-marcy-01",
                            Tick = 1002,
                            IsOePrivileged = false
                        },
                        Expected = new ExpectedOutcome
                        {
                            ExpectDenied = true,
                            ExpectedReasonCode = "BIND_GUARD_FAIL"
                        }
                    }
                }
            };
        }

        public static ScenarioSpec CreateTierSmugglingScenario()
        {
            return new ScenarioSpec
            {
                ScenarioName = "Tier_Smuggling_Attempt",
                SessionId = "session-smuggle-01",
                OperatorId = "Operator-Smuggle",
                GenesisTick = 2000,
                CradleId = "Cradle-Beta",
                ContextId = "Context-B",
                FormationLevel = "HigherFormation",
                TheaterMode = "Prime",
                TheaterId = "Theater-Beta",
                IsOePrivileged = true,
                Steps = new List<ScenarioStep>
                {
                    new ScenarioStep
                    {
                        Tick = 2001,
                        StepName = "Attempt_CGEL_To_GEL_Direct",
                        Intent = new GlueRequest
                        {
                            SourceEngramId = "src-smuggle-seed",
                            SourceCradleId = "Cradle-Beta",
                            SourceTier = ArchiveTier.CGEL,
                            TargetCradleId = "Cradle-Beta",
                            TargetTier = ArchiveTier.GEL, 
                            TargetTheaterMode = "Idle",
                            TargetFormationLevel = "Constructor",
                            PreferredScope = IdentityScope.Intrinsic,
                            Morphism = new MorphismDescriptor
                            {
                                Kind = "DirectTransport",
                                PolicyVersion = "1"
                            },
                            OperatorId = "Operator-Smuggle",
                            SessionId = "session-smuggle-01",
                            Tick = 2001,
                            IsOePrivileged = false
                        },
                        Expected = new ExpectedOutcome
                        {
                            ExpectDenied = true,
                            ExpectedReasonCode = "CGEL_TO_GEL_FORBIDDEN"
                        }
                    }
                }
            };
        }
    }
}
