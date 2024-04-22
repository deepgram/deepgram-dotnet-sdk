// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Text.Json;

using Deepgram.Models.PreRecorded.v1;
using Deepgram.Logger;
using Deepgram.Models.Authenticate.v1;

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
            var clientOptions = new DeepgramHttpClientOptions(null, "http://127.0.0.1");
            var deepgramClient = new PreRecordedClient("", clientOptions);

            try
            {
                var response = await deepgramClient.TranscribeUrl(
                    new UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"),
                    new PreRecordedSchema()
                    {
                        Model = "nova-2",
                    });

                    Console.WriteLine(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n\nException thrown: {ex.Message}.\n\n");
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

            // Terminate Libraries
            Deepgram.Library.Terminate();
        }
    }
}
