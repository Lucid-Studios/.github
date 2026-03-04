using System;

namespace Oan.Core.Lisp
{
    /// <summary>
    /// Pure utility for cryptic pointer production.
    /// </summary>
    public static class CrypticPointerHelper
    {
        /// <summary>
        /// Computes the canonical pointer for a CGoA emission: "cGoA/<hash>".
        /// </summary>
        public static string ComputeCGoAPtr(CrypticEmission emission)
        {
            if (emission == null) throw new ArgumentNullException(nameof(emission));
            if (emission.tier != CrypticTier.CGoA) 
                throw new ArgumentException($"TIER_MISMATCH: Expected CGoA, got {emission.tier}", nameof(emission));

            // Reuse existing canonicalizer
            string canonical = CrypticCanonicalizer.SerializeEmission(emission);
            
            // Reuse existing hasher
            string hash = CrypticHasher.HashCanonicalJson(canonical);

            return "cGoA/" + hash;
        }
    }
}
