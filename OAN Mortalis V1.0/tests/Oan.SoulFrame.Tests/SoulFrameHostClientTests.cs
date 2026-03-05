using System.Net;
using System.Text;
using Telemetry.GEL;
using SoulFrame.Host;

namespace Oan.SoulFrame.Tests;

public sealed class SoulFrameHostClientTests
{
    [Fact]
    public async Task RuntimeConnection_HealthEndpoint_ReturnsTrue()
    {
        var client = CreateClient((request, _) =>
        {
            if (request.RequestUri?.AbsolutePath == "/health")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        });

        var connected = await client.CheckConnectionAsync();

        Assert.True(connected);
    }

    [Fact]
    public async Task VmConnection_SpawnVm_ReturnsTrue()
    {
        var client = CreateClient((request, _) =>
        {
            if (request.RequestUri?.AbsolutePath == "/vm/spawn")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        });

        var started = await client.SpawnVmAsync();

        Assert.True(started);
    }

    [Fact]
    public async Task SemanticInference_Classify_ReturnsDeviceResponse()
    {
        var client = CreateClient((request, _) =>
        {
            if (request.RequestUri?.AbsolutePath != "/classify")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            var json = "{\"decision\":\"label:equation\",\"payload\":\"equation-structure\",\"confidence\":0.82}";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        });

        var response = await client.ClassifyAsync(new SoulFrameInferenceRequest
        {
            Task = "classify",
            Context = "3x + 7 = 25",
            OpalConstraints = new SoulFrameInferenceConstraints
            {
                Domain = "arithmetic",
                DriftLimit = 0.02,
                MaxTokens = 64
            },
            SoulFrameId = Guid.NewGuid(),
            ContextId = Guid.NewGuid()
        });

        Assert.True(response.Accepted);
        Assert.Equal("label:equation", response.Decision);
        Assert.Equal("equation-structure", response.Payload);
        Assert.Equal(0.82, response.Confidence, 3);
    }

    [Fact]
    public async Task ConstraintEnforcement_ExcessTokens_IsRefused()
    {
        var telemetry = new GelTelemetryAdapter();
        var adapter = new SoulFrameTelemetryAdapter(telemetry);
        var client = CreateClient((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)), adapter);

        var response = await client.InferAsync(new SoulFrameInferenceRequest
        {
            Task = "infer",
            Context = string.Join(' ', Enumerable.Repeat("token", 32)),
            OpalConstraints = new SoulFrameInferenceConstraints
            {
                Domain = "arithmetic",
                DriftLimit = 0.02,
                MaxTokens = 8
            },
            SoulFrameId = Guid.NewGuid(),
            ContextId = Guid.NewGuid()
        });

        Assert.False(response.Accepted);
        Assert.Contains(telemetry.Records, record => record.RuntimeState == "soulframe-host:constraintviolation");
    }

    [Fact]
    public async Task TelemetryEmission_InferenceLifecycle_IsEmitted()
    {
        var telemetry = new GelTelemetryAdapter();
        var adapter = new SoulFrameTelemetryAdapter(telemetry);
        var client = CreateClient((request, _) =>
        {
            if (request.RequestUri?.AbsolutePath == "/infer")
            {
                var json = "{\"decision\":\"infer-ok\",\"payload\":\"{}\",\"confidence\":0.7}";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }, adapter);

        var request = new SoulFrameInferenceRequest
        {
            Task = "infer",
            Context = "context sample",
            OpalConstraints = new SoulFrameInferenceConstraints
            {
                Domain = "general",
                DriftLimit = 0.02,
                MaxTokens = 64
            },
            SoulFrameId = Guid.NewGuid(),
            ContextId = Guid.NewGuid()
        };

        var response = await client.InferAsync(request);

        Assert.True(response.Accepted);
        Assert.Contains(telemetry.Records, record => record.RuntimeState == "soulframe-host:inferencerequested");
        Assert.Contains(telemetry.Records, record => record.RuntimeState == "soulframe-host:inferencecompleted");
    }

    private static SoulFrameHostClient CreateClient(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder,
        SoulFrameTelemetryAdapter? telemetry = null)
    {
        var handler = new DelegatingHandlerStub(responder);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://127.0.0.1:8181")
        };
        return new SoulFrameHostClient(httpClient, telemetry, "http://127.0.0.1:8181");
    }

    private sealed class DelegatingHandlerStub : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _responder;

        public DelegatingHandlerStub(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _responder(request, cancellationToken);
        }
    }
}
