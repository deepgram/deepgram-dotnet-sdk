// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Models.Listen.v1.WebSocket;
using ListenV1 = Deepgram.Clients.Listen.v1.WebSocket;

namespace Deepgram.Tests.EdgeCases.SpeechToText.V1.ClientExample
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            // Initialize Library with default logging
            Library.Initialize();

            // use the client factory with a API Key set with the "DEEPGRAM_API_KEY" environment variable
            var liveClient = ClientFactory.CreateListenWebSocketClient(1) as ListenV1.Client;
            if (liveClient == null)
            {
                Console.WriteLine("Failed to create ListenWebSocketClient");
                return;
            }

            // Subscribe to the EventResponseReceived event
            liveClient.Subscribe(new EventHandler<ResultResponse>((sender, e) =>
            {
                if (e.Channel.Alternatives[0].Transcript == "")
                {
                    return;
                }
                Console.WriteLine($"Speaker: {e.Channel.Alternatives[0].Transcript}");
            }));

            // Start the connection
            var liveSchema = new LiveSchema()
            {
                Model = "nova-2",
                Punctuate = true,
                SmartFormat = true,
            };
            await liveClient.Connect(liveSchema);

            // get the webcast data... this is a blocking operation
            try
            {
                var url = "http://stream.live.vc.bbcmedia.co.uk/bbc_world_service";
                using (HttpClient client = new HttpClient())
                {
                    using (Stream receiveStream = await client.GetStreamAsync(url))
                    {
                        while (liveClient.IsConnected())
                        {
                            byte[] buffer = new byte[2048];
                            await receiveStream.ReadAsync(buffer, 0, buffer.Length);
                            liveClient.Send(buffer);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            // Stop the connection
            await liveClient.Stop();

            // Teardown Library
            Library.Terminate();
        }
    }
}
