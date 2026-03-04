using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Oan.SoulFrame.Identity
{
    public sealed class OpalTipRegistry
    {
        private readonly ConcurrentDictionary<string, string> _tips = new ConcurrentDictionary<string, string>();

        public string? GetTip(string theaterId)
        {
            return _tips.TryGetValue(theaterId, out var tip) ? tip : null;
        }

        public bool TryAdvanceTip(string theaterId, string? parentTip, string newTip)
        {
            if (newTip == null) throw new ArgumentNullException(nameof(newTip));

            while (true)
            {
                if (!_tips.TryGetValue(theaterId, out var existing))
                {
                    if (parentTip != null) throw new InvalidOperationException("TIP_CONFLICT: Genesis tip must be null.");

                    if (_tips.TryAdd(theaterId, newTip))
                        return true;

                    continue; // race retry
                }

                if (existing != parentTip)
                    throw new InvalidOperationException($"TIP_CONFLICT: Existing tip {existing} does not match parent {parentTip ?? "null"}.");

                if (_tips.TryUpdate(theaterId, newTip, existing))
                    return true;

                // race retry
            }
        }

        public void LoadTipSnapshot(IEnumerable<OpalTheaterTip> tips)
        {
            if (!_tips.IsEmpty)
            {
                throw new InvalidOperationException("OPAL_REGISTRY_NOT_EMPTY: Cannot load snapshot into a dirty registry.");
            }

            foreach (var tip in tips)
            {
                if (!_tips.TryAdd(tip.TheaterId, tip.Tip))
                {
                    throw new InvalidOperationException($"OPAL_SNAPSHOT_DUPLICATE: Duplicate theater ID {tip.TheaterId} in snapshot.");
                }
            }
        }

        public IReadOnlyDictionary<string, string> GetAllTips() => _tips;
    }
}
