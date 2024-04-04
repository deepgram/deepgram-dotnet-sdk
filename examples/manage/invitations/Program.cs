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

            // JSON options
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
            {
                WriteIndented = true
            };

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
                Console.WriteLine($"\n\nListProjects() - ID: {myId}, Name: {myName}\n\n");
                break;
            }

            // list invites
            var listResp = await deepgramClient.GetInvites(myId);
            if (listResp.Invites.Count == 0)
            {
                Console.WriteLine("\n\nNo invites found\n\n");
            }
            else
            {
                Console.WriteLine($"\n\n{JsonSerializer.Serialize(listResp, options)}\n\n");
            }

            // send invite
            var createInvite = new InviteSchema()
            {
                Email = "spam@spam.com",
                Scope = "member",
            };

            var createResp = await deepgramClient.SendInvite(myId, createInvite);
            Console.WriteLine($"\n\n{JsonSerializer.Serialize(createResp, options)}\n\n");

            // list invites
            listResp = await deepgramClient.GetInvites(myId);
            if (listResp == null)
            {
                Console.WriteLine("\n\nNo invites found\n\n");
            }
            else
            {
                Console.WriteLine($"\n\n{JsonSerializer.Serialize(listResp, options)}\n\n");
            }

            // delete invite
            var delResp = await deepgramClient.DeleteInvite(myId, "spam@spam.com");
            Console.WriteLine($"\n\n{JsonSerializer.Serialize(listResp, options)}\n\n");

            // list invites
            listResp = await deepgramClient.GetInvites(myId);
            if (listResp == null)
            {
                Console.WriteLine("\n\nNo invites found\n\n");
            }
            else
            {
                Console.WriteLine($"\n\n{JsonSerializer.Serialize(listResp, options)}\n\n");
            }

            // Leave commented out unless you are running this from a secondary account
            //var leaveResp = await deepgramClient.LeaveProject(myId);
            //Console.WriteLine($"\n\n{JsonSerializer.Serialize(leaveResp, options)}\n\n");

            Console.WriteLine("\n\nPress any key to exit.");
            Console.ReadKey();

            // Teardown Library
            Library.Terminate();
        }
    }
}