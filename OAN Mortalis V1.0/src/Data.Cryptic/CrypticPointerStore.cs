namespace Data.Cryptic;

public sealed record CrypticPointer(string PointerId, string Location);

public interface ICrypticPointerStore
{
    Task<CrypticPointer?> GetPointerAsync(string pointerId, CancellationToken cancellationToken = default);
}

public sealed class InMemoryCrypticPointerStore : ICrypticPointerStore
{
    private readonly Dictionary<string, CrypticPointer> _pointers = new(StringComparer.Ordinal);

    public InMemoryCrypticPointerStore(IEnumerable<CrypticPointer>? seedPointers = null)
    {
        if (seedPointers is null)
        {
            return;
        }

        foreach (var pointer in seedPointers)
        {
            _pointers[pointer.PointerId] = pointer;
        }
    }

    public Task<CrypticPointer?> GetPointerAsync(string pointerId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pointerId);
        _pointers.TryGetValue(pointerId, out var pointer);
        return Task.FromResult(pointer);
    }
}
