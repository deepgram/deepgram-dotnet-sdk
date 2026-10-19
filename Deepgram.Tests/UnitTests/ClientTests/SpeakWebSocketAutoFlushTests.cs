// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Speak.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class SpeakWebSocketAutoFlushTests
{
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(10);

    [Test]
    public async Task SpeakClient_AutoFlush_Should_Send_Flush_And_Leave_The_Client_Usable()
    {
        using var server = new FlushCountingWebSocketServer();

        var options = new DeepgramWsClientOptions(
            apiKey: "test-api-key",
            baseAddress: $"ws://127.0.0.1:{server.Port}/v1",
            onPrem: true)
        {
            AutoFlushSpeakDelta = 1,
        };
        var client = new SpeakWebSocketClient("test-api-key", options);

        try
        {
            (await client.Connect(new SpeakSchema())).Should().BeTrue();

            client.SpeakWithText("Hello from the autoflush test.");

            // The autoflush thread must put a Flush frame on the wire.
            await server.FirstFlush.WaitAsync(WaitTimeout);

            // A user Flush after autoflush must not block on the autoflush lock.
            await Task.Run(() => client.Flush()).WaitAsync(WaitTimeout);
            await server.SecondFlush.WaitAsync(WaitTimeout);
        }
        finally
        {
            await client.Stop();
        }
    }

    /// <summary>
    /// In-process WebSocket server on a port the OS assigns and keeps bound. It performs the
    /// RFC 6455 upgrade by hand, then counts the Flush text frames it receives.
    /// </summary>
    private sealed class FlushCountingWebSocketServer : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cts = new();
        private readonly TaskCompletionSource<bool> _firstFlush = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _secondFlush = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private TcpClient? _client;

        public FlushCountingWebSocketServer()
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            _ = Task.Run(RunAsync);
        }

        public int Port { get; }

        public Task FirstFlush => _firstFlush.Task;

        public Task SecondFlush => _secondFlush.Task;

        private async Task RunAsync()
        {
            try
            {
                _client = await _listener.AcceptTcpClientAsync();
                var stream = _client.GetStream();
                await AcceptUpgradeAsync(stream);

                using var socket = WebSocket.CreateFromStream(stream, isServer: true, subProtocol: null, keepAliveInterval: Timeout.InfiniteTimeSpan);
                var flushCount = 0;
                while (!_cts.IsCancellationRequested && socket.State == WebSocketState.Open)
                {
                    using var message = new MemoryStream();
                    WebSocketReceiveResult result;
                    do
                    {
                        var buffer = new byte[1024];
                        result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                        message.Write(buffer, 0, result.Count);
                    } while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
                        return;
                    }

                    if (result.MessageType != WebSocketMessageType.Text)
                    {
                        continue;
                    }

                    using var json = JsonDocument.Parse(message.ToArray());
                    if (json.RootElement.TryGetProperty("type", out var type) && type.GetString() == "Flush")
                    {
                        flushCount++;
                        if (flushCount == 1)
                        {
                            _firstFlush.TrySetResult(true);
                        }
                        else if (flushCount == 2)
                        {
                            _secondFlush.TrySetResult(true);
                        }
                    }
                }
            }
            catch
            {
                // Server torn down (test cleanup) or client aborted; the test's gates time out instead.
            }
        }

        private async Task AcceptUpgradeAsync(NetworkStream stream)
        {
            var requestBytes = new List<byte>();
            var one = new byte[1];
            while (true)
            {
                var read = await stream.ReadAsync(one, 0, 1, _cts.Token);
                if (read == 0)
                {
                    throw new IOException("Client closed the connection during the upgrade.");
                }
                requestBytes.Add(one[0]);
                var len = requestBytes.Count;
                if (len >= 4 &&
                    requestBytes[len - 4] == (byte)'\r' && requestBytes[len - 3] == (byte)'\n' &&
                    requestBytes[len - 2] == (byte)'\r' && requestBytes[len - 1] == (byte)'\n')
                {
                    break;
                }
            }

            var request = Encoding.ASCII.GetString(requestBytes.ToArray());
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

        public void Dispose()
        {
            _cts.Cancel();
            _client?.Dispose();
            _listener.Stop();
            _cts.Dispose();
        }
    }
}
