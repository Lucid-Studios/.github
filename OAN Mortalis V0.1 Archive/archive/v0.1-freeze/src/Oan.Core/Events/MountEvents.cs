namespace Oan.Core.Events
{
    /// <summary>
    /// Ledger event for observable, deterministic mount creation.
    /// </summary>
    public record MountCommittedEvent
    {
        public required string MountId { get; init; }
        public required string CanonicalAddress { get; init; }
        public required string PolicyVersion { get; init; }
        public required Oan.Core.Governance.SatMode SatCeiling { get; init; }
        public required bool RequiresHitlForElevation { get; init; }
        public required long CreatedTick { get; init; }
    }
}
