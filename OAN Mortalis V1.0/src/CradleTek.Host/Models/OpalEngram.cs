namespace CradleTek.Host.Models;

public sealed class OpalEngram
{
    public OpalEngram(Guid identityId)
    {
        IdentityId = identityId;
        AppendOnlyLedgerBlockChain = new AppendOnlyLedgerBlockChain();
    }

    public Guid IdentityId { get; }

    // Identity substrate: OpalEngram:[AppendOnlyLedgerBlockChain:cSelfGEL:SelfGEL]
    public AppendOnlyLedgerBlockChain AppendOnlyLedgerBlockChain { get; }
}
