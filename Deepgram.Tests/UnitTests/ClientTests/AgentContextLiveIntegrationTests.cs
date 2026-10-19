// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Agent.v2.WebSocket;

namespace Deepgram.Tests.UnitTests.ClientTests;

public class AgentContextLiveIntegrationTests
{
    private static SettingsSchema BaseSettings()
    {
        var settings = new SettingsSchema();
        settings.Audio.Input.Encoding = "linear16";
        settings.Audio.Input.SampleRate = 24000;
        settings.Audio.Output.Encoding = "linear16";
        settings.Audio.Output.SampleRate = 24000;
        settings.Audio.Output.Container = "none";
        settings.Agent.Listen.Provider.Type = "deepgram";
        settings.Agent.Listen.Provider.Model = "nova-3";
        settings.Agent.Think.Provider.Type = "open_ai";
        settings.Agent.Think.Provider.Model = "gpt-4o-mini";
        settings.Agent.Speak.Provider.Type = "deepgram";
        settings.Agent.Speak.Provider.Model = "aura-2-thalia-en";
        settings.Tags = new List<string> { "dotnet-agent-context-test" };
        return settings;
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

    [Test]
    public async Task Live_Agent_Should_Accept_Seeded_Conversation_Context()
    {
        var apiKey = GlobalTestEnvironment.DeepgramApiKeyAtStartup
            ?? Environment.GetEnvironmentVariable("DEEPGRAM_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Assert.Ignore("DEEPGRAM_API_KEY is not set. Skipping agent context integration test.");
        }

        var settings = BaseSettings();
        settings.Agent.Context = new AgentContext
        {
            Messages = new List<AgentHistoryMessage>
            {
                AgentHistoryMessage.FromConversation("user", "My name is Ada."),
                AgentHistoryMessage.FromConversation("assistant", "Nice to meet you, Ada."),
                AgentHistoryMessage.FromFunctionCalls(new[]
                {
                    new HistoryFunctionCall
                    {
                        Id = "call-1",
                        Name = "get_weather",
                        ClientSide = true,
                        Arguments = "{\"city\":\"London\"}",
                        Response = "{\"temp_c\":11}",
                    },
                }),
            },
        };

        var (settingsApplied, errors) = await ConnectWith(apiKey!, settings);

        using (new AssertionScope())
        {
            settingsApplied.Should().BeTrue("the server must apply settings carrying seeded context");
            errors.Should().BeEmpty();
        }
    }
}
