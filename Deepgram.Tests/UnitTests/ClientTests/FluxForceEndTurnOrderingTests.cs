// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Flux.WebSocket;
using FluxConstants = Deepgram.Clients.Flux.WebSocket.Constants;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// Deterministic transport-ordering regression test for SendForceEndTurn (PR #426 review B1).
///
/// Send() enqueues binary audio for a background sender, while SendForceEndTurn() writes its
/// control frame via SendMessageImmediately(). Without an explicit flush, the immediate text
/// frame can acquire the socket send mutex before earlier audio has drained, so Flux ends the
/// turn before receiving the final chunks. The fix awaits Flush() first, which guarantees every
/// audio frame queued before the call is written to the socket before {"type":"ForceEndTurn"}.
///
/// This test proves the guarantee on a real WebSocket wire: it blocks the audio send by holding
/// the client's send mutex, queues audio, calls SendForceEndTurn(), verifies nothing overtakes
/// while blocked, then releases the mutex and asserts the server received every audio frame
/// BEFORE the ForceEndTurn text frame. Against the unfixed client this fails: the control frame
/// enters the mutex queue directly and overtakes the still-queued audio chunks.
/// </summary>
public class FluxForceEndTurnOrderingTests
{
    // Exposes the protected send mutex so the test can deterministically block socket writes.
    // No behavior is overridden.
    private sealed class TestFluxClient : Deepgram.Clients.Flux.WebSocket.Client
    {
        public TestFluxClient(string apiKey, DeepgramWsClientOptions options) : base(apiKey, options) { }

        public SemaphoreSlim SendMutex => _mutexSend;
    }

