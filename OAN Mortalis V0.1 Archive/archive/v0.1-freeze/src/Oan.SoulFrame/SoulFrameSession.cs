using Oan.Core.Events;
using Oan.Core.Meaning;
using Oan.Core.Governance;
using System.Collections.Generic;
using System.Linq;

namespace Oan.SoulFrame
{
    /// <summary>
    /// Represents the user relational channel and session state.
    /// Enforces the Anti-Swarm invariant by tracking the single active agent.
    /// </summary>
    public enum TheaterMode
    {
        Idle,
        Prime,
        OAN,
        Mantle
    }

    public enum FormationLevel
    {
        Constructor,
        HigherFormation
    }

    public class SoulFrameSession
    {
        // ... existing properties ...
        public string SessionId { get; set; }
        public string OperatorId { get; set; }

        public TheaterMode CurrentTheaterMode { get; private set; } = TheaterMode.Idle;
        public string? CurrentTheaterId { get; private set; }

        // Phase 2: Context + Formation
        public string CradleId { get; private set; } = "PrimaryCradle"; // Default for now
        public string ContextId { get; private set; }
        public FormationLevel FormationLevel { get; private set; } = FormationLevel.Constructor;
        
        public SoulFrameSession(string sessionId, string operatorId)
        {
             SessionId = sessionId;
             OperatorId = operatorId;
             ContextId = sessionId; // Default Context is Session (U0)
        }

        public void PromoteFormation()
        {
            FormationLevel = FormationLevel.HigherFormation;
        }

        public void SetTheaterMode(TheaterMode mode, string? theaterId)
        {
             CurrentTheaterMode = mode;
             CurrentTheaterId = theaterId;
             // If transitioning to Idle, clear ID? For now, keep last ID for audit.
             if (mode == TheaterMode.Idle) CurrentTheaterId = null;
        }

        /// <summary>
        /// The set of AgentProfileIds allowed in this session (The Roster).
        /// </summary>
        public HashSet<string> AllowedAgentProfileIds { get; set; } = new HashSet<string>();

        /// <summary>
        /// The currently active agent. Only intents from this agent are admissible.
        /// </summary>
        public string? ActiveAgentProfileId { get; private set; }

        /// <summary>
        /// World tick when the last switch occurred. Used for cooldowns.
        /// </summary>
        public long LastAgentSwitchTick { get; private set; }

        /// <summary>
        /// The current session SAT mode, updated via explicit elevation.
        /// </summary>
        public SatMode CurrentSatMode { get; private set; } = SatMode.Baseline;

        public bool IsQuiesced { get; private set; }
        public bool IsSealed { get; private set; }
        public bool IsCleared { get; private set; }
        public (string World, string Session)? LastSealedHashes { get; private set; }
 
        // Meaning Lattice State
        public FrameLock FrameLock { get; private set; } = new FrameLock { Goal = "", IsSet = false };
        public Dictionary<string, MeaningSpan> Spans { get; private set; } = new Dictionary<string, MeaningSpan>();
        public RiskBandAssessment? LastRiskAssessment { get; private set; }

        public MountRegistry Mounts { get; } = new MountRegistry();
        public Oan.SoulFrame.Identity.OpalTipRegistry OpalTips { get; } = new Oan.SoulFrame.Identity.OpalTipRegistry();
        public HodgeTheaterSeed? TheaterSeed { get; private set; }



        public void AddToRoster(string agentProfileId)
        {
            AllowedAgentProfileIds.Add(agentProfileId);
        }

        public bool ValidateActivation(string agentProfileId, long currentTick, long minDuration, out string error)
        {
            error = string.Empty;
            if (!AllowedAgentProfileIds.Contains(agentProfileId))
            {
                 error = "SOULFRAME.AGENT_NOT_IN_ROSTER";
                 return false;
            }
            
            // Check Cooldown
            long elapsed = currentTick - LastAgentSwitchTick;
            
            // Allow initial activation (ActiveAgentProfileId is null) provided we are not enforcing strict 0-start
            // Actually, if ActiveAgentProfileId is null, we should allow it.
            if (ActiveAgentProfileId != null && elapsed < minDuration)
            {
                 error = "SOULFRAME.SWITCH_COOLDOWN";
                 return false;
            }
            
            return true;
        }

        public void Apply(AgentActivationChangedEvent evt)
        {
            // Blindly apply state from event (Trust the Ledger)
            if (evt.ToAgentProfileId != null)
            {
                ActiveAgentProfileId = evt.ToAgentProfileId;
                LastAgentSwitchTick = evt.WorldTick;
            }
        }

        public void Apply(Oan.Core.Events.SessionQuiescedEvent evt)
        {
            IsQuiesced = true;
        }

        public void Apply(Oan.Core.Events.SessionSealedEvent evt)
        {
            IsSealed = true;
            LastSealedHashes = (evt.FinalWorldStateHash, evt.FinalSessionStateHash);
        }

