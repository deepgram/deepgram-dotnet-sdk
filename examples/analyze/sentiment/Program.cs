// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Analyze.v1;

namespace PreRecorded
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize Library with default logging
            // Normal logging is "Info" level
            Library.Initialize();

            // use the client factory with a API Key set with the "DEEPGRAM_API_KEY" environment variable
            var deepgramClient = ClientFactory.CreateAnalyzeClient();

            // check to see if the file exists
            if (!File.Exists(@"conversation.txt"))
            {
                Console.WriteLine("Error: File 'conversation.txt' not found.");
                return;
            }

            var audioData = File.ReadAllBytes(@"conversation.txt");
            var response = await deepgramClient.AnalyzeFile(
                audioData,
                new AnalyzeSchema()
                {
                    Language = "en",
                    Sentiment = true,
                });

            Console.WriteLine(response);
            Console.ReadKey();

            // Teardown Library
            Library.Terminate();
        }
    }
}