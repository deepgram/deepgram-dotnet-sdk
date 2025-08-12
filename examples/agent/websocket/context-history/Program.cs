// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Logger;
using Deepgram.Microphone;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Agent.v2.WebSocket;
using Deepgram.Clients.Interfaces.v2;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json;
using PortAudioSharp;

namespace SampleApp
{
    class Program
    {
        // Mock weather data for demo purposes (similar to Python version)
        private static readonly Dictionary<string, WeatherInfo> WeatherData = new()
        {
            ["new york"] = new() { Temperature = 72, Condition = "sunny", Humidity = 45 },
            ["london"] = new() { Temperature = 64, Condition = "cloudy", Humidity = 80 }, // 18°C to °F
            ["tokyo"] = new() { Temperature = 77, Condition = "rainy", Humidity = 90 },   // 25°C to °F
            ["paris"] = new() { Temperature = 68, Condition = "partly cloudy", Humidity = 60 }, // 20°C to °F
            ["sydney"] = new() { Temperature = 82, Condition = "sunny", Humidity = 50 },  // 28°C to °F
        };

        public class WeatherInfo
        {
            public int Temperature { get; set; }
            public string Condition { get; set; } = "";
            public int Humidity { get; set; }
        }

        public class WeatherResponse
        {
            public string Location { get; set; } = "";
            public int Temperature { get; set; }
            public string Unit { get; set; } = "";
            public string Condition { get; set; } = "";
            public int Humidity { get; set; }
            public string Description { get; set; } = "";
        }
        static async Task Main(string[] args)
        {
            try
            {
                // Initialize Library with clean logging (Information level for cleaner output)
                Deepgram.Library.Initialize(LogLevel.Information);
                // For debugging, you can use LogLevel.Debug or LogLevel.Verbose
                //Deepgram.Library.Initialize(LogLevel.Debug); // More detailed logs
                //Deepgram.Library.Initialize(LogLevel.Verbose); // Very chatty logging

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

                Console.WriteLine("\n🌟 Context History Demo - Weather Assistant with Function Calling");
                Console.WriteLine("📚 This demo showcases conversation history and function calling features");
                Console.WriteLine("Press any key to stop and exit...\n");

                // Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
                DeepgramWsClientOptions options = new DeepgramWsClientOptions(null, null, true);
                var agentClient = ClientFactory.CreateAgentWebSocketClient(apiKey: "", options: options);

                // Initialize conversation
                Console.WriteLine("🎤 Ready for conversation! Ask me about weather in any location...");

                // Create conversation history for context
                var conversationHistory = new List<object>
                {
                    new HistoryConversationText
                    {
                        Role = "user",
                        Content = "Hi, I'm looking for weather information. Can you help me with that?"
                    },
                    new HistoryConversationText
                    {
                        Role = "assistant",
                        Content = "Hello! Absolutely, I'd be happy to help you with weather information. I have access to current weather data and can check conditions for any location you're interested in. Just let me know which city or location you'd like to know about!"
                    }
                };

                // Example function call history (optional - showing previous weather check)
                var functionCallHistory = new HistoryFunctionCalls
                {
                    FunctionCalls = new List<HistoryFunctionCall>
                    {
                        new HistoryFunctionCall
                        {
                            Id = "fc_weather_demo_123",
                            Name = "get_weather",
                            ClientSide = true,
                            Arguments = "{\"location\": \"San Francisco\", \"unit\": \"fahrenheit\"}",
                            Response = "The weather in San Francisco is currently 68°F with partly cloudy skies. It's a pleasant day with light winds from the west at 8 mph."
                        }
                    }
                };

                // Subscribe to the EventResponseReceived event
                await agentClient.Subscribe(new EventHandler<OpenResponse>((sender, e) =>
                {
                    Console.WriteLine($"🔗 Connection opened - {e.Type}");
                }));

                await agentClient.Subscribe(new EventHandler<AudioResponse>((sender, e) =>
                {
                    // Audio received - process silently for cleaner output
                    if (e.Stream != null && e.Stream.Length > 0)
                    {
                        var audioData = e.Stream.ToArray();
                        // Play audio through speakers without logging each chunk
                        PlayAudioThroughSpeakers(audioData);
                    }
                }));

                await agentClient.Subscribe(new EventHandler<AgentAudioDoneResponse>((sender, e) =>
                {
                    Console.WriteLine($"🎤 Agent finished speaking - {e.Type}");
                }));

                await agentClient.Subscribe(new EventHandler<AgentStartedSpeakingResponse>((sender, e) =>
                {
                    Console.WriteLine($"🗣️ Agent is speaking - {e.Type}");
                }));

                await agentClient.Subscribe(new EventHandler<AgentThinkingResponse>((sender, e) =>
                {
                    Console.WriteLine($"💭 Agent is thinking - {e.Type}");
                }));

                await agentClient.Subscribe(new EventHandler<ConversationTextResponse>((sender, e) =>
                {
                    Console.WriteLine($"💬 Conversation: [{e.Role}] {e.Content}");
                }));

                                await agentClient.Subscribe(new EventHandler<FunctionCallRequestResponse>((sender, e) =>
                {
                    Console.WriteLine($"🔧 Function call request received - {e.Type}");

                    if (e.Functions != null && e.Functions.Count > 0)
                    {
                        foreach (var functionCall in e.Functions)
                        {
                            Console.WriteLine($"   Function: {functionCall.Name}");
                            Console.WriteLine($"   ID: {functionCall.Id}");
                            Console.WriteLine($"   Arguments: {functionCall.Arguments}");
                            Console.WriteLine($"   Client Side: {functionCall.ClientSide}");

                            // Handle the weather function call
                            if (functionCall.Name == "get_weather")
                            {
                                _ = Task.Run(() => HandleWeatherFunctionCall(agentClient, functionCall));
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("   No functions in request");
                    }
                }));

                // Subscribe to History events
                await agentClient.Subscribe(new EventHandler<HistoryResponse>((sender, e) =>
                {
                    if (e.FunctionCalls != null && e.FunctionCalls.Count > 0)
                    {
                        // This is function call history
                        Console.WriteLine($"📚 Function Call History received:");
                        foreach (var functionCall in e.FunctionCalls)
                        {
                            Console.WriteLine($"   📞 Function: {functionCall.Name}");
                            Console.WriteLine($"      Arguments: {functionCall.Arguments}");
                            Console.WriteLine($"      Response: {functionCall.Response}");
                            Console.WriteLine($"      Client-side: {functionCall.ClientSide}");
                        }
                    }
                    else if (!string.IsNullOrEmpty(e.Role) && !string.IsNullOrEmpty(e.Content))
                    {
                        // This is conversation history
                        Console.WriteLine($"📚 Conversation History: [{e.Role}] {e.Content}");
                    }
                    else
                    {
                        Console.WriteLine($"📚 History event received: {e}");
                    }
                }));

                await agentClient.Subscribe(new EventHandler<UserStartedSpeakingResponse>((sender, e) =>
                {
                    Console.WriteLine($"👤 User is speaking - {e.Type}");
                }));

                await agentClient.Subscribe(new EventHandler<WelcomeResponse>((sender, e) =>
                {
                    Console.WriteLine($"👋 Welcome - {e.Type}");
                }));

                await agentClient.Subscribe(new EventHandler<CloseResponse>((sender, e) =>
                {
                    Console.WriteLine($"🔚 Connection closed - {e.Type}");
                }));

                await agentClient.Subscribe(new EventHandler<SettingsAppliedResponse>((sender, e) =>
                {
                    Console.WriteLine($"⚙️ Settings applied - {e.Type}");
                }));

                await agentClient.Subscribe(new EventHandler<InjectionRefusedResponse>((sender, e) =>
                {
                    Console.WriteLine($"🚫 Injection refused - {e.Type}");
                }));

                await agentClient.Subscribe(new EventHandler<PromptUpdatedResponse>((sender, e) =>
                {
                    Console.WriteLine($"📝 Prompt updated - {e.Type}");
                }));

                await agentClient.Subscribe(new EventHandler<SpeakUpdatedResponse>((sender, e) =>
                {
                    Console.WriteLine($"🎵 Speak updated - {e.Type}");
                }));

                await agentClient.Subscribe(new EventHandler<UnhandledResponse>((sender, e) =>
                {
                    Console.WriteLine($"❓ Unhandled event - {e.Type}");
                }));

                await agentClient.Subscribe(new EventHandler<ErrorResponse>((sender, e) =>
                {
                    Console.WriteLine($"❌ Error received - {e.Type}. Error: {e.Message}");
                }));

                // Configure agent settings with context history and function calling
                var settingsConfiguration = new SettingsSchema();

                // Enable history feature for conversation context
                settingsConfiguration.Flags = new Flags { History = true };

                // Agent tags for analytics
                settingsConfiguration.Tags = new List<string> { "history-example", "function-calling", "weather-demo" };

                // Audio configuration
                settingsConfiguration.Audio.Input.Encoding = "linear16";
                settingsConfiguration.Audio.Input.SampleRate = 16000;
                settingsConfiguration.Audio.Output.Encoding = "linear16";
                settingsConfiguration.Audio.Output.SampleRate = 24000;
                settingsConfiguration.Audio.Output.Container = "none";

                // Agent configuration with context
                settingsConfiguration.Agent.Language = "en";

                // Provide conversation context/history
                settingsConfiguration.Agent.Context = new Context
                {
                    Messages = conversationHistory
                };

                settingsConfiguration.Agent.Listen.Provider.Type = "deepgram";
                settingsConfiguration.Agent.Listen.Provider.Model = "nova-2";

                settingsConfiguration.Agent.Speak.Provider.Type = "deepgram";
                settingsConfiguration.Agent.Speak.Provider.Model = "aura-asteria-en";

                // Configure the thinking/LLM provider with function calling
                settingsConfiguration.Agent.Think.Provider.Type = "open_ai";
                settingsConfiguration.Agent.Think.Provider.Model = "gpt-4o-mini";

                // Define available functions using OpenAPI-like schema
                settingsConfiguration.Agent.Think.Functions = new List<Function>
                {
                    new Function
                    {
                        Name = "get_weather",
                        Description = "Get the current weather conditions for a specific location",
                        Parameters = new Dictionary<string, object>
                        {
                            ["type"] = "object",
                            ["properties"] = new Dictionary<string, object>
                            {
                                ["location"] = new Dictionary<string, object>
                                {
                                    ["type"] = "string",
                                    ["description"] = "The city or location to get weather for (e.g., 'New York', 'London', 'Tokyo')"
                                },
                                ["unit"] = new Dictionary<string, object>
                                {
                                    ["type"] = "string",
                                    ["enum"] = new[] { "fahrenheit", "celsius" },
                                    ["description"] = "Temperature unit preference",
                                    ["default"] = "fahrenheit"
                                }
                            },
                            ["required"] = new[] { "location" }
                        }
                    }
                };

                settingsConfiguration.Agent.Think.Prompt = "You are a helpful weather assistant with access to current weather data. Use the get_weather function to provide accurate, up-to-date weather information when users ask about weather conditions. Always be conversational and provide context about the weather conditions.";

                settingsConfiguration.Agent.Greeting = "Hello! I'm your weather assistant with access to current weather data. What would you like to know?";

                bool bConnected = await agentClient.Connect(settingsConfiguration);
                if (!bConnected)
                {
                    Console.WriteLine("❌ Failed to connect to Deepgram WebSocket server.");
                    return;
                }

                Console.WriteLine("✅ Connected! Function calling configured and history enabled.");
                Console.WriteLine("📚 Initial context provided in agent configuration - ready for conversation!");

                // Microphone streaming with debugging
                Console.WriteLine("🎤 Starting microphone...");
                Microphone microphone = null;
                int audioDataCounter = 0;

                try
                {
                    // Create microphone with proper sample rate and debugging
                    microphone = new Microphone(
                        push_callback: (audioData, length) =>
                        {
                            audioDataCounter++;
                            if (audioDataCounter % 100 == 0) // Log every 100th chunk to reduce noise
                            {
                                Console.WriteLine($"[MIC] Captured audio chunk #{audioDataCounter}: {length} bytes");
                            }

                            // Create array with actual length
                            byte[] actualData = new byte[length];
                            Array.Copy(audioData, actualData, length);

                            // Send to agent
                            agentClient.SendBinary(actualData);
                        },
                        rate: 16000,        // Match the agent's expected input rate (16kHz)
                        chunkSize: 8192,    // Standard chunk size
                        channels: 1,        // Mono
                        device_index: PortAudio.DefaultInputDevice,
                        format: SampleFormat.Int16
                    );

                    microphone.Start();
                    Console.WriteLine("🎤 Microphone started successfully. Try asking: 'What's the weather like in New York?'");
                    Console.WriteLine("🔧 Function calling is enabled - I can fetch real weather data!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error starting microphone: {ex.Message}");
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
                Console.WriteLine($"❌ Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle weather function call requests with mock weather data
        /// </summary>
        private static async Task HandleWeatherFunctionCall(IAgentWebSocketClient agentClient, HistoryFunctionCall functionCall)
        {
            try
            {
                Console.WriteLine("🌤️ Processing weather function call...");

                // Parse the arguments
                var argumentsJson = functionCall.Arguments ?? "{}";
                var arguments = JsonSerializer.Deserialize<Dictionary<string, object>>(argumentsJson);

                string location = arguments.ContainsKey("location") ? arguments["location"].ToString() ?? "Unknown" : "Unknown";
                string unit = arguments.ContainsKey("unit") ? arguments["unit"].ToString() ?? "fahrenheit" : "fahrenheit";

                Console.WriteLine($"🌍 Getting weather for: {location} (in {unit})");

                // Get weather data (mock)
                var weatherData = GetWeather(location, unit);

                Console.WriteLine($"☀️ Weather response: {weatherData.Description}");

                // Send the function call response back to the agent
                var functionCallResponse = new FunctionCallResponseSchema
                {
                    Id = functionCall.Id,
                    Name = functionCall.Name,
                    Content = weatherData.Description  // Just send the description text, not full JSON
                };

                Console.WriteLine($"📤 Sending function response: {functionCallResponse.ToString()}");
                await agentClient.SendFunctionCallResponse(functionCallResponse);
                Console.WriteLine("✅ Function call response sent!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error handling weather function call: {ex.Message}");

                // Send error response
                var errorResponse = new FunctionCallResponseSchema
                {
                    Id = functionCall.Id,
                    Name = functionCall.Name,
                    Content = $"Error retrieving weather data: {ex.Message}"  // Just plain text error
                };
                Console.WriteLine($"📤 Sending error response: {errorResponse.ToString()}");
                await agentClient.SendFunctionCallResponse(errorResponse);
            }
        }

        /// <summary>
        /// Get weather data from mock data or generate random data (similar to Python version)
        /// </summary>
        private static WeatherResponse GetWeather(string location, string unit = "fahrenheit")
        {
            var locationKey = location.ToLower();
            WeatherInfo weather;

            if (WeatherData.ContainsKey(locationKey))
            {
                weather = WeatherData[locationKey];
            }
            else
            {
                // Return random weather for unknown locations
                var random = new Random();
                var conditions = new[] { "sunny", "cloudy", "rainy", "partly cloudy", "windy" };
                weather = new WeatherInfo
                {
                    Temperature = random.Next(50, 95), // Random Fahrenheit temperature
                    Condition = conditions[random.Next(conditions.Length)],
                    Humidity = random.Next(30, 90)
                };
            }

            // Convert temperature if needed
            int finalTemp = weather.Temperature;
            if (unit.ToLower() == "celsius")
            {
                // Convert from Fahrenheit to Celsius
                finalTemp = (int)((weather.Temperature - 32) * 5.0 / 9.0);
            }

            var response = new WeatherResponse
            {
                Location = location,
                Temperature = finalTemp,
                Unit = unit,
                Condition = weather.Condition,
                Humidity = weather.Humidity,
                Description = $"The weather in {location} is {weather.Condition} with a temperature of {finalTemp}°{(unit.ToLower() == "fahrenheit" ? "F" : "C")} and {weather.Humidity}% humidity."
            };

            return response;
        }

        /// <summary>
        /// Generate mock weather data (replace with real weather API)
        /// </summary>
        private static string GenerateMockWeatherResponse(string location, string unit)
        {
            var random = new Random();
            var conditions = new[] { "sunny", "partly cloudy", "cloudy", "light rain", "clear" };
            var condition = conditions[random.Next(conditions.Length)];

            // Generate temperature based on unit
            int temperature;
            string unitSymbol;

            if (unit.ToLower() == "celsius")
            {
                temperature = random.Next(-5, 35); // -5 to 35°C
                unitSymbol = "°C";
            }
            else
            {
                temperature = random.Next(20, 95); // 20 to 95°F
                unitSymbol = "°F";
            }

            var windSpeed = random.Next(0, 15);
            var directions = new[] { "north", "south", "east", "west", "northeast", "northwest", "southeast", "southwest" };
            var windDirection = directions[random.Next(directions.Length)];

            return $"The weather in {location} is currently {temperature}{unitSymbol} with {condition} skies. " +
                   $"Wind is coming from the {windDirection} at {windSpeed} mph. " +
                   $"It's a {(temperature > (unit.ToLower() == "celsius" ? 20 : 70) ? "warm" : "cool")} day!";
        }

        // Audio playback queue and position tracking
        private static Queue<byte[]> audioQueue = new Queue<byte[]>();
        private static byte[]? currentAudioBuffer = null;
        private static int audioPosition = 0;
        private static readonly object audioLock = new object();

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
                byte[] outputBuffer = new byte[bytesToWrite];

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
                            // Audio buffer logging removed for cleaner output
                        }
                        else
                        {
                            // No more audio, fill with silence but KEEP stream running for next audio
                            for (int i = bytesWritten; i < bytesToWrite; i++)
                                outputBuffer[i] = 0;

                            Marshal.Copy(outputBuffer, 0, output, bytesToWrite);
                            // DON'T stop the stream - keep it running for next conversation
                            return PortAudioSharp.StreamCallbackResult.Continue;
                        }
                    }

                    // Copy data from current buffer
                    int remainingInBuffer = currentAudioBuffer.Length - audioPosition;
                    int remainingToWrite = bytesToWrite - bytesWritten;
                    int bytesToCopy = Math.Min(remainingInBuffer, remainingToWrite);

                    Array.Copy(currentAudioBuffer, audioPosition, outputBuffer, bytesWritten, bytesToCopy);
                    audioPosition += bytesToCopy;
                    bytesWritten += bytesToCopy;
                }

                // Copy to output
                Marshal.Copy(outputBuffer, 0, output, bytesToWrite);
            }

            return PortAudioSharp.StreamCallbackResult.Continue;
        }
    }
}
