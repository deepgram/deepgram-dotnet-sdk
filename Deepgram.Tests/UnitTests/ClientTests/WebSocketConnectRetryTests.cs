// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using Deepgram.Abstractions.v2;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Listen.v2.WebSocket;
using ListenV2 = Deepgram.Clients.Listen.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// A failed Connect() must leave the client ready for another attempt. Before the fix, the socket
/// from the failed attempt stayed assigned, so the next Connect() hit the "already initialized"
/// guard and returned true without opening a connection.
/// </summary>
public class WebSocketConnectRetryTests
{
    /// <summary>Holds an OS-assigned port bound but not listening, so TCP connects are refused.</summary>
    private sealed class RefusedEndpoint : IDisposable
    {
        private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public RefusedEndpoint()
        {
            _socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            Endpoint = $"ws://127.0.0.1:{((IPEndPoint)_socket.LocalEndPoint!).Port}";
        }

        public string Endpoint { get; }

        public void Dispose() => _socket.Dispose();
    }

    private sealed class TestWebSocketClient : AbstractWebSocketClient
    {
        public TestWebSocketClient(string apiKey)
            : base(apiKey, new DeepgramWsClientOptions(apiKey) { KeepAlive = false }) { }

        public override Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null) =>
            Task.CompletedTask;
    }

    [Test]
    public async Task AbstractClient_Connect_After_A_Refused_Connect_Should_Fail_Again()
    {
        using var refusedEndpoint = new RefusedEndpoint();
        using var client = new TestWebSocketClient(new Faker().Random.Guid().ToString());

        Func<Task<bool>> connect = async () => await client.Connect(refusedEndpoint.Endpoint);

        await connect.Should().ThrowAsync<WebSocketException>();
        client.State().Should().Be(WebSocketState.None, "a failed connect must not keep the dead socket");

        Func<Task<bool>> retry = async () => await client.Connect(refusedEndpoint.Endpoint);

        await retry.Should().ThrowAsync<WebSocketException>(
            "a retry must attempt a new connection instead of reporting the failed one as connected");
    }

    [Test]
    public async Task AbstractClient_Connect_After_A_Cancelled_Connect_Should_Open_A_New_Connection()
    {
        using var server = new HandshakeServer();
        using var client = new TestWebSocketClient(new Faker().Random.Guid().ToString());

        using (var cancelled = new CancellationTokenSource())
        {
            cancelled.Cancel();
            (await client.Connect(server.Endpoint, cancelled)).Should().BeFalse();
        }

        var connected = await client.Connect(server.Endpoint);

        using (new AssertionScope())
        {
            connected.Should().BeTrue();
            client.IsConnected().Should().BeTrue("the retry must open a new connection");
            server.AcceptedConnections.Should().Be(1);
        }

        await client.Stop();
    }

    [Test]
    public async Task AbstractClient_Connect_After_An_Invalid_Header_Should_Open_A_New_Connection()
    {
        using var server = new HandshakeServer();
        using var client = new TestWebSocketClient(new Faker().Random.Guid().ToString());
        var invalidHeaders = new Dictionary<string, string> { ["invalid\nheader"] = "value" };

        Func<Task<bool>> invalidConnect = async () => await client.Connect(server.Endpoint, headers: invalidHeaders);
        await invalidConnect.Should().ThrowAsync<ArgumentException>();
        client.State().Should().Be(WebSocketState.None, "an invalid header must not publish a socket");

        (await client.Connect(server.Endpoint)).Should().BeTrue();
        server.AcceptedConnections.Should().Be(1);
        await client.Stop();
    }

    [Test]
    public async Task AbstractClient_A_Failed_Connect_Should_Not_Clear_A_Newer_Retry()
    {
        using var server = new HandshakeServer(delayFirstResponse: true);
        using var client = new TestWebSocketClient(new Faker().Random.Guid().ToString());
        using var cancelled = new CancellationTokenSource();

        var firstConnect = client.Connect(server.Endpoint, cancelled);
        await server.FirstHandshakeReceived.Task.WaitAsync(TimeSpan.FromSeconds(10));

        (await client.Stop()).Should().BeTrue();
        (await client.Connect(server.Endpoint)).Should().BeTrue();

        cancelled.Cancel();
        (await firstConnect).Should().BeFalse();
        client.IsConnected().Should().BeTrue("cleanup for the failed first attempt must leave the retry connected");

        server.ReleaseFirstResponse.TrySetResult(true);
        await client.Stop();
    }

    [Test]
    public async Task ListenClient_Connect_After_A_Refused_Connect_Should_Not_Report_Success()
    {
        var apiKey = new Faker().Random.Guid().ToString();
        using var refusedEndpoint = new RefusedEndpoint();
        var options = new DeepgramWsClientOptions(apiKey, refusedEndpoint.Endpoint, keepAlive: false, onPrem: true);
        using var client = new ListenV2.Client(apiKey, options);
        var schema = new LiveSchema { Model = "nova-3" };

        Func<Task<bool>> connect = async () => await client.Connect(schema);

        await connect.Should().ThrowAsync<WebSocketException>();

        Func<Task<bool>> retry = async () => await client.Connect(schema);

        await retry.Should().ThrowAsync<WebSocketException>(
            "a retry must attempt a new connection instead of reporting the failed one as connected");
        client.IsConnected().Should().BeFalse();
    }

    /// <summary>
    /// Accepts WebSocket upgrades on a loopback port and holds each connection open until disposed.
    /// </summary>
    private sealed class HandshakeServer : IDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource _cts = new();
        private readonly System.Collections.Concurrent.ConcurrentBag<TcpClient> _clients = new();
        private readonly bool _delayFirstResponse;
        private int _acceptedConnections;

        public string Endpoint { get; }

        public int AcceptedConnections => Volatile.Read(ref _acceptedConnections);

        public TaskCompletionSource<bool> FirstHandshakeReceived { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> ReleaseFirstResponse { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public HandshakeServer(bool delayFirstResponse = false)
        {
            _delayFirstResponse = delayFirstResponse;
            _listener.Start();
            Endpoint = $"ws://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}";
            _ = Task.Run(RunAsync);
        }

        public void Dispose()
        {
            _cts.Cancel();
            _listener.Stop();
            foreach (var client in _clients)
            {
                client.Dispose();
            }
            _cts.Dispose();
        }

        private async Task RunAsync()
        {
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync(_cts.Token);
                    _clients.Add(client);
                    var connectionNumber = Interlocked.Increment(ref _acceptedConnections);
                    _ = Task.Run(() => HandleClientAsync(client, connectionNumber));
                }
            }
            catch
            {
                // Server torn down by the test.
            }
        }

        private async Task HandleClientAsync(TcpClient client, int connectionNumber)
        {
            try
            {
                var stream = client.GetStream();
                var request = await ReadHeadersAsync(stream, _cts.Token);
                if (_delayFirstResponse && connectionNumber == 1)
                {
                    FirstHandshakeReceived.TrySetResult(true);
                    await ReleaseFirstResponse.Task.WaitAsync(_cts.Token);
                }
                var keyLine = request.Split(new[] { "\r\n" }, StringSplitOptions.None)
                    .First(l => l.StartsWith("Sec-WebSocket-Key:", StringComparison.OrdinalIgnoreCase));
                var key = keyLine.Substring(keyLine.IndexOf(':') + 1).Trim();
                string accept;
                using (var sha1 = SHA1.Create())
                {
                    accept = Convert.ToBase64String(
                        sha1.ComputeHash(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
                }

                var response =
                    "HTTP/1.1 101 Switching Protocols\r\n" +
                    "Upgrade: websocket\r\n" +
                    "Connection: Upgrade\r\n" +
                    $"Sec-WebSocket-Accept: {accept}\r\n\r\n";
                var responseBytes = Encoding.ASCII.GetBytes(response);
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length, _cts.Token);
            }
            catch
            {
                // Server torn down by the test.
            }
        }

        private static async Task<string> ReadHeadersAsync(NetworkStream stream, CancellationToken cancellationToken)
        {
            var bytes = new List<byte>();
            var buffer = new byte[1];
            while (true)
            {
                var read = await stream.ReadAsync(buffer, 0, 1, cancellationToken);
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
