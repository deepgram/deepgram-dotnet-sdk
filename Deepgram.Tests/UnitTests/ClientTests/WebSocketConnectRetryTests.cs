// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Net.Sockets;
using System.Net.WebSockets;
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
    /// <summary>
    /// Returns a loopback endpoint with nothing listening on it, so ConnectAsync fails with
    /// "connection refused". The port comes from the OS and is released before it is used.
    /// </summary>
    private static string GetRefusedEndpoint()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return $"ws://127.0.0.1:{port}";
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
        var refusedEndpoint = GetRefusedEndpoint();
        using var client = new TestWebSocketClient(new Faker().Random.Guid().ToString());

        Func<Task<bool>> connect = async () => await client.Connect(refusedEndpoint);

        await connect.Should().ThrowAsync<WebSocketException>();
        client.State().Should().Be(WebSocketState.None, "a failed connect must not keep the dead socket");

        Func<Task<bool>> retry = async () => await client.Connect(refusedEndpoint);

        await retry.Should().ThrowAsync<WebSocketException>(
            "a retry must attempt a new connection instead of reporting the failed one as connected");
    }

    [Test]
    public async Task AbstractClient_Connect_After_A_Cancelled_Connect_Should_Open_A_New_Connection()
    {
        using var server = new LoopbackWebSocketServer();
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
        using var server = new LoopbackWebSocketServer();
        using var client = new TestWebSocketClient(new Faker().Random.Guid().ToString());

        // A space is not a valid token character, so SetRequestHeader rejects the name.
        var invalidHeaders = new Dictionary<string, string> { ["Invalid Header"] = "value" };
        Func<Task<bool>> connect = async () => await client.Connect(server.Endpoint, headers: invalidHeaders);

        await connect.Should().ThrowAsync<ArgumentException>();
        client.State().Should().Be(WebSocketState.None, "a rejected header must not leave a socket assigned");

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
        var options = new DeepgramWsClientOptions(apiKey, GetRefusedEndpoint(), keepAlive: false, onPrem: true);
        using var client = new ListenV2.Client(apiKey, options);
        var schema = new LiveSchema { Model = "nova-3" };

        Func<Task<bool>> connect = async () => await client.Connect(schema);

        await connect.Should().ThrowAsync<WebSocketException>();

        Func<Task<bool>> retry = async () => await client.Connect(schema);

        await retry.Should().ThrowAsync<WebSocketException>(
            "a retry must attempt a new connection instead of reporting the failed one as connected");
        client.IsConnected().Should().BeFalse();
    }
}
