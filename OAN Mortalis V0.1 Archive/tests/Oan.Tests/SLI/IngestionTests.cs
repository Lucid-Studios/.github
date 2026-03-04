using System;
using System.Collections.Generic;
using System.Linq;
using Oan.Core.Ingestion;
using Xunit;

namespace Oan.Tests.SLI
{
    public class IngestionTests
    {
        [Fact]
        public void Ingest_Ok_Minimal_Is_Deterministic()
        {
            var raw = new RawDescriptor
            {
                Subject = "a1",
                Predicate = "move",
                Scope = "oan",
                Constraints = new Dictionary<string, string>
                {
                    { "z", "last" },
                    { "a", "first" }
                }
            };

            var res1 = IngestionService.Ingest(raw);
            var res2 = IngestionService.Ingest(raw);

            Assert.Equal(IngestionOutcome.OK, res1.Outcome);
            Assert.Equal(res1.Outcome, res2.Outcome);
            Assert.Equal(res1.Input!.Scope, res2.Input!.Scope);
            
            // Verify constraint ordering
            var keys = res1.Input.Constraints.Keys.ToList();
            Assert.Equal("a", keys[0]);
            Assert.Equal("z", keys[1]);
            
            Assert.Empty(res1.MissingFields);
            Assert.Null(res1.ReasonCode);
        }

        [Fact]
        public void Ingest_Missing_Fields_Returns_NeedsSpec_Ordered()
        {
            var raw = new RawDescriptor
            {
                Subject = "", // Blank
                Scope = null   // Null
            };

            var res = IngestionService.Ingest(raw);

            Assert.Equal(IngestionOutcome.NEEDS_SPEC, res.Outcome);
            Assert.Equal(2, res.MissingFields.Length);
            Assert.Equal("Scope", res.MissingFields[0]);
            Assert.Equal("Subject", res.MissingFields[1]);
            Assert.Equal(IngestionErrorCodes.NEEDS_SPECIFICATION, res.ReasonCode);
        }

        [Fact]
        public void Ingest_Malformed_Constraints_Rejects()
        {
            var raw = new RawDescriptor
            {
                Subject = "a",
                Scope = "s",
                Constraints = new Dictionary<string, string> { { "", "val" } }
            };

            var res = IngestionService.Ingest(raw);
            Assert.Equal(IngestionOutcome.REJECT, res.Outcome);
            Assert.Equal(IngestionErrorCodes.MALFORMED_CONSTRAINTS, res.ReasonCode);
        }

        [Fact]
        public void Ingest_ControlCharacters_In_Constraints_Rejects()
        {
            var raw = new RawDescriptor
            {
                Subject = "a",
                Scope = "s",
                Constraints = new Dictionary<string, string> { { "key", "val\n" } }
            };

            var res = IngestionService.Ingest(raw);
            Assert.Equal(IngestionOutcome.REJECT, res.Outcome);
            Assert.Equal(IngestionErrorCodes.MALFORMED_CONSTRAINTS, res.ReasonCode);
        }
    }
}
