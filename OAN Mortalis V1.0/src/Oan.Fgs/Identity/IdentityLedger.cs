using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Oan.Fgs.Identity
{
    public interface ISignatureVerifier
    {
        bool Verify(string eventId, string signature, string actorId, string cradleId);
    }

    /// <summary>
    /// Lab default: verifies nothing cryptographically, but still enforces that a signature exists
    /// on predicates that require signing. Replace with real verifier in Stage 3.
    /// </summary>
    public sealed class PermissiveSignatureVerifier : ISignatureVerifier
    {
        public bool Verify(string eventId, string signature, string actorId, string cradleId) => true;
    }

    public static class Canon
    {
        public static string Sha256Hex(string utf8)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(utf8);
            byte[] hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        /// <summary>
        /// Canonicalize arbitrary payload into stable JSON:
        /// - Object properties sorted by name (ordinal)
        /// - Arrays keep order
        /// - Numbers/strings/bools/null preserved as-is
        /// </summary>
        public static string CanonicalizePayload(object? payload)
        {
            if (payload is null) return "{}";

            // If user already provides canonical JSON, accept it verbatim.
            if (payload is string s)
                return s.Trim().Length == 0 ? "{}" : s;

            JsonElement element = JsonSerializer.SerializeToElement(payload);
            return CanonicalizeJson(element);
        }

        private static string CanonicalizeJson(JsonElement element)
        {
            using var ms = new System.IO.MemoryStream();
            using (var w = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = false }))
            {
                WriteCanonical(w, element);
            }
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static void WriteCanonical(Utf8JsonWriter w, JsonElement e)
        {
            switch (e.ValueKind)
            {
                case JsonValueKind.Object:
                    w.WriteStartObject();
                    var props = new List<JsonProperty>();
                    foreach (var p in e.EnumerateObject()) props.Add(p);
                    props.Sort((a, b) => StringComparer.Ordinal.Compare(a.Name, b.Name));
                    foreach (var p in props)
                    {
                        w.WritePropertyName(p.Name);
                        WriteCanonical(w, p.Value);
                    }
                    w.WriteEndObject();
                    break;

                case JsonValueKind.Array:
                    w.WriteStartArray();
                    foreach (var item in e.EnumerateArray())
                        WriteCanonical(w, item);
                    w.WriteEndArray();
                    break;

                case JsonValueKind.String:
                    w.WriteStringValue(e.GetString());
                    break;

                case JsonValueKind.Number:
                    // Preserve numeric representation as JSON number
                    if (e.TryGetInt64(out long l)) w.WriteNumberValue(l);
                    else if (e.TryGetDouble(out double d)) w.WriteNumberValue(d);
                    else w.WriteRawValue(e.GetRawText(), skipInputValidation: true);
                    break;

                case JsonValueKind.True:
                    w.WriteBooleanValue(true);
                    break;

                case JsonValueKind.False:
                    w.WriteBooleanValue(false);
                    break;

                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    w.WriteNullValue();
                    break;

                default:
                    w.WriteRawValue(e.GetRawText(), skipInputValidation: true);
                    break;
            }
        }

        public static string CanonicalEventCore(
            long index,
            string cradleId,
            string actorId,
            string predicateId,
            string payloadHash,
            string prevTip)
        {
            // Fixed field order, explicit null handling (none here), ordinal semantics by construction.
            return $"{{\"index\":{index},\"cradleId\":\"{Escape(cradleId)}\",\"actorId\":\"{Escape(actorId)}\",\"predicateId\":\"{Escape(predicateId)}\",\"payloadHash\":\"{Escape(payloadHash)}\",\"prevTip\":\"{Escape(prevTip)}\"}}";
        }

        private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    public class FgsEvent
    {
        // Deterministic identity fields (sovereign core)
        public long Index { get; internal set; } = -1;
        public string EventId { get; internal set; } = string.Empty; // set to hash(core) at append time
        public string CradleId { get; set; } = string.Empty;
        public string ActorId { get; set; } = string.Empty;
        public string PredicateId { get; set; } = string.Empty;
        public string PrevTip { get; set; } = GenesisTip;

        // Payload handling (canonical + hashed)
        public object? Payload { get; set; }
        public string PayloadHash { get; internal set; } = string.Empty;

        // Signature is enforced for specific predicate families (fail-closed).
        public string? Signature { get; set; }

        // Observational metadata (NOT part of EventId hashing)
        public long? ObservedTick { get; set; }

        public const string GenesisTip = "0000000000000000000000000000000000000000000000000000000000000000";

        internal string ComputeCoreCanonical()
        {
            if (Index < 0) throw new InvalidOperationException("Index not assigned.");
            return Canon.CanonicalEventCore(Index, CradleId, ActorId, PredicateId, PayloadHash, PrevTip);
        }

        internal string ComputeEventId()
        {
            string core = ComputeCoreCanonical();
            return Canon.Sha256Hex(core);
        }
    }

    public class IdentityLedger
    {
        private readonly List<FgsEvent> _events = new();
        private readonly ISignatureVerifier _verifier;

        private string _currentTip = FgsEvent.GenesisTip;

        public IdentityLedger(ISignatureVerifier? verifier = null)
        {
            _verifier = verifier ?? new PermissiveSignatureVerifier();
        }

        public string CurrentTip => _currentTip;
        public IReadOnlyList<FgsEvent> Events => _events;

        public void Append(FgsEvent @event)
        {
            // Strict fail-closed tip discipline
            if (@event.PrevTip != _currentTip)
                throw new InvalidOperationException($"Tip conflict: expected {_currentTip}, got {@event.PrevTip}. System fail-closed.");

            // Assign deterministic index
            @event.Index = _events.Count;

            // Canonicalize payload and compute payload hash deterministically
            string payloadCanonical = Canon.CanonicalizePayload(@event.Payload);
            string computedPayloadHash = Canon.Sha256Hex(payloadCanonical);
            @event.PayloadHash = computedPayloadHash;

            // Enforce signature presence for high-sensitivity predicate families
            if (RequiresSignature(@event.PredicateId))
            {
                if (string.IsNullOrWhiteSpace(@event.Signature))
                    throw new InvalidOperationException("Missing signature for signed predicate. System fail-closed.");
            }

            // Compute deterministic EventId from sovereign core
            @event.EventId = @event.ComputeEventId();

            // Verify signature if applicable (fail-closed)
            if (RequiresSignature(@event.PredicateId))
            {
                if (!_verifier.Verify(@event.EventId, @event.Signature!, @event.ActorId, @event.CradleId))
                    throw new InvalidOperationException("Signature verification failed. Integrity violation detected.");
            }

            _events.Add(@event);
            _currentTip = @event.EventId;
        }

        public bool VerifyChain()
        {
            string expectedPrev = FgsEvent.GenesisTip;

            for (int i = 0; i < _events.Count; i++)
            {
                var e = _events[i];

                if (e.Index != i) return false;
                if (e.PrevTip != expectedPrev) return false;

                // Recompute payload hash deterministically
                string payloadCanonical = Canon.CanonicalizePayload(e.Payload);
                string payloadHash = Canon.Sha256Hex(payloadCanonical);
                if (!StringComparer.Ordinal.Equals(payloadHash, e.PayloadHash)) return false;

                // Recompute event id deterministically
                string recomputedId = e.ComputeEventId();
                if (!StringComparer.Ordinal.Equals(recomputedId, e.EventId)) return false;

                // Signature check (if required)
                if (RequiresSignature(e.PredicateId))
                {
                    if (string.IsNullOrWhiteSpace(e.Signature)) return false;
                    if (!_verifier.Verify(e.EventId, e.Signature!, e.ActorId, e.CradleId)) return false;
                }

                expectedPrev = e.EventId;
            }

            return StringComparer.Ordinal.Equals(expectedPrev, _currentTip);
        }

        private static bool RequiresSignature(string predicateId)
        {
            // Stage 2 policy: require signatures for Identity (id) and Security (sc) families.
            // This is intentionally conservative; refine by predicate metadata later.
            return predicateId.StartsWith("fgs.core.id.", StringComparison.Ordinal) ||
                   predicateId.StartsWith("fgs.core.sc.", StringComparison.Ordinal);
        }
    }
}
