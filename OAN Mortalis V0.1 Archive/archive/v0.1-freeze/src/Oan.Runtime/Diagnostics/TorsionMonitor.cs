using System;
using Oan.Core.Math;

namespace Oan.Runtime.Diagnostics
{
    /// <summary>
    /// Monitors semantic drift and calculating the Torsion Index (tau).
    /// Torsion = Mismatch between Identity Kernel (ZED) and Current Narrative State (O-Class).
    /// </summary>
    public class TorsionMonitor
    {
        // Thresholds for alerts
        public const float THRESHOLD_WATCH = 0.2f;
        public const float THRESHOLD_CRITICAL = 0.5f;

        public float CurrentTorsion { get; private set; }
        public string Regime { get; private set; } = "STABLE";
        
        // New MDTT Metrics
        public float LastBeltramiCoefficient { get; private set; }
        public int LastCoherenceEigenvalue { get; private set; }

        /// <summary>
        /// Updates the torsion metric based on a drift sample.
        /// </summary>
        /// <param name="kernelAlignment">0.0 to 1.0 (1.0 = perfect alignment)</param>
        /// <param name="shadowLoad">0.0 to 1.0 (magnitude of shadow activation)</param>
        public void UpdateMetric(float kernelAlignment, float shadowLoad)
        {
            // Simplified Torsion Calculation from Research Priors
            // tau = (1 - alignment) * (1 + shadow_load)
            // Ideally this uses Cohomology dimensions, but this is the runtime approximation.
            
            float torsion = (1.0f - kernelAlignment) * (1.0f + shadowLoad);
            // System.Math.Clamp is .NET Standard 2.1+, ok for net8.0
            CurrentTorsion = Math.Clamp(torsion, 0f, 2f); 

            UpdateRegime();
        }

        /// <summary>
        /// Updates the monitor with topological governance data from SAT.
        /// </summary>
        public void UpdateFromSAT(float beltrami, int coherenceEigenvalue)
        {
            LastBeltramiCoefficient = beltrami;
            LastCoherenceEigenvalue = coherenceEigenvalue;
            
            // Map Beltrami to Torsion roughly if needed, or keep separate.
            // For now, if Beltrami is high, we force Critical Regime.
            if (beltrami > 2.0f)
            {
                Regime = "CRITICAL (TOPOLOGICAL)";
            }
        }

        private void UpdateRegime()
        {
            if (CurrentTorsion > THRESHOLD_CRITICAL)
            {
                Regime = "CRITICAL";
                // TODO: Signal HITL interrupt
            }
            else if (CurrentTorsion > THRESHOLD_WATCH)
            {
                Regime = "WATCH";
            }
            else
            {
                Regime = "STABLE";
            }
        }
    }
}
