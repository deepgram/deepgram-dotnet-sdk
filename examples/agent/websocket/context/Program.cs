// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Agent.v2.WebSocket;

namespace SampleApp
{
    /// <summary>
    /// Starts a Voice Agent with a seeded conversation history, so the agent resumes
    /// an earlier conversation instead of starting cold.
    ///
    /// Needs no microphone: it only proves the settings are accepted.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Library.Initialize();

                // Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
                var agentClient = ClientFactory.CreateAgentWebSocketClient();

                var settingsApplied = new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

                await agentClient.Subscribe(new EventHandler<SettingsAppliedResponse>((sender, e) =>
                {
                    Console.WriteLine("----> SettingsApplied: the server accepted the seeded context");
                    settingsApplied.TrySetResult(true);
                }));

                await agentClient.Subscribe(new EventHandler<ErrorResponse>((sender, e) =>
                {
                    Console.Error.WriteLine($"----> Error {e.Code}: {e.Description}");
                }));

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

                // Seed the conversation that happened before this session. Messages are
                // chronological, oldest first, and may mix spoken turns with the function
                // calls the agent already made.
                settings.Agent.Context = new AgentContext
                {
                    Messages = new List<AgentHistoryMessage>
                    {
                        AgentHistoryMessage.FromConversation("user", "My name is Ada."),
                        AgentHistoryMessage.FromConversation("assistant", "Nice to meet you, Ada."),
                        AgentHistoryMessage.FromConversation("user", "What is the weather in London?"),
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
                        AgentHistoryMessage.FromConversation("assistant", "It is 11 degrees in London."),
                    },
                };

                // Because the agent already knows the name, a greeting can refer back to it.
                settings.Agent.Greeting = "Welcome back, Ada.";
                settings.Tags = new List<string> { "dotnet-agent-context-example" };

                Console.WriteLine("Settings being sent:");
                Console.WriteLine(settings.ToString());

                bool connected = await agentClient.Connect(settings);
                if (!connected)
                {
                    Console.WriteLine("Failed to connect to the Deepgram agent endpoint.");
                    return;
                }

                var applied = await Task.WhenAny(settingsApplied.Task, Task.Delay(20000))
                    == settingsApplied.Task;
                Console.WriteLine(applied
                    ? "Seeded conversation context accepted."
                    : "Timed out waiting for SettingsApplied.");

                await agentClient.Stop();
                Library.Terminate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
    }
}
