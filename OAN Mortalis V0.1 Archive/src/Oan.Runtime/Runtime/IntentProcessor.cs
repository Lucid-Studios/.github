using System.Collections.Generic;
using Oan.Core;
using Oan.Core.Governance;
using Oan.SoulFrame;
using Oan.SoulFrame.Identity; // Added for EngrammitizationEncoding
using Oan.SoulFrame.Governance;
using Oan.Ledger;
using Oan.Core.Events;
using System;
using System.Linq;

namespace Oan.Runtime
{
    public class IntentProcessor
    {
        private readonly WorldState _worldState;
        private readonly SituationalAwarenessTuple _sat;
        private readonly SoulFrameSession _session; // Governance Context
        private readonly EventLog _ledger; // For activation events
        private readonly Oan.SoulFrame.SLI.SliGateService _sliGate;

        public IntentProcessor(WorldState worldState, SoulFrameSession session, EventLog ledger, Oan.SoulFrame.SLI.SliGateService sliGate)
        {
            _worldState = worldState;
            _session = session;
            _ledger = ledger;
            _sliGate = sliGate;
            _sat = new SituationalAwarenessTuple();
        }

        private SliResolutionResult CheckSli(Intent intent, string runId = "none")
        {
            var result = _sliGate.Resolve(intent, _session, _session.CurrentSatMode, _worldState.Tick, runId);
            
            // Log to ledger
            var evt = new Oan.Core.Events.SliDecisionEvent
            {
                Tick = _worldState.Tick,
                SessionId = _session.SessionId,
                OperatorId = _session.OperatorId,
                Handle = result.Handle,
                ResolvedAddress = result.ResolvedAddress,
                Allowed = result.Allowed,
                ReasonCode = result.ReasonCode,
                PolicyVersion = result.PolicyVersion,
                SatMode = result.SatModeAtDecision,
                MaskingApplied = result.MaskingApplied
            };
            
            _ledger.Append("SliDecision", evt, _worldState.Tick);
            
            return result;
        }

        public IntentResult ActivateAgent(string agentProfileId, string reason)
        {
            long tick = _worldState.Tick;
            if (_session.ValidateActivation(agentProfileId, tick, 10, out string error))
            {
                // Trusted Activation
                var evt = new Oan.Core.Events.AgentActivationChangedEvent
                {
                    SoulFrameSessionId = _session.SessionId,
                    OperatorId = _session.OperatorId,
                    FromAgentProfileId = _session.ActiveAgentProfileId,
                    ToAgentProfileId = agentProfileId,
                    PolicyVersion = 1,
                    WorldTick = tick,
                    Reason = reason
                };

                // Ledger appends
                // _ledger.Append("ActivationChanged", evt);
                _session.Apply(evt); // Update session state
                
                return new IntentResult 
                { 
                    Status = IntentStatus.Committed, 
                    ReasonCode = "OK",
                    PolicyVersion = "1",
                    StateDelta = new Dictionary<string, object> { { "ActiveAgent", agentProfileId } }
                };
            }
            
            return new IntentResult 
            { 
                Status = IntentStatus.Refused, 
                ReasonCode = error,
                PolicyVersion = "1",
                StateDelta = new Dictionary<string, object> { { "Message", $"Activation failed: {error}" } }
            };
        }

