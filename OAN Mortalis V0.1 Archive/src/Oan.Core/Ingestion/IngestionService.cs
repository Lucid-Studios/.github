using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Oan.Core.Ingestion
{
    public class IngestionService
    {
        public static IngestionResult Ingest(RawDescriptor raw)
        {
            if (raw == null)
            {
                return new IngestionResult(
                    IngestionOutcome.REJECT,
                    null,
                    Array.Empty<string>(),
                    IngestionErrorCodes.EMPTY_INPUT);
            }

            var missingFields = new List<string>();
            if (string.IsNullOrWhiteSpace(raw.Subject)) missingFields.Add("Subject");
            if (string.IsNullOrWhiteSpace(raw.Scope)) missingFields.Add("Scope");

            if (missingFields.Any())
            {
                return new IngestionResult(
                    IngestionOutcome.NEEDS_SPEC,
                    null,
                    missingFields.OrderBy(s => s, StringComparer.Ordinal).ToArray(),
                    IngestionErrorCodes.NEEDS_SPECIFICATION);
            }

            // Constraints validation
            if (raw.Constraints != null)
            {
                foreach (var kvp in raw.Constraints)
                {
                    if (string.IsNullOrWhiteSpace(kvp.Key))
                    {
                        return new IngestionResult(
                            IngestionOutcome.REJECT,
                            null,
                            Array.Empty<string>(),
                            IngestionErrorCodes.MALFORMED_CONSTRAINTS);
                    }
                    // Validate value characters for v0.1 (basic printable ASCII for now)
                    if (kvp.Value == null || kvp.Value.Any(c => char.IsControl(c)))
                    {
                        return new IngestionResult(
                            IngestionOutcome.REJECT,
                            null,
                            Array.Empty<string>(),
                            IngestionErrorCodes.MALFORMED_CONSTRAINTS);
                    }
                }
            }

            // Normalization
            var sortedConstraints = raw.Constraints?
                .ToImmutableSortedDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal)
                ?? ImmutableSortedDictionary<string, string>.Empty;

            var input = new StructuredInput
            {
                Subject = raw.Subject,
                Predicate = raw.Predicate,
                Scope = raw.Scope!,
                Constraints = sortedConstraints
            };

            return new IngestionResult(
                IngestionOutcome.OK,
                input,
                Array.Empty<string>(),
                null);
        }
    }
}
