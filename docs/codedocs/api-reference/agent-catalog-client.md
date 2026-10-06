---
title: "AgentCatalogClient"
description: "List Think models currently available for Voice Agent sessions."
---

Source files: `Deepgram/AgentCatalogClient.cs`, `Deepgram/Clients/Interfaces/v1/IAgentCatalogClient.cs`, and `Deepgram/Clients/Agent/v1/REST/Client.cs`.

Imports:

- `using Deepgram;`
- `using Deepgram.Models.Agent.v1.REST;`

Create the client with `ClientFactory.CreateAgentCatalogClient()`. It reads the current Voice Agent Think-model catalog from `GET /v1/agent/settings/think/models` on `agent.deepgram.com`. The response is a catalog, so `Provider`, `Id`, and `Name` remain strings rather than closed enums and may expand as models are added.

## Public Methods

```csharp
Task<AgentThinkModelsResponse> GetThinkModels(...)
```

```csharp
var client = ClientFactory.CreateAgentCatalogClient();
var catalog = await client.GetThinkModels();

foreach (var model in catalog.Models ?? new List<AgentThinkModel>())
{
    Console.WriteLine($"{model.Provider}: {model.Id} ({model.Name})");
}
```

The request is read-only and does not need a project ID. Configure an API key through the client constructor or `DEEPGRAM_API_KEY`.

Related pages: [AgentWebSocketClient](/docs/api-reference/agent-websocket-client), [AgentManageClient](/docs/api-reference/agent-manage-client), and [Library and ClientFactory](/docs/api-reference/library-and-client-factory).
