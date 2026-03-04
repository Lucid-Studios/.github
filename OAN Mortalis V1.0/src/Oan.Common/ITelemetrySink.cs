using System.Threading.Tasks;

namespace Oan.Common
{
    /// <summary>
    /// Append-only NDJSON telemetry sink.
    /// </summary>
    public interface ITelemetrySink
    {
        Task EmitAsync(object telemetryEvent);
    }
}
