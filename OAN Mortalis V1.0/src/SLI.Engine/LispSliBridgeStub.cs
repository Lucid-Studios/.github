using OAN.Core.Sli;
using SLI.Engine.Parser;

namespace SLI.Engine;

public sealed class LispSliBridgeStub : ISliBridge
{
    private readonly SliParser _parser = new();

    public Task<string> SendPacketAsync(string sliExpression, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sliExpression);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var expression = _parser.ParseSingle(sliExpression);
            return Task.FromResult($"(:result :status accepted :expr {expression.ToCanonicalString()})");
        }
        catch (FormatException ex)
        {
            return Task.FromResult($"(:result :status rejected :reason \"{ex.Message}\")");
        }
    }
}
