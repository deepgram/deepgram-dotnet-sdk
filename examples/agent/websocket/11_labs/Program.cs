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
                Deepgram.Library.Initialize(LogLevel.Default);
                // OR very chatty logging
                //Deepgram.Library.Initialize(LogLevel.Verbose); // LogLevel.Default, LogLevel.Debug, LogLevel.Verbose

                Console.WriteLine("\n\nPress any key to stop and exit...\n\n\n");

                // Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
                DeepgramWsClientOptions options = new DeepgramWsClientOptions(null, null, true);
                var agentClient = ClientFactory.CreateAgentWebSocketClient(apiKey: "", options: options);

                var lastAudioTime = DateTime.Now;
                var audioFileCount = 0;

                // Start the connection
                var settingsConfiguration = new SettingsSchema();
                settingsConfiguration.Agent.Think.Provider.Type = "open_ai";
                settingsConfiguration.Agent.Think.Provider.Model = "gpt-4o-mini";
                settingsConfiguration.Audio.Output.SampleRate = 24000;
                settingsConfiguration.Audio.Input.SampleRate = 24000;
                settingsConfiguration.Agent.Greeting = "Hello, how can I help you today?";
                settingsConfiguration.Agent.Listen.Provider.Type = "deepgram";
                settingsConfiguration.Agent.Listen.Provider.Model = "nova-3";
                settingsConfiguration.Agent.Listen.Provider.Keyterms = new List<string> { "Deepgram" };
                settingsConfiguration.Agent.Speak.Provider.Type = "eleven_labs";
                settingsConfiguration.Agent.Speak.Provider.ModelId = "eleven_multilingual_v1";
                settingsConfiguration.Agent.Speak.Endpoint = new Endpoint();
                settingsConfiguration.Agent.Speak.Endpoint.URL = "https://api.elevenlabs.io/v1/text-to-speech/pNInz6obpgDQGcFmaJgB/stream";
                settingsConfiguration.Agent.Speak.Endpoint.Headers = new Dictionary<string, string>
                {
                    { "xi-api-key", "" }, // Set your Eleven Labs API key here
                };

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

                subscribeResult = await agentClient.Subscribe(new EventHandler<AudioResponse>((sender, e) =>
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
                            int sampleRate = 24000;
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
                            wavHeader[37] = 0x61; // t
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
                if (!subscribeResult)
                {
                    Console.WriteLine("Failed to subscribe to AudioResponse event");
                    return;
                }

                subscribeResult = await agentClient.Subscribe(new EventHandler<AgentAudioDoneResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                if (!subscribeResult)
                {
                    Console.WriteLine("Failed to subscribe to AgentAudioDoneResponse event");
                    return;
                }

                subscribeResult = await agentClient.Subscribe(new EventHandler<AgentStartedSpeakingResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                if (!subscribeResult)
                {
                    Console.WriteLine("Failed to subscribe to AgentStartedSpeakingResponse event");
                    return;
                }

                subscribeResult = await agentClient.Subscribe(new EventHandler<AgentThinkingResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                if (!subscribeResult)
                {
                    Console.WriteLine("Failed to subscribe to AgentThinkingResponse event");
                    return;
                }

                subscribeResult = await agentClient.Subscribe(new EventHandler<ConversationTextResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                if (!subscribeResult)
                {
                    Console.WriteLine("Failed to subscribe to ConversationTextResponse event");
                    return;
                }

                subscribeResult = await agentClient.Subscribe(new EventHandler<FunctionCallRequestResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                if (!subscribeResult)
                {
                    Console.WriteLine("Failed to subscribe to FunctionCallRequestResponse event");
                    return;
                }

                subscribeResult = await agentClient.Subscribe(new EventHandler<UserStartedSpeakingResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                if (!subscribeResult)
                {
                    Console.WriteLine("Failed to subscribe to UserStartedSpeakingResponse event");
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

                subscribeResult = await agentClient.Subscribe(new EventHandler<InjectionRefusedResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                if (!subscribeResult)
                {
                    Console.WriteLine("Failed to subscribe to InjectionRefusedResponse event");
                    return;
                }

                subscribeResult = await agentClient.Subscribe(new EventHandler<PromptUpdatedResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received");
                }));
                if (!subscribeResult)
                {
                    Console.WriteLine("Failed to subscribe to PromptUpdatedResponse event");
                    return;
                }

                subscribeResult = await agentClient.Subscribe(new EventHandler<SpeakUpdatedResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received.");
                }));
                if (!subscribeResult)
                {
                    Console.WriteLine("Failed to subscribe to SpeakUpdatedResponse event");
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

                // Fetch and stream audio from URL
                string url = "https://dpgr.am/spacewalk.wav";
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(url);
                    var stream = await response.Content.ReadAsStreamAsync();
                    var buffer = new byte[8192];
                    int bytesRead;

                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        var chunk = new byte[bytesRead];
                        Array.Copy(buffer, chunk, bytesRead);
                        await agentClient.SendBinaryImmediately(chunk);
                    }
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
