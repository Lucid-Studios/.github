using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SoulFrame.Host;

public sealed class SoulFrameHostClient : ISoulFrameSemanticDevice
{
    private readonly HttpClient _httpClient;
    private readonly SoulFrameTelemetryAdapter? _telemetry;
    private readonly Uri _baseUri;

    public SoulFrameHostClient(
        HttpClient? httpClient = null,
        SoulFrameTelemetryAdapter? telemetry = null,
        string? hostEndpoint = null)
    {
        _baseUri = ResolveHostEndpoint(hostEndpoint);
        _httpClient = httpClient ?? new HttpClient { BaseAddress = _baseUri };
        _telemetry = telemetry;
    }

    public Uri HostEndpoint => _baseUri;

    public async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync("/health", cancellationToken).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<SoulFrameSession> CreateSessionAsync(
        string cmeId,
        Guid soulFrameId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cmeId);

        var session = new SoulFrameSession
        {
            SessionId = Guid.NewGuid(),
            CMEId = cmeId,
            SoulFrameId = soulFrameId,
            CreatedAt = DateTime.UtcNow,
            HostEndpoint = _baseUri.ToString(),
            State = SoulFrameSessionState.Created
        };

        var spawned = await ControlVmAsync(SoulFrameVmOperation.SpawnVm, cancellationToken).ConfigureAwait(false);
        session.State = spawned ? SoulFrameSessionState.Active : SoulFrameSessionState.Faulted;
        return session;
    }

    public Task<bool> SpawnVmAsync(CancellationToken cancellationToken = default) =>
        ControlVmAsync(SoulFrameVmOperation.SpawnVm, cancellationToken);

    public Task<bool> PauseVmAsync(CancellationToken cancellationToken = default) =>
        ControlVmAsync(SoulFrameVmOperation.PauseVm, cancellationToken);

    public Task<bool> ResetVmAsync(CancellationToken cancellationToken = default) =>
        ControlVmAsync(SoulFrameVmOperation.ResetVm, cancellationToken);

    public Task<bool> DestroyVmAsync(CancellationToken cancellationToken = default) =>
        ControlVmAsync(SoulFrameVmOperation.DestroyVm, cancellationToken);

    public Task<bool> UpgradeModelAsync(CancellationToken cancellationToken = default) =>
        ControlVmAsync(SoulFrameVmOperation.UpgradeModel, cancellationToken);

    public Task<SoulFrameInferenceResponse> InferAsync(
        SoulFrameInferenceRequest request,
        CancellationToken cancellationToken = default)
        => ExecuteTaskAsync("/infer", request, cancellationToken);

    public Task<SoulFrameInferenceResponse> ClassifyAsync(
        SoulFrameInferenceRequest request,
        CancellationToken cancellationToken = default)
        => ExecuteTaskAsync("/classify", request, cancellationToken);

    public Task<SoulFrameInferenceResponse> SemanticExpandAsync(
        SoulFrameInferenceRequest request,
        CancellationToken cancellationToken = default)
        => ExecuteTaskAsync("/semantic_expand", request, cancellationToken);

    public Task<SoulFrameInferenceResponse> EmbeddingAsync(
        SoulFrameInferenceRequest request,
        CancellationToken cancellationToken = default)
        => ExecuteTaskAsync("/embedding", request, cancellationToken);

    private async Task<SoulFrameInferenceResponse> ExecuteTaskAsync(
        string route,
        SoulFrameInferenceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            ValidateConstraints(request);
        }
        catch (InvalidOperationException violation)
        {
            await EmitAsync(
                    SoulFrameTelemetryEventType.ConstraintViolation,
                    request.SoulFrameId,
                    request.ContextId,
                    violation.Message,
                    cancellationToken)
                .ConfigureAwait(false);
            return FallbackResponse(request, accepted: false);
        }

        await EmitAsync(SoulFrameTelemetryEventType.InferenceRequested, request.SoulFrameId, request.ContextId, request.Task, cancellationToken)
            .ConfigureAwait(false);

        var envelope = new SoulFrameApiRequest
        {
            Task = request.Task,
            Context = request.Context,
            OpalConstraints = new SoulFrameApiConstraints
            {
                Domain = request.OpalConstraints.Domain,
                DriftLimit = request.OpalConstraints.DriftLimit,
                MaxTokens = request.OpalConstraints.MaxTokens
            }
        };

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(route, envelope, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                await EmitAsync(
                        SoulFrameTelemetryEventType.InferenceRefused,
                        request.SoulFrameId,
                        request.ContextId,
                        $"status:{(int)response.StatusCode}",
                        cancellationToken)
                    .ConfigureAwait(false);
                return FallbackResponse(request, accepted: false);
            }

            var payload = await response.Content.ReadFromJsonAsync<SoulFrameApiResponse>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            var result = new SoulFrameInferenceResponse
            {
                Accepted = true,
                Decision = payload?.Decision ?? $"{request.Task}-ok",
                Payload = payload?.Payload ?? request.Context,
                Confidence = payload?.Confidence ?? 0.5
            };

            if (request.OpalConstraints.DriftLimit < 0.05 && result.Confidence < 0.45)
            {
                await EmitAsync(
                        SoulFrameTelemetryEventType.DriftDetected,
                        request.SoulFrameId,
                        request.ContextId,
                        $"confidence:{result.Confidence:F3}",
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            await EmitAsync(
                    SoulFrameTelemetryEventType.InferenceCompleted,
                    request.SoulFrameId,
                    request.ContextId,
                    result.Decision,
                    cancellationToken)
                .ConfigureAwait(false);
            return result;
        }
        catch
        {
            await EmitAsync(
                    SoulFrameTelemetryEventType.InferenceRefused,
                    request.SoulFrameId,
                    request.ContextId,
                    "transport-error",
                    cancellationToken)
                .ConfigureAwait(false);
            return FallbackResponse(request, accepted: false);
        }
    }

    private async Task<bool> ControlVmAsync(SoulFrameVmOperation operation, CancellationToken cancellationToken)
    {
        var route = operation switch
        {
            SoulFrameVmOperation.SpawnVm => "/vm/spawn",
            SoulFrameVmOperation.PauseVm => "/vm/pause",
            SoulFrameVmOperation.ResetVm => "/vm/reset",
            SoulFrameVmOperation.DestroyVm => "/vm/destroy",
            SoulFrameVmOperation.UpgradeModel => "/vm/upgrade",
            _ => "/vm/spawn"
        };

        try
        {
            using var response = await _httpClient.PostAsync(route, content: null, cancellationToken).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static void ValidateConstraints(SoulFrameInferenceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.OpalConstraints);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Task);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Context);

        if (request.OpalConstraints.MaxTokens <= 0)
        {
            throw new InvalidOperationException("Opal constraint violation: max_tokens must be > 0.");
        }

        if (request.OpalConstraints.DriftLimit is < 0 or > 1)
        {
            throw new InvalidOperationException("Opal constraint violation: drift_limit must be in [0,1].");
        }

        var tokenCount = request.Context.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
        if (tokenCount > request.OpalConstraints.MaxTokens)
        {
            throw new InvalidOperationException("Opal constraint violation: context exceeds max token budget.");
        }
    }

    private async Task EmitAsync(
        SoulFrameTelemetryEventType eventType,
        Guid soulFrameId,
        Guid contextId,
        string detail,
        CancellationToken cancellationToken)
    {
        if (_telemetry is null)
        {
            return;
        }

        await _telemetry.EmitAsync(eventType, soulFrameId, contextId, detail, cancellationToken).ConfigureAwait(false);
    }

    private static SoulFrameInferenceResponse FallbackResponse(SoulFrameInferenceRequest request, bool accepted)
    {
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(request.Context)))
            .ToLowerInvariant()[..16];
        return new SoulFrameInferenceResponse
        {
            Accepted = accepted,
            Decision = accepted ? $"{request.Task}-fallback-ok" : $"{request.Task}-refused",
            Payload = JsonSerializer.Serialize(new { request.Task, hash }),
            Confidence = accepted ? 0.55 : 0.0
        };
    }

    private static Uri ResolveHostEndpoint(string? hostEndpoint)
    {
        var configured = !string.IsNullOrWhiteSpace(hostEndpoint)
            ? hostEndpoint
            : Environment.GetEnvironmentVariable("OAN_SOULFRAME_HOST_URL");

        return Uri.TryCreate(configured, UriKind.Absolute, out var parsed)
            ? parsed
            : new Uri("http://127.0.0.1:8181");
    }

    private sealed class SoulFrameApiRequest
    {
        [JsonPropertyName("task")]
        public required string Task { get; init; }

        [JsonPropertyName("context")]
        public required string Context { get; init; }

        [JsonPropertyName("opal_constraints")]
        public required SoulFrameApiConstraints OpalConstraints { get; init; }
    }

    private sealed class SoulFrameApiConstraints
    {
        [JsonPropertyName("domain")]
        public required string Domain { get; init; }

        [JsonPropertyName("drift_limit")]
        public required double DriftLimit { get; init; }

        [JsonPropertyName("max_tokens")]
        public required int MaxTokens { get; init; }
    }

    private sealed class SoulFrameApiResponse
    {
        [JsonPropertyName("decision")]
        public string? Decision { get; init; }

        [JsonPropertyName("payload")]
        public string? Payload { get; init; }

        [JsonPropertyName("confidence")]
        public double? Confidence { get; init; }
    }
}
