using CradleTek.CognitionHost.Models;

namespace CradleTek.CognitionHost.Interfaces;

public interface ICognitionEngine
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<CognitionResult> ExecuteAsync(CognitionRequest request, CancellationToken cancellationToken = default);
    Task ShutdownAsync(CancellationToken cancellationToken = default);
}
