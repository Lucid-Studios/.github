using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Oan.Spinal;

namespace Oan.Storage
{
    public sealed class NdjsonEngramStore : IEngramStore
    {
        private readonly string _filePath;

        public NdjsonEngramStore(string filePath)
        {
            _filePath = filePath;
        }

        public async Task AppendAsync(EngramEnvelope envelope)
        {
            string json = JsonSerializer.Serialize(envelope);
            await File.AppendAllTextAsync(_filePath, json + "\n").ConfigureAwait(false);
        }

        public async Task<IEnumerable<EngramEnvelope>> ReplayAsync()
        {
            var engrams = new List<EngramEnvelope>();
            if (!File.Exists(_filePath)) return engrams;

            using var reader = new StreamReader(_filePath);
            string? line;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var envelope = JsonSerializer.Deserialize<EngramEnvelope>(line);
                engrams.Add(envelope);
            }
            return engrams;
        }
    }
}
