// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Speak.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class SpeakStreamingSpeedLiveIntegrationTests
{
    [Test]
    public async Task Live_Speak_Streaming_Speed_Should_Be_Accepted_And_Synthesize_Audio()
    {
        var apiKey = GlobalTestEnvironment.DeepgramApiKeyAtStartup
            ?? Environment.GetEnvironmentVariable("DEEPGRAM_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Assert.Ignore("DEEPGRAM_API_KEY is not set. Skipping Aura TTS streaming speed integration test.");
        }

        var client = new SpeakWebSocketClient(apiKey);
        var audioBytes = 0L;
        var flushed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var errors = new List<ErrorResponse>();

        await client.Subscribe(new EventHandler<AudioResponse>((_, e) =>
        {
            if (e.Stream is not null)
            {
                Interlocked.Add(ref audioBytes, e.Stream.Length);
            }
        }));
        await client.Subscribe(new EventHandler<FlushedResponse>((_, _) => flushed.TrySetResult(true)));
        await client.Subscribe(new EventHandler<ErrorResponse>((_, e) => { lock (errors) { errors.Add(e); } }));

        var connected = await client.Connect(new SpeakSchema
        {
            Model = "aura-2-thalia-en",
            Encoding = "linear16",
            SampleRate = 24000,
            Speed = 1.2,
        });
        connected.Should().BeTrue("the live Speak endpoint must accept the speed parameter");

        client.SpeakWithText("Deepgram streaming speed control.");
        client.Flush();

        var flushedInTime = await Task.WhenAny(flushed.Task, Task.Delay(30000)) == flushed.Task;

        await client.Stop();

        using (new AssertionScope())
        {
            flushedInTime.Should().BeTrue("the server must acknowledge the flushed text");
            audioBytes.Should().BeGreaterThan(0, "a speed-adjusted request must still synthesize audio");
            errors.Should().BeEmpty("speed within the 0.7 to 1.5 range must not raise an error");
        }
    }
}
