namespace CradleTek.Host.Interfaces;

public interface IRuntimeService : ICradleService
{
    Task RunCycleAsync(CancellationToken cancellationToken = default);
}
