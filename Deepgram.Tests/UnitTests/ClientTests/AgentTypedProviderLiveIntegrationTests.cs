// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Agent.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class AgentTypedProviderLiveIntegrationTests
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

    private static async Task<(bool settingsApplied, List<ErrorResponse> errors)> ConnectWith(
        string apiKey, SettingsSchema settings)
    {
        var client = new AgentWebSocketClient(apiKey);
        var applied = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var errors = new List<ErrorResponse>();

        await client.Subscribe(new EventHandler<SettingsAppliedResponse>((_, _) => applied.TrySetResult(true)));
        await client.Subscribe(new EventHandler<ErrorResponse>((_, e) => { lock (errors) { errors.Add(e); } }));

        try
        {
            var connected = await client.Connect(settings);
            connected.Should().BeTrue("the agent endpoint must accept the connection");

            var appliedInTime = await Task.WhenAny(applied.Task, Task.Delay(20000)) == applied.Task;
            return (appliedInTime, errors);
        }
        finally
        {
            await client.Stop();
        }
    }

    private static SettingsSchema BaseSettings()
    {
        var settings = new SettingsSchema();
        settings.Audio.Input.Encoding = "linear16";
        settings.Audio.Input.SampleRate = 24000;
        settings.Audio.Output.Encoding = "linear16";
        settings.Audio.Output.SampleRate = 24000;
        settings.Audio.Output.Container = "none";
        settings.Tags = new List<string> { "dotnet-typed-provider-test" };
        return settings;
    }

    [Test]
    public async Task Live_Agent_Should_Accept_Typed_Nova_Listen_Provider()
    {
        var apiKey = RequireApiKey("DEEPGRAM_API_KEY is not set. Skipping typed listen provider integration test.");

        var settings = BaseSettings();
        settings.Agent.Listen = new Listen
        {
            Provider = new DeepgramListenProviderV1 { Model = "nova-3", Keyterms = new List<string> { "Deepgram" } },
        };
        settings.Agent.Think = new Think { Provider = new OpenAiThinkProvider { Model = "gpt-4o-mini" } };
        settings.Agent.Speak = new Speak { Provider = new DeepgramSpeakProvider { Model = "aura-2-thalia-en" } };

        var (settingsApplied, errors) = await ConnectWith(apiKey, settings);

        using (new AssertionScope())
        {
            settingsApplied.Should().BeTrue("the server must apply typed Nova listen settings");
            errors.Should().BeEmpty();
        }
    }

    [Test]
    public async Task Live_Agent_Should_Accept_Typed_Flux_Listen_Provider()
    {
        // DeepgramListenProviderV2 is how Flux STT is selected inside the Voice Agent.
        // Before typed providers this could only be expressed through an untyped dynamic.
        var apiKey = RequireApiKey("DEEPGRAM_API_KEY is not set. Skipping typed Flux STT listen provider integration test.");

        var settings = BaseSettings();
        settings.Agent.Listen = new Listen
        {
            Provider = new DeepgramListenProviderV2
            {
                Model = "flux-general-en",
                EotThreshold = 0.7,
                EotTimeoutMs = 5000,
            },
        };
        settings.Agent.Think = new Think { Provider = new OpenAiThinkProvider { Model = "gpt-4o-mini" } };
        settings.Agent.Speak = new Speak { Provider = new DeepgramSpeakProvider { Version = "v2", Model = "flux-kit-en" } };

        var (settingsApplied, errors) = await ConnectWith(apiKey, settings);

        using (new AssertionScope())
        {
            settingsApplied.Should().BeTrue("the server must apply a typed Flux STT listen provider");
            errors.Should().BeEmpty();
        }
    }
}
