// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Net.WebSockets;

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Flux.Speak.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// Wire-fidelity replay test for the Flux TTS (v2 speak) client.
///
/// Fixtures/FluxSpeak/frames.json holds the verbatim JSON control frames captured once from the
/// live wss://api.deepgram.com/v2/speak endpoint (see the provenance inside the file). This test
/// replays those identical frames through the C# client's receive path (ProcessTextMessage) and
/// asserts the emitted, typed events carry the values that were actually on the wire. It runs
/// fully offline in CI.
///
/// NOTE: the oracle here is the captured staging session itself, not an independent sibling-SDK
/// golden. Adding a Python-SDK-generated golden (as the STT FluxParityTests has) is a documented
/// follow-up once the Python Speak v2 types are available in the capture environment.
/// </summary>
public class FluxSpeakParityTests
{
    private static readonly string FixtureDir =
        Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "FluxSpeak");

    private static List<string> LoadFrames()
    {
        var path = Path.Combine(FixtureDir, "frames.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var frames = new List<string>();
        foreach (var el in doc.RootElement.GetProperty("frames").EnumerateArray())
        {
            frames.Add(el.GetRawText());
        }
        return frames;
    }

    private static MemoryStream ToStream(string json) => new(Encoding.UTF8.GetBytes(json));

    [Test]
    public async Task Captured_Staging_Frames_Should_Replay_Through_The_Client_In_Order()
    {
        var client = new FluxSpeakWebSocketClient(new Faker().Random.Guid().ToString(),
            new DeepgramWsClientOptions(new Faker().Random.Guid().ToString()) { OnPrem = true });

        var events = new List<object>();
        await client.Subscribe(new EventHandler<ConnectedResponse>((_, e) => events.Add(e)));
        await client.Subscribe(new EventHandler<SpeechStartedResponse>((_, e) => events.Add(e)));
        await client.Subscribe(new EventHandler<FlushedResponse>((_, e) => events.Add(e)));
        await client.Subscribe(new EventHandler<SpeechMetadataResponse>((_, e) => events.Add(e)));
        await client.Subscribe(new EventHandler<SessionMetadataResponse>((_, e) => events.Add(e)));
        await client.Subscribe(new EventHandler<WarningResponse>((_, e) => events.Add(e)));
        await client.Subscribe(new EventHandler<ErrorResponse>((_, e) => events.Add(e)));
        await client.Subscribe(new EventHandler<UnhandledResponse>((_, e) => events.Add(e)));

        var wsr = new WebSocketReceiveResult(1, WebSocketMessageType.Text, true);
        foreach (var frame in LoadFrames())
        {
            client.ProcessTextMessage(wsr, ToStream(frame));
        }

        using (new AssertionScope())
        {
            // Exact event sequence the staging session produced.
            events.Select(e => e.GetType().Name).Should().Equal(
                nameof(ConnectedResponse),
                nameof(SpeechStartedResponse),
                nameof(FlushedResponse),
                nameof(SpeechMetadataResponse),
                nameof(SessionMetadataResponse));

            var connected = (ConnectedResponse)events[0];
            connected.RequestId.Should().Be("019f60dc-5032-7fb1-87f8-8644d13ca13f");
            connected.ModelName.Should().Be("alexis");
            connected.ModelVersion.Should().Be("2026-07-02.2");
            connected.ModelUuids.Should().HaveCount(2, "the wire delivered model_uuids as a plural array");

            ((SpeechStartedResponse)events[1]).SpeechId.Should().Be("dg_sp_24682fee6658");
            ((FlushedResponse)events[2]).SpeechId.Should().Be("dg_sp_24682fee6658");

            var meta = (SpeechMetadataResponse)events[3];
            meta.SpeechId.Should().Be("dg_sp_24682fee6658");
            meta.AudioDurationMs.Should().Be(5360);
            meta.InputCharacterCount.Should().Be(94);
            meta.BillableCharacterCount.Should().Be(0);
            meta.ControlsApplied.Should().NotBeNull();
            meta.ControlsApplied!.PronunciationsApplied.Should().Be(0);
            meta.ControlsApplied.PronunciationWarnings.Should().Be(0);

            var session = (SessionMetadataResponse)events[4];
            session.TotalAudioDurationMs.Should().Be(5360);
            session.TotalInputCharacterCount.Should().Be(94);
            session.TotalBillableCharacterCount.Should().Be(0);
        }
    }

    [Test]
    public void Captured_Audio_Byte_Count_Matches_Reported_Duration()
    {
        // 257280 bytes of 24kHz mono linear16 = 257280 / 2 / 24000 s = 5.36s = 5360ms,
        // which equals the audio_duration_ms the SpeechMetadata frame reported.
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(FixtureDir, "frames.json")));
        var bytes = doc.RootElement.GetProperty("audio_total_bytes").GetInt32();
        var durationMs = bytes / 2.0 / 24000.0 * 1000.0;
        durationMs.Should().BeApproximately(5360, 1);
    }
}