        /// <summary>
        /// Reads-only evaluation. Does not mutate state or append to ledger.
        /// Checks invariants and SAT.
        /// </summary>
        public IntentResult EvaluateIntent(Intent intent, string runId = "none")
        {
            // --- THEATER MODE CHECK ---
            bool isBootstrap = intent.SliHandle == "sys/admin/theater.transition" || 
                               intent.SliHandle == "sys/admin/mount.commit" ||
                               intent.SliHandle == "sys/admin/formation.promote"; 

            if (_session.CurrentTheaterMode == TheaterMode.Idle)
            {
                if (!isBootstrap)
                {
                    return Refuse(intent, "THEATER_IDLE", "Session is in IDLE mode. Explicit transition required.");
                }
            }

            // --- SLI GATE CHECK ---
            var sli = CheckSli(intent, runId);
            if (!sli.Allowed)
            {
                return new IntentResult
                {
                    Status = IntentStatus.Refused,
                    ReasonCode = sli.ReasonCode,
                    PolicyVersion = sli.PolicyVersion,
                    StateDelta = new Dictionary<string, object> { { "Message", "SLI Gate Denied" } }
                };
            }

             // 0. Anti-Swarm Invariant Check (Read-Only)
            if (!isBootstrap && _session.ActiveAgentProfileId != intent.AgentProfileId)
            {
                if (_session.ActiveAgentProfileId == null)
                    return Refuse(intent, "SOULFRAME.NO_ACTIVE_AGENT", "No agent is currently active in this session.");
                
                return Refuse(intent, "SOULFRAME.AGENT_NOT_ACTIVE", $"Agent {intent.AgentProfileId} is not the active agent.");
            }

            // 1. Structural Validation
            if (string.IsNullOrEmpty(intent.SourceAgentId))
            {
                return Refuse(intent, "INVALID_SOURCE", "Source Agent ID is missing.");
            }

            // 2. State Validation
            // 2. State Validation
            if (!isBootstrap)
            {
                var agent = _worldState.GetEntity(intent.SourceAgentId);
                if (agent == null)
                {
                    return Refuse(intent, "AGENT_NOT_FOUND", $"Agent {intent.SourceAgentId} not found in world state.");
                }
            }

            // 3. Governance Check (SAT)
            float drift = 1.0f;
            float entropy = 0.5f;

            if (intent.Parameters.ContainsKey("DebugDrift"))
            {
                if (float.TryParse(intent.Parameters["DebugDrift"] as string, out float d))
                {
                    drift = d;
                }
            }

            var govAction = _sat.EvaluateSealEligibility(drift, entropy);
            
            if (govAction == GovernanceAction.Compost)
            {
                 return Refuse(intent, "GOVERNANCE_COMPOST", "SAT rejected intent due to high incoherence.");
            }
            if (govAction == GovernanceAction.RepetitionPenalty)
            {
                 return Refuse(intent, "GOVERNANCE_REPETITION", "SAT rejected intent due to high entropy.");
            }
            
            // If all checks pass, return Admissible (but do not commit)
            return new IntentResult
            {
                IntentId = intent.Id,
                Status = IntentStatus.Pending, // Indicates Admissible but not Committed
                ReasonCode = "ADMISSIBLE",
                PolicyVersion = "1",
                StateDelta = new Dictionary<string, object> { { "Check", "Passed" } }
            };
        }

