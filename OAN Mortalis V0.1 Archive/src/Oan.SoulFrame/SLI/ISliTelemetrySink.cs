using Oan.Core.Governance;

namespace Oan.SoulFrame.SLI
{
    public interface ISliTelemetrySink
    {
        void Append(SliTelemetryRecord record);
        void Append(Oan.Core.Governance.DriverIngestionEvent record);
        void Append(Oan.Core.Governance.DriverCommitEvent record);
        void Append(Oan.Core.Governance.DriverSatElevationRequestEvent record);
        void Append(Oan.Core.Governance.DriverSatElevationOutcomeEvent record);
    }
}
