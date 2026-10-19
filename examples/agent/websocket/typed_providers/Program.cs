// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Agent.v2.WebSocket;

namespace SampleApp
{
    /// <summary>
    /// Builds Voice Agent Settings with typed providers instead of untyped dynamics,
    /// connects, and waits for the server to acknowledge with SettingsApplied.
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

                await agentClient.Subscribe(new EventHandler<WelcomeResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> Welcome, request {e.RequestId}");
                }));

                await agentClient.Subscribe(new EventHandler<SettingsAppliedResponse>((sender, e) =>
                {
                    Console.WriteLine("----> SettingsApplied: the server accepted the typed providers");
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

                settings.Agent.Greeting = "Hello! How can I help you today?";

                // Speech-to-text. DeepgramListenProviderV2 selects Flux STT inside the agent and
                // carries the Flux turn-taking controls; DeepgramListenProviderV1 selects Nova.
                settings.Agent.Listen = new Listen
                {
                    Provider = new DeepgramListenProviderV2
                    {
                        Model = "flux-general-en",
                        EotThreshold = 0.7,
                        EotTimeoutMs = 5000,
                    },
                };

                // Reasoning. Swap in AnthropicThinkProvider, GoogleThinkProvider,
                // GroqThinkProvider or AwsBedrockThinkProvider as needed.
                settings.Agent.Think = new Think
                {
                    Provider = new OpenAiThinkProvider
                    {
                        Model = "gpt-4o-mini",
                        Temperature = 0.7,
                    },
                };

                // Text-to-speech. Version "v2" selects Flux TTS; "v1" selects Aura. Other options
                // are CartesiaSpeakProvider, ElevenLabsSpeakProvider, OpenAiSpeakProvider and
                // AwsPollySpeakProvider.
                settings.Agent.Speak = new Speak
                {
                    Provider = new DeepgramSpeakProvider
                    {
                        Version = "v2",
                        Model = "flux-kit-en",
                    },
                };

                settings.Tags = new List<string> { "dotnet-typed-providers-example" };

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
                    ? "Typed providers accepted."
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
