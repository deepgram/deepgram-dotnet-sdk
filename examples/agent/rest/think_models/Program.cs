// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Agent.v1.REST;

namespace SampleApp;

class Program
{
    static async Task<int> Main()
    {
        Library.Initialize();

        try
        {
            // Uses DEEPGRAM_API_KEY when no explicit key is supplied.
            var client = ClientFactory.CreateAgentCatalogClient();
            var catalog = await client.GetThinkModels();

            foreach (var model in catalog.Models ?? new List<AgentThinkModel>())
            {
                Console.WriteLine($"{model.Provider}: {model.Id} ({model.Name})");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Exception: {ex.Message}");
            return 1;
        }
        finally
        {
            Library.Terminate();
        }
    }
}
