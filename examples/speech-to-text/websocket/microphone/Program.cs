// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Logger;
using Deepgram.Microphone;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Listen.v1.WebSocket;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize Library with default logging
            // Normal logging is "Info" level
            Deepgram.Library.Initialize();
            // OR very chatty logging
            // Library.Initialize(LogLevel.Debug); // LogLevel.Default, LogLevel.Debug, LogLevel.Verbose

            // Initialize the microphone library
            Deepgram.Microphone.Library.Initialize();

            Console.WriteLine("\n\nPress any key to stop and exit...\n\n\n");

            // Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
            DeepgramWsClientOptions options = new DeepgramWsClientOptions(null, null, true);
            //options.AutoFlushReplyDelta = 2000; // if your live stream application is like "push to talk".
            var liveClient = new ListenWebSocketClient("", options);

            // Subscribe to the EventResponseReceived event
            liveClient.Subscribe(new EventHandler<OpenResponse>((sender, e) =>
            {
                Console.WriteLine($"\n\n----> {e.Type} received");
            }));
            liveClient.Subscribe(new EventHandler<MetadataResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            liveClient.Subscribe(new EventHandler<ResultResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
                if (e.Channel.Alternatives[0].Transcript.Trim() == "")
                {
                    return;
                }

                // Console.WriteLine("Transcription received: " + JsonSerializer.Serialize(e.Transcription));
                Console.WriteLine($"----> Speaker: {e.Channel.Alternatives[0].Transcript}");
            }));
            liveClient.Subscribe(new EventHandler<SpeechStartedResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            liveClient.Subscribe(new EventHandler<UtteranceEndResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            liveClient.Subscribe(new EventHandler<CloseResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            liveClient.Subscribe(new EventHandler<UnhandledResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            liveClient.Subscribe(new EventHandler<ErrorResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received. Error: {e.Message}");
            }));

            // Start the connection
            var liveSchema = new LiveSchema()
            {
                Model = "nova-2",
                Encoding = "linear16",
                SampleRate = 16000,
                Punctuate = true,
                SmartFormat = true,
                InterimResults = true,
                UtteranceEnd = "1000",
                VadEvents = true,
            };
            await liveClient.Connect(liveSchema);

            // Microphone streaming
            var microphone = new Microphone(liveClient.Send);
            microphone.Start();

            // Wait for the user to press a key
            Console.ReadKey();

            // Stop the microphone
            microphone.Stop();

            // Stop the connection
            await liveClient.Stop();

            // Terminate Libraries
            Deepgram.Microphone.Library.Terminate();
            Deepgram.Library.Terminate();
        }
    }
}
