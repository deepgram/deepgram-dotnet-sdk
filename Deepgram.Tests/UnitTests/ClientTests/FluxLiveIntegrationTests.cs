// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Flux.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

/// <summary>
/// One end-to-end test against the real Flux endpoint. Skipped unless the DEEPGRAM_API_KEY
/// environment variable is set, so normal CI runs never touch the network.
/// </summary>
public class FluxLiveIntegrationTests
{
    private const int ChunkBytes = 2560; // 80ms of linear16 @ 16000 Hz mono
    private const int WavHeaderBytes = 44;

    [Test]
    public async Task Live_Flux_EndToEnd_Connect_Stream_TurnInfo_CloseStream()
    {
        // Read the startup snapshot: the authentication tests clear DEEPGRAM_API_KEY
        // process-wide while exercising credential fallbacks (see GlobalTestEnvironment).
        var apiKey = GlobalTestEnvironment.DeepgramApiKeyAtStartup
            ?? Environment.GetEnvironmentVariable("DEEPGRAM_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Assert.Ignore("DEEPGRAM_API_KEY is not set. Skipping live Flux integration test.");
        }

        var audioPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "Flux", "preamble-16k.wav");
        if (!File.Exists(audioPath))
        {
            Assert.Ignore("Fixtures/Flux/preamble-16k.wav not present. Skipping live Flux integration test.");
        }

        var client = new FluxWebSocketClient(apiKey!);

        var connectedReceived = new TaskCompletionSource<ConnectedResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        var endOfTurnReceived = new TaskCompletionSource<TurnInfoResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        var closeReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var turnInfoCount = 0;
        var closeCount = 0;
        var errors = new List<ErrorResponse>();

        await client.Subscribe(new EventHandler<ConnectedResponse>((s, e) => connectedReceived.TrySetResult(e)));
        await client.Subscribe(new EventHandler<TurnInfoResponse>((s, e) =>
        {
            Interlocked.Increment(ref turnInfoCount);
            if (e.EventType == TurnEvent.EndOfTurn)
            {
                endOfTurnReceived.TrySetResult(e);
            }
        }));
        await client.Subscribe(new EventHandler<ErrorResponse>((s, e) => { lock (errors) { errors.Add(e); } }));
        await client.Subscribe(new EventHandler<CloseResponse>((s, e) =>
        {
            Interlocked.Increment(ref closeCount);
            closeReceived.TrySetResult(true);
        }));

        var connected = await client.Connect(new FluxSchema
        {
            Model = "flux-general-en",
            Encoding = "linear16",
            SampleRate = 16000,
        });
        connected.Should().BeTrue("the client must connect to the live Flux endpoint");

        // Stream the raw PCM (skip the 44-byte WAV header) in ~80ms chunks, paced like a
        // real-time capture device.
        var wav = File.ReadAllBytes(audioPath);
        for (var offset = WavHeaderBytes; offset < wav.Length; offset += ChunkBytes)
        {
            var length = Math.Min(ChunkBytes, wav.Length - offset);
            var chunk = new byte[length];
            Array.Copy(wav, offset, chunk, 0, length);
            client.Send(chunk);
            await Task.Delay(80);
        }

        // Trailing silence so Flux detects the end of speech: turns end on detected silence
        // (or eot_timeout_ms), not on CloseStream.
        var silence = new byte[ChunkBytes];
        for (var i = 0; i < 3000 / 80; i++)
        {
            client.Send(silence);
            await Task.Delay(80);
        }

        // Clean shutdown: CloseStream, then the server flushes the final results and closes.
        await client.Stop();

        (await Task.WhenAny(connectedReceived.Task, Task.Delay(5000))).Should().Be(connectedReceived.Task,
            "the server must send a Connected message");
        (await Task.WhenAny(endOfTurnReceived.Task, Task.Delay(10000))).Should().Be(endOfTurnReceived.Task,
            "the streamed speech must produce at least one EndOfTurn");
        (await Task.WhenAny(closeReceived.Task, Task.Delay(10000))).Should().Be(closeReceived.Task,
            "a Close event must be raised after CloseStream");

        using (new AssertionScope())
        {
            var connectedMsg = await connectedReceived.Task;
            connectedMsg.RequestId.Should().NotBeNullOrEmpty();
            connectedMsg.SequenceId.Should().Be(0);

            var endOfTurn = await endOfTurnReceived.Task;
            endOfTurn.Transcript.Should().NotBeNullOrEmpty();
            endOfTurn.Words.Should().NotBeEmpty();

            turnInfoCount.Should().BeGreaterThan(0);
            errors.Should().BeEmpty("no fatal errors are expected during a clean session");
            closeCount.Should().Be(1, "the Close event must be delivered exactly once per connection");
        }
    }
}
