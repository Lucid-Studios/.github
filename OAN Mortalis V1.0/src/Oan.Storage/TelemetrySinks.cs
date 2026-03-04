using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Oan.Common;

namespace Oan.Storage
{
    /// <summary>
    /// Base implementation for append-only NDJSON telemetry sinks.
    /// </summary>
    public abstract class NdjsonTelemetrySink : ITelemetrySink
    {
        private readonly string _filePath;
        private readonly Action<Exception>? _onFailure;

        protected NdjsonTelemetrySink(string filePath, Action<Exception>? onFailure = null)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _onFailure = onFailure;
        }

        public async Task EmitAsync(object telemetryEvent)
        {
            try
            {
                string json = JsonSerializer.Serialize(telemetryEvent);
                await File.AppendAllTextAsync(_filePath, json + "\n").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _onFailure?.Invoke(ex);
                // We rethrow to ensure the failure is detectable by the caller if needed.
                throw; 
            }
        }
    }

    public sealed class GovernanceTelemetrySink : NdjsonTelemetrySink
    {
        public GovernanceTelemetrySink(string filePath, Action<Exception>? onFailure) 
            : base(filePath, onFailure) { }
    }

    public sealed class StorageTelemetrySink : NdjsonTelemetrySink
    {
        public StorageTelemetrySink(string filePath, Action<Exception>? onFailure = null) 
            : base(filePath, onFailure) { }
    }
}
