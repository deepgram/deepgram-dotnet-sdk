// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Sockets;
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
    /// Minimal HTTP/1.1 keep-alive server that counts accepted TCP connections and records
    /// the Authorization header of every request it serves.
    /// </summary>
    private sealed class CountingServer : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly ConcurrentQueue<string> _authHeaders = new ConcurrentQueue<string>();
        private readonly CancellationTokenSource _shutdown = new CancellationTokenSource();
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

        public IReadOnlyCollection<string> AuthorizationHeaders => _authHeaders.ToArray();

        private async Task AcceptLoop()
        {
            while (!_shutdown.IsCancellationRequested)
            {
                TcpClient socket;
                try { socket = await _listener.AcceptTcpClientAsync(); }
                catch { return; }

                Interlocked.Increment(ref _connections);
                _ = Task.Run(() => Serve(socket));
            }
        }

        private async Task Serve(TcpClient socket)
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
                        if (line.StartsWith("Authorization:", StringComparison.OrdinalIgnoreCase))
                            _authHeaders.Enqueue(line.Substring("Authorization:".Length).Trim());
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

        public void Dispose()
        {
            _shutdown.Cancel();
            _listener.Stop();
            _shutdown.Dispose();
        }
    }

    private static DeepgramHttpClientOptions OptionsFor(int port, string apiKey = "fake-key") =>
        new DeepgramHttpClientOptions(apiKey, $"http://127.0.0.1:{port}/v1");

    private static UrlSource Source => new UrlSource("http://example.com/audio.wav");

    [Test]
    public async Task Clients_Sharing_An_HttpId_Should_Reuse_One_Tcp_Connection()
    {
        using var server = new CountingServer();
        var httpId = $"pooling-{Guid.NewGuid():N}";

        // The pattern that caused the regression: a brand new client for every request.
        for (var i = 0; i < 10; i++)
        {
            var client = ClientFactory.CreateListenRESTClient("fake-key", OptionsFor(server.Port), httpId);
            await client.TranscribeUrl(Source, new PreRecordedSchema());
        }

        server.Connections.Should().Be(1,
            "clients sharing an HTTP client ID must share the pooled handler, and therefore the connection");
    }

    [Test]
    public async Task Clients_Created_Without_An_HttpId_Should_Reuse_One_Tcp_Connection()
    {
        using var server = new CountingServer();

        // No httpId at all - the default name, and the path virtually every caller takes.
        for (var i = 0; i < 10; i++)
        {
            var client = ClientFactory.CreateListenRESTClient("fake-key", OptionsFor(server.Port));
            await client.TranscribeUrl(Source, new PreRecordedSchema());
        }

        server.Connections.Should().Be(1,
            "callers who never pass an httpId must still get connection reuse");
    }

    [Test]
    public async Task Clients_With_Different_HttpIds_Should_Not_Share_A_Connection()
    {
        using var server = new CountingServer();
        var first = $"pooling-a-{Guid.NewGuid():N}";
        var second = $"pooling-b-{Guid.NewGuid():N}";

        for (var i = 0; i < 5; i++)
        {
            var a = ClientFactory.CreateListenRESTClient("fake-key", OptionsFor(server.Port), first);
            var b = ClientFactory.CreateListenRESTClient("fake-key", OptionsFor(server.Port), second);
            await a.TranscribeUrl(Source, new PreRecordedSchema());
            await b.TranscribeUrl(Source, new PreRecordedSchema());
        }

        server.Connections.Should().Be(2,
            "a distinct HTTP client ID must remain a real isolation boundary");
    }

    [Test]
    public async Task Concurrently_Created_Clients_Should_Share_One_Connection_Pool()
    {
        using var server = new CountingServer();
        var httpId = $"pooling-concurrent-{Guid.NewGuid():N}";

        // Build the clients on many threads at once, so the provider cache is raced.
        var clients = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ =>
            Task.Run(() => ClientFactory.CreateListenRESTClient("fake-key", OptionsFor(server.Port), httpId))));

        // Then drive them one at a time, so a second connection can only come from a second pool.
        foreach (var client in clients)
        {
            await client.TranscribeUrl(Source, new PreRecordedSchema());
        }

        server.Connections.Should().Be(1,
            "racing the provider cache must still yield a single shared handler");
    }

    [Test]
    public async Task Clients_Sharing_A_Pool_Should_Not_Share_Credentials()
    {
        using var server = new CountingServer();
        var httpId = $"pooling-creds-{Guid.NewGuid():N}";

        var first = ClientFactory.CreateListenRESTClient("key-alpha", OptionsFor(server.Port, "key-alpha"), httpId);
        var second = ClientFactory.CreateListenRESTClient("key-beta", OptionsFor(server.Port, "key-beta"), httpId);

        await first.TranscribeUrl(Source, new PreRecordedSchema());
        await second.TranscribeUrl(Source, new PreRecordedSchema());

        server.Connections.Should().Be(1, "both clients share the pooled handler");
        server.AuthorizationHeaders.Should().BeEquivalentTo(
            new[] { "token key-alpha", "token key-beta" },
            "a shared connection pool must not make one client's credentials leak into another's request");
    }
}
