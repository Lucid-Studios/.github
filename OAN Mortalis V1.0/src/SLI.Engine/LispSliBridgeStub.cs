using System.Security.Cryptography;
using System.Text;
using OAN.Core.Sli;

namespace SLI.Engine;

public sealed class LispSliBridgeStub : ISliBridge
{
    // This bridge forwards packet payloads to the symbolic substrate boundary.
    // No SLI/Lisp interpretation is performed in C# runtime modules.
    public Task<string> SendPacketAsync(string sliExpression, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sliExpression);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(sliExpression));
        var packetHash = Convert.ToHexString(bytes);

        return Task.FromResult($"(bridge-ack :status accepted :hash {packetHash})");
    }
}