        public void Apply(Oan.Core.Events.SessionFoldedEvent evt)
        {
            // Folding might reset active agent or put it in a holding state
            // For now, we just acknowledge the fold.
        }

        public void Apply(Oan.Core.Events.SoulFrameClearedEvent evt)
        {
            IsCleared = true;
            AllowedAgentProfileIds.Clear();
            ActiveAgentProfileId = null;
        }

        public void Apply(Oan.Core.Events.DialecticTraceEvent evt)
        {
            switch (evt.Kind)
            {
                case Oan.Core.Events.DialecticEventType.SpansProposed:
                    // Assuming payload is compatibly castable or deserialized. 
                    // For in-memory, direct cast working.
                    if (evt.Payload is IEnumerable<MeaningSpan> proposedSpans)
                    {
                        foreach (var span in proposedSpans)
                        {
                            Spans[span.SpanId] = span;
                        }
                    }
                    else if (evt.Payload is MeaningSpan singleSpan)
                    {
                         Spans[singleSpan.SpanId] = singleSpan;
                    }
                    break;

                case Oan.Core.Events.DialecticEventType.SpanConfirmed:
                case Oan.Core.Events.DialecticEventType.SpanEdited:
                case Oan.Core.Events.DialecticEventType.SpanRejected:
                    if (evt.Payload is MeaningSpan updatedSpan)
                    {
                        if (Spans.ContainsKey(updatedSpan.SpanId))
                        {
                            Spans[updatedSpan.SpanId] = updatedSpan;
                        }
                    }
                    break;

                case Oan.Core.Events.DialecticEventType.FrameLockSet:
                    if (evt.Payload is FrameLock lockData)
                    {
                        FrameLock = lockData;
                    }
                    break;

                case Oan.Core.Events.DialecticEventType.RiskAssessed:
                    if (evt.Payload is RiskBandAssessment risk)
                    {
                        LastRiskAssessment = risk;
                    }
                    break;
            }
        }

        public void Apply(Oan.Core.Events.MountCommittedEvent evt)
        {
            // Reconstruct SliAddress from canonical string
            string[] parts = evt.CanonicalAddress.Split(':');
            if (parts.Length == 3)
            {
                var address = new SliAddress(
                    Enum.Parse<SliChannel>(parts[0], true),
                    Enum.Parse<SliPartition>(parts[1], true),
                    Enum.Parse<SliMirror>(parts[2], true)
                );

                var entry = new MountEntry
                {
                    Address = address,
                    MountId = evt.MountId,
                    PolicyVersion = evt.PolicyVersion,
                    SatCeiling = evt.SatCeiling,
                    RequiresHitlForElevation = evt.RequiresHitlForElevation,
                    CreatedTick = evt.CreatedTick
                };

                Mounts.TryAddMount(entry);
            }
        }

        public void Apply(SatElevationOutcomeEvent evt)
        {
            if (evt.Result == SatElevationResult.Granted)
            {
                CurrentSatMode = evt.ResultingMode;
            }
        }

        public void Apply(HodgeTheaterSeededEvent evt)
        {
            if (TheaterSeed == null)
            {
                TheaterSeed = evt.Seed;
            }
        }

        public void Apply(EngrammitizedEvent evt)
        {
            // Enforce Append-Only Continuity
            bool success = OpalTips.TryAdvanceTip(evt.TheaterId, evt.ParentTip, evt.NewTip);
            if (!success)
            {
                // Should have thrown exception if conflict, but if false for some reason:
                throw new System.InvalidOperationException($"CRITICAL: Failed to advance OpalTip for Theater {evt.TheaterId}");
            }
        }
    }

    /// <summary>
    /// Deterministic, append-only registry of SLI capability mounts.
    /// Invariant B: Rejects overwrites of existing mount keys.
    /// </summary>
    public class MountRegistry
    {
        private readonly Dictionary<(SliChannel, SliPartition, SliMirror), MountEntry> _mounts = 
            new Dictionary<(SliChannel, SliPartition, SliMirror), MountEntry>();

        public bool IsMounted(SliAddress address, out MountEntry? entry)
        {
            return _mounts.TryGetValue((address.Channel, address.Partition, address.Mirror), out entry);
        }

        public bool TryAddMount(MountEntry entry)
        {
            var key = (entry.Address.Channel, entry.Address.Partition, entry.Address.Mirror);
            if (_mounts.ContainsKey(key))
            {
                return false; // Invariant B: No mutation/overwrite
            }

            _mounts[key] = entry;
            return true;
        }

        /// <summary>
        /// For telemetry: get all mounts in deterministic order.
        /// </summary>
        public IEnumerable<MountEntry> GetActiveMounts()
        {
            return _mounts.Values.OrderBy(m => m.MountId, StringComparer.Ordinal);
        }
    }
}
