using Oan.Core.Governance;

namespace Oan.SoulFrame.SLI
{
    public sealed class NullSliTelemetrySink : ISliTelemetrySink
    {
        public void Append(SliTelemetryRecord record) { }
        public void Append(DriverIngestionEvent record) { }
        public void Append(DriverCommitEvent record) { }
        public void Append(DriverSatElevationRequestEvent record) { }
        public void Append(DriverSatElevationOutcomeEvent record) { }
    }
}
