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
    // Nothing listens on this address, so ConnectAsync fails with "connection refused".
    private const string RefusedEndpoint = "ws://127.0.0.1:1";

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
        using var client = new TestWebSocketClient(new Faker().Random.Guid().ToString());

        Assert.ThrowsAsync<WebSocketException>(async () => await client.Connect(RefusedEndpoint));
        client.State().Should().Be(WebSocketState.None, "a failed connect must not keep the dead socket");

        Func<Task<bool>> retry = async () => await client.Connect(RefusedEndpoint);

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
    public async Task ListenClient_Connect_After_A_Refused_Connect_Should_Not_Report_Success()
    {
        var apiKey = new Faker().Random.Guid().ToString();
        var options = new DeepgramWsClientOptions(apiKey, RefusedEndpoint, keepAlive: false, onPrem: true);
        using var client = new ListenV2.Client(apiKey, options);
        var schema = new LiveSchema { Model = "nova-3" };

        Assert.ThrowsAsync<WebSocketException>(async () => await client.Connect(schema));

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
        private int _acceptedConnections;

        public string Endpoint { get; }

        public int AcceptedConnections => Volatile.Read(ref _acceptedConnections);

        public HandshakeServer()
        {
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
                    var stream = client.GetStream();

                    var request = await ReadHeadersAsync(stream, _cts.Token);
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
                    Interlocked.Increment(ref _acceptedConnections);
                }
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
