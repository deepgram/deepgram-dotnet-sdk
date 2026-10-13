// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Net.Sockets;

namespace Deepgram.Tests.Fakes;

/// <summary>
/// Loopback server that fails the WebSocket upgrade for one connection: it answers with an HTTP
/// status code, with a malformed response, or with nothing at all.
/// </summary>
public sealed class HandshakeFailureServer : IDisposable
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
