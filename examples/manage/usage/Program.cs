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

            // get projects
            var projectResp = await deepgramClient.GetProjects();
            if (projectResp == null)
            {
                Console.WriteLine("ListProjects failed.");
                Environment.Exit(1);
            }

            string myId = null;
            string myName = null;
            foreach (var project in projectResp.Projects)
            {
                myId = project.ProjectId;
                myName = project.Name;
                break;
            }
            Console.WriteLine($"\n\n{projectResp}\n\n");

            // list requests
            string requestId = null;
            var requestsOptions = new UsageRequestsSchema();
            var listResp = await deepgramClient.GetUsageRequests(myId, requestsOptions);
            if (listResp == null)
            {
                Console.WriteLine("\n\nNo requests found\n\n");
            }
            else
            {
                Console.WriteLine($"\n\n{listResp}\n\n");

                foreach (var request in listResp.Requests)
                {
                    requestId = request.RequestId;
                    break;
                }
            }
            Console.WriteLine($"request_id: {requestId}\n\n");

            // get request
            var reqResp = await deepgramClient.GetUsageRequest(myId, requestId);
            if (reqResp == null)
            {
                Console.WriteLine("No request found");
            }
            else
            {
                Console.WriteLine($"\n\n{reqResp}\n\n");
            }

            // get fields
            var fieldsOptions = new UsageFieldsSchema();
            var fieldsResp = await deepgramClient.GetUsageFields(myId, fieldsOptions);
            if (fieldsResp == null)
            {
                Console.WriteLine("UsageFields not found.");
                Environment.Exit(1);
            }
            else
            {
                Console.WriteLine($"\n\n{fieldsResp}\n\n");
            }

            // list usage
            var summaryOptions = new UsageSummarySchema();
            var summaryResp = await deepgramClient.GetUsageSummary(myId, summaryOptions);
            if (summaryResp == null)
            {
                Console.WriteLine("UsageSummary not found");
            }
            else
            {
                Console.WriteLine($"\n\n{summaryResp}\n\n");
            }


            Console.WriteLine("\n\nPress any key to exit.");
            Console.ReadKey();

            // Teardown Library
            Library.Terminate();
        }
    }
}