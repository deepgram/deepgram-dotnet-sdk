---
title: "AgentManageClient"
description: "Manage reusable Voice Agent configurations and project-scoped template variables."
---

Source files: `Deepgram/AgentManageClient.cs`, `Deepgram/Clients/Interfaces/v1/IAgentManageClient.cs`, and `Deepgram/Clients/AgentManage/v1/Client.cs`.

Imports:

- `using Deepgram;`
- `using Deepgram.Models.AgentManage.v1;`

Create the client with `ClientFactory.CreateAgentManageClient()`. It manages reusable agent configurations at `/v1/projects/{project_id}/agents` and template variables at `/v1/projects/{project_id}/agent-variables`; it is separate from `AgentWebSocketClient`, which runs live conversations.

## Public Methods

```csharp
Task<AgentConfigurationsResponse> GetAgents(string projectId, ...)
Task<AgentConfigurationResponse> GetAgent(string projectId, string agentId, ...)
Task<AgentConfigurationResponse> CreateAgent(string projectId, AgentConfigurationSchema configuration, ...)
Task<AgentConfigurationResponse> UpdateAgentMetadata(string projectId, string agentId, AgentMetadataSchema metadata, ...)
Task<DeleteResponse> DeleteAgent(string projectId, string agentId, ...)

Task<AgentVariablesResponse> GetAgentVariables(string projectId, ...)
Task<AgentVariableResponse> GetAgentVariable(string projectId, string variableId, ...)
Task<AgentVariableResponse> CreateAgentVariable(string projectId, AgentVariableSchema variable, ...)
Task<AgentVariableResponse> UpdateAgentVariable(string projectId, string variableId, UpdateAgentVariableSchema update, ...)
Task<DeleteResponse> DeleteAgentVariable(string projectId, string variableId, ...)
```

`AgentConfigurationSchema.Config` is a JSON-encoded string representing the `agent` block of a Voice Agent settings message, not a nested C# object. Configurations are immutable after creation; `UpdateAgentMetadata` only replaces metadata. Template-variable keys use the `DG_<VARIABLE_NAME>` form and values may be any JSON type. Variables and metadata are visible to project members, so do not store secrets there.

```csharp
var projectId = Environment.GetEnvironmentVariable("DEEPGRAM_PROJECT_ID")
    ?? throw new InvalidOperationException("Set DEEPGRAM_PROJECT_ID to a disposable project.");
var client = ClientFactory.CreateAgentManageClient();

var variable = await client.CreateAgentVariable(projectId, new AgentVariableSchema
{
    Key = "DG_GREETING_20260917",
    Value = "Hello! How can I help you today?"
});

var agents = await client.GetAgents(projectId);
Console.WriteLine($"Found {agents.Agents?.Count ?? 0} reusable agent configurations.");
```

Use a disposable project for create/delete examples. A deleted variable name remains reserved in its project, and deleting a configuration still used by live sessions can interrupt those sessions.

Related pages: [Manage Project Resources](/docs/guides/manage-project-resources), [AgentWebSocketClient](/docs/api-reference/agent-websocket-client), and [reusable agent configurations](https://developers.deepgram.com/docs/reusable-agent-configurations).
