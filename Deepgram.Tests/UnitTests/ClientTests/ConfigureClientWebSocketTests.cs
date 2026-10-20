// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Net.Sockets;
using System.Net.WebSockets;
using Deepgram.Abstractions.v2;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class ConfigureClientWebSocketTests
{
    private const string ProbeHeader = "X-Configure-Probe";

    private sealed class TestWebSocketClient : AbstractWebSocketClient
    {
        public TestWebSocketClient(string apiKey, DeepgramWsClientOptions options)
            : base(apiKey, options) { }
    }

    [Test]
    public void AbstractClient_Connect_Should_Apply_ConfigureClientWebSocket()
    {
        using var server = new HeaderCaptureServer();
        var apiKey = new Faker().Random.Guid().ToString();
        var options = new DeepgramWsClientOptions(apiKey)
        {
            ConfigureClientWebSocket = socketOptions => socketOptions.SetRequestHeader(ProbeHeader, "abstract"),
        };
        using var client = new TestWebSocketClient(apiKey, options);

        Assert.ThrowsAsync<WebSocketException>(async () => await client.Connect(server.Endpoint));

        server.WaitForHeaders().Should().ContainKey(ProbeHeader).WhoseValue.Should().Be("abstract");
    }

    [Test]
    public void AbstractClient_ConfigureClientWebSocket_Should_Run_After_Sdk_Headers()
    {
        using var server = new HeaderCaptureServer();
        var apiKey = new Faker().Random.Guid().ToString();
        var options = new DeepgramWsClientOptions(apiKey)
        {
            ConfigureClientWebSocket = socketOptions => socketOptions.SetRequestHeader("Authorization", "token overridden"),
        };
        using var client = new TestWebSocketClient(apiKey, options);

        Assert.ThrowsAsync<WebSocketException>(async () => await client.Connect(server.Endpoint));

        server.WaitForHeaders().Should().ContainKey("Authorization").WhoseValue.Should().Be("token overridden");
    }

    [Test]
    public void AbstractClient_ConfigureClientWebSocket_Exception_Should_Propagate()
    {
        var apiKey = new Faker().Random.Guid().ToString();
        var options = new DeepgramWsClientOptions(apiKey)
        {
            ConfigureClientWebSocket = _ => throw new InvalidOperationException("configure failed"),
        };
        using var client = new TestWebSocketClient(apiKey, options);

        Assert.ThrowsAsync<InvalidOperationException>(async () => await client.Connect("ws://127.0.0.1:1"));
    }

    [Test]
    public void AbstractClient_ConfigureClientWebSocket_Exception_Should_Leave_Client_Reusable()
    {
        using var server = new HeaderCaptureServer();
        var apiKey = new Faker().Random.Guid().ToString();
        var shouldThrow = true;
        var options = new DeepgramWsClientOptions(apiKey)
        {
            ConfigureClientWebSocket = ThrowOnceThenProbe(() => shouldThrow, "retry"),
        };
        using var client = new TestWebSocketClient(apiKey, options);

        Assert.ThrowsAsync<InvalidOperationException>(async () => await client.Connect(server.Endpoint));
        shouldThrow = false;
        Assert.ThrowsAsync<WebSocketException>(async () => await client.Connect(server.Endpoint));

        server.WaitForHeaders().Should().ContainKey(ProbeHeader).WhoseValue.Should().Be("retry");
    }

    [Test]
    public void LegacyListenClient_Connect_Should_Apply_ConfigureClientWebSocket()
    {
        using var server = new HeaderCaptureServer();
        var apiKey = new Faker().Random.Guid().ToString();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, onPrem: true)
        {
            ConfigureClientWebSocket = socketOptions => socketOptions.SetRequestHeader(ProbeHeader, "listen"),
        };
#pragma warning disable CS0618 // The legacy client is intentionally covered for compatibility.
        using var client = new Deepgram.Clients.Listen.v1.WebSocket.Client(apiKey, options);

        Assert.ThrowsAsync<WebSocketException>(
            async () => await client.Connect(new Deepgram.Models.Listen.v1.WebSocket.LiveSchema { Model = "nova-3" }));
#pragma warning restore CS0618

        server.WaitForHeaders().Should().ContainKey(ProbeHeader).WhoseValue.Should().Be("listen");
    }

    [Test]
    public void LegacyListenClient_ConfigureClientWebSocket_Exception_Should_Leave_Client_Reusable()
    {
        using var server = new HeaderCaptureServer();
        var apiKey = new Faker().Random.Guid().ToString();
        var shouldThrow = true;
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, onPrem: true)
        {
            ConfigureClientWebSocket = ThrowOnceThenProbe(() => shouldThrow, "listen-retry"),
        };
#pragma warning disable CS0618 // The legacy client is intentionally covered for compatibility.
        using var client = new Deepgram.Clients.Listen.v1.WebSocket.Client(apiKey, options);
        var schema = new Deepgram.Models.Listen.v1.WebSocket.LiveSchema { Model = "nova-3" };

        Assert.ThrowsAsync<InvalidOperationException>(async () => await client.Connect(schema));
        shouldThrow = false;
        Assert.ThrowsAsync<WebSocketException>(async () => await client.Connect(schema));
