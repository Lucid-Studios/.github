using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Oan.Core.Events;
using Oan.Core.Meaning;


namespace Oan.SoulFrame.Services
{
    public class MeaningLatticeService
    {
        private readonly Action<string, object, long> _eventAppender;
        private readonly Func<string, SoulFrameSession> _sessionProvider;

        public MeaningLatticeService(Action<string, object, long> eventAppender, Func<string, SoulFrameSession> sessionProvider)
        {
            _eventAppender = eventAppender;
            _sessionProvider = sessionProvider;
        }

        public List<MeaningSpan> ProposeSpans(string sessionId, string naturalLanguage, string contextSnapshotId, string operatorId)
        {
            var session = _sessionProvider(sessionId); // Validate session exists
            if (session == null) throw new ArgumentException($"Session {sessionId} not found.");

            // Deterministic span extraction (Simple MVP: split by whitespace)
            var tokens = naturalLanguage.Split(new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            var spans = new List<MeaningSpan>();
            int currentOffset = 0;

            foreach (var token in tokens)
            {
                // Find token in text to get accurate offsets
                int start = naturalLanguage.IndexOf(token, currentOffset);
                if (start == -1) continue; // Should not happen with Split, but safe guard
                int end = start + token.Length;
                currentOffset = end;

                // Stable SpanId: Hash of sessionId + offset + text
                string spanIdHash = ComputeSha256Hash($"{sessionId}:{start}:{token}");

                var span = new MeaningSpan
                {
                    SpanId = spanIdHash.Substring(0, 12), // Shorten for readability/MVP
                    StartOffset = start,
                    EndOffset = end,
                    Text = token,
                    Role = SyntacticRole.Unknown,
                    AmbiguityScore = 0.5f, // Heuristic default
                    Status = MeaningStatus.Proposed
                };
                spans.Add(span);
            }

            // Append Event
            var evt = new DialecticTraceEvent
            {
                EventId = Guid.NewGuid().ToString(),
                SessionId = sessionId,
                Tick = DateTime.UtcNow.Ticks, // Using UtcNow.Ticks as proxy for WorldTick since we don't have direct access here. Ideally strictly injected.
                PolicyVersion = "0.1",
                Kind = DialecticEventType.SpansProposed,
                Payload = spans
            };
            
            // In a real system, the event applier would update the session. 
            // Here we assume the caller or the ledger consumer updates the session state.
            // For the return value, we return the proposed spans.
            
            _eventAppender("DialecticTrace", evt, evt.Tick);
            
            // OPTIONAL: Immediately apply to local session state for consistency if the session is kept in memory
            session.Apply(evt);

            return spans;
        }

        public MeaningSpan UpdateSpan(string sessionId, string spanId, string userGloss, MeaningStatus status, string operatorId)
        {
            var session = _sessionProvider(sessionId);
            if (session == null) throw new ArgumentException("Session not found");

            if (!session.Spans.TryGetValue(spanId, out var existingSpan))
            {
                throw new KeyNotFoundException($"Span {spanId} not found in session {sessionId}");
            }

            // Create a copy or modify? ideally immutable events.
            // We create a new object representing the new state of the span.
            var updatedSpan = new MeaningSpan
            {
                SpanId = existingSpan.SpanId,
                StartOffset = existingSpan.StartOffset,
                EndOffset = existingSpan.EndOffset,
                Text = existingSpan.Text,
                Role = existingSpan.Role,
                ProposedGloss = existingSpan.ProposedGloss,
                UserGloss = userGloss,
                AmbiguityScore = existingSpan.AmbiguityScore,
                Status = status
            };

            var evt = new DialecticTraceEvent
            {
                EventId = Guid.NewGuid().ToString(),
                SessionId = sessionId,
                Tick = DateTime.UtcNow.Ticks,
                PolicyVersion = "0.1",
                Kind = status == MeaningStatus.Confirmed ? DialecticEventType.SpanConfirmed :
                       status == MeaningStatus.Rejected ? DialecticEventType.SpanRejected :
                       DialecticEventType.SpanEdited,
                Payload = updatedSpan
            };

            _eventAppender("DialecticTrace", evt, evt.Tick);
            session.Apply(evt);

            return updatedSpan;
        }

        public FrameLock SetFrameLock(string sessionId, FrameLock frameLock, string operatorId)
        {
            var session = _sessionProvider(sessionId);
            if (session == null) throw new ArgumentException("Session not found");

            if (string.IsNullOrEmpty(frameLock.Goal)) throw new ArgumentException("Goal is required for FrameLock");

            frameLock.IsSet = true;
            frameLock.SetTick = DateTime.UtcNow.Ticks;
            frameLock.SetByOperatorId = operatorId;

            var evt = new DialecticTraceEvent
            {
                EventId = Guid.NewGuid().ToString(),
                SessionId = sessionId,
                Tick = frameLock.SetTick,
                PolicyVersion = "0.1",
                Kind = DialecticEventType.FrameLockSet,
                Payload = frameLock
            };

            _eventAppender("DialecticTrace", evt, evt.Tick);
            session.Apply(evt);

            return frameLock;
        }

        public object GetAnchoredContext(string sessionId)
        {
            var session = _sessionProvider(sessionId);
            if (session == null) throw new ArgumentException("Session not found");

            return new
            {
                FrameLock = session.FrameLock,
                ConfirmedSpans = session.Spans.Values.Where(s => s.Status == MeaningStatus.Confirmed),
                RejectedSpans = session.Spans.Values.Where(s => s.Status == MeaningStatus.Rejected),
                EditedSpans = session.Spans.Values.Where(s => s.Status == MeaningStatus.Edited),
                RiskAssessment = session.LastRiskAssessment
            };
        }

        public RiskBandAssessment AssessRisk(string sessionId)
        {
            var session = _sessionProvider(sessionId);
            if (session == null) throw new ArgumentException("Session not found");

            var assessment = new RiskBandAssessment
            {
                Band = RiskBand.SAFE,
                Uncertainty = 0.1f,
                Explanation = "Routine check."
            };

            // Lightweight heuristic
            // Check for words in proposed spans
            foreach (var span in session.Spans.Values)
            {
                var text = span.Text.ToLowerInvariant();
                if (text.Contains("ambiguous") || text.Contains("unsure"))
                {
                    assessment.Band = RiskBand.AMBIGUOUS;
                    assessment.ContributingSpanIds.Add(span.SpanId);
                    assessment.Explanation = "Ambiguous terms detected.";
                    assessment.Uncertainty = 0.8f;
                }
                if (text.Contains("forbidden") || text.Contains("kill"))
                {
                    assessment.Band = RiskBand.HARD_STOP;
                    assessment.ContributingSpanIds.Add(span.SpanId);
                    assessment.Explanation = "Disallowed category detected.";
                    assessment.Uncertainty = 0.0f;
                }
            }

            var evt = new DialecticTraceEvent
            {
                EventId = Guid.NewGuid().ToString(),
                SessionId = sessionId,
                Tick = DateTime.UtcNow.Ticks,
                PolicyVersion = "0.1",
                Kind = DialecticEventType.RiskAssessed,
                Payload = assessment
            };

            _eventAppender("DialecticTrace", evt, evt.Tick);
            session.Apply(evt);
            
            // If AMBIGUOUS, could also append ClarifyInvited, but skipping for brevity of MVP logic unless required.
            // Prompt says: "If AMBIGUOUS, also append ClarifyInvited."
            if (assessment.Band == RiskBand.AMBIGUOUS)
            {
                 var clarifyEvt = new DialecticTraceEvent
                {
                    EventId = Guid.NewGuid().ToString(),
                    SessionId = sessionId,
                    Tick = evt.Tick + 1,
                    PolicyVersion = "0.1",
                    Kind = DialecticEventType.ClarifyInvited,
                    Payload = new { Message = "Please clarify ambiguous terms.", assessment.ContributingSpanIds }
                };
                _eventAppender("DialecticTrace", clarifyEvt, clarifyEvt.Tick);
                // Apply? Session doesn't store ClarifyInvited explicitly in the properties I added, but good to have in ledger.
            }

            return assessment;
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
