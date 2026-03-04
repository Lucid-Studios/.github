namespace CradleTek.Host.Interfaces;

public interface IRuntimeService : ICradleService
{
    Task ActivateSoulFrameAsync(SoulFrame.Identity.Models.SoulFrame soulFrame, CancellationToken cancellationToken = default);
    Task RunCycleAsync(CancellationToken cancellationToken = default);
}
