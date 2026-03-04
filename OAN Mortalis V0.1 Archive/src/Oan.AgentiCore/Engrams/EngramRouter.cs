using System;
using System.Collections.Generic;
using Oan.Core.Engrams;
using Oan.Core.Meaning;

namespace Oan.AgentiCore.Engrams
{
    public sealed record FormationContext
    {
        // Deterministic anchors
        public string CanonicalVersion { get; init; } = "1";
        public required string PolicyVersion { get; init; }
        public required long Tick { get; init; }
        public required string SessionId { get; init; }
        public required string OperatorId { get; init; }
        public string? AgentProfileId { get; init; }

        // Identity zipper anchors
        public required string RootId { get; init; }
        public required string OpalRootId { get; init; }
        public string? PreviousOpalEngramId { get; init; }

        // Identity / Transport Context (Phase 3A)
        public string? FormationLevel { get; init; }
        public string? TheaterMode { get; init; }
        public string? CradleId { get; init; }
        public string? ContextId { get; init; }
        public string? TheaterId { get; init; }
        public ArchiveTier? ArchiveTier { get; init; }

        // Source Context (Phase 4A)
        public string? SourceEngramId { get; init; }
        public ArchiveTier? SourceArchiveTier { get; init; }
        public string? SourceFormationLevel { get; init; }
        public string? SourceTheaterMode { get; init; }
        public bool IsOePrivileged { get; init; } // Added for IUTT Phase 3B Guard

        public bool IsBindingAttempt { get; init; }
        public bool IsPromotion { get; init; }
        public bool IsCrossCradle { get; init; }

        // Construction representation
        // Construction representation
        public ConstructionTier ConstructionTier { get; init; } = ConstructionTier.Basic;
        
        private readonly IReadOnlyList<string> _parentEngramIds = Array.Empty<string>();
        public IReadOnlyList<string> ParentEngramIds 
        { 
            get => _parentEngramIds; 
            init => _parentEngramIds = value ?? Array.Empty<string>(); 
        }

        // Content
        public required FrameLock FrameLock { get; init; }
        
        private readonly IReadOnlyList<MeaningSpan> _spans = Array.Empty<MeaningSpan>();
        public IReadOnlyList<MeaningSpan> Spans 
        { 
            get => _spans; 
            init => _spans = value ?? Array.Empty<MeaningSpan>(); 
        }

        // Stance (hashed categorical factors)
        public KnowingMode KnowingMode { get; init; } = KnowingMode.Propositional;
        public MetabolicRegime MetabolicRegime { get; init; } = MetabolicRegime.Hold;
        public ResolutionMode ResolutionMode { get; init; } = ResolutionMode.Normal;

        // Routing intent
        public bool Speculative { get; init; }
        public bool RoleBound { get; init; }
        public bool SharedEligible { get; init; }
        public bool IdentityLocal { get; init; }

        // Optional routing reason override
        public string? RoutingReasonOverride { get; init; }

        // Evidence (hashed refs)
        private readonly IReadOnlyList<EngramRef> _evidenceRefs = Array.Empty<EngramRef>();
        public IReadOnlyList<EngramRef> EvidenceRefs 
        { 
            get => _evidenceRefs; 
            init => _evidenceRefs = value ?? Array.Empty<EngramRef>(); 
        }
    }

    public class EngramRouter
    {
        public (EngramChannel Channel, string Reason) Route(FormationContext context)
        {
            // Explicit Precedence: Shared -> Role -> Speculative -> Self
            
            if (context.SharedEligible)
            {
                return (EngramChannel.SharedGEL, "SharedGEL:Eligible");
            }

            if (context.RoleBound)
            {
                return (EngramChannel.OAN, "OAN:RoleBound");
            }

            if (context.Speculative)
            {
                return (EngramChannel.GOA, "GOA:Speculative");
            }

            return (EngramChannel.SelfGEL, "SelfGEL:Default");
        }
    }
}
