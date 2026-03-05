namespace SLI.Engine.Parser;

public sealed class TokenStream
{
    private readonly IReadOnlyList<string> _tokens;
    private int _index;

    public TokenStream(IReadOnlyList<string> tokens)
    {
        _tokens = tokens;
    }

    public bool EndOfStream => _index >= _tokens.Count;

    public string Peek()
    {
        if (EndOfStream)
        {
            throw new InvalidOperationException("Unexpected end of token stream.");
        }

        return _tokens[_index];
    }

    public string Read()
    {
        var token = Peek();
        _index++;
        return token;
    }
}
