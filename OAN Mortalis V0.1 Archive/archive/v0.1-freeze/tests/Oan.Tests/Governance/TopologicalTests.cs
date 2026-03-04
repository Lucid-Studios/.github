using System;
using System.Numerics;
using Xunit;
using Oan.Core.Math;
using Oan.SoulFrame.Governance;

namespace Oan.Tests.Governance
{
    public class TopologicalTests
    {
        [Fact]
        public void BeltramiCoefficient_CalculatesCorrectly()
        {
            // Case 1: Perfect Alignment
            Vector4 spine = new Vector4(0, 1, 0, 0);
            Vector4 signal = new Vector4(0, 1, 0, 0);
            float kappa = OpalonMath.CalculateBeltramiCoefficient(signal, spine);
            Assert.True(System.Math.Abs(kappa - 1.0f) < 0.01f, $"Expected Kappa ~ 1.0, got {kappa}");

            // Case 2: Orthogonal (High Drift)
            Vector4 drift = new Vector4(1, 0, 0, 0);
            float kappaDrift = OpalonMath.CalculateBeltramiCoefficient(drift, spine);
            Assert.True(kappaDrift > 100f, $"Expected High Kappa for orthogonal vectors, got {kappaDrift}");
            
            // Case 3: 45 degrees
            // Dot = 0.707
            // Kappa = 1 / 0.707 = 1.414
            Vector4 fortyFive = Vector4.Normalize(new Vector4(0, 1, 0, 0) + new Vector4(1, 0, 0, 0));
            float kappa45 = OpalonMath.CalculateBeltramiCoefficient(fortyFive, spine);
            Assert.True(System.Math.Abs(kappa45 - 1.414f) < 0.1f, $"Expected Kappa ~ 1.414, got {kappa45}");
        }

        [Fact]
        public void SAT_Governs_Drift()
        {
            var sat = new SituationalAwarenessTuple();
            
            // Low Drift
            var action1 = sat.CheckDrift(1.1f);
            Assert.Equal(GovernanceAction.None, action1);

            // Moderate Drift
            var action2 = sat.CheckDrift(1.5f);
            Assert.Equal(GovernanceAction.TemperatureDrop, action2);

            // High Drift
            var action3 = sat.CheckDrift(2.5f);
            Assert.Equal(GovernanceAction.Compost, action3);
        }

        [Fact]
        public void SAT_EvaluateSeal_Logic()
        {
            var sat = new SituationalAwarenessTuple();
            
            // Ideal: Drift 1.0, Entropy 0.0 -> 1.0 -> Seal
            Assert.Equal(GovernanceAction.Seal, sat.EvaluateSealEligibility(1.0f, 0.0f));

            // High Entropy: Drift 1.0, Entropy 0.9 -> 0.1 -> None (was Compost in old logic, fixed to None now)
            // Coherence = (1/1) * (1-0.9) = 0.1. > 0 but < 0.8
            Assert.Equal(GovernanceAction.None, sat.EvaluateSealEligibility(1.0f, 0.9f));
        }

        // Removed inference interceptor test as that class was not ported/needed for headless at this stage
        // Use SAT directly.
    }
}
