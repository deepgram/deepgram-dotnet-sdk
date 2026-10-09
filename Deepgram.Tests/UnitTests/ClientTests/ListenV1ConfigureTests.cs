// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using Deepgram.Clients.Interfaces.v2;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Listen.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class ListenV1ConfigureTests
{
    [Test]
    public async Task SendConfigure_Should_Serialize_Nullable_Keyterms_And_Features()
    {
        var client = Substitute.For<IListenWebSocketClient>();
        byte[]? sent = null;
        client.When(x => x.SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>()))
            .Do(call => sent = call.Arg<byte[]>());

        await client.SendConfigure(new ConfigureSchema
        {
            Keyterms = null,
            Features = new Dictionary<string, bool> { ["numerals"] = true },
        });

        using var json = JsonDocument.Parse(Encoding.UTF8.GetString(sent!));
        json.RootElement.GetProperty("type").GetString().Should().Be("Configure");
        json.RootElement.TryGetProperty("keyterms", out _).Should().BeFalse();
        json.RootElement.GetProperty("features").GetProperty("numerals").GetBoolean().Should().BeTrue();
    }

    [Test]
    public async Task SendConfigure_Should_Serialize_Empty_Keyterms()
    {
        var client = Substitute.For<IListenWebSocketClient>();
        byte[]? sent = null;
        client.When(x => x.SendMessageImmediately(Arg.Any<byte[]>(), Arg.Any<int>()))
            .Do(call => sent = call.Arg<byte[]>());

        await client.SendConfigure(new ConfigureSchema { Keyterms = new List<string>() });

        using var json = JsonDocument.Parse(Encoding.UTF8.GetString(sent!));
        json.RootElement.GetProperty("keyterms").EnumerateArray().Should().BeEmpty();
    }

    [Test]
    public void ErrorResponse_Should_Deserialize_Configure_Rejection_Code()
    {
        var error = JsonSerializer.Deserialize<ErrorResponse>("""{"type":"Error","variant":"InvalidConfigureMessage","code":"KeytermsNotSupported","description":"unsupported"}""");

        error!.Variant.Should().Be("InvalidConfigureMessage");
        error.Code.Should().Be("KeytermsNotSupported");
    }

    [Test]
    public async Task SendConfigure_Should_Drain_Queued_Audio_Before_Configure()
    {
        using var portProbe = new TcpListener(IPAddress.Loopback, 0);
        portProbe.Start();
        var port = ((IPEndPoint)portProbe.LocalEndpoint).Port;
        portProbe.Stop();

        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.Start();
        var received = ReceiveTwoFrames(listener);

        var options = new DeepgramWsClientOptions(
            apiKey: "test-api-key",
            baseAddress: $"ws://127.0.0.1:{port}/v1",
            onPrem: true);
        var client = new ListenWebSocketClient("test-api-key", options);

        try
        {
            (await client.Connect(new LiveSchema { Model = "nova-3" })).Should().BeTrue();
            client.Send(new byte[] { 1, 2, 3 });
            await client.SendConfigure(new ConfigureSchema { Features = new Dictionary<string, bool> { ["numerals"] = true } });

            var frames = await received.WaitAsync(TimeSpan.FromSeconds(10));
            frames[0].MessageType.Should().Be(WebSocketMessageType.Binary);
            frames[0].Payload.Should().Equal((byte)1, (byte)2, (byte)3);
            frames[1].MessageType.Should().Be(WebSocketMessageType.Text);
            using var configure = JsonDocument.Parse(frames[1].Payload);
            configure.RootElement.GetProperty("type").GetString().Should().Be("Configure");
        }
        finally
        {
            await client.Stop();
        }
    }

    private static async Task<List<(WebSocketMessageType MessageType, byte[] Payload)>> ReceiveTwoFrames(HttpListener listener)
    {
        var context = await listener.GetContextAsync();
        var socket = (await context.AcceptWebSocketAsync(null)).WebSocket;
        var frames = new List<(WebSocketMessageType MessageType, byte[] Payload)>();

        try
        {
            while (frames.Count < 2)
            {
                var buffer = new byte[1024];
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                frames.Add((result.MessageType, buffer[..result.Count]));
            }

            await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
        }
        finally
        {
            socket.Dispose();
        }

        return frames;
    }
}
