// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Listen.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class ListenStreamingEntitiesLiveIntegrationTests
{
    private const int WavHeaderBytes = 44;
    private const int ChunkBytes = 2560; // ~80ms of 16kHz 16-bit mono PCM

    private static string RequireApiKey(string skipReason)
    {
        var apiKey = GlobalTestEnvironment.DeepgramApiKeyAtStartup
            ?? Environment.GetEnvironmentVariable("DEEPGRAM_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Assert.Ignore(skipReason);
        }

        return apiKey!;
    }

    private static string RequireAudioFixture()
    {
        var audioPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "Flux", "preamble-16k.wav");
        if (!File.Exists(audioPath))
        {
            Assert.Ignore("Fixtures/Flux/preamble-16k.wav not present. Skipping streaming entities integration test.");
        }

        return audioPath;
    }

    private static async Task StreamWav(ListenWebSocketClient client, string audioPath)
    {
        var wav = File.ReadAllBytes(audioPath);
        for (var offset = WavHeaderBytes; offset < wav.Length; offset += ChunkBytes)
        {
            var length = Math.Min(ChunkBytes, wav.Length - offset);
            var chunk = new byte[length];
            Array.Copy(wav, offset, chunk, 0, length);
            client.Send(chunk);
            await Task.Delay(80);
        }
    }

    [Test]
    public async Task Live_Streaming_DetectEntities_Should_Return_Entities_On_Final_Results()
    {
        // The preamble fixture says "the United States", which the entity model tags.
        var apiKey = RequireApiKey("DEEPGRAM_API_KEY is not set. Skipping streaming detect_entities integration test.");
        var audioPath = RequireAudioFixture();

        var client = new ListenWebSocketClient(apiKey);
        var errors = new List<ErrorResponse>();
        var entitiesSeen = new List<Entity>();
        var finalWithEntities = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        await client.Subscribe(new EventHandler<ErrorResponse>((_, e) => { lock (errors) { errors.Add(e); } }));
        await client.Subscribe(new EventHandler<ResultResponse>((_, e) =>
        {
            if (e.Entities is null || e.Entities.Count == 0)
            {
                return;
            }

            lock (entitiesSeen)
            {
                entitiesSeen.AddRange(e.Entities);
            }

            // Entities are documented as final-results-only; prove that on the live wire.
            e.IsFinal.Should().BeTrue("entities must only appear on final results");
            finalWithEntities.TrySetResult(true);
        }));

        try
        {
            var connected = await client.Connect(new LiveSchema
            {
                Model = "nova-3",
                Encoding = "linear16",
                SampleRate = 16000,
                DetectEntities = true,
                InterimResults = true,
            });
            connected.Should().BeTrue("the client must connect to the live streaming endpoint");

            await StreamWav(client, audioPath);

            var sawEntities = await Task.WhenAny(finalWithEntities.Task, Task.Delay(20000)) == finalWithEntities.Task;

            using (new AssertionScope())
            {
                errors.Should().BeEmpty();
                sawEntities.Should().BeTrue("detect_entities must surface at least one entity for the preamble audio");

                List<Entity> snapshot;
                lock (entitiesSeen)
                {
                    snapshot = entitiesSeen.ToList();
                }

                snapshot.Should().NotBeEmpty();
                snapshot.Should().AllSatisfy(entity =>
                {
                    entity.Label.Should().NotBeNullOrWhiteSpace("every entity must carry a label");
                    entity.Value.Should().NotBeNullOrWhiteSpace("every entity must carry a value");
                });
            }
        }
        finally
        {
            await client.Stop();
        }
    }

    [Test]
    public async Task Live_Streaming_DiarizeModel_Should_Be_Accepted_And_Assign_Speakers()
    {
        var apiKey = RequireApiKey("DEEPGRAM_API_KEY is not set. Skipping streaming diarize_model integration test.");
        var audioPath = RequireAudioFixture();

        var client = new ListenWebSocketClient(apiKey);
        var errors = new List<ErrorResponse>();
        var speakerAssigned = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        await client.Subscribe(new EventHandler<ErrorResponse>((_, e) => { lock (errors) { errors.Add(e); } }));
        await client.Subscribe(new EventHandler<ResultResponse>((_, e) =>
        {
            var words = e.Channel?.Alternatives?.FirstOrDefault()?.Words;
            if (words is not null && words.Any(w => w.Speaker is not null))
            {
                // diarize_model enables diarization on its own, with no Diarize flag set.
                speakerAssigned.TrySetResult(true);
            }
        }));

        try
        {
            var connected = await client.Connect(new LiveSchema
            {
                Model = "nova-3",
                Encoding = "linear16",
                SampleRate = 16000,
                DiarizeModel = "latest",
            });
            connected.Should().BeTrue("the client must connect to the live streaming endpoint");

            await StreamWav(client, audioPath);

            var sawSpeaker = await Task.WhenAny(speakerAssigned.Task, Task.Delay(20000)) == speakerAssigned.Task;

            using (new AssertionScope())
            {
                errors.Should().BeEmpty();
                sawSpeaker.Should().BeTrue("diarize_model alone must enable diarization and assign speakers");
            }
        }
        finally
        {
            await client.Stop();
        }
    }
}
