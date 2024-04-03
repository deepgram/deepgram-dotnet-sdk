// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Text.Json;

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Live.v1;
using Deepgram.Logger;
using Deepgram.Microphone;
using System.Threading;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize Library with default logging
            // Normal logging is "Info" level
            //Deepgram.Library.Initialize();
            // OR very chatty logging
            Deepgram.Library.Initialize(LogLevel.Debug); // LogLevel.Default, LogLevel.Debug, LogLevel.Verbose
            Deepgram.Microphone.Library.Initialize();

            Console.WriteLine("\n\nPress any key to stop and exit...\n\n\n");

            // Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
            DeepgramWsClientOptions options = new DeepgramWsClientOptions(null, null, true);
            var liveClient = new LiveClient("", options);
            // OR
            //var liveClient = new LiveClienkt("set your DEEPGRAM_API_KEY here");

            // Subscribe to the EventResponseReceived event
            liveClient._openReceived += (sender, e) =>
            {
                Console.WriteLine($"{e.Type} received");
            };
            liveClient._metadataReceived += (sender, e) =>
            {
                Console.WriteLine($"{e.Type} received: {JsonSerializer.Serialize(e)}");
            };
            liveClient._resultsReceived += (sender, e) =>
            {
                if (e.Channel.Alternatives[0].Transcript == "")
                {
                    return;
                }

                // Console.WriteLine("Transcription received: " + JsonSerializer.Serialize(e.Transcription));
                Console.WriteLine($"\n\n\nSpeaker: {e.Channel.Alternatives[0].Transcript}\n\n\n");
            };
            liveClient._speechStartedReceived += (sender, e) =>
            {
                Console.WriteLine($"{e.Type} received");
            };
            liveClient._utteranceEndReceived += (sender, e) =>
            {
                Console.WriteLine($"{e.Type} received");
            };
            liveClient._closeReceived += (sender, e) =>
            {
                Console.WriteLine($"{e.Type} received");
            };
            liveClient._unhandledReceived += (sender, e) =>
            {
                Console.WriteLine($"{e.Type} received. Raw: {e.Raw}");
            };
            liveClient._errorReceived += (sender, e) =>
            {
                Console.WriteLine($"{e.Type} received. Error: {e.Message}");
            };

            // my own cancellation token
            //var cancellationToken = new CancellationTokenSource();

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
            //await liveClient.Connect(liveSchema, cancellationToken);
            await liveClient.Connect(liveSchema);

            // Microphone streaming
            var microphone = new Microphone(liveClient.Send);
            microphone.Start();

            // Wait for the user to press a key
            Console.ReadKey();

            Console.WriteLine("Stopping the microphone streaming...");
            microphone.Stop();

            //// START: test an external cancellation
            //cancellationToken.Cancel();
            //Thread.Sleep(10000);    // wait 10 seconds to cancel externally
            //// END: test an external cancellation

            // Stop the connection
            await liveClient.Stop();

            // Dispose the client
            liveClient.Dispose();

            // Terminate Libraries
            Deepgram.Microphone.Library.Terminate();
            Deepgram.Library.Terminate();
        }
    }
}
