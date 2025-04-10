// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Auth.v1;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize Library with default logging
            // Normal logging is "Info" level
            Library.Initialize();

            // use the client factory with a API Key set with the "DEEPGRAM_API_KEY" environment variable
            var deepgramClient = ClientFactory.CreateAuthClient();

            // generate token
            var tokenResp = await deepgramClient.GrantToken();
            if (tokenResp == null)
            {
                Console.WriteLine("GrantToken failed.");
                Environment.Exit(1);
            }

            string token = tokenResp.AccessToken;
            string ttl = tokenResp.ExpiresIn.ToString();
            Console.WriteLine($"Token: {token}");
            Console.WriteLine($"TTL: {ttl}");

            Console.WriteLine("\n\nPress any key to exit.");
            Console.ReadKey();

            // Teardown Library
            Library.Terminate();
        }
    }
}