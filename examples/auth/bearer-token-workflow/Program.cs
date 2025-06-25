// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.PreRecorded.v1;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize Library with default logging
            // Normal logging is "Info" level
            Library.Initialize();

            Console.WriteLine("=== Deepgram Bearer Token Authentication Workflow Demo ===\n");

            // STEP 1: Demonstrate different ways to configure authentication
            await DemonstrateAuthenticationPriority();

            // STEP 2: Complete workflow: API Key → GrantToken() → Bearer Auth → API calls
            await DemonstrateCompleteWorkflow();

            // STEP 3: Show dynamic authentication switching
            await DemonstrateDynamicSwitching();

            Console.WriteLine("\n=== Demo completed successfully! ===");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

            // Teardown Library
            Library.Terminate();
        }

        static async Task DemonstrateAuthenticationPriority()
        {
            Console.WriteLine("--- Authentication Priority Demo ---");

            // Priority 1: Explicit accessToken parameter (highest)
            var options1 = new DeepgramHttpClientOptions(
                apiKey: "api_key_from_param",
                accessToken: "access_token_from_param"
            );
            var truncatedToken = !string.IsNullOrEmpty(options1.AccessToken) && options1.AccessToken.Length > 20
                ? options1.AccessToken.Substring(0, 20) + "..."
                : options1.AccessToken;
            Console.WriteLine($"Priority 1 - Explicit AccessToken: Uses '{truncatedToken}'");
            Console.WriteLine($"                                    API Key is: '{(string.IsNullOrEmpty(options1.ApiKey) ? "empty" : options1.ApiKey)}'");

            // Priority 2: Explicit apiKey parameter  
            var options2 = new DeepgramHttpClientOptions(apiKey: "api_key_from_param");
            var truncatedApiKey = !string.IsNullOrEmpty(options2.ApiKey) && options2.ApiKey.Length > 20
                ? options2.ApiKey.Substring(0, 20) + "..."
                : options2.ApiKey;
            Console.WriteLine($"Priority 2 - Explicit API Key: Uses '{truncatedApiKey}'");

            // Priority 3: Environment variable demo (would use DEEPGRAM_ACCESS_TOKEN if set)
            Console.WriteLine("Priority 3 - Environment variables: DEEPGRAM_ACCESS_TOKEN > DEEPGRAM_API_KEY");

            Console.WriteLine();
        }

        static async Task DemonstrateCompleteWorkflow()
        {
            Console.WriteLine("--- Complete Workflow Demo ---");

            // Check if we have an API key to work with
            var apiKey = Environment.GetEnvironmentVariable("DEEPGRAM_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine("⚠️  DEEPGRAM_API_KEY environment variable not set.");
                Console.WriteLine("   Set it to run the complete workflow demo:");
                Console.WriteLine("   export DEEPGRAM_API_KEY='your_api_key_here'");
                Console.WriteLine();
                return;
            }

            try
            {
                Console.WriteLine("1. Starting with API Key authentication...");

                // Step 1: Use API Key to get an Access Token
                var authClient = ClientFactory.CreateAuthClient();
                var tokenResponse = await authClient.GrantToken();

                if (tokenResponse == null)
                {
                    Console.WriteLine("❌ Failed to obtain access token");
                    return;
                }

                Console.WriteLine($"2. ✅ Obtained access token (expires in {tokenResponse.ExpiresIn} seconds)");
                var displayToken = tokenResponse.AccessToken.Length > 20
                    ? tokenResponse.AccessToken.Substring(0, 20) + "..."
                    : tokenResponse.AccessToken;
                Console.WriteLine($"   Token: {displayToken}");

                // Step 2: Create a new client using the Bearer token
                var bearerOptions = new DeepgramHttpClientOptions(accessToken: tokenResponse.AccessToken);
                var prerecordedClient = new PreRecordedClient("", bearerOptions);

                Console.WriteLine("3. ✅ Created new client with Bearer token authentication");

                // Step 3: Use the Bearer token for an API call
                var urlSource = new UrlSource("https://dpgr.am/bueller.wav");
                var prerecordedOptions = new PreRecordedSchema()
                {
                    Model = "nova-2",
                    Punctuate = true,
                    SmartFormat = true,
                };

                Console.WriteLine("4. Making API call with Bearer token...");
                var response = await prerecordedClient.TranscribeUrl(urlSource, prerecordedOptions);

                if (response?.Results?.Channels?.Count > 0)
                {
                    var transcript = response.Results.Channels[0].Alternatives[0].Transcript;
                    Console.WriteLine($"5. ✅ API call successful with Bearer token!");
                    Console.WriteLine($"   Transcript: {transcript.Substring(0, Math.Min(100, transcript.Length))}...");
                }
                else
                {
                    Console.WriteLine("5. ❌ API call returned empty results");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in workflow: {ex.Message}");
            }

            Console.WriteLine();
        }

        static async Task DemonstrateDynamicSwitching()
        {
            Console.WriteLine("--- Dynamic Authentication Switching Demo ---");

            // Start with API Key
            var options = new DeepgramHttpClientOptions("initial_api_key_12345");
            var initialApiKey = !string.IsNullOrEmpty(options.ApiKey) && options.ApiKey.Length > 20
                ? options.ApiKey.Substring(0, 20) + "..."
                : options.ApiKey;
            Console.WriteLine($"Initial state - API Key: '{initialApiKey}'");
            Console.WriteLine($"                Access Token: '{(string.IsNullOrEmpty(options.AccessToken) ? "empty" : options.AccessToken)}'");

            // Switch to Access Token (clears API Key)
            options.SetAccessToken("new_access_token_67890");
            Console.WriteLine($"\nAfter SetAccessToken():");
            Console.WriteLine($"  API Key: '{(string.IsNullOrEmpty(options.ApiKey) ? "empty" : options.ApiKey)}'");
            var switchedToken = !string.IsNullOrEmpty(options.AccessToken) && options.AccessToken.Length > 20
                ? options.AccessToken.Substring(0, 20) + "..."
                : options.AccessToken;
            Console.WriteLine($"  Access Token: '{switchedToken}'");

            // Switch back to API Key (clears Access Token)
            options.SetApiKey("new_api_key_99999");
            Console.WriteLine($"\nAfter SetApiKey():");
            var finalApiKey = !string.IsNullOrEmpty(options.ApiKey) && options.ApiKey.Length > 20
                ? options.ApiKey.Substring(0, 20) + "..."
                : options.ApiKey;
            Console.WriteLine($"  API Key: '{finalApiKey}'");
            Console.WriteLine($"  Access Token: '{(string.IsNullOrEmpty(options.AccessToken) ? "empty" : options.AccessToken)}'");

            // Clear all credentials
            options.ClearCredentials();
            Console.WriteLine($"\nAfter ClearCredentials():");
            Console.WriteLine($"  API Key: '{(string.IsNullOrEmpty(options.ApiKey) ? "empty" : options.ApiKey)}'");
            Console.WriteLine($"  Access Token: '{(string.IsNullOrEmpty(options.AccessToken) ? "empty" : options.AccessToken)}'");

            Console.WriteLine("\n✅ Dynamic switching demo completed");
            Console.WriteLine();
        }
    }
}