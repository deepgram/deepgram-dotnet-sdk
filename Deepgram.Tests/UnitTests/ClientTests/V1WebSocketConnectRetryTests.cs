// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Net.WebSockets;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Exceptions.v1;

namespace Deepgram.Tests.UnitTests.ClientTests;

#pragma warning disable CS0618 // Regression coverage for the shipped v1 clients.

public class V1WebSocketConnectRetryTests
{
    [TestCase(false, false, false)]
    [TestCase(true, false, false)]
    [TestCase(false, false, true)]
    [TestCase(true, false, true)]
    [TestCase(false, true, false)]
    [TestCase(true, true, false)]
    public async Task FailedConnect_CanRetry_WithoutRaisingClose(bool speak, bool cancel, bool wrapException)
    {
        using var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var options = new DeepgramWsClientOptions("test-key", $"ws://127.0.0.1:{port}", keepAlive: false, onPrem: true)
        {
            ThrowDeepgramWebSocketExceptions = wrapException
        };
        using var listenClient = speak ? null : new Deepgram.Clients.Listen.v1.WebSocket.Client(options: options);
        using var speakClient = speak ? new Deepgram.Clients.Speak.v1.WebSocket.Client(options: options) : null;
        var closes = 0;
        listenClient?.Subscribe((object? _, Deepgram.Models.Listen.v1.WebSocket.CloseResponse _) => closes++);
        speakClient?.Subscribe((object? _, Deepgram.Models.Speak.v1.WebSocket.CloseResponse _) => closes++);

        async Task Connect(CancellationTokenSource token)
        {
            if (speak)
                await speakClient!.Connect(new Deepgram.Models.Speak.v1.WebSocket.SpeakSchema(), token);
            else
                await listenClient!.Connect(new Deepgram.Models.Listen.v1.WebSocket.LiveSchema(), token);
        }

        // Cancel a pending upgrade or explicitly reject it with an HTTP response.
        for (var attempt = 0; attempt < 2; attempt++)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var connect = Connect(timeout);
            using var accepted = await listener.AcceptTcpClientAsync().WaitAsync(TimeSpan.FromSeconds(5));
            if (cancel)
            {
                timeout.Cancel();
                await connect.WaitAsync(TimeSpan.FromSeconds(5));
            }
            else
            {
                using var stream = accepted.GetStream();
                using var reader = new StreamReader(stream, leaveOpen: true);
                while (!string.IsNullOrEmpty(await reader.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(5))))
                {
                }
                var response = System.Text.Encoding.ASCII.GetBytes(
                    "HTTP/1.1 401 Unauthorized\r\nContent-Length: 0\r\nConnection: close\r\n\r\n");
                await stream.WriteAsync(response);
                await stream.FlushAsync();
                Func<Task> failed = () => connect;
                if (wrapException)
                {
                    var exception = await failed.Should().ThrowAsync<DeepgramWebSocketException>();
                    exception.Which.HttpStatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
                }
                else
                    await failed.Should().ThrowAsync<WebSocketException>();
            }
            _ = timeout.Token; // Connect must not dispose a caller-owned source.
            var state = speak ? speakClient!.State() : listenClient!.State();
            state.Should().Be(WebSocketState.None);
        }

        if (speak)
            await speakClient!.Stop();
        else
            await listenClient!.Stop();
        closes.Should().Be(0, "no connection was opened");
    }
}
