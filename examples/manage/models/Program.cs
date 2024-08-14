// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

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

            // use the client factory with a API Key set with the "DEEPGRAM_API_KEY" environment variable
            var deepgramClient = ClientFactory.CreateManageClient();

            // get projects
            var projectResp = await deepgramClient.GetProjects();
            if (projectResp == null)
            {
                Console.WriteLine("ListProjects failed.");
                Environment.Exit(1);
            }

            string projectId = "";
            foreach (var project in projectResp.Projects)
            {
                projectId = project.ProjectId ?? "";
                string myName = project.Name ?? "";
                Console.WriteLine($"ListProjects() - ID: {projectId}, Name: {myName}");
                break;
            }
            Console.WriteLine("\n\n\n");

            // get models
            var modelListResp = await deepgramClient.GetModels();
            if (modelListResp == null)
            {
                Console.WriteLine("GetModels failed.");
                Environment.Exit(1);
            }

            string modelId = "";
            foreach (var model in modelListResp.Stt)
            {
                modelId = model.Uuid ?? "";
                string modelName = model.Name ?? "";
                Console.WriteLine($"Speech-to-Text Model - ID: {modelId}, Name: {modelName}");
            }
            foreach (var model in modelListResp.Tts)
            {
                string myId = model.Uuid ?? "";
                string myName = model.Name ?? "";
                Console.WriteLine($"Text-to-Speech - ID: {myId}, Name: {myName}");
            }
            Console.WriteLine("\n\n\n");

            // get model
            var modelResp = await deepgramClient.GetModel(modelId);
            if (modelResp == null)
            {
                Console.WriteLine("No invites found");
            }
            else
            {
                Console.WriteLine($"{modelResp}");
            }
            Console.WriteLine("\n\n\n");

            // get models per project
            var projModelsResp = await deepgramClient.GetProjectModels(projectId);
            if (projModelsResp == null)
            {
                Console.WriteLine("GetModels failed.");
                Environment.Exit(1);
            }

            modelId = "";
            foreach (var model in projModelsResp.Stt)
            {
                modelId = model.Uuid ?? "";
                string modelName = model.Name ?? "";
                Console.WriteLine($"Speech-to-Text Model - ID: {modelId}, Name: {modelName}");
            }
            foreach (var model in projModelsResp.Tts)
            {
                string myId = model.Uuid ?? "";
                string myName = model.Name ?? "";
                Console.WriteLine($"Text-to-Speech - ID: {myId}, Name: {myName}");
            }
            Console.WriteLine("\n\n\n");

            // get model
            var projModelResp = await deepgramClient.GetProjectModel(projectId, modelId);
            if (projModelResp == null)
            {
                Console.WriteLine("\n\nNo invites found\n\n");
            }
            else
            {
                Console.WriteLine($"{projModelResp}");
            }

            Console.WriteLine("\n\nPress any key to exit.");
            Console.ReadKey();

            // Teardown Library
            Library.Terminate();
        }
    }
}