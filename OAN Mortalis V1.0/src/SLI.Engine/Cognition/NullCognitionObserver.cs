using CradleTek.CognitionHost.Models;
using SLI.Engine.Models;

namespace SLI.Engine.Cognition;

public sealed class NullCognitionObserver : ICognitionObserver
{
    public static readonly NullCognitionObserver Instance = new();

    private NullCognitionObserver()
    {
    }

    public Task OnCognitionStartAsync(CognitionContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task OnCompassUpdateAsync(CognitiveCompassState compassState, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task OnDecisionCommitAsync(DecisionSpline decisionSpline, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
