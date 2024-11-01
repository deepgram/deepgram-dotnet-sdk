// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Logger;
using Deepgram.Microphone;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Listen.v2.WebSocket;

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

            // Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
            DeepgramWsClientOptions options = new DeepgramWsClientOptions(null, null, true);
            var liveClient = ClientFactory.CreateListenWebSocketClient(apiKey: "", options: options);


            // OR
            //var liveClient = new LiveClienkt("set your DEEPGRAM_API_KEY here");

            // Subscribe to the EventResponseReceived event
            await liveClient.Subscribe(new EventHandler<OpenResponse>((sender, e) =>
            {
                Console.WriteLine($"\n\n----> {e.Type} received");
            }));
            await liveClient.Subscribe(new EventHandler<MetadataResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            await liveClient.Subscribe(new EventHandler<ResultResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
                if (e.Channel.Alternatives[0].Transcript.Trim() == "")
                {
                    return;
                }

                // Console.WriteLine("Transcription received: " + JsonSerializer.Serialize(e.Transcription));
                Console.WriteLine($"----> Speaker: {e.Channel.Alternatives[0].Transcript}");
            }));
            await liveClient.Subscribe(new EventHandler<SpeechStartedResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            await liveClient.Subscribe(new EventHandler<UtteranceEndResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            await liveClient.Subscribe(new EventHandler<CloseResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            await liveClient.Subscribe(new EventHandler<UnhandledResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            await liveClient.Subscribe(new EventHandler<ErrorResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received. Error: {e.Message}");
            }));

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

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"Client iteration: #{i}\n");

                // connect
                //await liveClient.Connect(liveSchema, cancellationToken);
                await liveClient.Connect(liveSchema);

                // Microphone streaming
                var microphone = new Microphone(liveClient.Send);
                microphone.Start();

                //// START: test an external cancellation
                //cancellationToken.Cancel();
                await Task.Delay(10000);

                //// Wait for the user to press a key
                //Console.ReadKey();

                Console.WriteLine("Stopping the microphone streaming...");
                microphone.Stop();

                // Stop the connection
                await liveClient.Stop();

                // small delay before reconnecting
                await Task.Delay(2000);
            }

            // Terminate Libraries
            Deepgram.Microphone.Library.Terminate();
            Deepgram.Library.Terminate();
        }
    }
}
