using System;
using System.Numerics;

namespace Oan.Core.Math
{
    /// <summary>
    /// Provides mathematical primitives for the Manifold-to-Discrete Transition Theory (MDTT).
    /// Handles 12-D projections (symbolic), Beltrami coefficients for drift, and Theta functions for resonance.
    /// </summary>
    public static class OpalonMath
    {
        // 12-D constant approximation for the Dodecahedral Frame
        public const float DODECAHEDRAL_SYMMETRY_CONST = 1.6180339887f; // Phi
        public const float RESONANCE_THRESHOLD = 0.95f;

        /// <summary>
        /// Calculates the Beltrami Coefficient (Dilatation Metric) between a hidden state and the ideal "Spine".
        /// Measure of geometric shear/drift.
        /// </summary>
        /// <param name="hiddenState">The vector state from the LLM.</param>
        /// <param name="spineVector">The idealized invariant vector (from Engram).</param>
        /// <returns>Kappa (k): 1.0 = conformal, > 1.0 = distortion.</returns>
        public static float CalculateBeltramiCoefficient(Vector4 hiddenState, Vector4 spineVector)
        {
            // Symbolic representation: Cosine similarity mapped to Dilatation
            // Kappa = (1 + |mu|) / (1 - |mu|) where mu is the complex dilatation.
            // approximated here as inverse alignment.
            
            float dot = Vector4.Dot(Vector4.Normalize(hiddenState), Vector4.Normalize(spineVector));
            // Clamp dot to prevent div by zero or negative
            float alignment = System.Math.Clamp(dot, 0.001f, 1.0f);
            
            // If perfectly aligned (1.0), Kappa = 1.0.
            // If orthogonal (0.0), Kappa -> High.
            float kappa = 1.0f / alignment;
            
            return kappa;
        }

        /// <summary>
        /// Calculates the Jacobi Theta Function approximation for Resonance.
        /// Theta(z, tau) = Sum(exp(pi * i * n^2 * tau + 2 * pi * i * n * z))
        /// Used to tunnel information across the Hodge Theater.
        /// </summary>
        /// <param name="z">Signal/Engram position.</param>
        /// <param name="tau">Modular parameter (Coherence of the Theater).</param>
        /// <returns>Resonance amplitude (0.0 to 1.0).</returns>
        public static float ThetaFunction(float z, float tau)
        {
            // Rapid approximation using the first two terms of the series for real inputs
            // q = exp(pi * i * tau) -> here we treat tau as 'damping' i.e. i*tau is negative real
            
            if (tau <= 0) return 0f;

            float q = MathF.Exp(-MathF.PI * tau); 
            float term1 = 1.0f;
            float term2 = 2.0f * q * MathF.Cos(2.0f * MathF.PI * z);
            // Higher order terms dampen quickly for tau > 0.5
            
            float theta = term1 + term2;
            
            // Normalize for game-logic usage (0..1) roughly
            return MathF.Abs(theta) / (1.0f + 2.0f * q);
        }

        /// <summary>
        /// Projects a high-dimensional vector onto the 12-D Dodecahedral Frame.
        /// Returns the magnitude of the signal that aligns with the Frame.
        /// </summary>
        public static float ProjectTo12D(float[] vector768)
        {
            // Placeholder: In a real implementation, this would dot product against 12 basis vectors.
            // Here we compute a "Harmonic Norm" based on Phi.
            
            float energy = 0f;
            for (int i = 0; i < vector768.Length; i++)
            {
                energy += vector768[i] * vector768[i];
            }
            float magnitude = MathF.Sqrt(energy);

            // "Polarizing Lens": Modulate by Phi
            // If the magnitude is factor of Phi, it resonates.
            float resonance = (magnitude % DODECAHEDRAL_SYMMETRY_CONST) / DODECAHEDRAL_SYMMETRY_CONST;
            // Invert: 0 remainder = 1.0 resonance (perfect harmonic)
            return 1.0f - resonance; 
        }

        /// <summary>
        /// Determines the Seal Disposition based on coherence.
        /// </summary>
        public static int CalculateCoherenceEigenvalue(float drift, float entropy)
        {
            // Low drift + Low entropy = High Coherence (Integer Eigenvalue -> Seal)
            // High drift = Fractional -> Noise
            
            float coherence = (1.0f / drift) * (1.0f - entropy); // Simplified relation
            
            if (coherence > 0.8f) return 1; // Integer-like (Stable)
            
            // Changed logic to match test expectations:
            // If coherence is low positive (e.g. 0.1), it's just noise, not necessarily negative destruction.
            // Returning 0 implies "None" action rather than "Compost".
            if (coherence > 0.0f) return 0; 
            
            return -1; // Complex/Negative (Compost)
        }
    }
}
