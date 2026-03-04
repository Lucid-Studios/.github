using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Oan.Common;

namespace Oan.Storage
{
    /// <summary>
    /// File-backed implementation of public plane stores.
    /// </summary>
    public sealed class PublicPlaneStore : IPublicPlaneStores
    {
        private readonly string _basePath;
        private readonly ITelemetrySink _telemetry;

        public PublicPlaneStore(string basePath, ITelemetrySink telemetry)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        }

        public async Task AppendToGoAAsync(string engramHash, object payload)
        {
            await AppendInternalAsync("GoA", engramHash, payload);
        }

        public async Task AppendToGELAsync(string engramHash, object payload)
        {
            await AppendInternalAsync("GEL", engramHash, payload);
        }

        private async Task AppendInternalAsync(string storeName, string engramHash, object payload)
        {
            string filePath = Path.Combine(_basePath, $"{storeName}.ndjson");
            string json = JsonSerializer.Serialize(payload);
            await File.AppendAllTextAsync(filePath, json + "\n").ConfigureAwait(false);

            await _telemetry.EmitAsync(new
            {
                store_name = storeName,
                action = "append",
                pointer = engramHash,
                result = "OK"
            });
        }
    }

    /// <summary>
    /// File-backed implementation of cryptic plane stores.
    /// </summary>
    public sealed class CrypticPlaneStore : ICrypticPlaneStores
    {
        private readonly string _basePath;
        private readonly ITelemetrySink _telemetry;

        public CrypticPlaneStore(string basePath, ITelemetrySink telemetry)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        }

        public async Task AppendToCGoAAsync(string engramHash, object payload)
        {
            await AppendInternalAsync("cGoA", engramHash, payload);
        }

        private async Task AppendInternalAsync(string storeName, string engramHash, object payload)
        {
            string filePath = Path.Combine(_basePath, $"{storeName}.ndjson");
            string json = JsonSerializer.Serialize(payload);
            await File.AppendAllTextAsync(filePath, json + "\n").ConfigureAwait(false);

            await _telemetry.EmitAsync(new
            {
                store_name = storeName,
                action = "append",
                pointer = engramHash,
                result = "OK"
            });
        }
    }
}
