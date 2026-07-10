// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Net.WebSockets;
using System.Text.Json.Serialization;

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Flux.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// Behavioral parity tests for the Flux client, using a sibling SDK as the golden source.
///
/// Fixtures/Flux/frames.json holds verbatim text frames captured once from the live
/// wss://api.deepgram.com/v2/listen endpoint (see the capture provenance inside the file).
/// Fixtures/Flux/golden.json holds the events the official Deepgram PYTHON SDK produced when
/// parsing those identical frames (its construct_type(V2SocketClientResponse) path).
///
/// These tests replay the identical raw frames through the C# client's receive path
/// (ProcessTextMessage) and assert the emitted events are equivalent to the Python SDK's.
/// The Python SDK is the source of truth; the C# client is the thing under test.
///
/// Comparison rules: exact for strings/ints/enums/event order and count; floating-point
/// fields compared within 1e-9 (both sides parse the same JSON literals).
/// </summary>
public class FluxParityTests
{
    private static readonly string FixtureDir =
        Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "Flux");

    private record GoldenWord(
        [property: JsonPropertyName("word")] string? Word,
        [property: JsonPropertyName("confidence")] double? Confidence,
        [property: JsonPropertyName("start")] double? Start,
        [property: JsonPropertyName("end")] double? End);

    private record GoldenEvent(
        [property: JsonPropertyName("kind")] string? Kind,
        [property: JsonPropertyName("request_id")] string? RequestId,
        [property: JsonPropertyName("sequence_id")] int? SequenceId,
        [property: JsonPropertyName("event")] string? Event,
        [property: JsonPropertyName("turn_index")] int? TurnIndex,
        [property: JsonPropertyName("audio_window_start")] double? AudioWindowStart,
        [property: JsonPropertyName("audio_window_end")] double? AudioWindowEnd,
        [property: JsonPropertyName("transcript")] string? Transcript,
        [property: JsonPropertyName("words")] List<GoldenWord>? Words,
        [property: JsonPropertyName("end_of_turn_confidence")] double? EndOfTurnConfidence,
        [property: JsonPropertyName("languages")] List<string>? Languages,
        [property: JsonPropertyName("languages_hinted")] List<string>? LanguagesHinted,
        [property: JsonPropertyName("eager_eot_threshold")] double? EagerEotThreshold,
        [property: JsonPropertyName("eot_threshold")] double? EotThreshold,
        [property: JsonPropertyName("eot_timeout_ms")] int? EotTimeoutMs,
        [property: JsonPropertyName("keyterms")] List<string>? Keyterms,
        [property: JsonPropertyName("language_hints")] List<string>? LanguageHints,
        [property: JsonPropertyName("code")] string? Code,
        [property: JsonPropertyName("description")] string? Description,
        [property: JsonPropertyName("raw_type")] string? RawType);

    private record GoldenFile([property: JsonPropertyName("events")] List<GoldenEvent> Events);

    private record FramesFile([property: JsonPropertyName("frames")] List<string> Frames);

    private static (List<string> frames, List<GoldenEvent> golden) LoadFixtures()
    {
        var framesPath = Path.Combine(FixtureDir, "frames.json");
        var goldenPath = Path.Combine(FixtureDir, "golden.json");
        if (!File.Exists(framesPath) || !File.Exists(goldenPath))
        {
            Assert.Ignore("Flux wire fixtures not present (Fixtures/Flux/frames.json + golden.json). " +
                          "Run the one-time capture harness to generate them.");
        }

        var frames = JsonSerializer.Deserialize<FramesFile>(File.ReadAllText(framesPath))!.Frames;
        var golden = JsonSerializer.Deserialize<GoldenFile>(File.ReadAllText(goldenPath))!.Events;
        return (frames, golden);
    }

    /// <summary>
    /// Replays every raw frame through the C# client's receive path and collects the events
    /// it raises, in order, normalized to the same record shape as the Python golden file.
    /// </summary>
    private static async Task<List<GoldenEvent>> ReplayThroughCSharpClient(List<string> frames)
    {
        var options = new DeepgramWsClientOptions(new Faker().Random.Guid().ToString()) { OnPrem = true };
        var client = new FluxWebSocketClient(options.ApiKey, options);
        var received = new List<GoldenEvent>();
        var mutex = new object();

        void Add(GoldenEvent e)
        {
            lock (mutex)
            {
                received.Add(e);
            }
        }

        await client.Subscribe(new EventHandler<ConnectedResponse>((s, e) =>
            Add(new GoldenEvent("Connected", e.RequestId, e.SequenceId, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null))));
        await client.Subscribe(new EventHandler<TurnInfoResponse>((s, e) =>
            Add(new GoldenEvent("TurnInfo", e.RequestId, e.SequenceId, e.Event, e.TurnIndex, e.AudioWindowStart,
                e.AudioWindowEnd, e.Transcript,
                e.Words?.Select(w => new GoldenWord(w.HeardWord, w.Confidence, w.Start, w.End)).ToList(),
                e.EndOfTurnConfidence, e.Languages, e.LanguagesHinted,
                null, null, null, null, null, null, null, null))));
        await client.Subscribe(new EventHandler<ConfigureSuccessResponse>((s, e) =>
            Add(new GoldenEvent("ConfigureSuccess", e.RequestId, e.SequenceId, null, null, null, null, null, null,
                null, null, null, e.Thresholds?.EagerEotThreshold, e.Thresholds?.EotThreshold,
                e.Thresholds?.EotTimeoutMs, e.Keyterms, e.LanguageHints, null, null, null))));
        await client.Subscribe(new EventHandler<ConfigureFailureResponse>((s, e) =>
            Add(new GoldenEvent("ConfigureFailure", e.RequestId, e.SequenceId, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, e.Code, e.Description, null))));
        await client.Subscribe(new EventHandler<ErrorResponse>((s, e) =>
            Add(new GoldenEvent("Error", null, e.SequenceId, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, e.Code, e.Description, null))));
        await client.Subscribe(new EventHandler<UnhandledResponse>((s, e) =>
            Add(new GoldenEvent("Unhandled", null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, RawTypeOf(e.Raw)))));

        var result = new WebSocketReceiveResult(1, WebSocketMessageType.Text, true);
        foreach (var frame in frames)
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(frame));
            client.ProcessTextMessage(result, ms);
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

    private static void AssertDouble(double? actual, double? expected, string context)
    {
        if (expected is null)
        {
            actual.Should().BeNull(context);
        }
        else
        {
            actual.Should().NotBeNull(context);
            actual!.Value.Should().BeApproximately(expected.Value, 1e-9, context);
        }
    }

    [Test]
    public async Task CSharp_Client_Should_Emit_Events_Equivalent_To_Python_SDK_Golden()
    {
        var (frames, golden) = LoadFixtures();

        var actual = await ReplayThroughCSharpClient(frames);

        actual.Count.Should().Be(golden.Count,
            "the C# client must raise exactly one event per captured frame, like the Python SDK");

        for (var i = 0; i < golden.Count; i++)
        {
            var g = golden[i];
            var a = actual[i];
            var ctx = $"frame {i} ({g.Kind})";

            a.Kind.Should().Be(g.Kind, ctx);
            a.RequestId.Should().Be(g.RequestId, ctx);
            a.SequenceId.Should().Be(g.SequenceId, ctx);
            a.Event.Should().Be(g.Event, ctx);
            a.TurnIndex.Should().Be(g.TurnIndex, ctx);
            a.Transcript.Should().Be(g.Transcript, ctx);
            a.Code.Should().Be(g.Code, ctx);
            a.Description.Should().Be(g.Description, ctx);
            a.EotTimeoutMs.Should().Be(g.EotTimeoutMs, ctx);
            AssertDouble(a.AudioWindowStart, g.AudioWindowStart, ctx);
            AssertDouble(a.AudioWindowEnd, g.AudioWindowEnd, ctx);
            AssertDouble(a.EndOfTurnConfidence, g.EndOfTurnConfidence, ctx);
            AssertDouble(a.EagerEotThreshold, g.EagerEotThreshold, ctx);
            AssertDouble(a.EotThreshold, g.EotThreshold, ctx);

            (a.Languages ?? new List<string>()).Should().Equal(g.Languages ?? new List<string>(), ctx);
            (a.LanguagesHinted ?? new List<string>()).Should().Equal(g.LanguagesHinted ?? new List<string>(), ctx);
            (a.Keyterms ?? new List<string>()).Should().Equal(g.Keyterms ?? new List<string>(), ctx);
            (a.LanguageHints ?? new List<string>()).Should().Equal(g.LanguageHints ?? new List<string>(), ctx);

            if (g.Words is null)
            {
                (a.Words is null || a.Words.Count == 0).Should().BeTrue(ctx);
            }
            else
            {
                a.Words.Should().NotBeNull(ctx);
                a.Words!.Count.Should().Be(g.Words.Count, ctx);
                for (var w = 0; w < g.Words.Count; w++)
                {
                    var wCtx = $"{ctx} word {w}";
                    a.Words[w].Word.Should().Be(g.Words[w].Word, wCtx);
                    AssertDouble(a.Words[w].Confidence, g.Words[w].Confidence, wCtx);
                    AssertDouble(a.Words[w].Start, g.Words[w].Start, wCtx);
                    AssertDouble(a.Words[w].End, g.Words[w].End, wCtx);
                }
            }
        }
    }

    [Test]
    public void Captured_Fixtures_Should_Contain_The_Expected_Message_Mix()
    {
        var (_, golden) = LoadFixtures();

        using (new AssertionScope())
        {
            golden.Should().NotBeEmpty();
            golden[0].Kind.Should().Be("Connected", "the first server message on a Flux connection is Connected");
            golden.Should().Contain(e => e.Kind == "TurnInfo", "captured audio must produce turn results");
            golden.Should().Contain(e => e.Kind == "TurnInfo" && e.Event == "EndOfTurn",
                "the captured conversation should contain at least one completed turn");
            golden.Select(e => e.SequenceId).Where(s => s != null).Should().BeInAscendingOrder(
                "sequence_id increments across all server messages");
        }
    }
}
