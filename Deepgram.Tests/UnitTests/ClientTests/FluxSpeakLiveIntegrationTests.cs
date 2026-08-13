// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Flux.Speak.WebSocket;

// The batch (REST) client and its schema share simple names (SpeakSchema/TextSource) with the
// websocket surface, so alias the REST types to keep both usable in one file.
using RestSpeakSchema = Deepgram.Models.Flux.Speak.REST.SpeakSchema;
using RestTextSource = Deepgram.Models.Flux.Speak.REST.TextSource;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// End-to-end tests against the real Flux TTS (v2 speak) endpoint — the streaming websocket path
/// and the batch REST path. Both are skipped unless DEEPGRAM_API_KEY is set, so normal CI runs
/// never touch the network. These exercise what offline frame-replay cannot: live auth, the
/// websocket handshake, and binary audio framing.
/// </summary>
public class FluxSpeakLiveIntegrationTests
{
    private static string? ResolveApiKey()
    {
        // Read the startup snapshot: the authentication tests clear DEEPGRAM_API_KEY process-wide
        // while exercising credential fallbacks (see GlobalTestEnvironment).
        return GlobalTestEnvironment.DeepgramApiKeyAtStartup
            ?? Environment.GetEnvironmentVariable("DEEPGRAM_API_KEY");
    }

    [Test]
    public async Task Live_FluxSpeak_WebSocket_Connect_Stream_Flush_Close()
    {
        var apiKey = ResolveApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Assert.Ignore("DEEPGRAM_API_KEY is not set. Skipping live Flux TTS websocket test.");
        }

        var client = new FluxSpeakWebSocketClient(apiKey!);

        var connectedReceived = new TaskCompletionSource<ConnectedResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        var turnComplete = new TaskCompletionSource<SpeechMetadataResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        var configureAcked = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondTurnStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var interruptAcked = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var closeReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var audioBytes = 0L;
        var speechStartedCount = 0;
        var closeCount = 0;
        var errors = new List<ErrorResponse>();

        await client.Subscribe(new EventHandler<ConnectedResponse>((s, e) => connectedReceived.TrySetResult(e)));
        await client.Subscribe(new EventHandler<SpeechStartedResponse>((s, e) =>
        {
            if (Interlocked.Increment(ref speechStartedCount) == 2)
            {
                secondTurnStarted.TrySetResult(true);
            }
        }));
        await client.Subscribe(new EventHandler<AudioResponse>((s, e) =>
        {
            if (e.Stream is not null)
            {
                Interlocked.Add(ref audioBytes, e.Stream.Length);
            }
        }));
        await client.Subscribe(new EventHandler<SpeechMetadataResponse>((s, e) => turnComplete.TrySetResult(e)));
        await client.Subscribe(new EventHandler<ConfigureSuccessResponse>((s, e) => configureAcked.TrySetResult(true)));
        await client.Subscribe(new EventHandler<ConfigureFailureResponse>((s, e) => configureAcked.TrySetResult(true)));
        // Barge-in tolerance: a generation-based offset (see the streaming example's honesty note)
        // may not have advanced past the server's own audio-generation position, in which case the
        // server answers with a Warning (e.g. INVALID_INTERRUPT_OFFSET) instead of SpeechInterrupted.
        // Either response proves the round-trip works; that is all this live smoke test asserts.
        await client.Subscribe(new EventHandler<SpeechInterruptedResponse>((s, e) => interruptAcked.TrySetResult(true)));
        await client.Subscribe(new EventHandler<WarningResponse>((s, e) => interruptAcked.TrySetResult(true)));
        await client.Subscribe(new EventHandler<ErrorResponse>((s, e) => { lock (errors) { errors.Add(e); } }));
        await client.Subscribe(new EventHandler<CloseResponse>((s, e) =>
        {
            Interlocked.Increment(ref closeCount);
            closeReceived.TrySetResult(true);
        }));

