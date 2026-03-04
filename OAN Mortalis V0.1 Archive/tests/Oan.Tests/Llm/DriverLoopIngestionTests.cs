using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oan.Core;
using Oan.Core.Ingestion;
using Oan.Core.Governance;
using Oan.Place.Llm;
using Oan.Place.Llm.BridgeIr;
using Oan.SoulFrame.SLI;
using Xunit;

namespace Oan.Tests.Llm
{
    public class DriverLoopIngestionTests
    {
        [Fact]
        public async Task DriverLoop_IngestLoopDemo_Completes_In_Two_Attempts()
        {
            // Setup
            var model = new StubLanguageModel();
            var sink = new MemoryTelemetrySink();
            var prompt = "ingest_loop_demo";
            
            // Attempt 1
            var ir1 = await model.ProposeAsync(prompt);
            var parser1 = new BridgeIrParser(ir1);
            var parsed1 = (ParsedRaw)parser1.Parse();
            var raw1 = BridgeIrCompiler.CompileRawDescriptor(parsed1);
            var res1 = IngestionService.Ingest(raw1);
            
            Assert.Equal(IngestionOutcome.NEEDS_SPEC, res1.Outcome);
            Assert.Contains("Scope", res1.MissingFields);

            // Simulate Driver Loop Retry
            prompt = $"ingest_loop_demo Refusal: MissingFields: {string.Join(", ", res1.MissingFields)}";
            
            // Attempt 2
            var ir2 = await model.ProposeAsync(prompt);
            var parser2 = new BridgeIrParser(ir2);
            var parsed2 = (ParsedRaw)parser2.Parse();
            var raw2 = BridgeIrCompiler.CompileRawDescriptor(parsed2);
            var res2 = IngestionService.Ingest(raw2);

            Assert.Equal(IngestionOutcome.OK, res2.Outcome);
            Assert.Equal("public/oan/standard", res2.Input!.Scope);
            
            // Compile to Intent
            var intent = BridgeIrCompiler.CompileFromStructuredInput(res2.Input, model.ModelId);
            Assert.Equal("MoveTo", intent.Action);
            Assert.Equal("public/oan/move.commit", intent.SliHandle);
            Assert.Equal("1.0", intent.Parameters["speed"]);
        }

        private class MemoryTelemetrySink : ISliTelemetrySink
        {
            public List<object> Events { get; } = new List<object>();
            public void Append(SliTelemetryRecord record) => Events.Add(record);
            public void Append(DriverIngestionEvent record) => Events.Add(record);
            public void Append(DriverCommitEvent record) => Events.Add(record);
            public void Append(DriverSatElevationRequestEvent record) => Events.Add(record);
            public void Append(DriverSatElevationOutcomeEvent record) => Events.Add(record);
        }
    }
}
