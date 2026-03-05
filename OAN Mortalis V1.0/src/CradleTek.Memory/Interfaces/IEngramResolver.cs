using CradleTek.CognitionHost.Models;
using CradleTek.Memory.Models;

namespace CradleTek.Memory.Interfaces;

public interface IEngramResolver
{
    Task<EngramQueryResult> ResolveRelevantAsync(CognitionContext context, CancellationToken cancellationToken = default);
    Task<EngramQueryResult> ResolveConceptAsync(string concept, CancellationToken cancellationToken = default);
    Task<EngramQueryResult> ResolveClusterAsync(string clusterId, CancellationToken cancellationToken = default);
}
