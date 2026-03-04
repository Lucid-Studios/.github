using System.Threading;
using System.Threading.Tasks;

namespace Oan.Place.Llm
{
    public sealed class StubLanguageModel : IOanLanguageModel
    {
        public string ModelId => "stub.v0";

        public Task<string> ProposeAsync(string prompt, CancellationToken ct = default)
        {
            if (prompt.Contains("ingest_loop_demo"))
            {
                if (prompt.Contains("Scope"))
                {
                    // Attempt 2: Provides missing field
                    return Task.FromResult("(oan.raw\n  (subject \"agent-1\")\n  (predicate \"MoveTo\")\n  (scope \"public/oan/standard\")\n  (constraints (kv \"speed\" \"1.0\")))");
                }
                else
                {
                    // Attempt 1: Missing scope
                    return Task.FromResult("(oan.raw\n  (subject \"agent-1\")\n  (predicate \"MoveTo\"))");
                }
            }

            if (prompt.Contains("sat_elevation_demo"))
            {
                if (prompt.Contains("HITL_REQUIRED"))
                {
                    // Attempt 3: Public fallback after elevation is hitl-gated
                    return Task.FromResult("(oan.intent\n  (id \"llm-stub-elevate-3\")\n  (sli \"public/oan/move.commit\")\n  (kind \"MoveTo\")\n  (args (x 10.0) (y 20.0)))");
                }
                else if (prompt.Contains("SAT_INSUFFICIENT"))
                {
                    // Attempt 2: Request elevation
                    return Task.FromResult("(oan.intent\n  (id \"llm-stub-elevate-2\")\n  (sli \"sys/admin/sat.elevate.request\")\n  (kind \"RequestSatElevation\")\n  (args (RequestedMode \"Standard\") (Reason \"Need access to GEL\")))");
                }
                else
                {
                    // Attempt 1: Try private action that requires SAT elevation
                    return Task.FromResult("(oan.intent\n  (id \"llm-stub-elevate-1\")\n  (sli \"private/crypticgel/ref.store\")\n  (kind \"StoreRef\")\n  (args (RefId \"demo-123\")))");
                }
            }

            // Fallback to existing v0.1 logic
            double x = 0;
            double y = 0;
            if (prompt.Contains("--x "))
            {
                var parts = prompt.Split("--x ");
                if (parts.Length > 1)
                {
                    var valStr = parts[1].Split(' ')[0];
                    double.TryParse(valStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out x);
                }
            }
            if (prompt.Contains("--y "))
            {
                var parts = prompt.Split("--y ");
                if (parts.Length > 1)
                {
                    var valStr = parts[1].Split(' ')[0];
                    double.TryParse(valStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out y);
                }
            }

            string ir = $"(oan.intent\n  (id \"llm-stub-1\")\n  (sli \"public/oan/move.commit\")\n  (kind \"MoveTo\")\n  (args (x {x.ToString(System.Globalization.CultureInfo.InvariantCulture)}) (y {y.ToString(System.Globalization.CultureInfo.InvariantCulture)})))";
            return Task.FromResult(ir);
        }
    }
}
