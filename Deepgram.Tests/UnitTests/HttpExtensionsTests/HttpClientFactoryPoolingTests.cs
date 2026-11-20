// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Net.Sockets;
using System.Reflection;
using Deepgram.Encapsulations;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Listen.v1.REST;

namespace Deepgram.Tests.UnitTests.ExtensionsTests;

/// <summary>
/// Regression coverage for #498: every Deepgram client used to build its own
/// IHttpClientFactory, so each one got a private HttpMessageHandler and a private TCP
/// connection pool. A client-per-request caller therefore opened one TCP connection per
/// request, which exhausts SNAT ports at volume.
/// </summary>
public class HttpClientFactoryPoolingTests
{
    /// <summary>
    /// Minimal HTTP/1.1 keep-alive server that counts accepted TCP connections.
    /// </summary>
    private sealed class CountingServer : IDisposable
    {
        private readonly TcpListener _listener;
        private int _connections;

        public CountingServer()
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            _ = Task.Run(AcceptLoop);
        }

        public int Port { get; }
        public int Connections => Volatile.Read(ref _connections);

        private async Task AcceptLoop()
        {
            while (true)
            {
                TcpClient socket;
                try { socket = await _listener.AcceptTcpClientAsync(); }
                catch { return; }

                Interlocked.Increment(ref _connections);
                _ = Task.Run(() => Serve(socket));
            }
        }

        private static async Task Serve(TcpClient socket)
        {
            using (socket)
            using (var stream = socket.GetStream())
            {
                var buffer = new byte[8192];
                var pending = new StringBuilder();

                while (true)
                {
                    int headerEnd = -1;
                    while (headerEnd < 0)
                    {
                        int read;
                        try { read = await stream.ReadAsync(buffer, 0, buffer.Length); }
                        catch { return; }
                        if (read == 0) return;
                        pending.Append(Encoding.ASCII.GetString(buffer, 0, read));
                        headerEnd = pending.ToString().IndexOf("\r\n\r\n", StringComparison.Ordinal);
                    }

                    var text = pending.ToString();
                    var contentLength = 0;
                    foreach (var line in text.Substring(0, headerEnd).Split('\n'))
                    {
                        if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                            contentLength = int.Parse(line.Split(':')[1].Trim(), CultureInfo.InvariantCulture);
                    }

                    var received = text.Length - (headerEnd + 4);
                    while (received < contentLength)
                    {
                        var read = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (read == 0) return;
                        pending.Append(Encoding.ASCII.GetString(buffer, 0, read));
                        received += read;
                    }
                    pending.Clear();

                    const string body = "{\"metadata\":{},\"results\":{}}";
                    var response = "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\n"
                        + $"Content-Length: {body.Length}\r\nConnection: keep-alive\r\n\r\n{body}";
                    try { await stream.WriteAsync(Encoding.ASCII.GetBytes(response), 0, response.Length); }
                    catch { return; }
                }
            }
        }

        public void Dispose() => _listener.Stop();
    }

    private static DeepgramHttpClientOptions OptionsFor(int port) =>
        new DeepgramHttpClientOptions("fake-key", $"http://localhost:{port}/v1");

    [Test]
    public async Task Clients_Sharing_An_HttpId_Should_Reuse_One_Tcp_Connection()
    {
        using var server = new CountingServer();
        var httpId = $"pooling-{Guid.NewGuid():N}";
        var source = new UrlSource("http://example.com/audio.wav");

        // The pattern that caused the regression: a brand new client for every request.
        for (var i = 0; i < 10; i++)
        {
            var client = ClientFactory.CreateListenRESTClient("fake-key", OptionsFor(server.Port), httpId);
            await client.TranscribeUrl(source, new PreRecordedSchema());
        }

        server.Connections.Should().Be(1,
            "clients sharing an HTTP client ID must share the pooled handler, and therefore the connection");
    }

    [Test]
    public async Task Clients_With_Different_HttpIds_Should_Not_Share_A_Connection()
    {
        using var server = new CountingServer();
        var first = $"pooling-a-{Guid.NewGuid():N}";
        var second = $"pooling-b-{Guid.NewGuid():N}";
        var source = new UrlSource("http://example.com/audio.wav");

        for (var i = 0; i < 5; i++)
        {
            var a = ClientFactory.CreateListenRESTClient("fake-key", OptionsFor(server.Port), first);
            var b = ClientFactory.CreateListenRESTClient("fake-key", OptionsFor(server.Port), second);
            await a.TranscribeUrl(source, new PreRecordedSchema());
            await b.TranscribeUrl(source, new PreRecordedSchema());
        }

        server.Connections.Should().Be(2,
            "a distinct HTTP client ID must remain a real isolation boundary");
    }

    [Test]
    public void Create_Should_Return_Clients_Backed_By_The_Same_Handler_For_One_HttpId()
    {
        var httpId = $"handler-{Guid.NewGuid():N}";

        var first = HttpClientFactory.Create(httpId);
        var second = HttpClientFactory.Create(httpId);

        PrimaryHandlerOf(first).Should().BeSameAs(PrimaryHandlerOf(second),
            "the pooled handler owns the connection pool and must be shared");
    }

    private static object PrimaryHandlerOf(HttpClient client)
    {
        var field = typeof(HttpMessageInvoker).GetField("_handler",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var handler = field!.GetValue(client)!;

        while (handler is DelegatingHandler delegating && delegating.InnerHandler is not null)
        {
            handler = delegating.InnerHandler;
        }

        return handler;
    }
}
