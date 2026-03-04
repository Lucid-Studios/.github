using Xunit;
using Oan.SoulFrame.Identity;
using Oan.Core.Events;
using System.Collections.Generic;

namespace Oan.Tests.Identity
{
    public class OpalTipAdvancementTests
    {
        [Fact]
        public void OpalTipRegistry_StartsEmpty()
        {
            var registry = new OpalTipRegistry();
            Assert.Null(registry.GetTip("non-existent"));
        }

        [Fact]
        public void OpalTipRegistry_AdvancesTip_AndMaintainsState()
        {
            // Arrange
            var registry = new OpalTipRegistry();
            string theaterId = "theater-1";
            string tip1 = "tip-1";

            // Act
            bool success = registry.TryAdvanceTip(theaterId, null, tip1);

            // Assert
            Assert.True(success);
            Assert.Equal(tip1, registry.GetTip(theaterId));
        }

        [Fact]
        public void TipConflict_When_ParentMismatch_ShouldThrow()
        {
            // Arrange
            var registry = new OpalTipRegistry();
            string theaterId = "theater-conflict";
            string tip1 = "tip-1";
            string tip2 = "tip-2";

            // Act - First advance (Genesis)
            bool first = registry.TryAdvanceTip(theaterId, null, tip1);
            Assert.True(first);

            // Act & Assert - Wrong Parent
            var ex = Assert.Throws<InvalidOperationException>(() => 
                registry.TryAdvanceTip(theaterId, "wrong-parent", tip2));
            
            Assert.Contains("TIP_CONFLICT", ex.Message);
        }

        [Fact]
        public void BuildEngrammitizedEvent_ProducesConsistentNewTip()
        {
            // Arrange
            string theaterId = "theater-1";
            string? parentTip = "parent-1";
            string nfk = "normal-form-key";
            var witnesses = new List<string> { "evt-1", "evt-2" };
            long tick = 500;

            // Act
            var evt1 = TheaterIdentityService.BuildEngrammitizedEvent(theaterId, parentTip, nfk, witnesses, tick);
            var evt2 = TheaterIdentityService.BuildEngrammitizedEvent(theaterId, parentTip, nfk, witnesses, tick);

            // Assert
            Assert.Equal(evt1.NewTip, evt2.NewTip);
            Assert.NotEqual(evt1.NewTip, parentTip);
            Assert.Equal(2, evt1.WitnessEventIds.Count);
        }
    }
}
