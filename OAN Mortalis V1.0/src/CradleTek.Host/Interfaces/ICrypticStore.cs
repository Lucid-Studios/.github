namespace CradleTek.Host.Interfaces;

public interface ICrypticStore : ICradleService
{
    Task<string> StorePointerAsync(string pointer, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListPointersAsync(CancellationToken cancellationToken = default);
}
