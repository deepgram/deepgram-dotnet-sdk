// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System;
using System.Threading.Tasks;
using Deepgram;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Speak.v1.REST;
using Microsoft.Extensions.Options;

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

            // use the client factory with a API Key set with the "DEEPGRAM_API_KEY" environment variable
            //DeepgramHttpClientOptions options = new DeepgramHttpClientOptions("", "");
            //var deepgramClient = SpeakRESTClient("", options);
            var deepgramClient = new SpeakRESTClient();

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