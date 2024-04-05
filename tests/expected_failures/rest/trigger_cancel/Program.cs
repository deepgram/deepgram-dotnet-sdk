// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Text.Json;

using Deepgram.Models.PreRecorded.v1;
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
            var deepgramClient = new PreRecordedClient();

            // intentionally cancel the request after 250ms
            CancellationTokenSource cancelToken = new CancellationTokenSource(250);

            try
            {
                var response = await deepgramClient.TranscribeUrl(
                    new UrlSource("https://static.deepgram.com/examples/Bueller-Life-moves-pretty-fast.wav"),
                    new PrerecordedSchema()
                    {
                        Model = "nova-2",
                    },
                    cancelToken);

                    Console.WriteLine(JsonSerializer.Serialize(response));
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\n\nRequest was intentionally cancelled.\n\n");
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

            // Terminate Libraries
            Deepgram.Library.Terminate();
        }
    }
}
