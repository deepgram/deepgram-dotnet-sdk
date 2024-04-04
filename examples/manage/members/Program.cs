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
        private const string DELETE_MEMBER_BY_EMAIL = "MY_EMAIL";

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

            // Get projects
            var projectResp = deepgramClient.GetProjects();
            if (projectResp == null)
            {
                Console.WriteLine("ListProjects failed.");
                Environment.Exit(1);
            }

            string myId = null;
            foreach (var project in projectResp.Result.Projects)
            {
                myId = project.ProjectId;
                string myName = project.Name;
                Console.WriteLine($"\n\nListProjects() - ID: {myId}, Name: {myName}\n\n");
            }

            // List members
            string delMemberId = null;
            var listResp = deepgramClient.GetMembers(myId);
            if (listResp == null)
            {
                Console.WriteLine("No members found");
            }
            else
            {
                Console.WriteLine($"\n\n{JsonSerializer.Serialize(listResp, options)}\n\n");

                foreach (var member in listResp.Result.Members)
                {
                    if (member.Email == DELETE_MEMBER_BY_EMAIL)
                    {
                        delMemberId = member.MemberId;
                    }
                }
            }

            // Delete member
            if (delMemberId == null)
            {
                Console.WriteLine("");
                Console.WriteLine("This example requires a project that already exists with a name specified in DELETE_MEMBER_BY_EMAIL.");
                Console.WriteLine("This is required to exercise the RemoveMember function.");
                Console.WriteLine("In the absence of this, this example will exit early.");
                Console.WriteLine("");
                Environment.Exit(1);
            }

            var deleteResp = deepgramClient.RemoveMember(myId, delMemberId);
            if (deleteResp == null)
            {
                Console.WriteLine("\n\nRemoveMember failed.\n\n");
                Environment.Exit(1);
            }
            else
            {
                Console.WriteLine($"\n\n{JsonSerializer.Serialize(listResp, options)}\n\n");
            }

            // List members
            listResp = deepgramClient.GetMembers(myId);
            if (listResp == null)
            {
                Console.WriteLine("\n\nNo members found.\n\n");
            }
            else
            {
                Console.WriteLine($"\n\n{JsonSerializer.Serialize(listResp, options)}\n\n");
            }

            Console.WriteLine("\n\nPress any key to exit.");
            Console.ReadKey();

            // Teardown Library
            Library.Terminate();
        }
    }
}