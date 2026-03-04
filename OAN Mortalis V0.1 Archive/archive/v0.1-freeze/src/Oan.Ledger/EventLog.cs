using System;
using System.Collections.Generic;
using System.Linq;
using Oan.Core;

namespace Oan.Ledger
{
    public class LedgerEvent
    {
        public long Tick { get; set; }
        public required string Type { get; set; }
        public required object Payload { get; set; }
        public string Hash { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
    }

    public class EventLog
    {
        private List<LedgerEvent> _events = new List<LedgerEvent>();

        public void Append(string type, object payload, long tick = 0)
        {
            var evt = new LedgerEvent
            {
                Type = type,
                Payload = payload,
                Tick = tick
            };

            // Deterministic Id via SHA256
            string input = $"{type}|{tick}|{payload}";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] hash = System.Security.Cryptography.SHA256.HashData(bytes);
            evt.Id = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            _events.Add(evt);
        }

        public IEnumerable<LedgerEvent> GetEvents()
        {
            return _events.AsReadOnly();
        }
    }
}
