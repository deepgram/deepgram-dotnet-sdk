// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Channels;

namespace Deepgram.Tests.Fakes;

/// <summary>
/// Minimal in-process WebSocket server on a loopback port. It accepts any number of connections,
/// performs the RFC 6455 upgrade handshake, and records every data frame (opcode and unmasked
/// payload) each connection sends. Control frames are not answered, so a client close leaves the
/// client socket in CloseSent, the same as a server that is slow to reply. No external
/// dependencies and no real network.
/// </summary>
public sealed class LoopbackWebSocketServer : IDisposable
{
    private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource _cts = new();
    private readonly Func<Connection, Task>? _onConnected;
    private readonly Func<Task>? _beforeUpgrade;
    private readonly ConcurrentDictionary<int, TaskCompletionSource<Connection>> _connections = new();
    private int _acceptedConnections;
    private int _disposed;

    /// <param name="onConnected">
    /// Runs after the 101 response for each connection, while its frames are being read. Use it
    /// to script the server side, for example to close the connection straight away or when a
    /// given client frame arrives.
    /// </param>
    /// <param name="beforeUpgrade">
    /// Runs after each upgrade request is read and before the 101 response is written. Use it to
    /// hold a client inside its handshake.
    /// </param>
    public LoopbackWebSocketServer(Func<Connection, Task>? onConnected = null, Func<Task>? beforeUpgrade = null)
    {
        _onConnected = onConnected;
        _beforeUpgrade = beforeUpgrade;
        _listener.Start();
        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        Endpoint = $"ws://127.0.0.1:{Port}";
        _ = Task.Run(AcceptLoopAsync);
    }

    public sealed record DataFrame(bool IsText, byte[] Payload)
    {
        public string Text => Encoding.UTF8.GetString(Payload);
    }

    public int Port { get; }

    public string Endpoint { get; }

    /// <summary>
    /// Connections that completed the upgrade. Counted before the 101 response is written, so a
    /// client that has finished Connect() always sees its own connection in the count.
    /// </summary>
    public int AcceptedConnections => Volatile.Read(ref _acceptedConnections);

    /// <summary>Every data frame from every connection, in arrival order.</summary>
    public ConcurrentQueue<DataFrame> DataFrames { get; } = new();

    /// <summary>Completes when the connection with this 1-based number has been upgraded.</summary>
    public Task<Connection> WaitForConnectionAsync(int number) => GetConnectionSource(number).Task;

    public void Dispose()
    {
        // Idempotent: tests may dispose explicitly (to drop the server before client teardown)
        // and again via `using`.
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        _cts.Cancel();
        try { _listener.Stop(); } catch { /* already stopped */ }
        foreach (var source in _connections.Values)
        {
            if (source.Task.IsCompleted)
            {
                source.Task.Result.Abort();
            }
            else
            {
                source.TrySetCanceled();
            }
        }
        _cts.Dispose();
    }

    private TaskCompletionSource<Connection> GetConnectionSource(int number) =>
        _connections.GetOrAdd(number, _ => new TaskCompletionSource<Connection>(TaskCreationOptions.RunContinuationsAsynchronously));

