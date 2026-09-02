// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.AgentManage.v1;

namespace SampleApp
{
    /// <summary>
    /// Manages template variables for reusable Voice Agent configurations: create, list, get,
    /// update, and delete. Variables follow the DG_&lt;VARIABLE_NAME&gt; naming format and can
    /// substitute any JSON value in an agent configuration. A configuration references a
    /// variable as a bare, unquoted token - e.g. "greeting": DG_GREETING (no quotes, no
    /// braces); see the configurations example for that reference end to end.
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
            string? variableId = null;

            try
            {
                // find the project to manage variables in (first project on the account)
                var manageClient = ClientFactory.CreateManageClient();
                var projectsResp = await manageClient.GetProjects();
                projectId = projectsResp?.Projects is { Count: > 0 } projects ? projects[0].ProjectId : null;
                if (string.IsNullOrEmpty(projectId))
                {
                    Console.WriteLine("No projects found.");
                    return 1;
                }
                Console.WriteLine($"Using project: {projectId}");

                // create a template variable. Value can be any valid JSON type - here a string.
                // IsSensitive defaults to false (the only value the API accepts today). Do not
                // store secrets in variables - they are visible to every member of the project.
                var createResp = await agentManageClient.CreateAgentVariable(projectId, new AgentVariableSchema
                {
                    Key = "DG_GREETING",
                    Value = "Hello! How can I help you today?",
                });
                variableId = createResp.VariableId;
                Console.WriteLine($"\nCreated agent variable: {variableId}");

                // list all template variables for the project
                var listResp = await agentManageClient.GetAgentVariables(projectId);
                Console.WriteLine($"\nProject has {listResp.Variables?.Count ?? 0} agent variable(s):");
                foreach (var variable in listResp.Variables ?? new List<AgentVariableResponse>())
                {
                    Console.WriteLine($"  {variable.Key} = {variable.Value}");
                }

                // get the variable back
                var getResp = await agentManageClient.GetAgentVariable(projectId, variableId!);
                Console.WriteLine($"\nFetched agent variable:\n{getResp}");

                // update the value (the key cannot be changed after creation). The API currently
                // returns an empty body for this call, so fetch the variable again to see the
                // updated state.
                await agentManageClient.UpdateAgentVariable(projectId, variableId!, new UpdateAgentVariableSchema
                {
                    Value = "Welcome back! What can I do for you?",
                });
                var updated = await agentManageClient.GetAgentVariable(projectId, variableId!);
                Console.WriteLine($"\nUpdated agent variable:\n{updated}");

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return 1;
            }
            finally
            {
                // Delete the variable this example created, even after a failure, so no remote
                // resources are left behind.
                if (projectId is not null && variableId is not null)
                {
                    try
                    {
                        await agentManageClient.DeleteAgentVariable(projectId, variableId);
                        Console.WriteLine($"\nDeleted agent variable: {variableId}");
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
