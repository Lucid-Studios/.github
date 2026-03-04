using System.Collections.Generic;

namespace Oan.Core.Lisp
{
    /// <summary>
    /// Represents the high-level decisions an Evaluate session can return.
    /// </summary>
    public enum Decision
    {
        ALLOW,
        DENY,
        QUARANTINE,
        FREEZE,
        REQUIRE_HITL
    }

    /// <summary>
    /// Represents the core Lisp-structured control form.
    /// </summary>
    public class LispForm
    {
        public string op { get; set; } = string.Empty;
        public Dictionary<string, object> args { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object>? meta { get; set; }
    }

    /// <summary>
    /// A stable snapshot of the environment provided by the Host to the Interpreter.
    /// </summary>
    public class EvalContext
    {
        public long tick { get; set; }
        public string sat_mode { get; set; } = string.Empty;
        public string active_agent { get; set; } = string.Empty;
        public string theater_id { get; set; } = string.Empty;
        public string domain_status { get; set; } = "ACTIVE";

        // Sprint 14 (additive): required for ledger binding
        public IntentForm? intent { get; set; }   // must be non-null at Evaluate() boundary
    }

    /// <summary>
    /// Records the results of a specific transform (IA or CA) in the evaluation chain.
    /// </summary>
    public class TransformReceipt
    {
        public string id { get; set; } = string.Empty;     // e.g., "IA", "CA"
        public string version { get; set; } = "1";
        public string in_hash { get; set; } = string.Empty;
        public string out_hash { get; set; } = string.Empty;
        public string rationale_code { get; set; } = string.Empty;
        public string? notes { get; set; }
    }

    /// <summary>
    /// The structural audit header appended to session telemetry.
    /// </summary>
    public class LispFormHeader
    {
        public string form_hash { get; set; } = string.Empty;
        public string chain_hash { get; set; } = string.Empty;
        public List<string> receipt_hashes { get; set; } = new List<string>();
        public List<string>? cryptic_pointers { get; set; }
    }
}
