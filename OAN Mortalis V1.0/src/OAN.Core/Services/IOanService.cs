namespace OAN.Core.Services;

public interface IOanService
{
    string ServiceId { get; }
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
