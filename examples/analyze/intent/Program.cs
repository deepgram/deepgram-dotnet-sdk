// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Text.Json;

using Deepgram.Models.Analyze.v1;
using Deepgram.Logger;

namespace PreRecorded
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize Library with default logging
            // Normal logging is "Info" level
            // Library.Initialize();
            // OR to set logging level
            Library.Initialize(LogLevel.Debug);

            // JSON options
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
            {
                WriteIndented = true
            };

            // Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
            var deepgramClient = new AnalyzeClient();

            // check to see if the file exists
            if (!File.Exists(@"conversation.txt"))
            {
                Console.WriteLine("Error: File 'conversation.txt' not found.");
                return;
            }

            // increase timeout to 60 seconds
            CancellationTokenSource cancelToken = new CancellationTokenSource();
            cancelToken.CancelAfter(TimeSpan.FromSeconds(120));

            var audioData = File.ReadAllBytes(@"conversation.txt");
            var response = await deepgramClient.AnalyzeFile(
                audioData,
                new AnalyzeSchema()
                {
                    Language = "en",
                    Intents = true,
                },
                cancelToken);

            Console.WriteLine($"\n\n{JsonSerializer.Serialize(response, options)}\n\n");
            Console.ReadKey();

            // Teardown Library
            Library.Terminate();
        }
    }
}