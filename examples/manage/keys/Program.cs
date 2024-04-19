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
                Console.WriteLine($"ListProjects() - ID: {myId}, Name: {myName}");
                break;
            }

            // list keys
            var listResp = await deepgramClient.GetKeys(myId);
            if (listResp == null)
            {
                Console.WriteLine("\n\nNo keys found\n\n");
            }
            else
            {
                Console.WriteLine($"\n\n{listResp}\n\n");
            }

            // create key
            var createKey = new KeySchema()
            {
                Comment = "MyTestKey",
                Scopes = new List<string> { "member" },
            };

            string myKeyId = null;
            var createResp = await deepgramClient.CreateKey(myId, createKey);
            if (createResp == null)
            {
                Console.WriteLine("\n\nCreateKey failed.\n\n");
                Environment.Exit(1);
            }
            else
            {
                myKeyId = createResp.ApiKeyId;
                Console.WriteLine($"\n\n{createResp}\n\n");
            }

            // list keys
            listResp = await deepgramClient.GetKeys(myId);
            if (listResp == null)
            {
                Console.WriteLine("\n\nNo keys found\n\n");
            }
            else
            {
                Console.WriteLine($"\n\n{listResp}\n\n");
            }

            // get key
            var getResp = await deepgramClient.GetKey(myId, myKeyId);
            if (getResp == null)
            {
                Console.WriteLine("\n\nGetKey failed.\n\n");
                Environment.Exit(1);
            }
            else
            {
                Console.WriteLine($"\n\n{getResp}\n\n");
            }

            // delete key
            var deleteResp = await deepgramClient.DeleteKey(myId, myKeyId);
            if (deleteResp == null)
            {
                Console.WriteLine("\n\nDeleteKey failed.\n\n");
                Environment.Exit(1);
            }
            else
            {
                Console.WriteLine($"\n\n{deleteResp}\n\n");
            }

            // list keys
            listResp = await deepgramClient.GetKeys(myId);
            if (listResp == null)
            {
                Console.WriteLine("\n\nNo keys found\n\n");
            }
            else
            {
                Console.WriteLine($"\n\n{listResp}\n\n");
            }


            Console.WriteLine("\n\nPress any key to exit.");
            Console.ReadKey();

            // Teardown Library
            Library.Terminate();
        }
    }
}