    private async Task AcceptLoopAsync()
    {
        try
        {
            while (!_cts.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(_cts.Token);
                _ = Task.Run(() => HandleClientAsync(client));
            }
        }
        catch
        {
            // Server torn down by the test.
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        Connection? connection = null;
        try
        {
            var stream = client.GetStream();
            var request = await ReadHeadersAsync(stream, _cts.Token);
            var keyLine = request.Split(new[] { "\r\n" }, StringSplitOptions.None)
                .FirstOrDefault(l => l.StartsWith("Sec-WebSocket-Key:", StringComparison.OrdinalIgnoreCase));
            if (keyLine is null)
            {
                client.Dispose();
                return;
            }

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

            if (_beforeUpgrade is not null)
            {
                await _beforeUpgrade();
            }

            var number = Interlocked.Increment(ref _acceptedConnections);
            connection = new Connection(number, client, request, this);
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length, _cts.Token);
            GetConnectionSource(number).TrySetResult(connection);

            // Read frames while the script runs, so a script can wait for a client frame.
            var reading = connection.ReadFramesAsync(_cts.Token);
            if (_onConnected is not null)
            {
                await _onConnected(connection);
            }

            await reading;
        }
        catch
        {
            // Server torn down by the test, or the client aborted.
        }
        finally
        {
            connection?.MarkClosed();
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

    /// <summary>One upgraded connection, seen from the server side.</summary>
    public sealed class Connection
    {
        private readonly TcpClient _client;
        private readonly LoopbackWebSocketServer _server;
        private readonly SemaphoreSlim _writeLock = new(1, 1);
        private readonly Channel<DataFrame> _frames = Channel.CreateUnbounded<DataFrame>();
        private readonly TaskCompletionSource<bool> _closed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _closeFrameReceived = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _closeSent;

        internal Connection(int number, TcpClient client, string request, LoopbackWebSocketServer server)
        {
            Number = number;
            Request = request;
            _client = client;
            _server = server;
        }

        /// <summary>1-based, in accept order.</summary>
        public int Number { get; }

        /// <summary>The raw HTTP upgrade request, headers included.</summary>
        public string Request { get; }

        /// <summary>Completes when the client sends a close frame.</summary>
        public Task CloseFrameReceived => _closeFrameReceived.Task;

        /// <summary>Completes when the server stops reading this connection for any reason.</summary>
        public Task Closed => _closed.Task;

        /// <summary>Returns the next data frame this connection sent, waiting for it if needed.</summary>
        public async Task<DataFrame> ReadFrameAsync(CancellationToken cancellationToken = default) =>
            await _frames.Reader.ReadAsync(cancellationToken);

        /// <summary>Sends a server text frame.</summary>
        public Task SendTextAsync(string text) => WriteFrameAsync(0x1, Encoding.UTF8.GetBytes(text));

        /// <summary>Sends a server close frame (normal closure). The TCP connection stays open.</summary>
        public async Task SendCloseAsync()
        {
            Volatile.Write(ref _closeSent, 1);
            await WriteFrameAsync(0x8, new byte[] { 0x03, 0xE8 });
        }

        /// <summary>Drops the TCP connection without a close handshake.</summary>
        public void Abort()
        {
            try { _client.Close(); } catch { /* already closed */ }
        }

        internal void MarkClosed()
        {
            _frames.Writer.TryComplete();
            _closed.TrySetResult(true);
        }

        private async Task WriteFrameAsync(int opcode, byte[] payload)
        {
            // Server frames are unmasked (RFC 6455 5.1).
            var header = new List<byte> { (byte)(0x80 | opcode) };
            if (payload.Length < 126)
            {
                header.Add((byte)payload.Length);
            }
            else if (payload.Length <= ushort.MaxValue)
            {
                header.Add(126);
                header.Add((byte)(payload.Length >> 8));
                header.Add((byte)payload.Length);
            }
            else
            {
                header.Add(127);
                for (var i = 7; i >= 0; i--)
                {
                    header.Add((byte)((long)payload.Length >> (8 * i)));
                }
            }

            var frame = header.Concat(payload).ToArray();
            await _writeLock.WaitAsync();
            try
            {
                await _client.GetStream().WriteAsync(frame, 0, frame.Length);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        internal async Task ReadFramesAsync(CancellationToken cancellationToken)
        {
            var stream = _client.GetStream();
            while (!cancellationToken.IsCancellationRequested)
            {
                var header = await ReadExactAsync(stream, 2, cancellationToken);
                if (header == null)
                {
                    return;
                }

                var opcode = header[0] & 0x0F;
                var masked = (header[1] & 0x80) != 0;
                long payloadLength = header[1] & 0x7F;
                if (payloadLength == 126)
                {
                    var ext = await ReadExactAsync(stream, 2, cancellationToken);
                    if (ext == null) return;
                    payloadLength = (ext[0] << 8) | ext[1];
                }
                else if (payloadLength == 127)
                {
                    var ext = await ReadExactAsync(stream, 8, cancellationToken);
                    if (ext == null) return;
                    payloadLength = 0;
                    for (var i = 0; i < 8; i++)
                    {
                        payloadLength = (payloadLength << 8) | ext[i];
                    }
                }

                var maskKey = Array.Empty<byte>();
                if (masked)
                {
                    var mask = await ReadExactAsync(stream, 4, cancellationToken);
                    if (mask == null) return;
                    maskKey = mask;
                }

                var payload = await ReadExactAsync(stream, (int)payloadLength, cancellationToken);
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

                // Record only data frames: 0x1 text, 0x2 binary. A close frame ends the read loop.
                if (opcode == 0x1 || opcode == 0x2)
                {
                    var frame = new DataFrame(opcode == 0x1, payload);
                    _server.DataFrames.Enqueue(frame);
                    _frames.Writer.TryWrite(frame);
                }
                else if (opcode == 0x8)
                {
                    _closeFrameReceived.TrySetResult(true);

                    // Both sides have sent close: the server drops the TCP connection, as RFC
                    // 6455 7.1.1 asks. Otherwise the client waits about a second for it.
                    if (Volatile.Read(ref _closeSent) == 1)
                    {
                        Abort();
                    }
                    return;
                }
            }
        }

        private static async Task<byte[]?> ReadExactAsync(NetworkStream stream, int count, CancellationToken cancellationToken)
        {
            var buffer = new byte[count];
            var offset = 0;
            while (offset < count)
            {
                var read = await stream.ReadAsync(buffer, offset, count - offset, cancellationToken);
                if (read == 0)
                {
                    return null;
                }
                offset += read;
            }
            return buffer;
        }
    }
}
