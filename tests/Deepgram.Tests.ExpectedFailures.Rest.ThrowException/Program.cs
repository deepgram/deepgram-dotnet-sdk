// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Listen.v1.REST;

namespace PreRecorded
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize Library with default logging
            // Normal logging is "Info" level
            Library.Initialize();

            // Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
            var deepgramClient = ClientFactory.CreateListenRESTClient();

            try
            {
                var response = await deepgramClient.TranscribeUrl(
                    new UrlSource("https://dpgr.am/bad.wav"), // bad URL
                    new PreRecordedSchema()
                    {
                        Model = "nova-2",
                    });

                Console.WriteLine(response);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
            }
            Console.ReadKey();

            // Teardown Library
            Library.Terminate();
        }
    }
}