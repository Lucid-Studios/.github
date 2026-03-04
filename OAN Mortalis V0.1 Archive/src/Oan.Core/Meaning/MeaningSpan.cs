using System;

namespace Oan.Core.Meaning
{
    public enum SyntacticRole
    {
        Unknown,
        Subject,
        Predicate,
        Verb,
        Object,
        Constraint,
        Goal,
        Assumption
    }

    public enum MeaningStatus
    {
        Proposed,
        Confirmed,
        Edited,
        Rejected
    }

    public class MeaningSpan
    {
        public required string SpanId { get; set; }
        public int StartOffset { get; set; }
        public int EndOffset { get; set; }
        public required string Text { get; set; }
        public SyntacticRole Role { get; set; } = SyntacticRole.Unknown;
        public string? ProposedGloss { get; set; }
        public string? UserGloss { get; set; }
        public float AmbiguityScore { get; set; } // 0..1
        public MeaningStatus Status { get; set; } = MeaningStatus.Proposed;
    }
}
