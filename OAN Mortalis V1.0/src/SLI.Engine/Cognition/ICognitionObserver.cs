using CradleTek.CognitionHost.Models;
using SLI.Engine.Models;

namespace SLI.Engine.Cognition;

public interface ICognitionObserver
{
    Task OnCognitionStartAsync(CognitionContext context, CancellationToken cancellationToken = default);
    Task OnCompassUpdateAsync(CognitiveCompassState compassState, CancellationToken cancellationToken = default);
    Task OnDecisionCommitAsync(DecisionSpline decisionSpline, CancellationToken cancellationToken = default);
}
