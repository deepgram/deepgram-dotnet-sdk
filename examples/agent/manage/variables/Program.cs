// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.AgentManage.v1;

namespace SampleApp
{
    /// <summary>
    /// Manages template variables for reusable Voice Agent configurations: create, list, get,
    /// update, and delete. Variables follow the DG_&lt;VARIABLE_NAME&gt; naming format and can
    /// substitute any JSON value in an agent configuration via {{DG_VARIABLE_NAME}}
    /// placeholders.
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

            // find the project to manage variables in (first project on the account)
            var manageClient = ClientFactory.CreateManageClient();
            var projectsResp = await manageClient.GetProjects();
            var projectId = projectsResp?.Projects is { Count: > 0 } projects ? projects[0].ProjectId : null;
            if (string.IsNullOrEmpty(projectId))
            {
                Console.WriteLine("No projects found.");
                Environment.Exit(1);
            }
            Console.WriteLine($"Using project: {projectId}");

            // create a template variable. Value can be any valid JSON type - here a string.
            // Do not store secrets in variables - they are visible to every member of the
            // project.
            var createResp = await agentManageClient.CreateAgentVariable(projectId, new AgentVariableSchema
            {
                Key = "DG_GREETING",
                Value = "Hello! How can I help you today?",
            });
            Console.WriteLine($"\nCreated agent variable: {createResp.VariableId} ({createResp.Key})");
            var variableId = createResp.VariableId!;

            // list all template variables for the project
            var listResp = await agentManageClient.GetAgentVariables(projectId);
            Console.WriteLine($"\nProject has {listResp.Variables?.Count ?? 0} agent variable(s):");
            foreach (var variable in listResp.Variables ?? new List<AgentVariableResponse>())
            {
                Console.WriteLine($"  {variable.Key} = {variable.Value}");
            }

            // get the variable back
            var getResp = await agentManageClient.GetAgentVariable(projectId, variableId);
            Console.WriteLine($"\nFetched agent variable:\n{getResp}");

            // update the value (the key cannot be changed after creation)
            var updateResp = await agentManageClient.UpdateAgentVariable(projectId, variableId, new UpdateAgentVariableSchema
            {
                Value = "Welcome back! What can I do for you?",
            });
            Console.WriteLine($"\nUpdated agent variable:\n{updateResp}");

            // delete the variable
            await agentManageClient.DeleteAgentVariable(projectId, variableId);
            Console.WriteLine($"\nDeleted agent variable: {variableId}");

            // Teardown Library
            Library.Terminate();
        }
    }
}
