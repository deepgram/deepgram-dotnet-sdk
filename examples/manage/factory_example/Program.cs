// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Text.Json;

using Deepgram.Logger;
using Deepgram.Models.Manage.v1;

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
            var deepgramClient = ClientFactory.CreateManageClient();

            var response = await deepgramClient.GetProjects();
            if (response == null)
            {
                Console.WriteLine("No projects found.");
                return;
            }
            Console.WriteLine($"\n\n{response}\n\n");

            var projectId = "";
            foreach (var project in response.Projects)
            {
                Console.WriteLine($"Using Project ID: {project.ProjectId}");
                projectId = project.ProjectId;
                break;
            }

            var balanacesResponse = deepgramClient.GetBalances(projectId);
            if (balanacesResponse == null)
            {
                Console.WriteLine("\n\nNo balance found.\n\n");
                return;
            }
            Console.WriteLine($"\n\n{balanacesResponse}\n\n");

            string balanceId = "";
            foreach (var balance in balanacesResponse.Result.Balances)
            {
                Console.WriteLine($"Using Balance ID: {balance.BalanceId}");
                balanceId = balance.BalanceId;
                break;
            }

            var balanceResponse = deepgramClient.GetBalance(projectId, balanceId);
            if (balanceResponse == null)
            {
                Console.WriteLine("\n\nNo balance found.\n\n");
                return;
            }
            Console.WriteLine($"\n\n{balanceResponse}\n\n");

            Console.WriteLine("\n\nPress any key to exit.");
            Console.ReadKey();

            // Teardown Library
            Library.Terminate();
        }
    }
}