using CradleTek.Host.Models;

namespace CradleTek.Host.Interfaces;

public interface IMantleService : ICradleService
{
    Task ShadowSnapshotAsync(OpalEngram engram, CancellationToken cancellationToken = default);
    Task<OpalEngram?> RestoreLastKnownGoodAsync(Guid identityId, CancellationToken cancellationToken = default);
}
