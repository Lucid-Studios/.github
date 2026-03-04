using System.Collections.Generic;

namespace Oan.Core.Lisp
{
    /// <summary>
    /// Formal partition tiers for OAN Mortalis.
    /// </summary>
    public enum CrypticTier
    {
        GEL,
        GoA,
        SGEL,
        CGEL,
        CGoA,
        cSGEL
    }

    /// <summary>
    /// A pointer to a record in a Cryptic tier.
    /// </summary>
    public class CrypticPointer
    {
        public CrypticTier tier { get; set; }
        public string pointer { get; set; } = string.Empty;
        public string? hint { get; set; } // Optional; absence-over-null
    }

    /// <summary>
    /// A record produced by the Evaluate() boundary that target sensitive tiers.
    /// </summary>
    public class CrypticEmission
    {
        public CrypticTier tier { get; set; }
        public string kind { get; set; } = string.Empty; // e.g., "AccessLog", "MaskPointer"
        public string payload_hash { get; set; } = string.Empty;
        public IReadOnlyList<CrypticPointer>? pointers { get; set; } // Preserve order
        public string? notes { get; set; } // Absence-over-null
        public long? tick { get; set; } // Integer only
    }

    /// <summary>
    /// Telemetry event emitted when a cryptic tier is accessed.
    /// </summary>
    public class AccessLogEvent
    {
        public string operator_id { get; set; } = string.Empty;
        public string action { get; set; } = string.Empty; // e.g., "read", "promote"
        public CrypticTier tier { get; set; }
        public string target { get; set; } = string.Empty;
        public long tick { get; set; }
    }

    /// <summary>
    /// High-priority signal to halt or resume domain activity.
    /// </summary>
    public class FreezeDirective
    {
        public bool is_frozen { get; set; }
        public string reason_code { get; set; } = string.Empty;
        public long tick { get; set; }
    }

    /// <summary>
    /// Interface for emitting cryptic records.
    /// </summary>
    public interface ICrypticEmitter
    {
        void Emit(CrypticEmission emission);
    }

    /// <summary>
    /// Interface for mandatory governance access logging.
    /// </summary>
    public interface IAccessLogger
    {
        void Log(AccessLogEvent e);
    }

    /// <summary>
    /// Contract for evaluating cryptic-aware session policies.
    /// </summary>
    public interface ICrypticPolicy
    {
        bool IsOperationAllowed(bool isFrozen, string operationCode, CrypticTier tier);
    }

    /// <summary>
    /// Baseline no-op implementation for testing.
    /// </summary>
    public class NoopCrypticEmitter : ICrypticEmitter
    {
        public void Emit(CrypticEmission emission) { }
    }

    public class NoopAccessLogger : IAccessLogger
    {
        public void Log(AccessLogEvent e) { }
    }
}
