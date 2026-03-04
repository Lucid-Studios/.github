using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oan.Spinal
{
    public readonly record struct EngramId(string Value)
    {
        public override string ToString() => Value;
    }

    public enum EngramType
    {
        RunStarted,
        YieldPointReached,
        ModeTransition,
        IntentProposed,
        EventCommitted,
        Collapse
    }

    public readonly record struct TipRef(string Hash)
    {
        public override string ToString() => Hash;
    }

    public readonly record struct EngramEnvelope(
        TipRef previousTip,
        string scopeId,
        string scopeVersion,
        EngramType eventType,
        string objectiveHash,
        string constraintHash,
        string capabilityHash,
        string plane,
        string flightPhase,
        long tick,
        string payloadHash
    )
    {
        public EngramId ComputeId()
        {
            string data = $"{previousTip}|{scopeId}|{scopeVersion}|{eventType}|{objectiveHash}|{constraintHash}|{capabilityHash}|{plane}|{flightPhase}|{tick}|{payloadHash}";
            return new EngramId(Primitives.ComputeHash(data));
        }
    }

    public interface IEngramStore
    {
        Task AppendAsync(EngramEnvelope envelope);
        Task<IEnumerable<EngramEnvelope>> ReplayAsync();
    }
}
