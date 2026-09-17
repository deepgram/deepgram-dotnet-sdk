// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Net.WebSockets;

using Deepgram.Clients.Listen.v2.WebSocket;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Listen.v2.WebSocket;
using Common = Deepgram.Models.Common.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class ListenWebSocketClientTests
{
    private string _apiKey = null!;
    private DeepgramWsClientOptions _options = null!;

    [SetUp]
    public void Setup()
    {
        _apiKey = new Faker().Random.Guid().ToString();
        _options = new DeepgramWsClientOptions(_apiKey) { OnPrem = true };
    }

    private static void FeedTextMessage(Client client, string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        using var stream = new MemoryStream(bytes);
        client.ProcessTextMessage(new WebSocketReceiveResult(bytes.Length, WebSocketMessageType.Text, true), stream);
    }

    [Test]
    public async Task ProcessTextMessage_Should_Raise_SpeechStarted_Event()
    {
        var client = new Client(_apiKey, _options);
        SpeechStartedResponse? received = null;
        await client.Subscribe(new EventHandler<SpeechStartedResponse>((_, response) => received = response));

        FeedTextMessage(client, """{ "type": "SpeechStarted", "channel": [0, 1], "timestamp": 1.25 }""");

        using (new AssertionScope())
        {
            received.Should().NotBeNull();
            received!.Type.Should().Be(ListenType.SpeechStarted);
            received.Channel.Should().Equal(0, 1);
            received.Timestamp.Should().Be(1.25m);
        }
    }

    [Test]
    public async Task ProcessTextMessage_Should_Raise_UtteranceEnd_Event()
    {
        var client = new Client(_apiKey, _options);
        UtteranceEndResponse? received = null;
        await client.Subscribe(new EventHandler<UtteranceEndResponse>((_, response) => received = response));

        FeedTextMessage(client, """{ "type": "UtteranceEnd", "channel": [0, 1], "last_word_end": 4.5 }""");

        using (new AssertionScope())
        {
            received.Should().NotBeNull();
            received!.Type.Should().Be(ListenType.UtteranceEnd);
            received.Channel.Should().Equal(0, 1);
            received.LastWordEnd.Should().Be(4.5m);
        }
    }

    [Test]
    public async Task ProcessTextMessage_With_Unknown_Type_Should_Not_Invoke_Typed_Listeners()
    {
        var client = new Client(_apiKey, _options);
        var speechStartedCount = 0;
        var utteranceEndCount = 0;
        Common.UnhandledResponse? unhandled = null;
        await client.Subscribe(new EventHandler<SpeechStartedResponse>((_, _) => speechStartedCount++));
        await client.Subscribe(new EventHandler<UtteranceEndResponse>((_, _) => utteranceEndCount++));
        await client.Subscribe(new EventHandler<Common.UnhandledResponse>((_, response) => unhandled = response));

        var json = """{ "type": "FutureMessage" }""";
        Action act = () => FeedTextMessage(client, json);

        using (new AssertionScope())
        {
            act.Should().NotThrow("unknown messages must remain non-fatal");
            speechStartedCount.Should().Be(0);
            utteranceEndCount.Should().Be(0);
            unhandled.Should().NotBeNull();
            unhandled!.Raw.Should().Be(json);
        }
    }
}
