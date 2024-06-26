// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Text.Json;

using Deepgram;
using Deepgram.Models.Speak.v1;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize Library with default logging
            // Normal logging is "Info" level
            Library.Initialize();

            // Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
            var deepgramClient = new SpeakClient();

            var response = await deepgramClient.ToFile(
                new TextSource("How much wood could a woodchuck chuck? If a woodchuck could chuck wood? As much wood as a woodchuck could chuck, if a woodchuck could chuck wood."),
                "test.mp3",
                new SpeakSchema()
                {
                    Model = "aura-asteria-en",
                });

            //Console.WriteLine(response);
            Console.WriteLine(response);
            Console.ReadKey();

            // Teardown Library
            Library.Terminate();
        }
    }
}