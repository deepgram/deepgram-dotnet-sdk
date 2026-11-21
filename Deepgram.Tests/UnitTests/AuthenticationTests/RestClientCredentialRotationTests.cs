// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;

namespace Deepgram.Tests.UnitTests.AuthenticationTests;

[TestFixture]
public class RestClientCredentialRotationTests
{
    private sealed class CapturingHandler : HttpMessageHandler
    {
        public string? LastAuthorization;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastAuthorization = request.Headers.Authorization?.ToString();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class TestClientV1 : Deepgram.Abstractions.v1.AbstractRestClient
    {
        public TestClientV1(IDeepgramClientOptions options, HttpMessageHandler handler) : base(null, options)
        {
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.deepgram.com/v1") };
        }
    }

    private sealed class TestClientV2 : Deepgram.Abstractions.v2.AbstractRestClient
    {
        public TestClientV2(IDeepgramClientOptions options, HttpMessageHandler handler) : base(null, options)
        {
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.deepgram.com/v1") };
        }
    }

    [Test]
    public async Task V1_Should_Use_Rotated_Credentials_On_Next_Request()
    {
        var options = new DeepgramHttpClientOptions("old_key");
        var handler = new CapturingHandler();
        var client = new TestClientV1(options, handler);

        await client.GetAsync<object>("projects");
        handler.LastAuthorization.Should().Be("token old_key");

        options.SetApiKey("new_key");
        await client.GetAsync<object>("projects");
        handler.LastAuthorization.Should().Be("token new_key");

        options.SetAccessToken("jwt_value");
        await client.GetAsync<object>("projects");
        handler.LastAuthorization.Should().Be("Bearer jwt_value");
    }

    [Test]
    public async Task V2_Should_Use_Rotated_Credentials_On_Next_Request()
    {
        var options = new DeepgramHttpClientOptions("old_key");
        var handler = new CapturingHandler();
        var client = new TestClientV2(options, handler);

        options.SetApiKey("new_key");
        await client.GetAsync<object>("projects");
        handler.LastAuthorization.Should().Be("token new_key");

        options.SetAccessToken("jwt_value");
        await client.GetAsync<object>("projects");
        handler.LastAuthorization.Should().Be("Bearer jwt_value");
    }

    [Test]
    public async Task V1_Should_Handle_Concurrent_Requests_After_Rotation()
    {
        var options = new DeepgramHttpClientOptions("old_key");
        var handler = new CapturingHandler();
        var client = new TestClientV1(options, handler);

        options.SetApiKey("new_key");
        await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => client.GetAsync<object>("projects")));
        handler.LastAuthorization.Should().Be("token new_key");
    }
}
