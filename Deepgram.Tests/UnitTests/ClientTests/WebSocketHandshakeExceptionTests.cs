// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Net.Sockets;
using System.Net.WebSockets;
using Deepgram.Abstractions.v2;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Exceptions.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class WebSocketHandshakeExceptionTests
{
    private sealed class TestWebSocketClient : AbstractWebSocketClient
    {
        public TestWebSocketClient(string apiKey) : base(apiKey) { }
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
    public void LegacyListenClient_Connect_429_Should_Propagate_Handshake_Exception()
    {
        using var server = new HandshakeFailureServer(HttpStatusCode.TooManyRequests);
        var apiKey = new Faker().Random.Guid().ToString();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, onPrem: true);
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
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, onPrem: true);
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

    private sealed class HandshakeFailureServer : IDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly Task _serverTask;
        private readonly HttpStatusCode? _statusCode;
        private readonly bool _sendMalformedResponse;

        public string Endpoint { get; }

        public HandshakeFailureServer(HttpStatusCode? statusCode = null, bool sendMalformedResponse = false)
        {
            _statusCode = statusCode;
            _sendMalformedResponse = sendMalformedResponse;
            _listener.Start();
            Endpoint = $"ws://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}";
            _serverTask = Task.Run(RunAsync);
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _listener.Stop();
            try
            {
                _serverTask.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                // Cancellation is expected when the test exits before the client connects.
            }
            catch (ObjectDisposedException)
            {
                // TcpListener.Stop() can interrupt an in-flight accept.
            }
            _cancellationTokenSource.Dispose();
        }

        private async Task RunAsync()
        {
            using var client = await _listener.AcceptTcpClientAsync(_cancellationTokenSource.Token);
            using var stream = client.GetStream();
            await ReadHeadersAsync(stream, _cancellationTokenSource.Token);

            if (_statusCode is not HttpStatusCode statusCode)
            {
                if (_sendMalformedResponse)
                {
                    var malformedResponse = Encoding.ASCII.GetBytes("not an HTTP response\r\n\r\n");
                    await stream.WriteAsync(malformedResponse, _cancellationTokenSource.Token);
                }
                return;
            }

            var response = $"HTTP/1.1 {(int)statusCode} {statusCode}\r\nContent-Length: 0\r\nConnection: close\r\n\r\n";
            var responseBytes = Encoding.ASCII.GetBytes(response);
            await stream.WriteAsync(responseBytes, _cancellationTokenSource.Token);
        }

        private static async Task ReadHeadersAsync(NetworkStream stream, CancellationToken cancellationToken)
        {
            var bytes = new List<byte>();
            var buffer = new byte[1];
            while (true)
            {
                var read = await stream.ReadAsync(buffer, cancellationToken);
                if (read == 0)
                {
                    return;
                }

                bytes.Add(buffer[0]);
                var count = bytes.Count;
                if (count >= 4 && bytes[count - 4] == '\r' && bytes[count - 3] == '\n' &&
                    bytes[count - 2] == '\r' && bytes[count - 1] == '\n')
                {
                    return;
                }
            }
        }
    }
}
