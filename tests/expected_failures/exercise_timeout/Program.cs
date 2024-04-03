// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Live.v1;
using Deepgram.Logger;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Normal logging is "Info" level
            Library.Initialize();
            // OR very chatty logging
            //Library.Initialize(LogLevel.Debug); // LogLevel.Default, LogLevel.Debug, LogLevel.Verbose

            // Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
            var liveClient = new LiveClient();

            // Subscribe to the EventResponseReceived event
            liveClient.Subscribe(new EventHandler<OpenResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            liveClient.Subscribe(new EventHandler<ResultResponse>((sender, e) =>
            {
                if (e.Channel.Alternatives[0].Transcript == "")
                {
                    return;
                }

                // Console.WriteLine("Transcription received: " + JsonSerializer.Serialize(e.Transcription));
                Console.WriteLine($"\n\n\n----> Speaker: {e.Channel.Alternatives[0].Transcript}\n\n\n");
            }));
            liveClient.Subscribe(new EventHandler<CloseResponse>((sender, e) =>
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
            };
            await liveClient.Connect(liveSchema);

            // Wait for 20 seconds
            Console.WriteLine("\n\nWe are intentionally looking to timeout here to exercise the functionality.");
            Console.WriteLine("This should take less than 20 seconds to timeout.");
            await Task.Delay(20000);

            // Stop the connection
            await liveClient.Stop();

            // Dispose the client
            liveClient.Dispose();

            // Terminate Libraries
            Deepgram.Library.Terminate();
        }
    }
}
