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

            // Set "DEEPGRAM_API_KEY" environment variable to your Deepgram API Key
            var deepgramClient = new ManageClient();

            var response = await deepgramClient.GetProjects();
            if (response == null)
            {
                Console.WriteLine("No projects found.");
                return;
            }

            Console.WriteLine(JsonSerializer.Serialize(response));

            //var projectId = "";
            //foreach (var project in response.Projects)
            //{
            //    Console.WriteLine($"Project ID: {project.ProjectId}");
            //    projectId = project.ProjectId;
            //    break;
            //}

            //var balanacesResponse = deepgramClient.GetBalances(projectId);
            //if (balanacesResponse == null || balanacesResponse.Balances == null)
            //{
            //    Console.WriteLine("No balance found.");
            //    return;
            //}

            //Console.WriteLine("\n\nBalances:");
            //Console.WriteLine(JsonSerializer.Serialize(balanacesResponse));
            //Console.WriteLine("\n\n");

            //string balanceId = "";
            //foreach (var balance in balanacesResponse.Balances)
            //{
            //    Console.WriteLine($"Balance ID: {balance.BalanceId}");
            //    balanceId = balance.BalanceId;
            //    break;
            //}

            //var balanaceResponse = deepgramClient.GetBalance(projectId, balanceId);
            //if (balanaceResponse == null)
            //{
            //    Console.WriteLine("No balance found.");
            //    return;
            //}

            //Console.WriteLine("\n\nBalances:");
            //Console.WriteLine(JsonSerializer.Serialize(balanacesResponse));
            //Console.WriteLine("\n\n");

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

            // Teardown Library
            Library.Terminate();
        }
    }
}