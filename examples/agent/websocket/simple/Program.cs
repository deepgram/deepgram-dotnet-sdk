// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Logger;
using Deepgram.Microphone;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Agent.v2.WebSocket;
using System.Collections.Generic;
using PortAudioSharp;

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

                // Initialize the microphone library
                Console.WriteLine("Initializing microphone library...");
                try
                {
                    Deepgram.Microphone.Library.Initialize();
                    Console.WriteLine("Microphone library initialized successfully.");

                    // Get default input device
                    int defaultDevice = PortAudio.DefaultInputDevice;
                    if (defaultDevice == PortAudio.NoDevice)
                    {
                        Console.WriteLine("Error: No default input device found.");
                        return;
                    }

                    var deviceInfo = PortAudio.GetDeviceInfo(defaultDevice);
                    Console.WriteLine($"Using default input device: {deviceInfo.name}");
                    Console.WriteLine($"Sample rate: {deviceInfo.defaultSampleRate}");
                    Console.WriteLine($"Channels: {deviceInfo.maxInputChannels}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error initializing microphone library: {ex.Message}");
                    return;
                }

                Console.WriteLine("\n\nPress any key to stop and exit...\n\n\n");

                // Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
                DeepgramWsClientOptions options = new DeepgramWsClientOptions(null, null, true);
                var agentClient = ClientFactory.CreateAgentWebSocketClient(apiKey: "", options: options);

                // current time
                var lastAudioTime = DateTime.Now;
                var audioFileCount = 0;

                // Subscribe to the EventResponseReceived event
                await agentClient.Subscribe(new EventHandler<OpenResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e.Type} received");
                }));
                await agentClient.Subscribe(new EventHandler<AudioResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e.Type} received");

                    // if the last audio response is more than 5 seconds ago, add a wav header
                    if (DateTime.Now.Subtract(lastAudioTime).TotalSeconds > 7)
                    {
                        audioFileCount = audioFileCount + 1; // increment the audio file count

                        // delete the file if it exists
                        if (File.Exists($"output_{audioFileCount}.wav"))
                        {
                            File.Delete($"output_{audioFileCount}.wav");
                        }

                        using (BinaryWriter writer = new BinaryWriter(File.Open($"output_{audioFileCount}.wav", FileMode.Append)))
                        {
                            Console.WriteLine("Adding WAV header to output.wav");
                            byte[] wavHeader = new byte[44];
                            int sampleRate = 48000;
                            short bitsPerSample = 16;
                            short channels = 1;
                            int byteRate = sampleRate * channels * (bitsPerSample / 8);
                            short blockAlign = (short)(channels * (bitsPerSample / 8));

                            wavHeader[0] = 0x52; // R
                            wavHeader[1] = 0x49; // I
                            wavHeader[2] = 0x46; // F
                            wavHeader[3] = 0x46; // F
                            wavHeader[4] = 0x00; // Placeholder for file size (will be updated later)
                            wavHeader[5] = 0x00; // Placeholder for file size (will be updated later)
                            wavHeader[6] = 0x00; // Placeholder for file size (will be updated later)
                            wavHeader[7] = 0x00; // Placeholder for file size (will be updated later)
                            wavHeader[8] = 0x57; // W
                            wavHeader[9] = 0x41; // A
                            wavHeader[10] = 0x56; // V
                            wavHeader[11] = 0x45; // E
                            wavHeader[12] = 0x66; // f
                            wavHeader[13] = 0x6D; // m
                            wavHeader[14] = 0x74; // t
                            wavHeader[15] = 0x20; // Space
                            wavHeader[16] = 0x10; // Subchunk1Size (16 for PCM)
                            wavHeader[17] = 0x00; // Subchunk1Size
                            wavHeader[18] = 0x00; // Subchunk1Size
                            wavHeader[19] = 0x00; // Subchunk1Size
                            wavHeader[20] = 0x01; // AudioFormat (1 for PCM)
                            wavHeader[21] = 0x00; // AudioFormat
                            wavHeader[22] = (byte)channels; // NumChannels
                            wavHeader[23] = 0x00; // NumChannels
                            wavHeader[24] = (byte)(sampleRate & 0xFF); // SampleRate
                            wavHeader[25] = (byte)((sampleRate >> 8) & 0xFF); // SampleRate
                            wavHeader[26] = (byte)((sampleRate >> 16) & 0xFF); // SampleRate
                            wavHeader[27] = (byte)((sampleRate >> 24) & 0xFF); // SampleRate
                            wavHeader[28] = (byte)(byteRate & 0xFF); // ByteRate
                            wavHeader[29] = (byte)((byteRate >> 8) & 0xFF); // ByteRate
                            wavHeader[30] = (byte)((byteRate >> 16) & 0xFF); // ByteRate
                            wavHeader[31] = (byte)((byteRate >> 24) & 0xFF); // ByteRate
                            wavHeader[32] = (byte)blockAlign; // BlockAlign
                            wavHeader[33] = 0x00; // BlockAlign
                            wavHeader[34] = (byte)bitsPerSample; // BitsPerSample
                            wavHeader[35] = 0x00; // BitsPerSample
                            wavHeader[36] = 0x64; // d
                            wavHeader[37] = 0x61; // a
                            wavHeader[38] = 0x74; // t
                            wavHeader[39] = 0x61; // a
                            wavHeader[40] = 0x00; // Placeholder for data chunk size (will be updated later)
                            wavHeader[41] = 0x00; // Placeholder for data chunk size (will be updated later)
                            wavHeader[42] = 0x00; // Placeholder for data chunk size (will be updated later)
                            wavHeader[43] = 0x00; // Placeholder for data chunk size (will be updated later)

                            writer.Write(wavHeader);
                        }
                    }

                    if (e.Stream != null)
                    {
                        using (BinaryWriter writer = new BinaryWriter(File.Open($"output_{audioFileCount}.wav", FileMode.Append)))
                        {
                            writer.Write(e.Stream.ToArray());
                        }
                    }

                    // record the last audio time
                    lastAudioTime = DateTime.Now;
                }));
                await agentClient.Subscribe(new EventHandler<AgentAudioDoneResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                await agentClient.Subscribe(new EventHandler<AgentStartedSpeakingResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                await agentClient.Subscribe(new EventHandler<AgentThinkingResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                await agentClient.Subscribe(new EventHandler<ConversationTextResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                await agentClient.Subscribe(new EventHandler<FunctionCallRequestResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                await agentClient.Subscribe(new EventHandler<UserStartedSpeakingResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                await agentClient.Subscribe(new EventHandler<WelcomeResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                await agentClient.Subscribe(new EventHandler<CloseResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                await agentClient.Subscribe(new EventHandler<SettingsAppliedResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                await agentClient.Subscribe(new EventHandler<InjectionRefusedResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                await agentClient.Subscribe(new EventHandler<PromptUpdatedResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                await agentClient.Subscribe(new EventHandler<SpeakUpdatedResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received.");
                }));
                await agentClient.Subscribe(new EventHandler<UnhandledResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                await agentClient.Subscribe(new EventHandler<ErrorResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received. Error: {e.Message}");
                }));

                // Start the connection
                var settingsConfiguration = new SettingsSchema();
                settingsConfiguration.Agent.Think.Provider.Type = "open_ai";
                settingsConfiguration.Agent.Think.Provider.Model = "gpt-4o-mini";
                settingsConfiguration.Audio.Output.SampleRate = 16000;
                settingsConfiguration.Audio.Output.Container = "wav";
                settingsConfiguration.Audio.Input.SampleRate = 44100;
                settingsConfiguration.Agent.Greeting = "Hello, how can I help you today?";
                settingsConfiguration.Agent.Listen.Provider.Type = "deepgram";
                settingsConfiguration.Agent.Listen.Provider.Model = "nova-3";
                settingsConfiguration.Agent.Listen.Provider.Keyterms = new List<string> { "Deepgram" };
                settingsConfiguration.Agent.Speak.Provider.Type = "deepgram";
                settingsConfiguration.Agent.Speak.Provider.Model = "aura-2-thalia-en";

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

                // Microphone streaming
                Console.WriteLine("Starting microphone...");
                Microphone microphone = null;
                try
                {
                    microphone = new Microphone(agentClient.SendBinary);
                    microphone.Start();
                    Console.WriteLine("Microphone started successfully. Waiting for audio input...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error starting microphone: {ex.Message}");
                    return;
                }

                // Wait for the user to press a key
                Console.ReadKey();

                // Stop the microphone
                if (microphone != null)
                {
                    microphone.Stop();
                }

                // Stop the connection
                await agentClient.Stop();

                // Terminate Libraries
                Deepgram.Microphone.Library.Terminate();
                Deepgram.Library.Terminate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
    }
}
