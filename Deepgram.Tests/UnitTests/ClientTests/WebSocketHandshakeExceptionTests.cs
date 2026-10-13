// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Net.WebSockets;
using Deepgram.Abstractions.v2;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Exceptions.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class WebSocketHandshakeExceptionTests
{
    private sealed class TestWebSocketClient : AbstractWebSocketClient
    {
        public TestWebSocketClient(string apiKey, bool typedErrors = true)
            : base(apiKey, new DeepgramWsClientOptions(apiKey) { ThrowDeepgramWebSocketExceptions = typedErrors }) { }
    }

    [Test]
    public void AbstractClient_Connect_429_Should_Expose_HttpStatus_And_WebSocketException()
    {
        using var server = new HandshakeFailureServer(HttpStatusCode.TooManyRequests);
        using var client = new TestWebSocketClient(new Faker().Random.Guid().ToString());

        var exception = Assert.ThrowsAsync<DeepgramWebSocketException>(
            async () => await client.Connect(server.Endpoint));

        AssertHandshakeException(exception, HttpStatusCode.TooManyRequests);
    }

    [Test]
    public void AbstractClient_Connect_Without_HttpResponse_Should_Expose_Null_Status()
    {
        using var server = new HandshakeFailureServer(sendMalformedResponse: true);
        using var client = new TestWebSocketClient(new Faker().Random.Guid().ToString());

        var exception = Assert.ThrowsAsync<DeepgramWebSocketException>(
            async () => await client.Connect(server.Endpoint));

        AssertHandshakeException(exception, null);
    }

    [Test]
    public void AbstractClient_Connect_429_Without_OptIn_Should_Rethrow_Raw_WebSocketException()
    {
        using var server = new HandshakeFailureServer(HttpStatusCode.TooManyRequests);
        using var client = new TestWebSocketClient(new Faker().Random.Guid().ToString(), typedErrors: false);

        Assert.ThrowsAsync<WebSocketException>(async () => await client.Connect(server.Endpoint));
    }

    [Test]
    public void LegacyListenClient_Connect_429_Should_Propagate_Handshake_Exception()
    {
        using var server = new HandshakeFailureServer(HttpStatusCode.TooManyRequests);
        var apiKey = new Faker().Random.Guid().ToString();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, onPrem: true)
        {
            ThrowDeepgramWebSocketExceptions = true,
        };
#pragma warning disable CS0618 // The legacy client is intentionally covered for compatibility.
        using var client = new Deepgram.Clients.Listen.v1.WebSocket.Client(apiKey, options);

        var exception = Assert.ThrowsAsync<DeepgramWebSocketException>(
            async () => await client.Connect(new Deepgram.Models.Listen.v1.WebSocket.LiveSchema { Model = "nova-3" }));
#pragma warning restore CS0618

        AssertHandshakeException(exception, HttpStatusCode.TooManyRequests);
    }

    [Test]
    public void LegacySpeakClient_Connect_429_Should_Propagate_Handshake_Exception()
    {
        using var server = new HandshakeFailureServer(HttpStatusCode.TooManyRequests);
        var apiKey = new Faker().Random.Guid().ToString();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, onPrem: true)
        {
            ThrowDeepgramWebSocketExceptions = true,
        };
#pragma warning disable CS0618 // The legacy client is intentionally covered for compatibility.
        using var client = new Deepgram.Clients.Speak.v1.WebSocket.Client(apiKey, options);

        var exception = Assert.ThrowsAsync<DeepgramWebSocketException>(
            async () => await client.Connect(new Deepgram.Models.Speak.v1.WebSocket.SpeakSchema()));
#pragma warning restore CS0618

        AssertHandshakeException(exception, HttpStatusCode.TooManyRequests);
    }

    private static void AssertHandshakeException(DeepgramWebSocketException exception, HttpStatusCode? expectedStatus)
    {
        using (new AssertionScope())
        {
            exception.HttpStatusCode.Should().Be(expectedStatus);
            exception.InnerException.Should().BeOfType<WebSocketException>();
        }
    }
}