    [Test]
    public async Task SendForceEndTurn_Should_Not_Overtake_Queued_Audio()
    {
        using var server = new RecordingWebSocketServer();
        var apiKey = new Faker().Random.Guid().ToString();
        var options = new DeepgramWsClientOptions(apiKey, $"ws://127.0.0.1:{server.Port}", keepAlive: false, onPrem: true);
        var client = new TestFluxClient(apiKey, options);

        try
        {
            var connected = await client.Connect(new FluxSchema
            {
                Model = "flux-general-en",
                Encoding = "linear16",
                SampleRate = 16000,
            });
            connected.Should().BeTrue("the in-process WebSocket server must accept the connection");

            // Block the audio send: the background sender dequeues a message, then waits on the
            // send mutex before writing it to the socket. Holding the mutex freezes the wire.
            (await client.SendMutex.WaitAsync(TimeSpan.FromSeconds(5))).Should().BeTrue();
            var mutexHeld = true;
            try
            {
                // Queue three audio chunks. The sender dequeues the first and blocks on the held
                // mutex; the remaining two stay in the send queue behind it.
                byte[][] chunks = { new byte[] { 0x01, 0x01 }, new byte[] { 0x02, 0x02 }, new byte[] { 0x03, 0x03 } };
                foreach (var chunk in chunks)
                {
                    client.Send(chunk);
                }

                // Give the background sender a moment to pick up the first chunk and block on
                // the held mutex. The correctness guarantee below does not depend on this
                // settling — the flush marker is enqueued behind ALL queued audio either way —
                // it only makes the blocked-wire assertions exercise the deepest interleaving.
                await Task.Delay(200);

                // Now request the manual end of turn while the audio send is blocked.
                var forceEndTurnTask = client.SendForceEndTurn();

                // While audio is blocked, the control frame must not reach the wire: nothing at
                // all may have been written, and SendForceEndTurn must still be waiting.
                await Task.Delay(250);
                forceEndTurnTask.IsCompleted.Should().BeFalse(
                    "SendForceEndTurn must wait for queued audio to flush before sending the control frame");
                server.DataFrames.Should().BeEmpty("no frame can reach the wire while the send mutex is held");

                // Unblock the wire and let everything drain.
                mutexHeld = false;
                client.SendMutex.Release();

                (await Task.WhenAny(forceEndTurnTask, Task.Delay(TimeSpan.FromSeconds(10)))).Should().BeSameAs(forceEndTurnTask,
                    "SendForceEndTurn must complete once the queue has drained");
                await forceEndTurnTask;

                // The server must have received all three audio frames strictly before the
                // ForceEndTurn text frame.
                await WaitUntilAsync(() => server.DataFrames.Count >= 4, TimeSpan.FromSeconds(10),
                    "the server should receive the three audio frames and the control frame");

                var frames = server.DataFrames.ToArray();
                using (new AssertionScope())
                {
                    frames.Length.Should().Be(4);
                    for (var i = 0; i < chunks.Length; i++)
                    {
                        frames[i].IsText.Should().BeFalse($"frame {i} must be binary audio, not a control message");
                        frames[i].Payload.Should().Equal(chunks[i], $"audio chunk {i} must arrive in send order");
                    }
                    frames[3].IsText.Should().BeTrue("the control frame must be the last frame on the wire");
                    var controlJson = Encoding.UTF8.GetString(frames[3].Payload);
                    using var doc = JsonDocument.Parse(controlJson);
                    doc.RootElement.GetProperty("type").GetString().Should().Be(FluxConstants.ForceEndTurn);
                }
            }
            finally
            {
                if (mutexHeld)
                {
                    client.SendMutex.Release();
                }
            }
        }
        finally
        {
            // Close the server side first so the client teardown does not wait out the
            // CloseStream grace period against a server that never answers.
            server.Dispose();
            try
            {
                await client.Stop();
            }
            catch
            {
                // The connection was torn down by the server close above; nothing to clean up.
            }
        }
    }

    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan timeout, string because)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return;
            }
            await Task.Delay(20);
        }
        condition().Should().BeTrue(because);
    }

    /// <summary>
    /// Minimal in-process WebSocket server: accepts one connection, performs the RFC 6455
    /// upgrade handshake, and records every data frame (opcode + unmasked payload) in arrival
    /// order. Control frames (ping/pong/close) are ignored. No external dependencies.
    /// </summary>
    private sealed class RecordingWebSocketServer : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cts = new();
        private TcpClient? _client;

        public sealed record DataFrame(bool IsText, byte[] Payload);

        public System.Collections.Concurrent.ConcurrentQueue<DataFrame> DataFrames { get; } = new();

        public int Port { get; }

        public RecordingWebSocketServer()
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            _ = Task.Run(RunAsync);
        }

        private async Task RunAsync()
        {
            try
            {
                _client = await _listener.AcceptTcpClientAsync();
                var stream = _client.GetStream();

                // --- Upgrade handshake ---
                var requestBytes = new List<byte>();
                var one = new byte[1];
                while (true)
                {
                    var read = await stream.ReadAsync(one, 0, 1, _cts.Token);
                    if (read == 0)
                    {
                        return;
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
                    .FirstOrDefault(l => l.StartsWith("Sec-WebSocket-Key:", StringComparison.OrdinalIgnoreCase));
                var key = keyLine!.Substring(keyLine.IndexOf(':') + 1).Trim();
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

                // --- Frame read loop (client frames are masked per RFC 6455) ---
                while (!_cts.IsCancellationRequested)
                {
                    var header = await ReadExactAsync(stream, 2);
                    if (header == null)
                    {
                        return;
                    }

                    var opcode = header[0] & 0x0F;
                    var masked = (header[1] & 0x80) != 0;
                    long payloadLength = header[1] & 0x7F;
                    if (payloadLength == 126)
                    {
                        var ext = await ReadExactAsync(stream, 2);
                        if (ext == null) return;
                        payloadLength = (ext[0] << 8) | ext[1];
                    }
                    else if (payloadLength == 127)
                    {
                        var ext = await ReadExactAsync(stream, 8);
                        if (ext == null) return;
                        payloadLength = 0;
                        for (var i = 0; i < 8; i++)
                        {
                            payloadLength = (payloadLength << 8) | ext[i];
                        }
                    }

                    byte[] maskKey = Array.Empty<byte>();
                    if (masked)
                    {
                        var mask = await ReadExactAsync(stream, 4);
                        if (mask == null) return;
                        maskKey = mask;
                    }

                    var payload = await ReadExactAsync(stream, (int)payloadLength);
                    if (payload == null)
                    {
                        return;
                    }
                    if (masked)
                    {
                        for (var i = 0; i < payload.Length; i++)
                        {
                            payload[i] ^= maskKey[i % 4];
                        }
                    }

                    // Record only data frames: 0x1 text, 0x2 binary. Close ends the loop.
                    if (opcode == 0x1 || opcode == 0x2)
                    {
                        DataFrames.Enqueue(new DataFrame(opcode == 0x1, payload));
                    }
                    else if (opcode == 0x8)
                    {
                        return;
                    }
                }
            }
            catch
            {
                // Server torn down (test cleanup) or client aborted; nothing to record.
            }
        }

        private async Task<byte[]?> ReadExactAsync(NetworkStream stream, int count)
        {
            var buffer = new byte[count];
            var offset = 0;
            while (offset < count)
            {
                var read = await stream.ReadAsync(buffer, offset, count - offset, _cts.Token);
                if (read == 0)
                {
                    return null;
                }
                offset += read;
            }
            return buffer;
        }

        private int _disposed;

        public void Dispose()
        {
            // Idempotent: the test disposes explicitly (to close the server before client
            // teardown) and again via `using`.
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
            {
                return;
            }
            _cts.Cancel();
            try { _client?.Close(); } catch { /* already closed */ }
            try { _listener.Stop(); } catch { /* already stopped */ }
            _cts.Dispose();
        }
    }
}
