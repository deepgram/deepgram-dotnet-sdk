// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Logger;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Agent.v2.WebSocket;
using System.Collections.Generic;
using System.Net.Http;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Initialize Library with default logging
                // Normal logging is "Info" level
                Deepgram.Library.Initialize(LogLevel.Debug);
                // OR very chatty logging
                //Deepgram.Library.Initialize(LogLevel.Verbose); // LogLevel.Default, LogLevel.Debug, LogLevel.Verbose

                Console.WriteLine("\n\nPress any key to stop and exit...\n\n\n");

                // Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
                DeepgramWsClientOptions options = new DeepgramWsClientOptions(null, null, true);
                var agentClient = ClientFactory.CreateAgentWebSocketClient(apiKey: "", options: options);

                // Start the connection
                var settingsConfiguration = new SettingsSchema();
                settingsConfiguration.Agent.Think.Provider.Type = "open_ai";
                settingsConfiguration.Agent.Think.Provider.Model = "gpt-4o-mini";
                settingsConfiguration.Audio.Output.SampleRate = 24000;
                settingsConfiguration.Audio.Output.Container = "wav";
                settingsConfiguration.Audio.Input.SampleRate = 24000;
                settingsConfiguration.Agent.Greeting = "Hello, how can I help you today?";
                settingsConfiguration.Agent.Listen.Provider.Type = "deepgram";
                settingsConfiguration.Agent.Listen.Provider.Model = "nova-3";
                settingsConfiguration.Agent.Listen.Provider.Keyterms = new List<string> { "Deepgram" };
                settingsConfiguration.Agent.Speak.Provider.Type = "deepgram";
                settingsConfiguration.Agent.Speak.Provider.Model = "aura-2-thalia-en";
                settingsConfiguration.Agent.Speak.Provider.ArbitraryKey = "test";

                Console.WriteLine("Using the following settings:");
                Console.WriteLine(settingsConfiguration);

                // To avoid issues with empty objects, Voice and Endpoint are instantiated as null. Construct them as needed.
                // settingsConfiguration.Agent.Speak.Provider.Voice = new CartesiaVoice();
                // settingsConfiguration.Agent.Speak.Provider.Voice.Id = "en-US-Wavenet-D";
                // settingsConfiguration.Agent.Speak.Endpoint = new Endpoint();
                // settingsConfiguration.Agent.Think.Endpoint = new Endpoint();

                bool bConnected = await agentClient.Connect(settingsConfiguration);
                if (!bConnected)
                {
                    Console.WriteLine("Failed to connect to Deepgram WebSocket server.");
                    return;
                }


                // Subscribe to the EventResponseReceived event
                var subscribeResult = await agentClient.Subscribe(new EventHandler<OpenResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e.Type} received");
                }));
                if (!subscribeResult)
                {
                    Console.WriteLine("Failed to subscribe to OpenResponse event");
                    return;
                }

                subscribeResult = await agentClient.Subscribe(new EventHandler<WelcomeResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                if (!subscribeResult)
                {
                    Console.WriteLine("Failed to subscribe to WelcomeResponse event");
                    return;
                }

                subscribeResult = await agentClient.Subscribe(new EventHandler<CloseResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                if (!subscribeResult)
                {
                    Console.WriteLine("Failed to subscribe to CloseResponse event");
                    return;
                }

                subscribeResult = await agentClient.Subscribe(new EventHandler<SettingsAppliedResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                if (!subscribeResult)
                {
                    Console.WriteLine("Failed to subscribe to SettingsAppliedResponse event");
                    return;
                }

                subscribeResult = await agentClient.Subscribe(new EventHandler<UnhandledResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                if (!subscribeResult)
                {
                    Console.WriteLine("Failed to subscribe to UnhandledResponse event");
                    return;
                }

                subscribeResult = await agentClient.Subscribe(new EventHandler<ErrorResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received. Error: {e.Message}");
                }));
                if (!subscribeResult)
                {
                    Console.WriteLine("Failed to subscribe to ErrorResponse event");
                    return;
                }

                // Wait for the user to press a key
                Console.ReadKey();

                // Stop the connection
                await agentClient.Stop();

                // Terminate Libraries
                Deepgram.Library.Terminate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
    }
}
