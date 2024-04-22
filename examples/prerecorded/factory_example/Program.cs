// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Text.Json;

using Deepgram.Logger;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.PreRecorded.v1;

namespace PreRecorded
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize Library with default logging
            // Normal logging is "Info" level
            //Library.Initialize();
            // OR very chatty logging
            Library.Initialize(LogLevel.Debug); // LogLevel.Default, LogLevel.Debug, LogLevel.Verbose

            // use the client factory with a API Key set with the "DEEPGRAM_API_KEY" environment variable
            var deepgramClient = ClientFactory.CreatePreRecordedClient();

            // check to see if the file exists
            if (!File.Exists(@"Bueller-Life-moves-pretty-fast.wav"))
            {
                Console.WriteLine("Error: File 'Bueller-Life-moves-pretty-fast.wav' not found.");
                return;
            }

            // increase timeout to a very large value
            CancellationTokenSource cancelToken = new CancellationTokenSource();
            cancelToken.CancelAfter(3600000);

            var audioData = File.ReadAllBytes(@"Bueller-Life-moves-pretty-fast.wav");
            var response = await deepgramClient.TranscribeFile(
                audioData,
                new PreRecordedSchema()
                {
                    Model = "nova-2",
                    Punctuate = true,
                },
                cancelToken);

            Console.WriteLine($"\n\n{response}\n\n");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            // Teardown Library
            Library.Terminate();
        }
    }
}