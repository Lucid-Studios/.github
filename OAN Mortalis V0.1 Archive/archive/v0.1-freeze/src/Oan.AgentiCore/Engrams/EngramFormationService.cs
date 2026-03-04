using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Oan.Core.Engrams;
using Oan.Core.Meaning;

namespace Oan.AgentiCore.Engrams
{
    public class EngramFormationService
    {
        private readonly EngramStore _store;
        private readonly EngramRouter _router;

        public EngramFormationService(EngramStore store, EngramRouter router)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _router = router ?? throw new ArgumentNullException(nameof(router));
        }

        public EngramBlock FormEngram(FormationContext context)
        {
            // 1. Build Factors
            var factors = new List<EngramFactor>();
            int order = 0;

            // FrameLock -> RootBase
            if (context.FrameLock != null)
            {
                factors.Add(new EngramFactor 
                {
                    Tier = FactorTier.RootBase,
                    Kind = EngramFactorKind.FrameLock,
                    Key = "Goal",
                    Value = context.FrameLock.Goal,
                    Order = order++,
                    Weight = 1.0m
                });

                // Constraints -> Basic
                if (context.FrameLock.Constraints != null)
                {
                    foreach (var c in context.FrameLock.Constraints.OrderBy(s => s, StringComparer.Ordinal))
                    {
                        factors.Add(new EngramFactor
                        {
                            Tier = FactorTier.Basic,
                            Kind = EngramFactorKind.Constraint,
                            Key = "Constraint", 
                            Value = c,
                            Order = order++,
                            Weight = 1.0m
                        });
                    }
                }

                 // Assumptions -> Basic
                if (context.FrameLock.Assumptions != null)
                {
                    foreach (var a in context.FrameLock.Assumptions.OrderBy(s => s, StringComparer.Ordinal))
                    {
                        factors.Add(new EngramFactor
                        {
                            Tier = FactorTier.Basic,
                            Kind = EngramFactorKind.Assumption,
                            Key = "Assumption",
                            Value = a,
                            Order = order++,
                            Weight = 1.0m
                        });
                    }
                }
            }

            // Spans -> Basic
            if (context.Spans != null)
            {
                foreach (var span in context.Spans.OrderBy(s => s.StartOffset).ThenBy(s => s.SpanId, StringComparer.Ordinal))
                {
                    var kind = span.Status == MeaningStatus.Edited ? EngramFactorKind.MeaningSpanEdited : EngramFactorKind.MeaningSpanConfirmed;
                    factors.Add(new EngramFactor
                    {
                        Tier = FactorTier.Basic,
                        Kind = kind,
                        Key = span.SpanId, // Use SpanId as Key
                        Value = span.Text, // Use Text as Value
                        Order = order++,
                        Weight = 1.0m
                    });
                }
            }

            // Inject Vervaeke Stance Factors (Tier: RootBase)
            // KnowingMode (Order 10)
            factors.Add(new EngramFactor
            {
                Tier = FactorTier.RootBase,
                Kind = EngramFactorKind.KnowingMode,
                Key = "KnowingMode",
                Value = context.KnowingMode.ToString(),
                Order = 10,
                Weight = 1.0m
            });

            // MetabolicRegime (Order 20)
            factors.Add(new EngramFactor
            {
                Tier = FactorTier.RootBase,
                Kind = EngramFactorKind.MetabolicRegime,
                Key = "MetabolicRegime",
                Value = context.MetabolicRegime.ToString(),
                Order = 20,
                Weight = 1.0m
            });

            // ResolutionMode (Order 30)
            factors.Add(new EngramFactor
            {
                Tier = FactorTier.RootBase,
                Kind = EngramFactorKind.ResolutionMode,
                Key = "ResolutionMode",
                Value = context.ResolutionMode.ToString(),
                Order = 30,
                Weight = 1.0m
            });

            // 3. Route
            var (channel, reason) = _router.Route(context);
            if (!string.IsNullOrEmpty(context.RoutingReasonOverride))
            {
                reason = context.RoutingReasonOverride;
            }

            // Phase 3B: ArchiveTier Resolution & Guards
            var tier = context.ArchiveTier ?? ArchiveTier.GEL;

            // Guard 1: OE Access for GOA/CGEL
            if ((tier == ArchiveTier.GOA || tier == ArchiveTier.CGEL) && !context.IsOePrivileged)
            {
               throw new UnauthorizedAccessException("Restricted Tier (GOA/CGEL) requires OE privilege.");
            }

            // Guard 2: CGEL cannot write to GEL
            if (tier == ArchiveTier.CGEL && channel == EngramChannel.SelfGEL)
            {
                throw new InvalidOperationException("CGEL cannot write directly to SelfGEL channel.");
            }

            // Guard 3: Binding/Promotion Modality Strictness (Universal)
            if (context.IsBindingAttempt || context.IsPromotion)
            {
                if (context.TheaterMode != "Prime" || context.FormationLevel != "HigherFormation")
                {
                    throw new InvalidOperationException("Binding/Promotion intent requires Prime TheaterMode and HigherFormation Level.");
                }
            }

            // 4. Build Header
            var header = new EngramBlockHeader
            {
                CanonicalVersion = context.CanonicalVersion,
                PolicyVersion = context.PolicyVersion,
                Tick = context.Tick,
                SessionId = context.SessionId,
                OperatorId = context.OperatorId,
                AgentProfileId = context.AgentProfileId,
                Channel = channel,
                RoutingReason = reason,
                RootId = context.RootId,
                ConstructionTier = context.ConstructionTier,
                ParentEngramIds = context.ParentEngramIds.OrderBy(x => x, StringComparer.Ordinal).ToList(),
                OpalRootId = context.OpalRootId,
                PreviousOpalEngramId = context.PreviousOpalEngramId,
                
                // Identity Context (Phase 3A)
                ContextId = context.ContextId,
                CradleId = context.CradleId,
                FormationLevel = context.FormationLevel,
                TheaterId = context.TheaterId,
                TheaterMode = context.TheaterMode,
                
                // Archive Tier (Phase 3B)
                ArchiveTier = tier
            };

            // 5. Build Block (without ID first)
            // 5. Build Block (without ID first)
            // Map EvidenceRefs to Strings using EngramRefCodec.Format() ("Rel:Id")
            var refsList = context.EvidenceRefs.Select(r => EngramRefCodec.Format(r)).OrderBy(x => x, StringComparer.Ordinal).ToList();
            
            var tempBlock = new EngramBlock
            {
                Header = header,
                Factors = factors,
                Refs = refsList,
                EngramId = "", // Placeholder
                Hash = "" // Placeholder
            };

            // 6. Canonicalize & Hash (Phase 3A: Identity Scope)
            
            // 6a. Construct Transport Descriptor
            var descriptor = new TransportDescriptor
            {
                SourceFormationLevel = context.SourceFormationLevel ?? context.FormationLevel ?? "Constructor",
                TargetFormationLevel = context.FormationLevel ?? "Constructor",
                SourceTheaterMode = context.SourceTheaterMode ?? context.TheaterMode ?? "Idle",
                TargetTheaterMode = context.TheaterMode ?? "Idle",
                IsCrossCradle = context.IsCrossCradle,
                IsBindingAttempt = context.IsBindingAttempt,
                IsPromotion = context.IsPromotion
            };

            // 6b. Resolve Scope & Profile
            var scope = IdentityScopeMatrix.Resolve(descriptor);
            var profile = IdentityScopeMatrix.MapToProfile(scope);

            // 6c. Canonicalize with Profile
            var canonicalString = EngramCanonicalizer.Serialize(tempBlock, profile);
            var canonicalBytes = System.Text.Encoding.UTF8.GetBytes(canonicalString);
            
            // Hash relies on bytes.
            string hash;
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
               var hashBytes = sha256.ComputeHash(canonicalBytes);
               var sb = new System.Text.StringBuilder(hashBytes.Length * 2);
               foreach (var b in hashBytes) sb.Append(b.ToString("x2"));
               hash = sb.ToString();
            }

