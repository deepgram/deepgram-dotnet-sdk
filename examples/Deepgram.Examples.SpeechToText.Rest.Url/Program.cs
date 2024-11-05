// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepgram;
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

            // create a ListenRESTClient directly (without using the factory method) with a API Key
            // set using the "DEEPGRAM_API_KEY" environment variable
            var deepgramClient = new ListenRESTClient();

            var customOptions = new Dictionary<string, string>();
            customOptions["smart_format"] = "true";

            var response = await deepgramClient.TranscribeUrl(
                new UrlSource("https://dpgr.am/bueller.wav"),
                new PreRecordedSchema()
                {
                    Model = "nova-2",
                },
                null, // use the default timeout
                customOptions);

            Console.WriteLine(response);
            Console.ReadKey();

            // Teardown Library
            Library.Terminate();
        }
    }
}