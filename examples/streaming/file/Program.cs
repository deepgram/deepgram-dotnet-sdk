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
            // Initialize Library with default logging
            // Normal logging is "Info" level
            Library.Initialize();
            // OR very chatty logging
            //Library.Initialize(LogLevel.Debug); // LogLevel.Default, LogLevel.Debug, LogLevel.Verbose

            // Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
            var liveClient = new LiveClient();

            // Subscribe to the EventResponseReceived event
            liveClient.Subscribe(new EventHandler<ResultResponse>((sender, e) =>
            {
                if (e.Channel.Alternatives[0].Transcript == "")
                {
                    return;
                }

                // Console.WriteLine("Transcription received: " + JsonSerializer.Serialize(e.Transcription));
                Console.WriteLine($"\n\n\nSpeaker: {e.Channel.Alternatives[0].Transcript}\n\n\n");
            }));

            // Start the connection
            var liveSchema = new LiveSchema()
            {
                Model = "nova-2",
                Punctuate = true,
                SmartFormat = true,
            };
            await liveClient.Connect(liveSchema);

            // Send some audio data
            var audioData = File.ReadAllBytes(@"preamble.wav");
            liveClient.Send(audioData);

            // Wait for a while to receive responses
            await Task.Delay(45000);

            // Stop the connection
            await liveClient.Stop();

            // Dispose the client
            liveClient.Dispose();

            // Teardown Library
            Library.Terminate();
        }
    }
}
