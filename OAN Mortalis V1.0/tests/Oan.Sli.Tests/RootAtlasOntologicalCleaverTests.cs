using CradleTek.Memory.Services;

namespace Oan.Sli.Tests;

public sealed class RootAtlasOntologicalCleaverTests
{
    [Fact]
    public async Task CleaveAsync_ResolvesKnownPartialAndUnknownTokens()
    {
        var cleaver = new RootAtlasOntologicalCleaver();
        var result = await cleaver.CleaveAsync("Arithmetic defines operations like addition and subtraction hypernumeration.");

        Assert.NotEmpty(result.Resolutions);
        Assert.Contains(result.Known, entry => entry.RootTerm.Equals("arithmetic", StringComparison.OrdinalIgnoreCase));
        Assert.True(result.PartiallyKnown.Count > 0);
        Assert.Contains("hypernumeration", result.Unknown, StringComparer.OrdinalIgnoreCase);
        Assert.InRange(result.Metrics.KnownRatio, 0d, 1d);
        Assert.InRange(result.Metrics.UnknownRatio, 0d, 1d);
    }
}