        /// <summary>
        /// Commits the intent to the world state.
        /// Re-validates invariant to prevent race conditions or bypasses.
        /// </summary>
        public IntentResult CommitIntent(Intent intent)
        {
            int eventsBefore = _ledger.GetEvents().Count();

            // 0. Anti-Swarm Invariant Check (Defense-in-Depth)
            if (_session.IsQuiesced || _session.IsSealed || _session.IsCleared)
            {
                return Refuse(intent, "SOULFRAME.SESSION_CLOSED", "Session is closed for new commits.");
            }

            // 0b. Theater Mode Invariant Check
            bool isBootstrap = intent.SliHandle == "sys/admin/theater.transition" || 
                               intent.SliHandle == "sys/admin/mount.commit" ||
                               intent.SliHandle == "sys/admin/formation.promote"; 

            if (_session.CurrentTheaterMode == TheaterMode.Idle)
            {
                // Only Allow Bootstrap Handles in Idle
                if (!isBootstrap)
                {
                    return Refuse(intent, "THEATER_IDLE", "Session is in IDLE mode. Explicit transition required.");
                }
            }

            // --- SLI GATE CHECK ---
            var sli = CheckSli(intent);
            if (!sli.Allowed)
            {
                return Refuse(intent, sli.ReasonCode, $"SLI Deny: {sli.ReasonCode}", sli.PolicyVersion);
            }

            // Active Agent Check (Skip for Bootstrap)
            if (!isBootstrap && _session.ActiveAgentProfileId != intent.AgentProfileId)
            {
                // Strict refusal logic repeated
                if (_session.ActiveAgentProfileId == null)
                    return Refuse(intent, "SOULFRAME.NO_ACTIVE_AGENT", "Commit refused: No agent active.");
                
                return Refuse(intent, "SOULFRAME.AGENT_NOT_ACTIVE", $"Commit refused: Agent {intent.AgentProfileId} not active.");
            }

            IntentResult result;

            // 4. Special Handle Processing: Mount Capability
            if (intent.SliHandle == "sys/admin/mount.commit")
            {
                result = ProcessMount(intent, sli);
            }
            // 4b. Special Handle Processing: SAT Elevation
            else if (intent.SliHandle == "sys/admin/sat.elevate.request")
            {
                result = ProcessElevation(intent, sli);
            }
            else if (intent.SliHandle == "sys/admin/theater.transition")
            {
                result = ProcessTheaterTransition(intent, sli);
            }
            else if (intent.SliHandle == "sys/admin/formation.promote")
            {
                result = ProcessFormationPromotion(intent, sli);
            }
            else
            {
                // 5. Generic Commit / Apply
                ApplyIntent(intent);
                result = new IntentResult
                {
                    IntentId = intent.Id,
                    Status = IntentStatus.Committed,
                    ReasonCode = "INTENT_APPROVED",
                    PolicyVersion = "1",
                    StateDelta = new Dictionary<string, object>
                    {
                        { "Action", intent.Action },
                        { "Target", intent.TargetAgentId ?? "None" },
                        { "Tick", _worldState.Tick }
                    }
                };
            }

            // --- IDENTITY LAYER (ENGRAMMITIZATION) ---
            if (result.Status == IntentStatus.Committed)
            {
                ProcessIdentityAdvancement(intent, result, eventsBefore);
            }

            return result;
        }

