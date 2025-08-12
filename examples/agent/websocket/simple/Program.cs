// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Logger;
using Deepgram.Microphone;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Agent.v2.WebSocket;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

                // Initialize conversation
                Console.WriteLine("🎤 Ready for conversation! Speak into your microphone...");

                // Subscribe to the EventResponseReceived event
                await agentClient.Subscribe(new EventHandler<OpenResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e.Type} received");
                }));
                                                await agentClient.Subscribe(new EventHandler<AudioResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e.Type} received");

                    if (e.Stream != null && e.Stream.Length > 0)
                    {
                        var audioData = e.Stream.ToArray();
                        Console.WriteLine($"🔊 Queueing {audioData.Length} bytes of agent speech for playback");

                        // Play audio through speakers
                        PlayAudioThroughSpeakers(audioData);
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Received empty audio stream");
                    }
                }));
                                await agentClient.Subscribe(new EventHandler<AgentAudioDoneResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received - Agent finished speaking 🎤");
                }));
                await agentClient.Subscribe(new EventHandler<AgentStartedSpeakingResponse>((sender, e) =>
                {
                    Console.WriteLine($"----> {e} received - Agent is speaking 🗣️");
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
                    Console.WriteLine($"----> {e} received - User is speaking 👤");
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

                // Configure audio settings - keep your input format, fix output
                settingsConfiguration.Audio.Input.Encoding = "linear16";
                settingsConfiguration.Audio.Input.SampleRate = 24000;
                settingsConfiguration.Audio.Output.Encoding = "linear16";  // Use linear16 for output too
                settingsConfiguration.Audio.Output.SampleRate = 24000;
                settingsConfiguration.Audio.Output.Container = "none";

                settingsConfiguration.Agent.Greeting = "Hello! How can I help you today?";
                settingsConfiguration.Agent.Listen.Provider.Type = "deepgram";
                settingsConfiguration.Agent.Listen.Provider.Model = "nova-3";
                settingsConfiguration.Agent.Listen.Provider.Keyterms = new List<string> { "Deepgram" };
                settingsConfiguration.Agent.Speak.Provider.Type = "deepgram";
                settingsConfiguration.Agent.Speak.Provider.Model = "aura-2-thalia-en";

                // Add tags to test the new tagging capabilities
                settingsConfiguration.Tags = new List<string> { "dotnet-example","live-agent-test" };

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

                // Microphone streaming with debugging
                Console.WriteLine("Starting microphone...");
                Microphone microphone = null;
                int audioDataCounter = 0;

                                try
                {
                                    // Create microphone with proper sample rate and debugging
                                microphone = new Microphone(
                    push_callback: (audioData, length) =>
                    {
                        audioDataCounter++;
                        Console.WriteLine($"[MIC] Captured audio chunk #{audioDataCounter}: {length} bytes");

                        // Create array with actual length
                        byte[] actualData = new byte[length];
                        Array.Copy(audioData, actualData, length);

                        // Send to agent
                        agentClient.SendBinary(actualData);
                    },
                    rate: 24000,        // Match the agent's expected input rate (24kHz)
                    chunkSize: 8192,    // Standard chunk size
                    channels: 1,        // Mono
                    device_index: PortAudio.DefaultInputDevice,
                    format: SampleFormat.Int16
                );

                    microphone.Start();
                    Console.WriteLine("Microphone started successfully. Speak into your microphone now!");
                    Console.WriteLine("You should see '[MIC] Captured audio chunk' messages when speaking...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error starting microphone: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
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

                // Stop and dispose PortAudio output stream
                if (_outputStream != null)
                {
                    _outputStream.Stop();
                    _outputStream.Dispose();
                    _outputStream = null;
                }

                // Terminate Libraries
                Deepgram.Microphone.Library.Terminate();
                Deepgram.Library.Terminate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

                // Audio playback queue and position tracking
        private static Queue<byte[]> audioQueue = new Queue<byte[]>();
        private static byte[]? currentAudioBuffer = null;
        private static int audioPosition = 0;
        private static readonly object audioLock = new object();

        // Preallocated buffer for OutputCallback to avoid per-callback allocations
        private static readonly byte[] PreallocatedOutputBuffer = new byte[8192]; // Max buffer size

        /// <summary>
        /// Plays audio data through the system's default output device (speakers)
        /// </summary>
        /// <param name="audioData">PCM audio data to play</param>
        static void PlayAudioThroughSpeakers(byte[] audioData)
        {
            try
            {
                lock (audioLock)
                {
                    // Add to queue for playback
                    audioQueue.Enqueue(audioData);
                }

                // Start playback stream if not already running
                StartAudioPlayback();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error queuing audio: {ex.Message}");
            }
        }

        private static PortAudioSharp.Stream? _outputStream = null;

        private static void StartAudioPlayback()
        {
            if (_outputStream != null)
                return; // Already playing

            try
            {
                // Get default output device
                int outputDevice = PortAudio.DefaultOutputDevice;
                if (outputDevice == PortAudio.NoDevice)
                {
                    Console.WriteLine("⚠️ No default output device found for audio playback");
                    return;
                }

                var deviceInfo = PortAudio.GetDeviceInfo(outputDevice);
                Console.WriteLine($"🔊 Playing through: {deviceInfo.name}");

                // Set up output stream parameters
                var outputParams = new PortAudioSharp.StreamParameters
                {
                    device = outputDevice,
                    channelCount = 1, // mono
                    sampleFormat = PortAudioSharp.SampleFormat.Int16,
                    suggestedLatency = deviceInfo.defaultLowOutputLatency,
                    hostApiSpecificStreamInfo = IntPtr.Zero
                };

                // Create and start the output stream
                _outputStream = new PortAudioSharp.Stream(
                    inParams: null,
                    outParams: outputParams,
                    sampleRate: 24000, // Match agent output (24kHz)
                    framesPerBuffer: 512,
                    streamFlags: PortAudioSharp.StreamFlags.ClipOff,
                    callback: OutputCallback,
                    userData: IntPtr.Zero
                );

                _outputStream.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error starting audio playback: {ex.Message}");
                _outputStream = null;
            }
        }

                private static PortAudioSharp.StreamCallbackResult OutputCallback(nint input, nint output, uint frameCount, ref PortAudioSharp.StreamCallbackTimeInfo timeInfo, PortAudioSharp.StreamCallbackFlags statusFlags, nint userDataPtr)
        {
            lock (audioLock)
            {
                int bytesToWrite = (int)(frameCount * sizeof(Int16)); // 16-bit samples

                int bytesWritten = 0;
                while (bytesWritten < bytesToWrite)
                {
                    // Get next buffer if current one is exhausted
                    if (currentAudioBuffer == null || audioPosition >= currentAudioBuffer.Length)
                    {
                        if (audioQueue.Count > 0)
                        {
                            currentAudioBuffer = audioQueue.Dequeue();
                            audioPosition = 0;
                            // Removed Console.WriteLine to avoid blocking real-time thread
                        }
                        else
                        {
                            // No more audio, fill remaining output with silence
                            int remainingBytes = bytesToWrite - bytesWritten;

                            // Clear the preallocated buffer for silence
                            Array.Clear(PreallocatedOutputBuffer, 0, remainingBytes);
                            Marshal.Copy(PreallocatedOutputBuffer, 0, output + bytesWritten, remainingBytes);

                            return PortAudioSharp.StreamCallbackResult.Continue;
                        }
                    }

                    // Copy data directly from current buffer to output
                    int remainingInBuffer = currentAudioBuffer.Length - audioPosition;
                    int remainingToWrite = bytesToWrite - bytesWritten;
                    int bytesToCopy = Math.Min(remainingInBuffer, remainingToWrite);

                    // Direct memory copy to output buffer
                    Marshal.Copy(currentAudioBuffer, audioPosition, output + bytesWritten, bytesToCopy);
                    audioPosition += bytesToCopy;
                    bytesWritten += bytesToCopy;
                }
            }

            return PortAudioSharp.StreamCallbackResult.Continue;
        }
    }
}
