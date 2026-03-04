namespace Oan.Core.Lisp
{
    public enum IntentKind
    {
        Unknown = 0,
        Query = 1,
        Command = 2,
        TransformRequest = 3,
        Diagnostic = 4
    }

    /// <summary>
    /// Represents a structured envelope for a governance request.
    /// Used alongside Lisp evaluation to provide intent context and support future cleaver receipts.
    /// </summary>
    public sealed class IntentForm
    {
        // Required
        public IntentKind kind { get; set; }
        public string verb { get; set; } = string.Empty;   // non-empty (e.g. "read", "append", "explain")
        public string scope { get; set; } = string.Empty;  // taxonomy string, non-empty
        public long tick { get; set; }                     // integer only

        // Optional (omit when null/empty)
        public string? subject { get; set; }               // e.g. ptr, entity id, topic
        public string? object_ref { get; set; }            // e.g. "cGoA/<hash>"
        public string? note { get; set; }
    }
}