            // 7. Final Block
            var finalBlock = new EngramBlock
            {
                Header = header,
                Factors = factors,
                Refs = tempBlock.Refs,
                EngramId = hash, // ID is the Hash for MVP
                Hash = hash
            };

            // 8. Store
            _store.Append(finalBlock, canonicalBytes);

            // Phase 4A: Minimal CleaveRecord (Sidecar)
            // Determine Source Tier (Default to Target if unspecified, assuming no transport)
            var sourceTier = context.SourceArchiveTier ?? tier;

            // Check Transport Conditions
            bool isTransport = context.IsPromotion
                            || context.IsCrossCradle
                            || (sourceTier != tier)
                            || (descriptor.SourceFormationLevel != descriptor.TargetFormationLevel)
                            || (descriptor.SourceTheaterMode != descriptor.TargetTheaterMode);

            if (isTransport)
            {
                var sourceId = context.SourceEngramId ?? "";
                var resultId = finalBlock.EngramId;
                
                // Deterministic CleaveId Generation
                // Formula: SHA256(SourceEngramId + ResultEngramId + TargetTier + TargetFormationLevel + TargetTheaterMode + ScopeUsed + IsCrossCradle + IsPromotion)
                var input = sourceId 
                          + resultId 
                          + tier.ToString() 
                          + descriptor.TargetFormationLevel 
                          + descriptor.TargetTheaterMode 
                          + scope.ToString() 
                          + context.IsCrossCradle.ToString() 
                          + context.IsPromotion.ToString();

                string cleaveId;
                using (var sha = System.Security.Cryptography.SHA256.Create())
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(input);
                    var hashBytes = sha.ComputeHash(bytes);
                    var sb = new System.Text.StringBuilder(hashBytes.Length * 2);
                    foreach (var b in hashBytes) sb.Append(b.ToString("x2"));
                    cleaveId = sb.ToString();
                }

                var record = new CleaveRecord
                {
                    CleaveId = cleaveId,
                    SourceEngramId = context.SourceEngramId,
                    ResultEngramId = resultId,
                    SourceTier = sourceTier,
                    TargetTier = tier,
                    SourceFormationLevel = descriptor.SourceFormationLevel,
                    TargetFormationLevel = descriptor.TargetFormationLevel,
                    SourceTheaterMode = descriptor.SourceTheaterMode,
                    TargetTheaterMode = descriptor.TargetTheaterMode,
                    ScopeUsed = scope,
                    IsCrossCradle = context.IsCrossCradle,
                    IsPromotion = context.IsPromotion
                };

                _store.AppendCleaveRecord(record);
            }

            return finalBlock;
        }
    }
}
