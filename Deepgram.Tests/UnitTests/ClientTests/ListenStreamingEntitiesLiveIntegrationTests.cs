// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Listen.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class ListenStreamingEntitiesLiveIntegrationTests
{
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

    [Test]
    public async Task Live_Streaming_DetectEntities_Should_Be_Accepted()
    {
        var apiKey = RequireApiKey("DEEPGRAM_API_KEY is not set. Skipping streaming detect_entities integration test.");

        var client = new ListenWebSocketClient(apiKey);
        ErrorResponse? error = null;
        await client.Subscribe(new EventHandler<ErrorResponse>((_, response) => error = response));

        try
        {
            await client.Connect(new LiveSchema { Model = "nova-3", DetectEntities = true });

            client.IsConnected().Should().BeTrue();
            await Task.Delay(TimeSpan.FromSeconds(1));
            error.Should().BeNull();
        }
        finally
        {
            await client.Stop();
        }
    }

    [Test]
    public async Task Live_Streaming_DiarizeModel_Should_Be_Accepted()
    {
        var apiKey = RequireApiKey("DEEPGRAM_API_KEY is not set. Skipping streaming diarize_model integration test.");

        var client = new ListenWebSocketClient(apiKey);
        ErrorResponse? error = null;
        await client.Subscribe(new EventHandler<ErrorResponse>((_, response) => error = response));

        try
        {
            await client.Connect(new LiveSchema { Model = "nova-3", DiarizeModel = "latest" });

            client.IsConnected().Should().BeTrue();
            await Task.Delay(TimeSpan.FromSeconds(1));
            error.Should().BeNull();
        }
        finally
        {
            await client.Stop();
        }
    }
}
