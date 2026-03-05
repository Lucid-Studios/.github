using SLI.Engine;

namespace Oan.Sli.Tests;

public sealed class SliEngineBridgeTests
{
    [Fact]
    public async Task Bridge_ParsesPacket_InMemory()
    {
        var bridge = new LispSliBridgeStub();
        var packet = "(packet :env runtime :frame cradle :mode emit :op noop :timestamp 0)";

        var result = await bridge.SendPacketAsync(packet);

        Assert.True(result.Contains(":status accepted", StringComparison.OrdinalIgnoreCase));
        Assert.True(result.Contains("(packet :env runtime :frame cradle :mode emit :op noop :timestamp 0)", StringComparison.OrdinalIgnoreCase));
    }
}
