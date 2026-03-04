using Oan.Core.Meaning;

namespace Oan.Core.Events
{
    public enum DialecticEventType
    {
        SpansProposed,
        SpanConfirmed,
        SpanEdited,
        SpanRejected,
        FrameLockSet,
        ClarifyInvited,
        ClarifyResolved,
        RiskAssessed
    }

    public class DialecticTraceEvent
    {
        public required string EventId { get; set; }
        public required string SessionId { get; set; }
        public long Tick { get; set; }
        public required string PolicyVersion { get; set; }
        public DialecticEventType Kind { get; set; }
        
        // Use a lightweight payload object, or specific fields. 
        // Given the requirement for "small structured payload", we'll keep it generic object 
        // but typically it will be one of the Meaning models or a delta.
        public object? Payload { get; set; }
    }
}
