using System.Collections.Generic;
using System.Collections.Immutable;

namespace Oan.Core.Ingestion
{
    public enum IngestionOutcome
    {
        OK,
        NEEDS_SPEC,
        REJECT
    }

    public record RawDescriptor
    {
        public string? Subject { get; init; }
        public string? Predicate { get; init; }
        public string? Scope { get; init; }
        public IReadOnlyDictionary<string, string>? Constraints { get; init; }
    }

    public record StructuredInput
    {
        public string? Subject { get; init; }
        public string? Predicate { get; init; }
        public required string Scope { get; init; }
        public required ImmutableSortedDictionary<string, string> Constraints { get; init; }
    }

    public record IngestionResult(
        IngestionOutcome Outcome,
        StructuredInput? Input,
        string[] MissingFields,
        string? ReasonCode);
}
