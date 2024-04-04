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
        private const string DELETE_PROJECT_BY_NAME = "DELETE_PROJECT_NAME";

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
            var listResp = await deepgramClient.GetProjects();
            if (listResp == null)
            {
                Console.WriteLine("ListProjects failed.");
                Environment.Exit(1);
            }

            string myId = null;
            string myName = null;
            string myDeleteId = null;
            foreach (var project in listResp.Projects)
            {
                if (project.Name == DELETE_PROJECT_BY_NAME)
                {
                    myDeleteId = project.ProjectId;
                }
                myId = project.ProjectId;
                myName = project.Name;
            }
            Console.WriteLine($"\n\n{JsonSerializer.Serialize(listResp, options)}\n\n");

            // get project
            var getResp = await deepgramClient.GetProject(myId);
            Console.WriteLine($"\n\n{JsonSerializer.Serialize(getResp, options)}\n\n");

            // update project
            var updateOptions = new ProjectSchema()
            {
                Name = "My TEST RENAME Example"
            };

            var updateResp = await deepgramClient.UpdateProject(myId, updateOptions);
            if (updateResp == null)
            {
                Console.WriteLine("\n\nUpdateProject failed.\n\n");
                Environment.Exit(1);
            }
            Console.WriteLine($"\n\n{JsonSerializer.Serialize(updateResp, options)}\n\n");

            // get project
            getResp = await deepgramClient.GetProject(myId);
            if (getResp == null)
            {
                Console.WriteLine("\n\nGetProject failed.\n\n");
                Environment.Exit(1);
            }
            Console.WriteLine($"\n\n{JsonSerializer.Serialize(getResp, options)}\n\n");

            // update project
            updateOptions = new ProjectSchema()
            {
                Name = myName,
            };
            updateResp = await deepgramClient.UpdateProject(myId, updateOptions);
            if (updateResp == null)
            {
                Console.WriteLine("\n\nUpdateProject failed.\n\n");
                Environment.Exit(1);
            }
            Console.WriteLine($"\n\n{JsonSerializer.Serialize(updateResp, options)}\n\n");

            // get project
            getResp = await deepgramClient.GetProject(myId);
            if (getResp == null)
            {
                Console.WriteLine("\n\nGetProject failed.\n\n");
                Environment.Exit(1);
            }
            Console.WriteLine($"\n\n{JsonSerializer.Serialize(getResp, options)}\n\n");

            // delete project
            if (myDeleteId == null)
            {
                Console.WriteLine("");
                Console.WriteLine("This example requires a project who already exists who name is in the value \"DELETE_PROJECT_ID\".");
                Console.WriteLine("This is required to exercise the UpdateProject and DeleteProject function.");
                Console.WriteLine("In the absence of this, this example will exit early.");
                Console.WriteLine("");
                Environment.Exit(1);
            }

            var respDelete = await deepgramClient.DeleteProject(myDeleteId);
            if (respDelete == null)
            {
                Console.WriteLine("\n\nDeleteProject failed.\n\n");
                Environment.Exit(1);
            }
            Console.WriteLine($"\n\n{JsonSerializer.Serialize(respDelete, options)}\n\n");

            Console.WriteLine("\n\nPress any key to exit.");
            Console.ReadKey();

            // Teardown Library
            Library.Terminate();
        }
    }
}