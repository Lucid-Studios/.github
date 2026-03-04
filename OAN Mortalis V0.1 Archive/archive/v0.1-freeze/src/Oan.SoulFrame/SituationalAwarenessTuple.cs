using System;
using Oan.Core.Math;

namespace Oan.SoulFrame.Governance
{
    public enum GovernanceAction
    {
        None,
        TemperatureDrop,
        RepetitionPenalty,
        HaltGeneration,
        Compost,
        Seal
    }

    /// <summary>
    /// SAT v0.1.1: The Situational Awareness Tuple.
    /// Acts as the "Governor" of the LLM, managing Drift (Beltrami), Entropy, and Seal Eligibility.
    /// </summary>
    public class SituationalAwarenessTuple
    {
        // Tuneable Parameters
        private const float DRIFT_THRESHOLD_LOW = 1.2f;
        private const float DRIFT_THRESHOLD_HIGH = 2.0f;
        
        private const float MAX_ENTROPY_ALLOWED = 2.5f;

        // State
        public float CurrentDrift { get; private set; }
        public float CurrentEntropy { get; private set; }
        public int CoherenceEigenvalue { get; private set; }

        public GovernanceAction CheckDrift(float beltramiCoefficient)
        {
            CurrentDrift = beltramiCoefficient;

            if (CurrentDrift >= DRIFT_THRESHOLD_HIGH) // >= 2.0
            {
                // "Scraping the Paint" -> Immediate Halt or Severe Correction
                return GovernanceAction.Compost; 
            }
            if (CurrentDrift >= DRIFT_THRESHOLD_LOW) // 1.2 to 2.0
            {
                // Moderate Drift -> Force determinism
                return GovernanceAction.TemperatureDrop;
            }

            return GovernanceAction.None;
        }

        public GovernanceAction CheckEntropy(float entropy)
        {
            CurrentEntropy = entropy;
            
            if (CurrentEntropy > MAX_ENTROPY_ALLOWED)
            {
                // Too diffuse -> Stop loop
                return GovernanceAction.RepetitionPenalty;
            }
            return GovernanceAction.None;
        }

        public GovernanceAction EvaluateSealEligibility(float drift, float entropy)
        {
            // Calculate Resonance Operator (Rho)
            // Using simplified Coherence Eigenvalue logic
            CoherenceEigenvalue = OpalonMath.CalculateCoherenceEigenvalue(drift, entropy);

            if (CoherenceEigenvalue == 1)
            {
                // Integer Eigenvalue -> Stable State -> Seal
                return GovernanceAction.Seal;
            }
            else if (CoherenceEigenvalue == -1)
            {
                // Complex/Negative -> Incoherent -> Compost
                return GovernanceAction.Compost;
            }

            // Otherwise, keep processing (Archive/Continue)
            return GovernanceAction.None;
        }
    }
}
