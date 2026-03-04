using Oan.Core.Ingestion;

namespace Oan.Core.Events
{
    public record IngestionProcessedEvent
    {
        public long Tick { get; init; }
        public required string SessionId { get; init; }
        public required string RunId { get; init; }
        public required string RawInputDescriptor { get; init; }
        public required StructuredInput Output { get; init; }
    }
}
