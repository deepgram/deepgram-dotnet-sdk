// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Text;

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Speak.v1.WebSocket;
using Deepgram.Logger;


namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize Library with default logging
            // Normal logging is "Info" level
            //Library.Initialize();
            // OR very chatty logging
            Library.Initialize(LogLevel.Verbose); // LogLevel.Default, LogLevel.Debug, LogLevel.Verbose

            //// use the client factory with a API Key set with the "DEEPGRAM_API_KEY" environment variable
            //DeepgramWsClientOptions options = new DeepgramWsClientOptions(null, "ENTER URL HERE");
            //options.AutoFlushSpeakDelta = 1000;
            //var speakClient = ClientFactory.CreateSpeakWebSocketClient("", options);
            var speakClient = ClientFactory.CreateSpeakWebSocketClient();

            // Subscribe to the EventResponseReceived event
            speakClient.Subscribe(new EventHandler<OpenResponse>((sender, e) =>
            {
                Console.WriteLine($"\n\n----> {e.Type} received");
            }));
            speakClient.Subscribe(new EventHandler<MetadataResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
                Console.WriteLine($"----> RequestId: {e.RequestId}");
            }));
            speakClient.Subscribe(new EventHandler<AudioResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");

                if (e.Stream != null)
                {
                    using (BinaryWriter writer = new BinaryWriter(File.Open("output.mp3", FileMode.Append)))
                    {
                        writer.Write(e.Stream.ToArray());
                    }
                }
            }));
            speakClient.Subscribe(new EventHandler<FlushedResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            speakClient.Subscribe(new EventHandler<ClearedResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            speakClient.Subscribe(new EventHandler<CloseResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            speakClient.Subscribe(new EventHandler<UnhandledResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            speakClient.Subscribe(new EventHandler<WarningResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received");
            }));
            speakClient.Subscribe(new EventHandler<ErrorResponse>((sender, e) =>
            {
                Console.WriteLine($"----> {e.Type} received. Error: {e.Message}");
            }));

            // Start the connection
            var speakSchema = new SpeakSchema();
            await speakClient.Connect(speakSchema);

            // Send some Text to convert to audio
            speakClient.SpeakWithText("Hello World!");

            //Flush the audio
            speakClient.Flush();

            // Wait for the user to press a key
            Console.WriteLine("\n\nPress any key to stop and exit...\n\n\n");
            Console.ReadKey();

            // Stop the connection
            await speakClient.Stop();

            // Terminate Libraries
            Library.Terminate();
        }
    }
}