        private void ProcessIdentityAdvancement(Intent intent, IntentResult result, int eventsBefore)
        {
            string runId = intent.Parameters.ContainsKey("RunId") ? intent.Parameters["RunId"] as string ?? "none" : "none";
            var atlasEntry = Oan.SoulFrame.Atlas.RootAtlasRegistry.Get(intent.SliHandle);
            string policyVersion = atlasEntry?.PolicyVersion ?? "sli.policy.v0.1";
            string intentKind = atlasEntry?.IntentKind ?? "Generic";

            // 1. Seed Theater if needed? NO. Implicit seeding is BANNED in Phase 1.
            // Requirement: "Theater transitions must be explicit; no implicit shift"
            // If internal logic is busted, we fail hard or do nothing.
            
            if (_session.CurrentTheaterId == null || _session.CurrentTheaterMode == TheaterMode.Idle)
            {
                // Should have been caught by CommitIntent logic, but double check.
                return; 
            }

            string currentTheaterId = _session.CurrentTheaterId;

            // 2. Collect Witnesses
            var newEvents = _ledger.GetEvents().Skip(eventsBefore).ToList();
            List<string> witnessIds = newEvents.Select(e => e.Id).ToList();
            List<string> witnessTypes = newEvents.Select(e => e.Type).ToList();

            // Explicitly using Oan.SoulFrame.Identity.TheaterIdentityService to ensure correct resolution
            byte[] nfkInput = Oan.SoulFrame.Identity.TheaterIdentityService.ComputeNormalFormKeyInput(
                policyVersion,
                intentKind,
                intent.SliHandle ?? string.Empty,
                witnessTypes);
            
            string nfk = Oan.SoulFrame.Identity.TheaterIdentityService.HashBytes(nfkInput);
            
            // 3. Mode Switch: Prime vs OAN/Mantle
            // 3. Mode Switch: Prime vs OAN/Mantle
            // + Phase 2 Constraint: Identity-binding requires (TheaterMode == Prime && FormationLevel == HigherFormation)
            
            if (_session.CurrentTheaterMode == TheaterMode.Prime)
            {
                if (_session.FormationLevel == FormationLevel.HigherFormation)
                {
                    string? parentTip = _session.OpalTips.GetTip(currentTheaterId);
                
                    var engramEvt = Oan.SoulFrame.Identity.TheaterIdentityService.BuildEngrammitizedEvent(
                        currentTheaterId, parentTip, nfk, witnessIds, _worldState.Tick);

                    if (intent.Parameters.ContainsKey("Factors") && intent.Parameters["Factors"] is Dictionary<string, string> f)
                    {
                        engramEvt = engramEvt with { Factors = f };
                    }

                    _ledger.Append("Engrammitized", engramEvt, _worldState.Tick);
                    _session.Apply(engramEvt);
                }
                else
                {
                    // Prime but Constructor (U0) -> Ephemeral Log
                    var logEvt = new EphemeralTheaterLogEvent
                    {
                        TheaterId = currentTheaterId,
                        TheaterMode = $"{_session.CurrentTheaterMode}({_session.FormationLevel})",
                        NormalFormKey = nfk,
                        Tick = _worldState.Tick
                    };
                    _ledger.Append("EphemeralTheaterLog", logEvt, _worldState.Tick);
                }
            }
            else if (_session.CurrentTheaterMode == TheaterMode.OAN)
            {
                // Non-binding log
                var logEvt = new EphemeralTheaterLogEvent
                {
                    TheaterId = currentTheaterId,
                    TheaterMode = $"{_session.CurrentTheaterMode}({_session.FormationLevel})",
                    NormalFormKey = nfk,
                    Tick = _worldState.Tick
                };
                _ledger.Append("EphemeralTheaterLog", logEvt, _worldState.Tick);
            }
            // Mantle: Do nothing (Silence)
        }
        private IntentResult ProcessMount(Intent intent, SliResolutionResult sli)
        {
            var chanStr = intent.Parameters.ContainsKey("Channel") ? intent.Parameters["Channel"] as string : "Public";
            var partStr = intent.Parameters.ContainsKey("Partition") ? intent.Parameters["Partition"] as string : "OAN";
            var mirrStr = intent.Parameters.ContainsKey("Mirror") ? intent.Parameters["Mirror"] as string : "Standard";
            var ceilStr = intent.Parameters.ContainsKey("SatCeiling") ? intent.Parameters["SatCeiling"] as string : "Standard";

            var chan = (SliChannel)Enum.Parse(typeof(SliChannel), chanStr ?? "Public", true);
            var part = (SliPartition)Enum.Parse(typeof(SliPartition), partStr ?? "OAN", true);
            var mirr = (SliMirror)Enum.Parse(typeof(SliMirror), mirrStr ?? "Standard", true);
            var ceiling = (SatMode)Enum.Parse(typeof(SatMode), ceilStr ?? "Standard", true);
            var hitl = (bool)(intent.Parameters.ContainsKey("RequiresHitl") ? intent.Parameters["RequiresHitl"] : false);
            
            var address = new SliAddress(chan, part, mirr);
            var canonical = MountEntry.GetCanonicalAddressString(address);
            var policy = sli.PolicyVersion;
            
            // Invariant A: SHA256(runId + canonical_address + policy)
            var runId = intent.Parameters.ContainsKey("RunId") ? intent.Parameters["RunId"] as string ?? "none" : "none";
            var input = $"{runId}{canonical}{policy}";
            using var sha = System.Security.Cryptography.SHA256.Create();
            var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            var mountId = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            var mountEvt = new Oan.Core.Events.MountCommittedEvent
            {
                MountId = mountId,
                CanonicalAddress = canonical,
                PolicyVersion = policy,
                SatCeiling = ceiling,
                RequiresHitlForElevation = hitl,
                CreatedTick = _worldState.Tick
            };

            _ledger.Append("MountCommitted", mountEvt, _worldState.Tick);
            _session.Apply(mountEvt);

            return new IntentResult
            {
                IntentId = intent.Id,
                Status = IntentStatus.Committed,
                ReasonCode = "MOUNT_SUCCESS",
                PolicyVersion = policy,
                StateDelta = new Dictionary<string, object> { { "MountId", mountId } }
            };
        }

