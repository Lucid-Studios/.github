namespace OAN.Core.Sli;

public interface ISliBridge
{
    Task<string> SendPacketAsync(string sliExpression, CancellationToken cancellationToken = default);
}
