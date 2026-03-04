using System;
using System.Collections.Generic;
using System.Text.Json;
using Oan.Core.Ingestion;

namespace Oan.Host.Cli
{
    public static class SliIngestCommands
    {
        public static void RunScenario(string scenario)
        {
            IngestionResult result = scenario switch
            {
                "ingest_ok_minimal" => RunOkMinimal(),
                "ingest_needs_spec_missing_subject_or_scope" => RunNeedsSpec(),
                "ingest_reject_malformed_constraints" => RunRejectMalformed(),
                _ => throw new ArgumentException($"Unknown scenario: {scenario}")
            };

            var output = new
            {
                Scenario = scenario,
                Outcome = result.Outcome.ToString(),
                ReasonCode = result.ReasonCode,
                MissingFields = result.MissingFields,
                Input = result.Input == null ? null : new
                {
                    result.Input.Subject,
                    result.Input.Predicate,
                    result.Input.Scope,
                    Constraints = result.Input.Constraints // Already sorted ImmutableSortedDictionary
                }
            };

            Console.WriteLine(JsonSerializer.Serialize(output));
        }

        private static IngestionResult RunOkMinimal()
        {
            var raw = new RawDescriptor
            {
                Subject = "agent-1",
                Predicate = "MoveTo",
                Scope = "public/oan/standard",
                Constraints = new Dictionary<string, string>
                {
                    { "speed", "0.5" },
                    { "priority", "high" }
                }
            };
            return IngestionService.Ingest(raw);
        }

        private static IngestionResult RunNeedsSpec()
        {
            var raw = new RawDescriptor
            {
                Subject = null,
                Predicate = "MoveTo",
                Scope = null
            };
            return IngestionService.Ingest(raw);
        }

        private static IngestionResult RunRejectMalformed()
        {
            var raw = new RawDescriptor
            {
                Subject = "agent-1",
                Predicate = "MoveTo",
                Scope = "public/oan/standard",
                Constraints = new Dictionary<string, string>
                {
                    { "", "emptyKey" }
                }
            };
            return IngestionService.Ingest(raw);
        }
    }
}