        private IntentResult ProcessElevation(Intent intent, SliResolutionResult sli)
        {
            var modeStr = intent.Parameters.ContainsKey("RequestedMode") ? intent.Parameters["RequestedMode"] as string : "Standard";
            var targetMode = (SatMode)Enum.Parse(typeof(SatMode), modeStr ?? "Standard", true);
            var reason = intent.Parameters.ContainsKey("Reason") ? intent.Parameters["Reason"] as string ?? "None" : "None";
            var runId = intent.Parameters.ContainsKey("RunId") ? intent.Parameters["RunId"] as string ?? "none" : "none";

            // Emit Requested Event
            var reqEvt = new Oan.Core.Events.SatElevationRequestedEvent
            {
                RunId = runId,
                Tick = _worldState.Tick,
                SessionId = _session.SessionId,
                RequestedMode = targetMode,
                TargetAddress = intent.Parameters.ContainsKey("TargetAddress") ? intent.Parameters["TargetAddress"] as string ?? "None" : "None",
                Reason = reason
            };
            _ledger.Append("SatElevationRequested", reqEvt, _worldState.Tick);

            // Deterministic Headless Outcome: HITL Required
            var outcomeEvt = new Oan.Core.Events.SatElevationOutcomeEvent
            {
                RunId = runId,
                Tick = _worldState.Tick,
                SessionId = _session.SessionId,
                Result = Oan.Core.Events.SatElevationResult.HitlRequired,
                OutcomeCode = "HITL_GATED",
                ResultingMode = _session.CurrentSatMode // No change
            };
            _ledger.Append("SatElevationOutcome", outcomeEvt, _worldState.Tick);
            // We do NOT apply the outcome here because it's not Granted.

            return new IntentResult
            {
                IntentId = intent.Id,
                Status = IntentStatus.Refused,
                ReasonCode = "HITL_REQUIRED",
                PolicyVersion = sli.PolicyVersion,
                StateDelta = new Dictionary<string, object> { { "Message", "SAT Elevation requires HITL approval." } }
            };
        }

        private IntentResult ProcessTheaterTransition(Intent intent, SliResolutionResult sli)
        {
             var targetModeStr = intent.Parameters.ContainsKey("TargetMode") ? intent.Parameters["TargetMode"] as string : "Idle";
             if (!Enum.TryParse<TheaterMode>(targetModeStr, true, out var targetMode))
             {
                 return Refuse(intent, "INVALID_THEATER_MODE", $"Unknown mode: {targetModeStr}");
             }

             string runId = intent.Parameters.ContainsKey("RunId") ? intent.Parameters["RunId"] as string ?? "none" : "none";
             
             // Idempotency: If already in mode, do nothing or re-confirm?
             // For now: Always transition (creates new TheaterId if Prime/OAN?)
             // Constraint: "ensure only Prime is eligible for identity-bearing engram commits"
             // Constraint: "Theater transitions must be explicit"

             // Generate TheaterId
             string atlasHash = TheaterIdentityService.ComputeRootAtlasHash();
             string theaterId = TheaterIdentityService.ComputeTheaterId(runId, atlasHash, sli.PolicyVersion, _worldState.Tick);

             // 1. Seed Event (if not just Switching)
             // Actually, every Transition might implies a new "Act" or TheaterId?
             // Let's assume one TheaterId per session for now? NO, that defeats the purpose of "Seeding".
             // Let's seed a new Theater on every transition to NON-IDLE.
             
             if (targetMode != TheaterMode.Idle)
             {
                 var seed = new HodgeTheaterSeed
                 {
                    TheaterId = theaterId,
                    RunId = runId,
                    GenesisTick = _worldState.Tick,
                    RootAtlasHash = atlasHash,
                    TheaterPolicyVersion = sli.PolicyVersion,
                    EntropyRegime = "standard",
                    InitialSatMode = _session.CurrentSatMode.ToString(),
                    MountRulesHash = "none"
                 };
                var seedEvt = new HodgeTheaterSeededEvent
                {
                    TheaterId = theaterId,
                    RunId = runId,
                    Tick = _worldState.Tick,
                    Seed = seed
                };
                _ledger.Append("HodgeTheaterSeeded", seedEvt, _worldState.Tick);
                _session.Apply(seedEvt);
             }
             else
             {
                 theaterId = "IDLE"; // Placeholder
             }

             // 2. Transition Event
             var transEvt = new TheaterTransitionEvent
             {
                 SessionId = _session.SessionId,
                 FromMode = _session.CurrentTheaterMode.ToString(),
                 ToMode = targetMode.ToString(),
                 TheaterId = theaterId,
                 Reason = intent.Parameters.ContainsKey("Reason") ? intent.Parameters["Reason"] as string ?? "Manual" : "Manual",
                 Tick = _worldState.Tick
             };
             
             _ledger.Append("TheaterTransition", transEvt, _worldState.Tick);
             
             // 3. Apply to Session
             _session.SetTheaterMode(targetMode, targetMode == TheaterMode.Idle ? null : theaterId);
             
             return new IntentResult
             {
                 IntentId = intent.Id,
                 Status = IntentStatus.Committed,
                 ReasonCode = "THEATER_TRANSITION_OK",
                 PolicyVersion = sli.PolicyVersion,
                 StateDelta = new Dictionary<string, object> 
                 { 
                     { "Mode", targetMode.ToString() },
                     { "TheaterId", theaterId }
                 }
             };
        }

