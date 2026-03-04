using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Oan.Core.Engrams;

namespace Oan.AgentiCore.Engrams
{
    public class EngramStore
    {
        // Store Tuple of Block + CanonicalBytes
        private readonly ConcurrentDictionary<string, (EngramBlock Block, byte[] Bytes)> _store = new();

        public void Append(EngramBlock block, byte[] canonicalBytes)
        {
            if (string.IsNullOrEmpty(block.EngramId))
            {
                throw new ArgumentNullException(nameof(block.EngramId), "EngramId cannot be null or empty.");
            }
            if (canonicalBytes == null || canonicalBytes.Length == 0)
            {
                 throw new ArgumentNullException(nameof(canonicalBytes), "Canonical bytes required for integrity.");
            }

            // Attempt to add
            var value = (Block: block, Bytes: canonicalBytes);
            if (!_store.TryAdd(block.EngramId, value))
            {
                // Key exists. Check for idempotency using Canonical Bytes.
                if (_store.TryGetValue(block.EngramId, out var existing))
                {
                    // Strict Byte Comparison
                    if (!existing.Bytes.SequenceEqual(canonicalBytes))
                    {
                        throw new InvalidOperationException($"CRITICAL: Engram ID collision with different Canonical Content for {block.EngramId}. Store integrity compromised.");
                    }

                    // Idempotent success.
                    return;
                }
            }
        }

        public EngramBlock? GetById(string id)
        {
            if (_store.TryGetValue(id, out var result))
            {
                return result.Block;
            }
            return null;
        }

        public IEnumerable<EngramBlock> GetAll()
        {
            return _store.Values.Select(v => v.Block);
        }

        // Phase 4A: CleaveRecord Sidecar (in-memory mock for now)
        private readonly ConcurrentQueue<CleaveRecord> _cleaveRecords = new();

        public void AppendCleaveRecord(CleaveRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            _cleaveRecords.Enqueue(record);
        }

        public IEnumerable<CleaveRecord> GetCleaveRecords()
        {
            return _cleaveRecords.ToArray();
        }

        // Phase 4C: GOA Sidecars
        private readonly ConcurrentDictionary<string, ResidueSet> _residueSets = new();
        private readonly ConcurrentDictionary<string, UptakePlan> _uptakePlans = new();

        public void AppendResidueSet(ResidueSet residue)
        {
            if (residue == null) throw new ArgumentNullException(nameof(residue));
            if (!_residueSets.TryAdd(residue.ResidueSetId, residue))
            {
                // Idempotency check
                if (!_residueSets.TryGetValue(residue.ResidueSetId, out var existing) || existing != residue)
                {
                    throw new InvalidOperationException($"ResidueSet ID collision: {residue.ResidueSetId}");
                }
            }
        }

        public void AppendUptakePlan(UptakePlan plan)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (!_uptakePlans.TryAdd(plan.UptakePlanId, plan))
            {
                // Idempotency check
                if (!_uptakePlans.TryGetValue(plan.UptakePlanId, out var existing) || existing != plan)
                {
                    throw new InvalidOperationException($"UptakePlan ID collision: {plan.UptakePlanId}");
                }
            }
        }

        public ResidueSet? GetResidueSet(string id) => _residueSets.TryGetValue(id, out var rs) ? rs : null;
        public UptakePlan? GetUptakePlan(string id) => _uptakePlans.TryGetValue(id, out var up) ? up : null;

        // Phase 4D: GovernanceDecisions
        private readonly ConcurrentDictionary<string, GovernanceDecision> _decisions = new();

        public void AppendGovernanceDecision(GovernanceDecision decision)
        {
            if (decision == null) throw new ArgumentNullException(nameof(decision));
            if (!_decisions.TryAdd(decision.DecisionId, decision))
            {
                // Idempotency check
                if (!_decisions.TryGetValue(decision.DecisionId, out var existing) || existing.Verdict != decision.Verdict)
                {
                    throw new InvalidOperationException($"GovernanceDecision ID collision: {decision.DecisionId}");
                }
            }
        }

        public GovernanceDecision? GetGovernanceDecision(string id) => _decisions.TryGetValue(id, out var d) ? d : null;
        public IEnumerable<GovernanceDecision> GetAllDecisions() => _decisions.Values.ToArray();

        public List<UptakePlan> GetAllUptakePlans() => _uptakePlans.Values.ToList();
        public List<ResidueSet> GetAllResidueSets() => _residueSets.Values.ToList();
    }
}
