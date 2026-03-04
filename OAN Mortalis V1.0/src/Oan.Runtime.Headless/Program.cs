using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Oan.Common;
using Oan.Cradle;
using Oan.Storage;

namespace Oan.Runtime.Headless
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("OAN Mortalis v1.0 Headless Host Booting...");

            // 1) Ensure storage roots exist
            Directory.CreateDirectory("public_root");
            Directory.CreateDirectory("cryptic_root");

            // 2) Initialize Telemetry Sinks
            // Note: Governance failure triggers a log message for now. 
            // In a real scenario, this would be wired to the host's long-lived authority.
            var govTelemetry = new GovernanceTelemetrySink("governance.ndjson", ex => {
                Console.WriteLine($"[FATAL] Governance Telemetry Failure: {ex.Message}");
            });
            var storageTelemetry = new StorageTelemetrySink("storage.ndjson");

            // 3) Initialize Stores
            var publicStore = new PublicPlaneStore("public_root", storageTelemetry);
            var crypticStore = new CrypticPlaneStore("cryptic_root", storageTelemetry);

            // 4) Build Store Registry
            var stores = new StoreRegistry(
                govTelemetry,
                storageTelemetry,
                publicStore,
                true,
                crypticStore,
                true
            );

            // 5) Initialize Host
            var host = new CradleTekHost(stores);
            await host.InitializeAsync();

            // 6) CLI Routing
            if (args.Length > 0 && args[0].Equals("evaluate", StringComparison.OrdinalIgnoreCase))
            {
                var result = await host.EvaluateAsync("agent-001", "theater-A", new { input = "CLI_TRIGGER" });
                Console.WriteLine("Evaluation Result:");
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                Console.WriteLine("Status: ALIVE");
                Console.WriteLine("Usage: dotnet run -- evaluate");
            }
        }
    }
}