#pragma warning restore CS0618

        server.WaitForHeaders().Should().ContainKey(ProbeHeader).WhoseValue.Should().Be("listen-retry");
    }

    [Test]
    public void LegacySpeakClient_Connect_Should_Apply_ConfigureClientWebSocket()
    {
        using var server = new HeaderCaptureServer();
        var apiKey = new Faker().Random.Guid().ToString();
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, onPrem: true)
        {
            ConfigureClientWebSocket = socketOptions => socketOptions.SetRequestHeader(ProbeHeader, "speak"),
        };
#pragma warning disable CS0618 // The legacy client is intentionally covered for compatibility.
        using var client = new Deepgram.Clients.Speak.v1.WebSocket.Client(apiKey, options);

        Assert.ThrowsAsync<WebSocketException>(
            async () => await client.Connect(new Deepgram.Models.Speak.v1.WebSocket.SpeakSchema()));
#pragma warning restore CS0618

        server.WaitForHeaders().Should().ContainKey(ProbeHeader).WhoseValue.Should().Be("speak");
    }

    [Test]
    public void LegacySpeakClient_ConfigureClientWebSocket_Exception_Should_Leave_Client_Reusable()
    {
        using var server = new HeaderCaptureServer();
        var apiKey = new Faker().Random.Guid().ToString();
        var shouldThrow = true;
        var options = new DeepgramWsClientOptions(apiKey, server.Endpoint, onPrem: true)
        {
            ConfigureClientWebSocket = ThrowOnceThenProbe(() => shouldThrow, "speak-retry"),
        };
#pragma warning disable CS0618 // The legacy client is intentionally covered for compatibility.
        using var client = new Deepgram.Clients.Speak.v1.WebSocket.Client(apiKey, options);
        var schema = new Deepgram.Models.Speak.v1.WebSocket.SpeakSchema();

        Assert.ThrowsAsync<InvalidOperationException>(async () => await client.Connect(schema));
        shouldThrow = false;
        Assert.ThrowsAsync<WebSocketException>(async () => await client.Connect(schema));
#pragma warning restore CS0618

        server.WaitForHeaders().Should().ContainKey(ProbeHeader).WhoseValue.Should().Be("speak-retry");
    }

    /// <summary>
    /// Throws while <paramref name="shouldThrow"/> returns true; afterwards sets the probe header to <paramref name="probeValue"/>.
    /// </summary>
    private static Action<ClientWebSocketOptions> ThrowOnceThenProbe(Func<bool> shouldThrow, string probeValue) =>
        socketOptions =>
        {
            if (shouldThrow())
            {
                throw new InvalidOperationException("configure failed");
            }
            socketOptions.SetRequestHeader(ProbeHeader, probeValue);
        };

    /// <summary>
    /// Accepts one connection, records the upgrade request's headers, and rejects the upgrade with 429.
    /// </summary>
    private sealed class HeaderCaptureServer : IDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly TaskCompletionSource<Dictionary<string, string>> _headers =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly Task _serverTask;

        public string Endpoint { get; }

        public HeaderCaptureServer()
        {
            _listener.Start();
            Endpoint = $"ws://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}";
            _serverTask = Task.Run(RunAsync);
        }

        public Dictionary<string, string> WaitForHeaders()
        {
            _headers.Task.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue("the client should send an upgrade request");
            return _headers.Task.Result;
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
            catch (SocketException)
            {
                // TcpListener.Stop() can interrupt an in-flight accept.
            }
            catch (IOException)
            {
                // NetworkStream wraps a connection reset this way.
            }
            _cancellationTokenSource.Dispose();
        }

        private async Task RunAsync()
        {
            using var client = await _listener.AcceptTcpClientAsync(_cancellationTokenSource.Token);
            using var stream = client.GetStream();
            var request = await ReadHeadersAsync(stream, _cancellationTokenSource.Token);

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in request.Split("\r\n").Skip(1))
            {
                var separator = line.IndexOf(':');
                if (separator > 0)
                {
                    headers[line[..separator].Trim()] = line[(separator + 1)..].Trim();
                }
            }
            _headers.TrySetResult(headers);

            var response = Encoding.ASCII.GetBytes("HTTP/1.1 429 TooManyRequests\r\nContent-Length: 0\r\nConnection: close\r\n\r\n");
            await stream.WriteAsync(response, _cancellationTokenSource.Token);
        }

        private static async Task<string> ReadHeadersAsync(NetworkStream stream, CancellationToken cancellationToken)
        {
            var bytes = new List<byte>();
            var buffer = new byte[1];
            while (true)
            {
                var read = await stream.ReadAsync(buffer, cancellationToken);
                if (read == 0)
                {
                    break;
                }

                bytes.Add(buffer[0]);
                var count = bytes.Count;
                if (count >= 4 && bytes[count - 4] == '\r' && bytes[count - 3] == '\n' &&
                    bytes[count - 2] == '\r' && bytes[count - 1] == '\n')
                {
                    break;
                }
            }
            return Encoding.ASCII.GetString(bytes.ToArray());
        }
    }
}
