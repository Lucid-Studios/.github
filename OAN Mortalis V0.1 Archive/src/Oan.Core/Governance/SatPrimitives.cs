namespace Oan.Core.Governance
{
    public enum SatMode { Parked, Idling, PreFlight, Flight, PostFlight }
    public enum SatBond { Inactive, Preparing, Active, Sealing }
    public enum SatEntropyRegime { OAN, OE }
    public enum SatTrend { Rising, Stable, Falling, Increasing, Decreasing }
    public enum SatDriftLevel { Low, Moderate, High }

    /// <summary>
    /// Represents a point-in-time snapshot of the Symbolic Atlas Topology (SAT) state.
    /// Bit-stable representation for governance hashing.
    /// </summary>
    public sealed class SatFrame
    {
        // Required
        public SatMode m { get; set; }
        public string scope { get; set; } = string.Empty; // taxonomy string (non-empty)
        public SatBond b { get; set; }
        public SatEntropyRegime er { get; set; }
        public SatTrend et { get; set; }
        public SatDriftLevel dl { get; set; }
        public long tick { get; set; }

        // Optional (omit when null/empty)
        public string? note { get; set; }
        public string? operator_id { get; set; }
    }
}
