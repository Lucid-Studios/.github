using SLI.Engine;

namespace Oan.Sli.Tests;

public sealed class SliEngineBridgeTests
{
    [Fact]
    public async Task Bridge_ForwardsPacket_ToLispEngine()
    {
        const string sbclPath = @"C:\Program Files\Steel Bank Common Lisp\sbcl.exe";
        if (!File.Exists(sbclPath))
        {
            return;
        }

        Environment.SetEnvironmentVariable("OAN_SLI_LISP_EXECUTABLE", sbclPath);

        var bridge = new LispSliBridgeStub();
        var packet = "(packet :env runtime :frame cradle :mode emit :op noop :timestamp 0)";

        var result = await bridge.SendPacketAsync(packet);

        Assert.Contains(":STATUS :ACCEPTED", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(":ROUTE :RUNTIME", result, StringComparison.OrdinalIgnoreCase);
    }
}