        private IntentResult ProcessFormationPromotion(Intent intent, SliResolutionResult sli)
        {
             // Check if already promoted
             if (_session.FormationLevel == FormationLevel.HigherFormation)
             {
                 return new IntentResult
                 {
                     IntentId = intent.Id,
                     Status = IntentStatus.Committed, // Idempotent success
                     ReasonCode = "ALREADY_PROMOTED",
                     PolicyVersion = sli.PolicyVersion,
                     StateDelta = new Dictionary<string, object> { { "Level", _session.FormationLevel.ToString() } }
                 };
             }

             // Logic: U0 -> U1 (One way)
             // Requires: Explicit intent (checked by Handle)
             // Requires: Prime? Not necessarily, can promote in OAN? 
             // Constraint: "U0->U1 requires explicit morphism Φ operator" -> This intent acts as that operator.

             var evt = new FormationPromotedEvent
             {
                 SessionId = _session.SessionId,
                 FromContext = _session.ContextId,
                 ToContext = _session.ContextId, // Keeping same context ID for now, just elevating level
                 FormationLevel = FormationLevel.HigherFormation.ToString(),
                 Reason = intent.Parameters.ContainsKey("Reason") ? intent.Parameters["Reason"] as string ?? "Manual" : "Manual",
                 Tick = _worldState.Tick
             };

             _ledger.Append("FormationPromoted", evt, _worldState.Tick);
             _session.PromoteFormation();

             return new IntentResult
             {
                 IntentId = intent.Id,
                 Status = IntentStatus.Committed,
                 ReasonCode = "FORMATION_PROMOTED",
                 PolicyVersion = sli.PolicyVersion,
                 StateDelta = new Dictionary<string, object> { { "Level", "HigherFormation" } }
             };
        }
        
        // Legacy/Generic entry point
        public IntentResult Process(Intent intent)
        {
            var eval = EvaluateIntent(intent);
            if (eval.Status == IntentStatus.Refused) return eval;
            
            return CommitIntent(intent);
        }

        private void ApplyIntent(Intent intent)
        {
            // Simple logic for prototype
             _worldState.IncrementTick();
             
             if (intent.Action == "Move" && intent.Parameters.ContainsKey("Destination"))
             {
                 // Update position (Mock)
                 _worldState.UpdateEntityState(intent.SourceAgentId, "LastMove", intent.Parameters["Destination"]);
             }
        }

        private IntentResult Refuse(Intent intent, string code, string message, string? policyVersion = "1")
        {
            return new IntentResult
            {
                IntentId = intent.Id,
                Status = IntentStatus.Refused,
                ReasonCode = code,
                PolicyVersion = policyVersion,
                StateDelta = new Dictionary<string, object> { { "Message", message } }
            };
        }
    }
}
