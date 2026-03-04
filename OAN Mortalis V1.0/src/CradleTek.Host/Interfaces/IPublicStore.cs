namespace CradleTek.Host.Interfaces;

public interface IPublicStore : ICradleService
{
    Task PublishPointerAsync(string pointer, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListPublishedPointersAsync(CancellationToken cancellationToken = default);
}
