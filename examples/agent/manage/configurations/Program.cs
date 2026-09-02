// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.AgentManage.v1;

namespace SampleApp
{
    /// <summary>
    /// Manages reusable Voice Agent configurations: create, list, get, update metadata, and
    /// delete. Once created, the agent_id can be passed in place of the full agent object in a
    /// Voice Agent Settings message. Demonstrates one end-to-end template variable reference:
    /// a DG_GREETING variable is created and referenced inside the configuration JSON as a
    /// bare, unquoted DG_GREETING token (no quotes, no braces).
    /// https://developers.deepgram.com/docs/reusable-agent-configurations
    /// </summary>
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Initialize Library with default logging
            // Normal logging is "Info" level
            Library.Initialize();

            // use the client factory with a API Key set with the "DEEPGRAM_API_KEY" environment variable
            var agentManageClient = ClientFactory.CreateAgentManageClient();

            // Track created remote resources so cleanup runs even when a later call fails.
            string? projectId = null;
            string? agentId = null;
            string? variableId = null;

            try
            {
                // find the project to manage agents in (first project on the account)
                var manageClient = ClientFactory.CreateManageClient();
                var projectsResp = await manageClient.GetProjects();
                projectId = projectsResp?.Projects is { Count: > 0 } projects ? projects[0].ProjectId : null;
                if (string.IsNullOrEmpty(projectId))
                {
                    Console.WriteLine("No projects found.");
                    return 1;
                }
                Console.WriteLine($"Using project: {projectId}");

                // create the template variable the configuration below references. The key gets
                // a unique per-run suffix because the live API currently reserves a deleted
                // variable's name forever within a project ("This project already has a variable
                // with that name") - a fixed key would make this example work exactly once.
                var variableKey = $"DG_GREETING_{DateTime.UtcNow:yyyyMMddHHmmss}";
                var variableResp = await agentManageClient.CreateAgentVariable(projectId, new AgentVariableSchema
                {
                    Key = variableKey,
                    Value = "Hello! How can I help you today?",
                });
                variableId = variableResp.VariableId;
                Console.WriteLine($"\nCreated agent variable {variableKey}: {variableId}");

                // create an agent configuration. Config is a JSON-encoded STRING representing the
                // agent block of a Settings message. The bare DG_* token (unquoted, no braces)
                // references the template variable created above; the service substitutes its
                // value when the configuration is used. Do not store secrets in the config or
                // metadata - they are visible to every member of the project.
                var createResp = await agentManageClient.CreateAgent(projectId, new AgentConfigurationSchema
                {
                    Config = $$"""
                    {
                        "language": "en",
                        "listen": { "provider": { "type": "deepgram", "model": "nova-3" } },
                        "think": {
                            "provider": { "type": "open_ai", "model": "gpt-4o-mini" },
                            "prompt": "You are a helpful customer service agent."
                        },
                        "greeting": {{variableKey}},
                        "speak": { "provider": { "type": "deepgram", "version": "v2", "model": "flux-kit-en" } }
                    }
                    """,
                    Metadata = new Dictionary<string, string>
                    {
                        ["name"] = "customer-service-agent",
                        ["environment"] = "development",
                    },
                });
                agentId = createResp.AgentId;
                Console.WriteLine($"\nCreated agent configuration: {agentId}");

                // list all agent configurations for the project
                var listResp = await agentManageClient.GetAgents(projectId);
                Console.WriteLine($"\nProject has {listResp.Agents?.Count ?? 0} agent configuration(s):");
                foreach (var agent in listResp.Agents ?? new List<AgentConfigurationResponse>())
                {
                    Console.WriteLine($"  {agent.AgentId}");
                }

                // get the configuration back. It is returned uninterpolated: the bare DG_GREETING
                // token appears as-is, not replaced with the variable's value.
                var getResp = await agentManageClient.GetAgent(projectId, agentId!);
                Console.WriteLine($"\nFetched agent configuration (uninterpolated):\n{getResp}");

                // update the metadata. The config itself is immutable - to change it, delete the
                // agent and create a new one. The API currently returns an empty body for this
                // call, so fetch the agent again to see the updated state.
                await agentManageClient.UpdateAgentMetadata(projectId, agentId!, new AgentMetadataSchema
                {
                    Metadata = new Dictionary<string, string>
                    {
                        ["name"] = "customer-service-agent",
                        ["environment"] = "production",
                    },
                });
                var updated = await agentManageClient.GetAgent(projectId, agentId!);
                Console.WriteLine($"\nUpdated agent metadata:\n{updated}");

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return 1;
            }
            finally
            {
                // Delete everything this example created, even after a failure, so no remote
                // resources are left behind. WARNING: deleting an agent configuration that live
                // sessions still reference can cause a production outage - migrate active
                // sessions to a new configuration first.
                if (projectId is not null && agentId is not null)
                {
                    try
                    {
                        await agentManageClient.DeleteAgent(projectId, agentId);
                        Console.WriteLine($"\nDeleted agent configuration: {agentId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Cleanup failed for agent configuration {agentId}: {ex.Message}");
                    }
                }
                if (projectId is not null && variableId is not null)
                {
                    try
                    {
                        await agentManageClient.DeleteAgentVariable(projectId, variableId);
                        Console.WriteLine($"Deleted agent variable: {variableId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Cleanup failed for agent variable {variableId}: {ex.Message}");
                    }
                }

                // Teardown Library
                Library.Terminate();
            }
        }
    }
}
