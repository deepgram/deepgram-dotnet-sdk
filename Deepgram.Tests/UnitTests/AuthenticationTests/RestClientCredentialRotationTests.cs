// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Encapsulations;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram.Tests.UnitTests.AuthenticationTests;

[TestFixture]
public class RestClientCredentialRotationTests
{
    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly System.Collections.Concurrent.ConcurrentQueue<string?> _authorizationHeaders = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _authorizationHeaders.Enqueue(request.Headers.Authorization?.ToString());
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            });
        }

        public IReadOnlyCollection<string?> AuthorizationHeaders => _authorizationHeaders.ToArray();
    }

    private sealed class TestClientV1 : Deepgram.Abstractions.v1.AbstractRestClient
    {
        public TestClientV1(IDeepgramClientOptions options, HttpMessageHandler handler) : base(null, options)
        {
            _httpClient = HttpClientFactory.ConfigureDeepgram(new HttpClient(handler), options);
        }
    }

    private sealed class TestClientV2 : Deepgram.Abstractions.v2.AbstractRestClient
    {
        public TestClientV2(IDeepgramClientOptions options, HttpMessageHandler handler) : base(null, options)
        {
            _httpClient = HttpClientFactory.ConfigureDeepgram(new HttpClient(handler), options);
        }
    }

    [Test]
    public async Task V1_Should_Use_Rotated_Credentials_On_Next_Request()
    {
        var options = new DeepgramHttpClientOptions("old_key");
        var handler = new CapturingHandler();
        var client = new TestClientV1(options, handler);

        await client.GetAsync<object>("projects");
        handler.AuthorizationHeaders.Last().Should().Be("token old_key");

        options.SetApiKey("new_key");
        await client.GetAsync<object>("projects");
        handler.AuthorizationHeaders.Last().Should().Be("token new_key");

        options.SetAccessToken("jwt_value");
        await client.GetAsync<object>("projects");
        handler.AuthorizationHeaders.Last().Should().Be("Bearer jwt_value");
    }

    [Test]
    public async Task V2_Should_Use_Rotated_Credentials_On_Next_Request()
    {
        var options = new DeepgramHttpClientOptions("old_key");
        var handler = new CapturingHandler();
        var client = new TestClientV2(options, handler);

        options.SetApiKey("new_key");
        await client.GetAsync<object>("projects");
        handler.AuthorizationHeaders.Last().Should().Be("token new_key");

        options.SetAccessToken("jwt_value");
        await client.GetAsync<object>("projects");
        handler.AuthorizationHeaders.Last().Should().Be("Bearer jwt_value");
    }

    [Test]
    public async Task V1_Should_Handle_Concurrent_Requests_After_Rotation()
    {
        var options = new DeepgramHttpClientOptions("old_key");
        var handler = new CapturingHandler();
        var client = new TestClientV1(options, handler);

        options.SetApiKey("new_key");
        await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => client.GetAsync<object>("projects")));
        handler.AuthorizationHeaders.Should().HaveCount(20);
        handler.AuthorizationHeaders.Should().OnlyContain(header => header == "token new_key");
    }

    [Test]
    public async Task V1_Should_Preserve_Caller_Authorization_Header()
    {
        var options = new DeepgramHttpClientOptions("client_key");
        var handler = new CapturingHandler();
        var client = new TestClientV1(options, handler);

        await client.GetAsync<object>("projects", headers: new Dictionary<string, string>
        {
            ["Authorization"] = "Bearer caller_token",
        });

        handler.AuthorizationHeaders.Should().ContainSingle().Which.Should().Be("Bearer caller_token");
    }

    [Test]
    public async Task V2_Should_Preserve_Caller_Authorization_Header()
    {
        var options = new DeepgramHttpClientOptions("client_key");
        var handler = new CapturingHandler();
        var client = new TestClientV2(options, handler);

        await client.GetAsync<object>("projects", headers: new Dictionary<string, string>
        {
            ["Authorization"] = "Bearer caller_token",
        });

        handler.AuthorizationHeaders.Should().ContainSingle().Which.Should().Be("Bearer caller_token");
    }

    [Test]
    public async Task V1_Should_Preserve_Global_Authorization_Header()
    {
        var options = new DeepgramHttpClientOptions("client_key", headers: new Dictionary<string, string>
        {
            ["Authorization"] = "Bearer global_token",
        });
        var handler = new CapturingHandler();
        var client = new TestClientV1(options, handler);

        await client.GetAsync<object>("projects");

        handler.AuthorizationHeaders.Should().ContainSingle().Which.Should().Be("Bearer global_token");
    }

    [Test]
    public async Task V2_Should_Preserve_Global_Authorization_Header()
    {
        var options = new DeepgramHttpClientOptions("client_key", headers: new Dictionary<string, string>
        {
            ["Authorization"] = "Bearer global_token",
        });
        var handler = new CapturingHandler();
        var client = new TestClientV2(options, handler);

        await client.GetAsync<object>("projects");

        handler.AuthorizationHeaders.Should().ContainSingle().Which.Should().Be("Bearer global_token");
    }

    [Test]
    public async Task V1_Should_Clear_Authorization_After_Clearing_Credentials()
    {
        var options = new DeepgramHttpClientOptions("old_key", onPrem: true);
        var handler = new CapturingHandler();
        var client = new TestClientV1(options, handler);

        options.ClearCredentials();
        await client.GetAsync<object>("projects");

        handler.AuthorizationHeaders.Should().ContainSingle().Which.Should().BeNull();
    }

    [Test]
    public async Task V2_Should_Clear_Authorization_After_Clearing_Credentials()
    {
        var options = new DeepgramHttpClientOptions("old_key", onPrem: true);
        var handler = new CapturingHandler();
        var client = new TestClientV2(options, handler);

        options.ClearCredentials();
        await client.GetAsync<object>("projects");

        handler.AuthorizationHeaders.Should().ContainSingle().Which.Should().BeNull();
    }
}
