// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Net.WebSockets;
using System.Text.Json.Serialization;

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

    // ---- GA additions replay (Configure/ConfigureSuccess, Interrupt/SpeechInterrupted) ---------
    //
    // frames.json/golden.json above are EA captures and stay untouched as a regression fixture.
    // frames-ga.json is a separate, verbatim capture (see its _provenance field) exercising the
    // GA-only messages this change adds: Configure/ConfigureSuccess and Interrupt/SpeechInterrupted,
    // against the live production endpoint. Captured via a standalone raw-WebSocket script kept
    // independent of the SDK's own parser (so this replay is checked against reality, not just
    // against itself) - see the Flux TTS GA delta PR for the capture script.

    private static List<string> LoadGaFrames()
    {
        var path = Path.Combine(FixtureDir, "frames-ga.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var frames = new List<string>();
        foreach (var el in doc.RootElement.GetProperty("frames").EnumerateArray())
        {
            frames.Add(el.GetRawText());
        }
        return frames;
    }

    [Test]
    public async Task GA_Frames_Should_Replay_Through_The_Client_In_Order()
    {
        var client = new FluxSpeakWebSocketClient(new Faker().Random.Guid().ToString(),
            new DeepgramWsClientOptions(new Faker().Random.Guid().ToString()) { OnPrem = true });

        var events = new List<object>();
        await client.Subscribe(new EventHandler<ConnectedResponse>((_, e) => events.Add(e)));
        await client.Subscribe(new EventHandler<ConfigureSuccessResponse>((_, e) => events.Add(e)));
        await client.Subscribe(new EventHandler<SpeechStartedResponse>((_, e) => events.Add(e)));
        await client.Subscribe(new EventHandler<SpeechInterruptedResponse>((_, e) => events.Add(e)));
        await client.Subscribe(new EventHandler<SessionMetadataResponse>((_, e) => events.Add(e)));
        await client.Subscribe(new EventHandler<ConfigureFailureResponse>((_, e) => events.Add(e)));
        await client.Subscribe(new EventHandler<ErrorResponse>((_, e) => events.Add(e)));

        var wsr = new WebSocketReceiveResult(1, WebSocketMessageType.Text, true);
        foreach (var frame in LoadGaFrames())
        {
            client.ProcessTextMessage(wsr, ToStream(frame));
        }

        using (new AssertionScope())
        {
            events.Select(e => e.GetType().Name).Should().Equal(
                nameof(ConnectedResponse),
                nameof(ConfigureSuccessResponse),
                nameof(SpeechStartedResponse),
                nameof(SpeechInterruptedResponse),
                nameof(SessionMetadataResponse));

            var connected = (ConnectedResponse)events[0];
            connected.ModelName.Should().Be("alexis", "the wire model_name is the bare voice name, not the full flux-{voice}-{lang} string");
            connected.ModelUuids.Should().HaveCount(2, "the wire delivered model_uuids as a plural array, matching the EA capture's shape");

            ((ConfigureSuccessResponse)events[1]).Applied!.Speed.Should().Be(1.05);
            ((SpeechStartedResponse)events[2]).SpeechId.Should().Be("dg_sp_c78f63c65fa2");

            var interrupted = (SpeechInterruptedResponse)events[3];
            interrupted.AudioPlayedMs.Should().Be(554);
            interrupted.TextSpoken.Should().Be("Sure,");
            interrupted.TextRemaining.Should().Be(" I can help you cancel your subscription. Let me pull up your account and check on that for you right away.");
            interrupted.Metadata!.ControlsApplied!.BreaksApplied.Should().Be(0);

            var session = (SessionMetadataResponse)events[4];
            session.TotalAudioDurationMs.Should().Be(554);
            // Real-capture curiosity, not asserted here as "correct": for this interrupted turn the
            // server's SessionMetadata.total_input_character_count (46) matched the turn's
            // billable_character_count, not its full input_character_count (112). Left as observed
            // behavior; a server-side accounting question, not something this client controls.
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

    // ---- Independent golden parity (mirrors the STT FluxParityTests) --------------------------
    //
    // Fixtures/FluxSpeak/golden.json holds the events the official Deepgram PYTHON SDK produced
    // when its typed model classes (SpeakV2Connected, ...) parsed the identical frames.json.
    // This decouples the expected values from our own capture: the C# client must match the
    // Python SDK, not itself. The golden is produced by the capture harness (make_golden.py,
    // which feeds these frames through the Python SDK's SpeakV2* model classes — the same
    // approach as the STT golden). Until golden.json is present the test self-skips, exactly
    // like the STT parity test does when its fixtures are absent.

    private record GoldenEvent(
        [property: JsonPropertyName("kind")] string? Kind,
        [property: JsonPropertyName("request_id")] string? RequestId,
        [property: JsonPropertyName("model_name")] string? ModelName,
        [property: JsonPropertyName("model_version")] string? ModelVersion,
        [property: JsonPropertyName("model_uuids")] List<string>? ModelUuids,
        [property: JsonPropertyName("speech_id")] string? SpeechId,
        [property: JsonPropertyName("audio_duration_ms")] int? AudioDurationMs,
        [property: JsonPropertyName("input_character_count")] int? InputCharacterCount,
        [property: JsonPropertyName("billable_character_count")] int? BillableCharacterCount,
        [property: JsonPropertyName("pronunciations_applied")] int? PronunciationsApplied,
        [property: JsonPropertyName("pronunciation_warnings")] int? PronunciationWarnings,
        [property: JsonPropertyName("total_audio_duration_ms")] int? TotalAudioDurationMs,
        [property: JsonPropertyName("total_input_character_count")] int? TotalInputCharacterCount,
        [property: JsonPropertyName("total_billable_character_count")] int? TotalBillableCharacterCount,
        [property: JsonPropertyName("code")] string? Code,
        [property: JsonPropertyName("description")] string? Description,
        [property: JsonPropertyName("raw_type")] string? RawType);

    private record GoldenFile([property: JsonPropertyName("events")] List<GoldenEvent> Events);

    private static async Task<List<GoldenEvent>> ReplayThroughCSharpClient(List<string> frames)
    {
        var client = new FluxSpeakWebSocketClient(new Faker().Random.Guid().ToString(),
            new DeepgramWsClientOptions(new Faker().Random.Guid().ToString()) { OnPrem = true });
        var received = new List<GoldenEvent>();
        var mutex = new object();

        void Add(GoldenEvent e) { lock (mutex) { received.Add(e); } }

        await client.Subscribe(new EventHandler<ConnectedResponse>((_, e) =>
            Add(new GoldenEvent("Connected", e.RequestId, e.ModelName, e.ModelVersion,
                e.ModelUuids?.ToList(), null, null, null, null, null, null, null, null, null, null, null, null))));
        await client.Subscribe(new EventHandler<SpeechStartedResponse>((_, e) =>
            Add(new GoldenEvent("SpeechStarted", null, null, null, null, e.SpeechId, null, null, null, null, null,
                null, null, null, null, null, null))));
        await client.Subscribe(new EventHandler<FlushedResponse>((_, e) =>
            Add(new GoldenEvent("Flushed", null, null, null, null, e.SpeechId, null, null, null, null, null,
                null, null, null, null, null, null))));
        await client.Subscribe(new EventHandler<SpeechMetadataResponse>((_, e) =>
            Add(new GoldenEvent("SpeechMetadata", null, null, null, null, e.SpeechId, e.AudioDurationMs,
                e.InputCharacterCount, e.BillableCharacterCount,
                e.ControlsApplied?.PronunciationsApplied, e.ControlsApplied?.PronunciationWarnings,
                null, null, null, null, null, null))));
        await client.Subscribe(new EventHandler<SessionMetadataResponse>((_, e) =>
            Add(new GoldenEvent("SessionMetadata", null, null, null, null, null, null, null, null, null, null,
                e.TotalAudioDurationMs, e.TotalInputCharacterCount, e.TotalBillableCharacterCount,
                null, null, null))));
        await client.Subscribe(new EventHandler<WarningResponse>((_, e) =>
            Add(new GoldenEvent("Warning", null, null, null, null, null, null, null, null, null, null,
                null, null, null, e.Code, e.Description, null))));
        await client.Subscribe(new EventHandler<ErrorResponse>((_, e) =>
            Add(new GoldenEvent("Error", null, null, null, null, null, null, null, null, null, null,
                null, null, null, e.Code, e.Description, null))));
        await client.Subscribe(new EventHandler<UnhandledResponse>((_, e) =>
            Add(new GoldenEvent("Unhandled", null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, RawTypeOf(e.Raw)))));

        var wsr = new WebSocketReceiveResult(1, WebSocketMessageType.Text, true);
        foreach (var frame in frames)
        {
            client.ProcessTextMessage(wsr, ToStream(frame));
        }

        return received;
    }

    private static string? RawTypeOf(string? raw)
    {
        if (raw is null)
        {
            return null;
        }
        using var doc = JsonDocument.Parse(raw);
        return doc.RootElement.TryGetProperty("type", out var t) ? t.GetString() : null;
    }

    [Test]
    public async Task CSharp_Client_Should_Emit_Events_Equivalent_To_Python_SDK_Golden()
    {
        var goldenPath = Path.Combine(FixtureDir, "golden.json");
        if (!File.Exists(goldenPath))
        {
            Assert.Ignore("Fixtures/FluxSpeak/golden.json not present. Run the capture harness " +
                          "(flux-speak-capture/make_golden.py) against the Python SDK to generate it.");
        }

        var frames = LoadFrames();
        var golden = JsonSerializer.Deserialize<GoldenFile>(File.ReadAllText(goldenPath))!.Events;

        var actual = await ReplayThroughCSharpClient(frames);

        actual.Count.Should().Be(golden.Count,
            "the C# client must raise exactly one event per captured frame, like the Python SDK");

        using (new AssertionScope())
        {
            for (var i = 0; i < golden.Count; i++)
            {
                var g = golden[i];
                var a = actual[i];
                var ctx = $"frame {i} ({g.Kind})";

                a.Kind.Should().Be(g.Kind, ctx);
                a.RequestId.Should().Be(g.RequestId, ctx);
                a.ModelName.Should().Be(g.ModelName, ctx);
                a.ModelVersion.Should().Be(g.ModelVersion, ctx);
                (a.ModelUuids ?? new List<string>()).Should().Equal(g.ModelUuids ?? new List<string>(), ctx);
                a.SpeechId.Should().Be(g.SpeechId, ctx);
                a.AudioDurationMs.Should().Be(g.AudioDurationMs, ctx);
                a.InputCharacterCount.Should().Be(g.InputCharacterCount, ctx);
                a.BillableCharacterCount.Should().Be(g.BillableCharacterCount, ctx);
                a.PronunciationsApplied.Should().Be(g.PronunciationsApplied, ctx);
                a.PronunciationWarnings.Should().Be(g.PronunciationWarnings, ctx);
                a.TotalAudioDurationMs.Should().Be(g.TotalAudioDurationMs, ctx);
                a.TotalInputCharacterCount.Should().Be(g.TotalInputCharacterCount, ctx);
                a.TotalBillableCharacterCount.Should().Be(g.TotalBillableCharacterCount, ctx);
                a.Code.Should().Be(g.Code, ctx);
                a.Description.Should().Be(g.Description, ctx);
                a.RawType.Should().Be(g.RawType, ctx);
            }
        }
    }
}
