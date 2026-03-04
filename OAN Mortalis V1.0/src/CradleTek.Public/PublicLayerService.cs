using CradleTek.Host.Interfaces;

namespace CradleTek.Public;

public sealed class PublicLayerService : IPublicStore
{
    private readonly List<string> _publishedPointers = [];

    public string ContainerName => "PublicLayer";

    public string GelService => "GEL";
    public string GoAService => "GoA";
    public string PrimeSliService => "PrimeSLI";

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task PublishPointerAsync(string pointer, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pointer);
        _publishedPointers.Add(pointer);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> ListPublishedPointersAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<string>>(_publishedPointers.ToList());
    }
}
