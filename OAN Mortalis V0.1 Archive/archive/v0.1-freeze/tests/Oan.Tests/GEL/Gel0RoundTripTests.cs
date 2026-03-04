using System;
using System.Collections.Generic;
using System.Linq;
using Oan.Core;
using Oan.Core.Engrams; // EngramBlock
using Oan.Place.GEL;
using Oan.Tests.Common;
using Xunit;

namespace Oan.Tests.GEL
{
    public class Gel0RoundTripTests
    {
        [Fact]
        public void Gel0_RoundTrip_Through_Engram_IsInvariant()
        {
            // 1. Construct factor triple (p,r,s)
            string[] prefix = new[] { "un" };
            string root = "accept"; 
            string[] suffix = new[] { "able" };

            // 2. Compute nf0 via Substrate
            var substrate = new Gel0Substrate();
            string nf0 = substrate.Normalize(prefix, root, suffix);

            // 3. Store factors via Intent (simulating payload persistence)
            var ctx1 = TestScaffolding.CreatePrimeSession();
            
            // We must be in HigherFormation/Prime to trigger Engrammitization
            // Scaffolding transitions to Prime.
            // But FormationLevel defaults to Constructor (U0).
            // U0 -> EphemeralTheaterLog (no EngrammitizedEvent).
            // We need U1 (HigherFormation).
            // Scaffolding promotes formation? 
            // Step 4728 TestScaffolding promotes formation IF available.
            // "var promResult = processor.Process(promIntent);"
            // So ctx1 should be U1.
            
            var factorsPayload = new Dictionary<string, string>
            {
                { "p", string.Join(",", prefix) },
                { "r", root },
                { "s", string.Join(",", suffix) }
            };

            var intent = new Intent
            {
                SourceAgentId = "system",
                AgentProfileId = "system",
                Action = "TheaterTransition",
                SliHandle = "sys/admin/theater.transition",
                Parameters = new Dictionary<string, object>
                { 
                    { "TargetMode", "Prime" },
                    { "Reason", "RoundTripTest" },
                    { "Factors", factorsPayload }
                }
            };
            
            var result = ctx1.Processor.Process(intent);
            Assert.Equal(IntentStatus.Committed, result.Status);

            // 4. Verify Persistence (Ledger Event)
            var resultsWithFactors = ctx1.Ledger.GetEvents()
                .Where(e => e.Payload is Oan.Core.Events.EngrammitizedEvent ee && ee.Factors != null)
                .Select(e => (Oan.Core.Events.EngrammitizedEvent)e.Payload)
                .ToList();

            Assert.NotEmpty(resultsWithFactors);
            var engramEvt = resultsWithFactors.Last();
            
            Assert.NotNull(engramEvt.Factors);
            Assert.Equal(factorsPayload["p"], engramEvt.Factors["p"]);
            Assert.Equal(factorsPayload["r"], engramEvt.Factors["r"]);
            Assert.Equal(factorsPayload["s"], engramEvt.Factors["s"]);

            // 5. Verify Content Integriy (Round Trip)
            var p1 = engramEvt.Factors["p"].Split(',', StringSplitOptions.RemoveEmptyEntries);
            var r1 = engramEvt.Factors["r"];
            var s1 = engramEvt.Factors["s"].Split(',', StringSplitOptions.RemoveEmptyEntries);
            
            string nf1 = substrate.Normalize(p1, r1, s1);
            Assert.Equal(nf0, nf1);
        }
    }
}
