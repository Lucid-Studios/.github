using CradleTek.Host.Interfaces;

namespace CradleTek.Runtime;

public sealed class RuntimeLayerService : IRuntimeService
{
    public string ContainerName => "RuntimeLayer";
    public string OanService => "OAN";

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task RunCycleAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
