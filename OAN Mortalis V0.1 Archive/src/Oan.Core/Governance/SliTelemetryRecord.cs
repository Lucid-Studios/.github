using System;

namespace Oan.Core.Governance
{
    /// <summary>
    /// Deterministic telemetry record for SLI Gate decisions.
    /// No wall-clock timestamps; all ordering and identification is logical or derived.
    /// </summary>
    public record SliTelemetryRecord
    {
        public required string RunId { get; init; }            // SHA256 of session|tick|scenario|operator
        public required long Tick { get; init; }
        public required string SessionId { get; init; }
        public required string OperatorId { get; init; }
        public required string ActiveSatMode { get; init; }
        public required string[] MountedPartitions { get; init; } // Ordinal sorted
        public required string RequestedHandle { get; init; }
        public required string RequestedKind { get; init; }
        public required string ResolvedAddress { get; init; }     // Visibility/Domain/Partition
        public required bool PartitionMounted { get; init; }
        public required bool SatSatisfied { get; init; }
        public required bool CrypticRequested { get; init; }
        public required bool MaskingApplied { get; init; }
        public required bool Allowed { get; init; }
        public required string ReasonCode { get; init; }
        public required string PolicyVersion { get; init; }
        public required bool MountPresent { get; init; }
        public string? MountId { get; init; }
        public string? Notes { get; init; }
    }
}
