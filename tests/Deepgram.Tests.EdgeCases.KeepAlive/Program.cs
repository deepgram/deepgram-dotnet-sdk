// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System;
using System.Threading.Tasks;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Listen.v2.WebSocket;

namespace Deepgram.Tests.EdgeCases.KeepAlive
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            // Initialize Library with default logging
            // Normal logging is "Info" level
            Library.Initialize();
            // OR very chatty logging
            //Library.Initialize(LogLevel.Debug); // LogLevel.Default, LogLevel.Debug, LogLevel.Verbose

            Console.WriteLine("\n\nPress any key to stop and exit...\n\n\n");

            // Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
            DeepgramWsClientOptions options = new DeepgramWsClientOptions(null, null, true);
            var liveClient = ClientFactory.CreateListenWebSocketClient("", options);

            // Subscribe to the EventResponseReceived event
            await liveClient.Subscribe(new EventHandler<OpenResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            await liveClient.Subscribe(new EventHandler<ResultResponse>((sender, e) =>
            {
                if (e.Channel.Alternatives[0].Transcript == "")
                {
                    return;
                }

                // Console.WriteLine("Transcription received: " + JsonSerializer.Serialize(e.Transcription));
                Console.WriteLine($"\n\n\n----> Speaker: {e.Channel.Alternatives[0].Transcript}\n\n\n");
            }));
            await liveClient.Subscribe(new EventHandler<CloseResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            await liveClient.Subscribe(new EventHandler<ErrorResponse>((sender, e) =>
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
            };
            await liveClient.Connect(liveSchema);

            // Wait for the user to press a key
            Console.WriteLine("\n\nWe are intentionally waiting here to test the KeepAlive functionality...");
            Console.WriteLine("Press any key to stop and exit...");
            Console.ReadKey();

            // Stop the connection
            await liveClient.Stop();

            // Terminate Libraries
            Library.Terminate();
        }
    }
}