        var connected = await client.Connect(new SpeakSchema
        {
            Model = "flux-alexis-en",
            Encoding = "linear16",
            SampleRate = 24000,
        });
        connected.Should().BeTrue("the client must connect to the live Flux TTS endpoint");

        // Stream text into the active turn, then flush to end the turn and generate the audio.
        await client.SendText("Your appointment is confirmed for 3pm tomorrow.");
        await client.SendFlush();

        (await Task.WhenAny(connectedReceived.Task, Task.Delay(5000))).Should().Be(connectedReceived.Task,
            "the server must send a Connected message");
        (await Task.WhenAny(turnComplete.Task, Task.Delay(30000))).Should().Be(turnComplete.Task,
            "the flushed turn must produce a SpeechMetadata once its audio has been sent");

        // GA: mid-stream Configure, acked by ConfigureSuccess or ConfigureFailure.
        await client.SendConfigure(new ConfigureSchema { Speed = 1.1 });
        (await Task.WhenAny(configureAcked.Task, Task.Delay(10000))).Should().Be(configureAcked.Task,
            "Configure must be acknowledged with ConfigureSuccess or ConfigureFailure");

        // GA: barge-in on a second, deliberately long turn.
        var audioBeforeSecondTurn = Interlocked.Read(ref audioBytes);
        await client.SendText(
            "This sentence is deliberately long so that audio is still streaming when the interrupt is sent, " +
            "which is what makes the barge-in observable in a live end-to-end run against the real endpoint.");
        (await Task.WhenAny(secondTurnStarted.Task, Task.Delay(10000))).Should().Be(secondTurnStarted.Task,
            "the second turn must start before the interrupt is sent");
        var playbackOffsetMs = (Interlocked.Read(ref audioBytes) - audioBeforeSecondTurn) / 48; // 24kHz mono linear16
        await client.SendInterrupt(Math.Max(playbackOffsetMs, 1));
        (await Task.WhenAny(interruptAcked.Task, Task.Delay(10000))).Should().Be(interruptAcked.Task,
            "Interrupt must be acknowledged with SpeechInterrupted or a Warning");

        // Clean shutdown: sends {"type":"Close"} and drains any remaining audio.
        await client.Stop();

        (await Task.WhenAny(closeReceived.Task, Task.Delay(10000))).Should().Be(closeReceived.Task,
            "a Close event must be raised after Close");

        using (new AssertionScope())
        {
            var connectedMsg = await connectedReceived.Task;
            connectedMsg.RequestId.Should().NotBeNullOrEmpty();

            var meta = await turnComplete.Task;
            meta.AudioDurationMs.Should().BeGreaterThan(0, "the synthesized turn must report a non-zero duration");
            meta.InputCharacterCount.Should().BeGreaterThan(0);

            speechStartedCount.Should().BeGreaterThan(0, "the turn must emit at least one SpeechStarted");
            audioBytes.Should().BeGreaterThan(0, "binary audio frames must be received and surfaced");
            errors.Should().BeEmpty("no fatal errors are expected during a clean session");
            closeCount.Should().Be(1, "the Close event must be delivered exactly once per connection");
        }
    }

    [Test]
    public async Task Live_FluxSpeak_Rest_ToStream_Returns_Audio()
    {
        var apiKey = ResolveApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Assert.Ignore("DEEPGRAM_API_KEY is not set. Skipping live Flux TTS REST test.");
        }

        var client = ClientFactory.CreateFluxSpeakRESTClient(apiKey!);

        var response = await client.ToStream(
            new RestTextSource("Your appointment is confirmed for 3pm tomorrow."),
            new RestSpeakSchema
            {
                Model = "flux-alexis-en",
                Encoding = "mp3",
                BitRate = 48000,
            });

        using (new AssertionScope())
        {
            response.Should().NotBeNull();
            response.RequestId.Should().NotBeNullOrEmpty("the batch response must carry a request id");
            response.Stream.Should().NotBeNull();
            response.Stream!.Length.Should().BeGreaterThan(0, "the batch endpoint must return synthesized audio bytes");
        }
    }
}
