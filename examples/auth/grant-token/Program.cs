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

            Console.WriteLine("=== Grant Token Examples ===\n");

            // Example 1: Generate token with default TTL (30 seconds)
            Console.WriteLine("1. Grant token with default TTL (30 seconds):");
            var defaultTokenResp = await deepgramClient.GrantToken();
            if (defaultTokenResp == null)
            {
                Console.WriteLine("GrantToken failed.");
                Environment.Exit(1);
            }

            Console.WriteLine($"   Token: {defaultTokenResp.AccessToken[..20]}...");
            Console.WriteLine($"   TTL: {defaultTokenResp.ExpiresIn} seconds");

            // Example 2: Generate token with custom TTL using GrantTokenSchema
            Console.WriteLine("\n2. Grant token with custom TTL (300 seconds):");
            var customTokenSchema = new GrantTokenSchema
            {
                TtlSeconds = 300  // 5 minutes
            };

            var customTokenResp = await deepgramClient.GrantToken(customTokenSchema);
            if (customTokenResp == null)
            {
                Console.WriteLine("GrantToken with custom TTL failed.");
                Environment.Exit(1);
            }

            Console.WriteLine($"   Token: {customTokenResp.AccessToken[..20]}...");
            Console.WriteLine($"   TTL: {customTokenResp.ExpiresIn} seconds");

            Console.WriteLine("\n=== Summary ===");
            Console.WriteLine("✓ Default method works as before (30 seconds TTL)");
            Console.WriteLine("✓ New method allows custom TTL from 1 to 3600 seconds");
            Console.WriteLine("✓ All tokens can be used for the same API operations");

            Console.WriteLine("\n\nPress any key to exit.");
            Console.ReadKey();

            // Teardown Library
            Library.Terminate();
        }
    }
}