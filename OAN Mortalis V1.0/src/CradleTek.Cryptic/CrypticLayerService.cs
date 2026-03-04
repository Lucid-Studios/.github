using CradleTek.Host.Interfaces;

namespace CradleTek.Cryptic;

public sealed class CrypticLayerService : ICrypticStore
{
    private readonly List<string> _pointers = [];

    public string ContainerName => "CrypticLayer";

    public string cGELService => "cGEL";
    public string cGoAService => "cGoA";
    public string CrypticSliService => "CrypticSLI";

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<string> StorePointerAsync(string pointer, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pointer);
        _pointers.Add(pointer);
        return Task.FromResult(pointer);
    }

    public Task<IReadOnlyList<string>> ListPointersAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<string>>(_pointers.ToList());
    }
}
