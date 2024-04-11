// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Text.Json;

using Deepgram;
using Deepgram.Models.Speak.v1;
using Deepgram.Logger;

namespace SampleApp
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

            // use the client factory with a API Key set with the "DEEPGRAM_API_KEY" environment variable
            var deepgramClient = ClientFactory.CreateSpeakClient();

            var response = await deepgramClient.ToFile(
                new TextSource("Hello World!"),
                "test.mp3",
                new SpeakSchema()
                {
                    Model = "aura-asteria-en",
                });

            //Console.WriteLine(response);
            Console.WriteLine(JsonSerializer.Serialize(response, options));
            Console.ReadKey();

            // Teardown Library
            Library.Terminate();
        }
    }
}