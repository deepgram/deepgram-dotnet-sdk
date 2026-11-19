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

    [Test]
    public async Task Live_Agent_Should_Accept_Typed_Phonellm_Think_Provider_Options()
    {
        var apiKey = RequireApiKey("DEEPGRAM_API_KEY is not set. Skipping Deepgram think provider integration test.");

        var settings = BaseSettings();
        settings.MipOptOut = true;
        settings.Agent.Listen = new Listen
        {
            Provider = new DeepgramListenProviderV1 { Model = "nova-3" },
        };
        settings.Agent.Think = new Think
        {
            Provider = new DeepgramThinkProvider
            {
                Model = "phonellm-mini-alpha-2-fp8",
                Temperature = 0.2,
                ReasoningMode = "none",
            },
        };
        settings.Agent.Speak = new Speak { Provider = new DeepgramSpeakProvider { Model = "aura-2-thalia-en" } };

        var (settingsApplied, errors) = await ConnectWith(apiKey, settings);

        using (new AssertionScope())
        {
            settingsApplied.Should().BeTrue("the server must apply the typed phonellm provider options");
            errors.Should().BeEmpty();
        }
    }

    [Test]
    public async Task Live_Agent_Should_Accept_Typed_ThinkFast_VoicePromptOptOut()
    {
        var apiKey = RequireApiKey("DEEPGRAM_API_KEY is not set. Skipping Think-Fast voice prompt integration test.");

        var settings = BaseSettings();
        settings.MipOptOut = false;
        settings.Agent.Listen = new Listen
        {
            Provider = new DeepgramListenProviderV1 { Model = "nova-3" },
        };
        settings.Agent.Think = new Think
        {
            Provider = new DeepgramThinkProvider
            {
                Model = "think-fast",
                VoicePromptOptOut = true,
            },
        };
        settings.Agent.Speak = new Speak { Provider = new DeepgramSpeakProvider { Model = "aura-2-thalia-en" } };

        var (settingsApplied, errors) = await ConnectWith(apiKey, settings);

        using (new AssertionScope())
        {
            settingsApplied.Should().BeTrue("the server must apply think-fast voice prompt opt-out");
            errors.Should().BeEmpty();
        }
    }

    [Test]
    public async Task Live_Agent_Should_Accept_ThinkLive_And_Function_Fillers()
    {
        var apiKey = RequireApiKey("DEEPGRAM_API_KEY is not set. Skipping Think Live integration test.");

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
        settings.Agent.Think = new Think
        {
            Provider = new OpenAiThinkProvider { Model = "gpt-5.4-mini" },
            Functions = new List<Function>
            {
                new()
                {
                    Name = "lookup_order",
                    Description = "Look up an order by identifier.",
                    Parameters = new Dictionary<string, object>
                    {
                        ["type"] = "object",
                        ["properties"] = new Dictionary<string, object>
                        {
                            ["order_id"] = new Dictionary<string, object> { ["type"] = "string" },
                        },
                        ["required"] = new List<string> { "order_id" },
                    },
                    DeferUntilEot = true,
                    InitialAgentFiller = true,
                    AgentFillerMaxCount = 2,
                    AgentFillerIntervalMs = 3000,
                },
            },
        };
        settings.Agent.ThinkLive = new ThinkLiveSettings
        {
            Provider = new DeepgramThinkLiveProvider
            {
                AgentBackchannelSensitivity = 0.4,
                PresenceCheckMaxCount = 3,
                PresenceCheckIntervalMs = 5000,
                FrustrationMaxCount = 2,
                RestrictedTerms = new List<string> { "forbidden phrase" },
            },
        };
        settings.Agent.Speak = new Speak
        {
            Provider = new DeepgramSpeakProvider { Version = "v2", Model = "flux-kit-en" },
        };

        var (settingsApplied, errors) = await ConnectWith(apiKey, settings);

        using (new AssertionScope())
        {
            settingsApplied.Should().BeTrue("the server must apply typed Think Live settings");
            errors.Should().BeEmpty();
        }
    }

    [Test]
    public async Task Live_Agent_Should_Acknowledge_UpdateThinkLive()
    {
        var apiKey = RequireApiKey("DEEPGRAM_API_KEY is not set. Skipping Think Live update integration test.");
        var client = ClientFactory.CreateAgentProtocolClient(apiKey);
        var settingsApplied = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thinkLiveUpdated = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var errors = new List<ErrorResponse>();

        await client.Subscribe(new EventHandler<SettingsAppliedResponse>((_, _) => settingsApplied.TrySetResult(true)));
        await client.Subscribe(new EventHandler<ThinkLiveUpdatedResponse>((_, _) => thinkLiveUpdated.TrySetResult(true)));
        await client.Subscribe(new EventHandler<ErrorResponse>((_, e) => { lock (errors) { errors.Add(e); } }));

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
        settings.Agent.Think = new Think { Provider = new OpenAiThinkProvider { Model = "gpt-5.4-mini" } };
        settings.Agent.ThinkLive = new ThinkLiveSettings
        {
            Provider = new DeepgramThinkLiveProvider { FrustrationMaxCount = 2 },
        };
        settings.Agent.Speak = new Speak
        {
            Provider = new DeepgramSpeakProvider { Version = "v2", Model = "flux-kit-en" },
        };

        try
        {
            (await client.Connect(settings)).Should().BeTrue("the agent must connect before Think Live is updated");
            (await Task.WhenAny(settingsApplied.Task, Task.Delay(20000))).Should().Be(settingsApplied.Task,
                "the server must apply the initial Think Live settings");

            await client.SendUpdateThinkLive(new AgentUpdateThinkLiveSchema
            {
                ThinkLive = new ThinkLiveSettings
                {
                    Provider = new DeepgramThinkLiveProvider
                    {
                        AgentBackchannelSensitivity = 0.4,
                        PresenceCheckMaxCount = 2,
                        PresenceCheckIntervalMs = 5000,
                        FrustrationMaxCount = 3,
                        RestrictedTerms = new List<string> { "human", "transfer" },
                    },
                },
            });

            (await Task.WhenAny(thinkLiveUpdated.Task, Task.Delay(10000))).Should().Be(thinkLiveUpdated.Task,
                "the server must acknowledge UpdateThink_Live with ThinkLiveUpdated");
            errors.Should().BeEmpty();
        }
        finally
        {
            await client.Stop();
        }
    }
}
