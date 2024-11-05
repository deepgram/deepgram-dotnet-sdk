// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Listen.v2.WebSocket;

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
                Library.Initialize();

                // use the client factory with a API Key set with the "DEEPGRAM_API_KEY" environment variable
                var liveClient = ClientFactory.CreateListenWebSocketClient();

                // Subscribe to the EventResponseReceived event
                await liveClient.Subscribe(new EventHandler<ResultResponse>((sender, e) =>
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
                bool bConnected = await liveClient.Connect(liveSchema);
                if (!bConnected)
                {
                    Console.WriteLine("Failed to connect to the server");
                    return;
                }

                // Send some audio data
                var audioData = File.ReadAllBytes(@"preamble.wav");
                liveClient.Send(audioData);

                // Wait for a while to receive responses
                await Task.Delay(45000);

                // Stop the connection
                await liveClient.Stop();

                // Teardown Library
                Library.Terminate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
    }
}
