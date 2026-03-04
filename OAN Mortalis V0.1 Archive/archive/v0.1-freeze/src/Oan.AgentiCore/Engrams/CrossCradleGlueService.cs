using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Oan.Core.Engrams;

namespace Oan.AgentiCore.Engrams
{
    public sealed class CrossCradleGlueService
    {
        private readonly EngramStore _store;
        private readonly IGovernanceKernel _kernel;

        public CrossCradleGlueService(EngramStore store, IGovernanceKernel kernel)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        }

        public GlueResult ApplyGlue(GlueRequest req)
        {
            // 1. Load source EngramBlock
            var sourceBlock = _store.GetById(req.SourceEngramId);
            if (sourceBlock == null)
            {
                throw new KeyNotFoundException($"Source engram {req.SourceEngramId} not found in store.");
            }

            // --- GOVERNANCE KERNEL EVALUATION ---
            var govReq = new GovernanceRequest
            {
                Kind = GovernanceOpKind.CrossCradleGlue,
                SessionId = req.SessionId,
                ScenarioName = "CrossCradleTransport",
                OperatorId = req.OperatorId,
                Tick = req.Tick,
                
                SourceCradleId = req.SourceCradleId,
                SourceContextId = sourceBlock.Header.ContextId ?? "Unknown",
                SourceTheaterId = sourceBlock.Header.TheaterId ?? "Unknown",
                SourceTheaterMode = sourceBlock.Header.TheaterMode ?? "Idle",
                SourceFormationLevel = sourceBlock.Header.FormationLevel ?? "HigherFormation",
                SourceArchiveTier = req.SourceTier,

                TargetCradleId = req.TargetCradleId,
                TargetContextId = "Auto",
                TargetTheaterId = sourceBlock.Header.TheaterId ?? "Unknown",
                TargetTheaterMode = req.TargetTheaterMode,
                TargetFormationLevel = req.TargetFormationLevel,
                TargetArchiveTier = req.TargetTier,

                IsCrossCradle = req.SourceCradleId != req.TargetCradleId,
                IsPromotion = sourceBlock.Header.FormationLevel != req.TargetFormationLevel,
                IsBindingAttempt = (req.TargetTier == ArchiveTier.GEL && req.TargetTheaterMode == "Prime" && req.TargetFormationLevel == "HigherFormation"),

                MorphismId = req.Morphism.MorphismId,
                PolicyVersion = req.Morphism.PolicyVersion,
                SgelFingerprint = req.IsOePrivileged ? "Simulated-OE-Privilege" : null // Simple indicator for now
            };

            var decision = _kernel.Evaluate(govReq);
            _store.AppendGovernanceDecision(decision);

            if (decision.Verdict == GovernanceVerdict.Deny)
            {
                throw new InvalidOperationException($"Governance Denied: {decision.ReasonCode}");
            }
            // ------------------------------------

            // 2. Enforce tier access (Kernel already checked, but keep for defense-in-depth)
            if ((req.TargetTier == ArchiveTier.GOA || req.TargetTier == ArchiveTier.CGEL) && !req.IsOePrivileged)
            {
                throw new UnauthorizedAccessException("Restricted Tier (GOA/CGEL) requires OE privilege.");
            }

            // CEL-GEL direct prohibition
            if (req.TargetTier == ArchiveTier.GEL && req.SourceTier == ArchiveTier.CGEL)
            {
                throw new InvalidOperationException("Forbid direct CGEL->GEL transport (must go CGEL->GOA first).");
            }


            // 3. Construct TransportDescriptor
            var descriptor = new TransportDescriptor
            {
                SourceFormationLevel = sourceBlock.Header.FormationLevel ?? "HigherFormation",
                TargetFormationLevel = req.TargetFormationLevel,
                SourceTheaterMode = sourceBlock.Header.TheaterMode ?? "Idle",
                TargetTheaterMode = req.TargetTheaterMode,
                IsCrossCradle = req.SourceCradleId != req.TargetCradleId,
                IsPromotion = sourceBlock.Header.FormationLevel != req.TargetFormationLevel,
                IsBindingAttempt = (req.TargetTier == ArchiveTier.GEL && req.TargetTheaterMode == "Prime" && req.TargetFormationLevel == "HigherFormation")
            };

            // 4. Resolve IdentityScope
            var scope = IdentityScopeMatrix.Resolve(descriptor);
            var profile = IdentityScopeMatrix.MapToProfile(scope);

            // 5. Build Header for target context
            var header = sourceBlock.Header with
            {
                CradleId = req.TargetCradleId,
                ArchiveTier = req.TargetTier,
                TheaterMode = req.TargetTheaterMode,
                FormationLevel = req.TargetFormationLevel,
                PolicyVersion = req.Morphism.PolicyVersion,
                Tick = req.Tick,
                SessionId = req.SessionId,
                OperatorId = req.OperatorId,
                
                // Add provenance reference
                ParentEngramIds = (sourceBlock.Header.ParentEngramIds ?? new List<string>())
                                  .Concat(new[] { req.SourceEngramId })
                                  .OrderBy(x => x, StringComparer.Ordinal)
                                  .Distinct()
                                  .ToList()
            };

            // 6. Build Block (without ID first)
            var tempBlock = sourceBlock with
            {
                Header = header,
                EngramId = "", // Placeholder
                Hash = "" // Placeholder
            };

            // 7. Canonicalize & Hash
            var canonicalString = EngramCanonicalizer.Serialize(tempBlock, profile);
            var canonicalBytes = Encoding.UTF8.GetBytes(canonicalString);

            string hash;
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(canonicalBytes);
                var sb = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes) sb.Append(b.ToString("x2"));
                hash = sb.ToString();
            }

            // 8. Final Block
            var finalBlock = tempBlock with
            {
                EngramId = hash,
                Hash = hash
            };

            // 9. Store
            _store.Append(finalBlock, canonicalBytes);

            // 10. Emit CleaveRecord (Sidecar)
            // Deterministic CleaveId Formula: SHA256(SourceEngramId + ResultEngramId + TargetTier + TargetFormationLevel + TargetTheaterMode + ScopeUsed + IsCrossCradle + IsPromotion)
            var cleaveInput = req.SourceEngramId 
                            + finalBlock.EngramId 
                            + req.TargetTier.ToString() 
                            + descriptor.TargetFormationLevel 
                            + descriptor.TargetTheaterMode 
                            + scope.ToString() 
                            + descriptor.IsCrossCradle.ToString() 
                            + descriptor.IsPromotion.ToString();

            string cleaveId;
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(cleaveInput);
                var hashBytes = sha.ComputeHash(bytes);
                var sb = new StringBuilder(hashBytes.Length * 2);
                foreach (var b in hashBytes) sb.Append(b.ToString("x2"));
                cleaveId = sb.ToString();
            }

            var record = new CleaveRecord
            {
                CleaveId = cleaveId,
                SourceEngramId = req.SourceEngramId,
                ResultEngramId = finalBlock.EngramId,
                SourceTier = req.SourceTier,
                TargetTier = req.TargetTier,
                SourceFormationLevel = descriptor.SourceFormationLevel,
                TargetFormationLevel = descriptor.TargetFormationLevel,
                SourceTheaterMode = descriptor.SourceTheaterMode,
                TargetTheaterMode = descriptor.TargetTheaterMode,
                ScopeUsed = scope,
                IsCrossCradle = descriptor.IsCrossCradle,
                IsPromotion = descriptor.IsPromotion
            };

            _store.AppendCleaveRecord(record);

            return new GlueResult
            {
                ResultEngramId = finalBlock.EngramId,
                CleaveId = cleaveId,
                ScopeUsed = scope
            };
        }
    }
}
