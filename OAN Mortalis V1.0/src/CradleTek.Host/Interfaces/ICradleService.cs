namespace CradleTek.Host.Interfaces;

public interface ICradleService
{
    string ContainerName { get; }
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
