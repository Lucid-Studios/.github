using Xunit;
using Oan.Core;
using Oan.Runtime;
using Oan.SoulFrame;
using Oan.Ledger;
using Oan.Core.Events;
using System.Linq;
using System.Collections.Generic;

namespace Oan.Tests.Identity
{
    public class EngrammitizationSentinelTests
    {
        [Fact]
        public void IdentitySentinel_GoldenMasterVerification()
        {
            // Arrange

                // Arrange (Lawful Scaffolding)
                var ctx = Oan.Tests.Common.TestScaffolding.CreatePrimeSession("test-session", "test-operator", new[] { "agent-1" });
                var session = ctx.Session;
                var processor = ctx.Processor;
                
                // Activate agent-1 manually via Intent for lawfulness? 
                // Scaffolding adds agent to roster. 
                // Processor interaction requires activation.
                processor.ActivateAgent("agent-1", "test setup");

                // Intent 1: Seeding
                var intent1 = new Intent
                {
                    SourceAgentId = "agent-1",
                    AgentProfileId = "agent-1",
                    Action = "Move",
                    SliHandle = "public/oan/move.commit"
                };
                intent1.Parameters["RunId"] = "sentinel-run";

                // Act
                processor.CommitIntent(intent1);
                string? theaterId = session.TheaterSeed?.TheaterId;
                string? tip1 = session.OpalTips.GetTip(theaterId!);

                // Intent 2: Advancement
                var intent2 = new Intent
                {
                    SourceAgentId = "agent-1",
                    AgentProfileId = "agent-1",
                    Action = "Move",
                    SliHandle = "public/oan/move.commit"
                };
                intent2.Parameters["RunId"] = "sentinel-run";
                
                processor.CommitIntent(intent2);
                string? tip2 = session.OpalTips.GetTip(theaterId!);
                
                // Capture for update
                File.WriteAllText("actual_theaterId.txt", theaterId);
                File.WriteAllText("actual_tip1.txt", tip1);
                File.WriteAllText("actual_tip2.txt", tip2);

                // Assert
                Assert.NotNull(theaterId);
                Assert.NotNull(tip1);
                Assert.NotNull(tip2);
                Assert.NotEqual(tip1, tip2);

            // Verify hashes are deterministic (Golden Master v0.1 Hardened)
            
            // TheaterId
            Assert.Equal("9055e9809534f72a2147ed1cb6035dcdc72a5d53770c61b28b4c6a79f3a85c39", theaterId);
            
            // OpalTip1 (First Commit)
            Assert.Equal("e9e83532776765d789e81abe3451d3135d611bd8f7171ba28955d50c70e482f9", tip1);
            
            // OpalTip2 (Second Commit - Chained)
            Assert.Equal("ce7edb34869635f70c066f3d73bac7739d1f0a1947dfaba3fe9dbee7199ce68c", tip2);
        }

        [Fact]
        public void OpalTip_AppendOnlyEnforcement()
        {
            var tips = new Oan.SoulFrame.Identity.OpalTipRegistry();
            string theaterId = "test-theater";
            string tip1 = "hash-1";
            string tip2 = "hash-2";

            // 1. Genesis (Parent must be null)
            Assert.True(tips.TryAdvanceTip(theaterId, null, tip1));
            Assert.Equal(tip1, tips.GetTip(theaterId));

            // 2. Genesis invalid (Parent not null)
            Assert.Throws<System.InvalidOperationException>(() => tips.TryAdvanceTip(theaterId, "bad-parent", tip1));

            // 3. Valid Advance
            Assert.True(tips.TryAdvanceTip(theaterId, tip1, tip2));
            Assert.Equal(tip2, tips.GetTip(theaterId));

            // 4. Invalid Advance (Mismatch parent)
            Assert.Throws<System.InvalidOperationException>(() => tips.TryAdvanceTip(theaterId, "wrong-parent", "hash-3"));

            // 5. Rollback attempt (Advance from old tip)
            Assert.Throws<System.InvalidOperationException>(() => tips.TryAdvanceTip(theaterId, tip1, "hash-fork"));
        }
    }
}
