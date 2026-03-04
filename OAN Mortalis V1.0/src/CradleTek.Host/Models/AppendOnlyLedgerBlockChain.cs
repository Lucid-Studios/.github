namespace CradleTek.Host.Models;

public sealed class AppendOnlyLedgerBlockChain
{
    private readonly List<cSelfGEL> _crypticBlocks = [];
    private readonly List<SelfGEL> _publicBlocks = [];

    public IReadOnlyList<cSelfGEL> cSelfGEL => _crypticBlocks;
    public IReadOnlyList<SelfGEL> SelfGEL => _publicBlocks;

    public void AppendCryptic(cSelfGEL block)
    {
        ArgumentNullException.ThrowIfNull(block);
        _crypticBlocks.Add(block);
    }

    public void AppendPublic(SelfGEL block)
    {
        ArgumentNullException.ThrowIfNull(block);
        _publicBlocks.Add(block);
    }
}
