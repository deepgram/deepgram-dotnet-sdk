// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Text.Json;
using Deepgram.Logger;
using Deepgram.Models.PreRecorded.v1;

namespace PreRecorded
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

            // JSON options
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
            {
                WriteIndented = true
            };

            // Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
            var deepgramClient = new PreRecordedClient();

            // check to see if the file exists
            if (!File.Exists(@"Bueller-Life-moves-pretty-fast.wav"))
            {
                Console.WriteLine("Error: File 'Bueller-Life-moves-pretty-fast.wav' not found.");
                return;
            }

            var audioData = File.ReadAllBytes(@"Bueller-Life-moves-pretty-fast.wav");
            var response = await deepgramClient.TranscribeFile(
                audioData,
                new PrerecordedSchema()
                {
                    Model = "nova-2",
                    Punctuate = true,
                });

            Console.WriteLine($"\n\n{JsonSerializer.Serialize(response, options)}\n\n");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            // Teardown Library
            Library.Terminate();
        }
    }
}