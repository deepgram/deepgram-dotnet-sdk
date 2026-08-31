// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.AgentManage.v1;

namespace SampleApp
{
    /// <summary>
    /// Manages reusable Voice Agent configurations: create, list, get, update metadata, and
    /// delete. Once created, the agent_id can be passed in place of the full agent object in a
    /// Voice Agent Settings message.
    /// https://developers.deepgram.com/docs/voice-agent/configuration/reusable-configurations
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize Library with default logging
            // Normal logging is "Info" level
            Library.Initialize();

            // use the client factory with a API Key set with the "DEEPGRAM_API_KEY" environment variable
            var agentManageClient = ClientFactory.CreateAgentManageClient();

            // find the project to manage agents in (first project on the account)
            var manageClient = ClientFactory.CreateManageClient();
            var projectsResp = await manageClient.GetProjects();
            var projectId = projectsResp?.Projects is { Count: > 0 } projects ? projects[0].ProjectId : null;
            if (string.IsNullOrEmpty(projectId))
            {
                Console.WriteLine("No projects found.");
                Environment.Exit(1);
            }
            Console.WriteLine($"Using project: {projectId}");

            // create an agent configuration. Config is a JSON-encoded STRING representing the
            // agent block of a Settings message. Do not store secrets in the config or
            // metadata - they are visible to every member of the project.
            var createResp = await agentManageClient.CreateAgent(projectId, new AgentConfigurationSchema
            {
                Config = """
                {
                    "language": "en",
                    "listen": { "provider": { "type": "deepgram", "model": "nova-3" } },
                    "think": {
                        "provider": { "type": "open_ai", "model": "gpt-4o-mini" },
                        "prompt": "You are a helpful customer service agent."
                    },
                    "speak": { "provider": { "type": "deepgram", "version": "v2", "model": "flux-kit-en" } }
                }
                """,
                Metadata = new Dictionary<string, string>
                {
                    ["name"] = "customer-service-agent",
                    ["environment"] = "development",
                },
            });
            Console.WriteLine($"\nCreated agent configuration: {createResp.AgentId}");
            var agentId = createResp.AgentId!;

            // list all agent configurations for the project
            var listResp = await agentManageClient.GetAgents(projectId);
            Console.WriteLine($"\nProject has {listResp.Agents?.Count ?? 0} agent configuration(s):");
            foreach (var agent in listResp.Agents ?? new List<AgentConfigurationResponse>())
            {
                Console.WriteLine($"  {agent.AgentId}");
            }

            // get the configuration back (uninterpolated: any {{DG_*}} template variable
            // placeholders appear as-is)
            var getResp = await agentManageClient.GetAgent(projectId, agentId);
            Console.WriteLine($"\nFetched agent configuration:\n{getResp}");

            // update the metadata. The config itself is immutable - to change it, delete the
            // agent and create a new one. The API currently returns an empty body for this
            // call, so fetch the agent again to see the updated state.
            await agentManageClient.UpdateAgentMetadata(projectId, agentId, new AgentMetadataSchema
            {
                Metadata = new Dictionary<string, string>
                {
                    ["name"] = "customer-service-agent",
                    ["environment"] = "production",
                },
            });
            var updated = await agentManageClient.GetAgent(projectId, agentId);
            Console.WriteLine($"\nUpdated agent metadata:\n{updated}");

            // delete the configuration. WARNING: deleting an agent configuration that live
            // sessions still reference can cause a production outage - migrate active sessions
            // to a new configuration first.
            await agentManageClient.DeleteAgent(projectId, agentId);
            Console.WriteLine($"\nDeleted agent configuration: {agentId}");

            // Teardown Library
            Library.Terminate();
        }
    }
}
