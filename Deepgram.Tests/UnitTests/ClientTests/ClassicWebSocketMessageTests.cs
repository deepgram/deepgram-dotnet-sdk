// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Net.WebSockets;

using Deepgram.Models.Authenticate.v1;
using ListenClient = Deepgram.Clients.Listen.v2.WebSocket.Client;
using ListenMetadataResponse = Deepgram.Models.Listen.v2.WebSocket.MetadataResponse;
using ListenResultResponse = Deepgram.Models.Listen.v2.WebSocket.ResultResponse;
using ListenSpeechStartedResponse = Deepgram.Models.Listen.v2.WebSocket.SpeechStartedResponse;
using ListenUnhandledResponse = Deepgram.Models.Listen.v2.WebSocket.UnhandledResponse;
using ListenUtteranceEndResponse = Deepgram.Models.Listen.v2.WebSocket.UtteranceEndResponse;
using SpeakAudioResponse = Deepgram.Models.Speak.v2.WebSocket.AudioResponse;
using SpeakClearedResponse = Deepgram.Models.Speak.v2.WebSocket.ClearedResponse;
using SpeakWebSocketClient = Deepgram.Clients.Speak.v2.WebSocket.Client;
using SpeakFlushedResponse = Deepgram.Models.Speak.v2.WebSocket.FlushedResponse;
using SpeakMetadataResponse = Deepgram.Models.Speak.v2.WebSocket.MetadataResponse;
using SpeakUnhandledResponse = Deepgram.Models.Speak.v2.WebSocket.UnhandledResponse;
using SpeakWarningResponse = Deepgram.Models.Speak.v2.WebSocket.WarningResponse;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class ClassicWebSocketMessageTests
{
    private string _apiKey = null!;
    private DeepgramWsClientOptions _options = null!;
    private WebSocketReceiveResult _textFrame = null!;

    [SetUp]
    public void SetUp()
    {
        _apiKey = new Faker().Random.Guid().ToString();
        _options = new DeepgramWsClientOptions(_apiKey) { OnPrem = true };
        _textFrame = new WebSocketReceiveResult(1, WebSocketMessageType.Text, true);
    }

    [Test]
    public async Task Listen_ProcessTextMessage_Should_Route_Typed_Events()
    {
        var client = new ListenClient(_apiKey, _options);
        ListenResultResponse? result = null;
        ListenMetadataResponse? metadata = null;
        ListenUtteranceEndResponse? utteranceEnd = null;
        ListenSpeechStartedResponse? speechStarted = null;

        await client.Subscribe(new EventHandler<ListenResultResponse>((_, response) => result = response));
        await client.Subscribe(new EventHandler<ListenMetadataResponse>((_, response) => metadata = response));
        await client.Subscribe(new EventHandler<ListenUtteranceEndResponse>((_, response) => utteranceEnd = response));
        await client.Subscribe(new EventHandler<ListenSpeechStartedResponse>((_, response) => speechStarted = response));

        client.ProcessTextMessage(_textFrame, ToStream("""{ "type": "Results", "is_final": true }"""));
        client.ProcessTextMessage(_textFrame, ToStream("""{ "type": "Metadata", "request_id": "request-1" }"""));
        client.ProcessTextMessage(_textFrame, ToStream("""{ "type": "UtteranceEnd", "last_word_end": 2.5 }"""));
        client.ProcessTextMessage(_textFrame, ToStream("""{ "type": "SpeechStarted", "timestamp": 1.5 }"""));

        using (new AssertionScope())
        {
            result!.IsFinal.Should().BeTrue();
            metadata!.RequestId.Should().Be("request-1");
            utteranceEnd!.LastWordEnd.Should().Be(2.5m);
            speechStarted!.Timestamp.Should().Be(1.5m);
        }
    }

    [TestCase("""{ "type": "FutureListenMessage" }""")]
    [TestCase("""{ "type": 1 }""")]
    [TestCase("""{ "request_id": "missing-type" }""")]
    public async Task Listen_ProcessTextMessage_Should_Route_Unknown_Events_To_Unhandled(string json)
    {
        var client = new ListenClient(_apiKey, _options);
        ListenUnhandledResponse? unhandled = null;
        await client.Subscribe(new EventHandler<ListenUnhandledResponse>((_, response) => unhandled = response));

        client.ProcessTextMessage(_textFrame, ToStream(json));

        using (new AssertionScope())
        {
            unhandled.Should().NotBeNull("unknown server frames must not be dropped");
            unhandled!.Raw.Should().Be(json);
        }
    }

    [Test]
    public async Task Speak_ProcessMessages_Should_Route_Typed_Events_And_Audio()
    {
        var client = new SpeakWebSocketClient(_apiKey, _options);
        SpeakMetadataResponse? metadata = null;
        SpeakFlushedResponse? flushed = null;
        SpeakClearedResponse? cleared = null;
        SpeakWarningResponse? warning = null;
        SpeakAudioResponse? audio = null;

        await client.Subscribe(new EventHandler<SpeakMetadataResponse>((_, response) => metadata = response));
        await client.Subscribe(new EventHandler<SpeakFlushedResponse>((_, response) => flushed = response));
        await client.Subscribe(new EventHandler<SpeakClearedResponse>((_, response) => cleared = response));
        await client.Subscribe(new EventHandler<SpeakWarningResponse>((_, response) => warning = response));
        await client.Subscribe(new EventHandler<SpeakAudioResponse>((_, response) => audio = response));

        client.ProcessTextMessage(_textFrame, ToStream("""{ "type": "Metadata", "request_id": "request-2" }"""));
        client.ProcessTextMessage(_textFrame, ToStream("""{ "type": "Flushed", "sequence_id": 4 }"""));
        client.ProcessTextMessage(_textFrame, ToStream("""{ "type": "Cleared", "sequence_id": 5 }"""));
        client.ProcessTextMessage(_textFrame, ToStream("""{ "type": "Warning", "warn_code": "slow", "warn_msg": "Slow down" }"""));
        var bytes = new byte[] { 1, 2, 3 };
        client.ProcessBinaryMessage(new WebSocketReceiveResult(bytes.Length, WebSocketMessageType.Binary, true), new MemoryStream(bytes));

        using (new AssertionScope())
        {
            metadata!.RequestId.Should().Be("request-2");
            flushed!.SequenceId.Should().Be(4);
            cleared!.SequenceId.Should().Be(5);
            warning!.WarnCode.Should().Be("slow");
            audio!.Stream!.ToArray().Should().Equal(bytes);
        }
    }

    [TestCase("""{ "type": "FutureSpeakMessage" }""")]
    [TestCase("""{ "type": false }""")]
    [TestCase("""{ "request_id": "missing-type" }""")]
    public async Task Speak_ProcessTextMessage_Should_Route_Unknown_Events_To_Unhandled(string json)
    {
        var client = new SpeakWebSocketClient(_apiKey, _options);
        SpeakUnhandledResponse? unhandled = null;
        await client.Subscribe(new EventHandler<SpeakUnhandledResponse>((_, response) => unhandled = response));

        client.ProcessTextMessage(_textFrame, ToStream(json));

        using (new AssertionScope())
        {
            unhandled.Should().NotBeNull("unknown server frames must not be dropped");
            unhandled!.Raw.Should().Be(json);
        }
    }

    private static MemoryStream ToStream(string json) => new(Encoding.UTF8.GetBytes(json));
}
