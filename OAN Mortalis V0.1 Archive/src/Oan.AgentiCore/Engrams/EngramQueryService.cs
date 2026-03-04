using System;
using System.Collections.Generic;
using System.Linq;
using Oan.Core.Engrams;

namespace Oan.AgentiCore.Engrams
{
    public class EngramQueryService : IRecallSurface
    {
        private readonly EngramStore _store;

        public EngramQueryService(EngramStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public EngramBlock? GetById(string engramId)
        {
            return _store.GetById(engramId);
        }

        // Deterministic Ordering: Primary Tick (asc), Secondary EngramId (ordinal asc)
        private IEnumerable<EngramBlock> BaseQuery()
        {
             return _store.GetAll()
                .OrderBy(b => b.Header.Tick)
                .ThenBy(b => b.EngramId, StringComparer.Ordinal);
        }

        public IReadOnlyList<EngramBlock> QueryByRootId(string rootId, int limit, long? afterTick = null)
        {
            var query = BaseQuery().Where(b => b.Header.RootId == rootId);
            
            if (afterTick.HasValue)
            {
                query = query.Where(b => b.Header.Tick > afterTick.Value);
            }

            return query.Take(limit).ToList();
        }

        public IReadOnlyList<EngramBlock> QueryByOpalRootId(string opalRootId, int limit, long? afterTick = null)
        {
            var query = BaseQuery().Where(b => b.Header.OpalRootId == opalRootId);
            
            if (afterTick.HasValue)
            {
                query = query.Where(b => b.Header.Tick > afterTick.Value);
            }

            return query.Take(limit).ToList();
        }

        public IReadOnlyList<EngramBlock> QueryBySessionId(string sessionId, int limit)
        {
            return BaseQuery()
                .Where(b => b.Header.SessionId == sessionId)
                .Take(limit)
                .ToList();
        }

        public IReadOnlyList<EngramBlock> QueryByChannel(EngramChannel channel, int limit)
        {
            return BaseQuery()
                .Where(b => b.Header.Channel == channel)
                .Take(limit)
                .ToList();
        }

        public IReadOnlyList<EngramBlock> QueryByStance(
            KnowingMode? knowing = null, 
            MetabolicRegime? metabolic = null, 
            ResolutionMode? resolution = null, 
            int limit = 100)
        {
            var query = BaseQuery();

            if (knowing.HasValue)
            {
                string kVal = knowing.Value.ToString();
                query = query.Where(b => HasFactor(b, EngramFactorKind.KnowingMode, kVal));
            }

            if (metabolic.HasValue)
            {
                string mVal = metabolic.Value.ToString();
                query = query.Where(b => HasFactor(b, EngramFactorKind.MetabolicRegime, mVal));
            }

            if (resolution.HasValue)
            {
                string rVal = resolution.Value.ToString();
                query = query.Where(b => HasFactor(b, EngramFactorKind.ResolutionMode, rVal));
            }

            return query.Take(limit).ToList();
        }

        private bool HasFactor(EngramBlock block, EngramFactorKind kind, string value)
        {
            // Case-sensitive exact match on Enums as they are canonicalized
            return block.Factors.Any(f => f.Kind == kind && f.Value == value);
        }
    }
}
