using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Oan.Core;
using Oan.Core.Events;
using Oan.Ledger;
using Oan.SoulFrame;
using Oan.Runtime; // Added for WorldState if needed

namespace Oan.CradleTek
{
    public class CloseoutReceipt
    {
        public string SessionId { get; set; } = string.Empty;
        public long Tick { get; set; }
        public int PolicyVersion { get; set; }
        public string FinalWorldHash { get; set; } = string.Empty;
        public string FinalSessionHash { get; set; } = string.Empty;
        public string? ActiveAgentProfileId { get; set; }
    }

    public class SessionOrchestrator
    {
        private readonly EventLog _ledger;
        private readonly SoulFrameSession _session;
        private readonly WorldState _worldState;

        public SessionOrchestrator(EventLog ledger, SoulFrameSession session, WorldState worldState)
        {
            _ledger = ledger;
            _session = session;
            _worldState = worldState;
        }

        public CloseoutReceipt CloseoutSession(string sessionId, string operatorId, string requestId)
        {
            if (_session.SessionId != sessionId)
            {
                throw new InvalidOperationException("Session ID mismatch");
            }

            long currentTick = _worldState.Tick;
            int policyVersion = 1; // Fixed for now
            string provenance = $"{operatorId}:{requestId}";

            // 1. Quiesce
            var quiescedEvt = new SessionQuiescedEvent
            {
                SoulFrameSessionId = sessionId,
                WorldTick = currentTick,
                PolicyVersion = policyVersion,
                Provenance = provenance
            };
            AppendAndApply("SessionQuiesced", quiescedEvt, currentTick);

            // 2. Compute Hashes (Deterministic Snapshot)
            // Note: In production, use a dedicated deterministic serializer.
            // Here we use JSON with default settings as a proxy for the requirement.
            string worldHash = ComputeHash(_worldState);
            string sessionHash = ComputeHash(_session); // Serializes public props including Roster/States

            // 3. Seal
            var sealedEvt = new SessionSealedEvent
            {
                SoulFrameSessionId = sessionId,
                WorldTick = currentTick,
                PolicyVersion = policyVersion,
                FinalWorldStateHash = worldHash,
                FinalSessionStateHash = sessionHash,
                ActiveAgentProfileId = _session.ActiveAgentProfileId,
                Provenance = provenance
            };
            AppendAndApply("SessionSealed", sealedEvt, currentTick);

            // 4. Fold (AgentiCore transition)
            var foldedEvt = new SessionFoldedEvent
            {
                SoulFrameSessionId = sessionId,
                WorldTick = currentTick,
                PolicyVersion = policyVersion,
                AgentiCoreProfileId = _session.ActiveAgentProfileId, 
                FoldMode = "Holding",
                Provenance = provenance
            };
            AppendAndApply("SessionFolded", foldedEvt, currentTick);

            // 5. Clear
            var clearedEvt = new SoulFrameClearedEvent
            {
                SoulFrameSessionId = sessionId,
                WorldTick = currentTick,
                PolicyVersion = policyVersion,
                Provenance = provenance
            };
            AppendAndApply("SoulFrameCleared", clearedEvt, currentTick);

            return new CloseoutReceipt
            {
                SessionId = sessionId,
                Tick = currentTick,
                PolicyVersion = policyVersion,
                FinalWorldHash = worldHash,
                FinalSessionHash = sessionHash,
                ActiveAgentProfileId = _session.ActiveAgentProfileId // Should be null now
            };
        }

        private void AppendAndApply(string type, object evt, long tick)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            _ledger.Append(type, evt, tick);
            
            // Dynamic dispatch to Apply overload
            // In a real system, we'd have a proper event router.
            // Here we cast or use dynamic to hit the overloads we added.
            dynamic session = _session;
            session.Apply((dynamic)evt);
        }

        private string ComputeHash(object obj)
        {
            try
            {
                // Simple stable serialization for prototype
                var json = JsonSerializer.Serialize(obj);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                byte[] hashBytes = SHA256.HashData(bytes);
                return Convert.ToHexString(hashBytes);
            }
            catch
            {
                return "HASH_ERROR";
            }
        }
    }
}
