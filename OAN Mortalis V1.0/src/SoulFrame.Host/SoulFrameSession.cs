namespace SoulFrame.Host;

public enum SoulFrameSessionState
{
    Created,
    Active,
    Paused,
    Destroyed,
    Faulted
}

public enum SoulFrameVmOperation
{
    SpawnVm,
    PauseVm,
    ResetVm,
    DestroyVm,
    UpgradeModel
}

public sealed class SoulFrameSession
{
    public required Guid SessionId { get; init; }
    public required string CMEId { get; init; }
    public required Guid SoulFrameId { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string HostEndpoint { get; init; }
    public required SoulFrameSessionState State { get; set; }
}

public sealed class SoulFrameInferenceConstraints
{
    public required string Domain { get; init; }
    public required double DriftLimit { get; init; }
    public required int MaxTokens { get; init; }
}

public sealed class SoulFrameInferenceRequest
{
    public required string Task { get; init; }
    public required string Context { get; init; }
    public required SoulFrameInferenceConstraints OpalConstraints { get; init; }
    public required Guid SoulFrameId { get; init; }
    public required Guid ContextId { get; init; }
}

public sealed class SoulFrameInferenceResponse
{
    public required bool Accepted { get; init; }
    public required string Decision { get; init; }
    public required string Payload { get; init; }
    public required double Confidence { get; init; }
}

public interface ISoulFrameSemanticDevice
{
    Task<SoulFrameInferenceResponse> InferAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default);
    Task<SoulFrameInferenceResponse> ClassifyAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default);
    Task<SoulFrameInferenceResponse> SemanticExpandAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default);
    Task<SoulFrameInferenceResponse> EmbeddingAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default);
}

public sealed class NullSoulFrameSemanticDevice : ISoulFrameSemanticDevice
{
    public static NullSoulFrameSemanticDevice Instance { get; } = new();

    public Task<SoulFrameInferenceResponse> InferAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(Fallback("infer", request.Context));

    public Task<SoulFrameInferenceResponse> ClassifyAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(Fallback("classify", request.Context));

    public Task<SoulFrameInferenceResponse> SemanticExpandAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(Fallback("semantic_expand", request.Context));

    public Task<SoulFrameInferenceResponse> EmbeddingAsync(SoulFrameInferenceRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(Fallback("embedding", request.Context));

    private static SoulFrameInferenceResponse Fallback(string task, string context)
    {
        return new SoulFrameInferenceResponse
        {
            Accepted = true,
            Decision = $"{task}-fallback",
            Payload = context,
            Confidence = 0.5
        };
    }
}

