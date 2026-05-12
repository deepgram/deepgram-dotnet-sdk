---
name: deepgram-dotnet-management-api
description: "Use when writing or reviewing C# code in this repo that calls Deepgram Management APIs for projects, models, keys, members, invitations, usage, balances, and auth token grants. Covers `ClientFactory.CreateManageClient()` and `ClientFactory.CreateAuthClient()`. Unlike some other SDKs, this repo does not currently expose reusable Voice Agent configuration management endpoints."
---

# Using Deepgram Management API (.NET SDK)

Administrative REST endpoints for projects, models, API keys, members, invites, usage, balances, and auth token grants.

## When to use this product

- List or mutate projects.
- List models or per-project model availability.
- Manage keys, members, invites, scopes, usage, balances.
- Mint short-lived bearer tokens with `GrantToken()`.

**Use a different skill when:**
- You want to run a live agent session → `deepgram-dotnet-voice-agent`.
- You want STT/TTS rather than project administration.

## Authentication

```bash
dotnet add package Deepgram
```

```csharp
using Deepgram;

Library.Initialize();
var manageClient = ClientFactory.CreateManageClient();
var authClient = ClientFactory.CreateAuthClient();
```

Both factory calls read credentials from the `DEEPGRAM_API_KEY` (or `DEEPGRAM_ACCESS_TOKEN`) environment variable by default. To pass them explicitly: `ClientFactory.CreateManageClient(apiKey: "...", options: ...)` / `ClientFactory.CreateAuthClient(apiKey: "...", options: ...)`. `DeepgramHttpClientOptions` throws if neither the env var nor an explicit credential is provided.

## Quick start — projects and models

```csharp
using Deepgram.Models.Manage.v1;

var client = ClientFactory.CreateManageClient();

var projects = await client.GetProjects();
var projectId = projects.Projects[0].ProjectId;

var project = await client.GetProject(projectId);
var models = await client.GetModels(new ModelSchema { IncludeOutdated = true });
var projectModels = await client.GetProjectModels(projectId);
```

## Quick start — keys / invites / members / usage

```csharp
var client = ClientFactory.CreateManageClient();
var projectId = (await client.GetProjects()).Projects[0].ProjectId;

var key = await client.CreateKey(projectId, new KeySchema()
{
    Comment = "MyTestKey",
    Scopes = new List<string> { "member" },
});

await client.SendInvite(projectId, new InviteSchema()
{
    Email = "spam@spam.com",
    Scope = "member",
});

var members = await client.GetMembers(projectId);
var usage = await client.GetUsageRequests(projectId, new UsageRequestsSchema());
var balances = await client.GetBalances(projectId);
```

## Quick start — auth token grant

```csharp
using Deepgram.Models.Auth.v1;
using Deepgram.Models.Authenticate.v1;

var authClient = ClientFactory.CreateAuthClient();
var token = await authClient.GrantToken(new GrantTokenSchema
{
    TtlSeconds = 300,
});

var bearerOptions = new DeepgramHttpClientOptions(accessToken: token.AccessToken);
var prerecordClient = ClientFactory.CreateListenRESTClient(options: bearerOptions);
```

## Key methods

Management:
- `GetProjects`, `GetProject`, `UpdateProject`, `DeleteProject`, `LeaveProject`
- `GetModels`, `GetModel`, `GetProjectModels`, `GetProjectModel`
- `GetKeys`, `GetKey`, `CreateKey`, `DeleteKey`
- `GetInvites`, `SendInvite`, `DeleteInvite`
- `GetMembers`, `GetMemberScopes`, `UpdateMemberScope`, `RemoveMember`
- `GetUsageRequests`, `GetUsageRequest`, `GetUsageFields`, `GetUsageSummary`
- `GetBalances`, `GetBalance`

Auth:
- `GrantToken()`
- `GrantToken(GrantTokenSchema)`

## References

- In-repo: `Deepgram/Clients/Manage/v1/Client.cs`, `Deepgram/Clients/Auth/v1/Client.cs`, `Deepgram/Models/Manage/v1/*.cs`, `Deepgram/Models/Auth/v1/*.cs`
- OpenAPI: https://developers.deepgram.com/openapi.yaml
- Product docs: https://developers.deepgram.com/reference/manage/projects/list, https://developers.deepgram.com/reference/auth/grant-token

## Guard pattern for destructive operations

```csharp
// 1. Verify the resource exists
var key = await client.GetKey(projectId, keyId);
Console.WriteLine($"Found key: {key.ApiKey.Comment} ({keyId})");

// 2. Delete
await client.DeleteKey(projectId, keyId);

// 3. Verify deletion succeeded
try
{
    await client.GetKey(projectId, keyId);
    Console.Error.WriteLine("ERROR: key still exists after deletion");
}
catch
{
    Console.WriteLine("Key deleted successfully.");
}
```

## Gotchas

1. **This repo does not currently expose Voice Agent configuration CRUD.** Do not copy Python `client.voice_agent.configurations.*` examples into C#.
2. **Destructive methods are irreversible.** `DeleteProject`, `DeleteKey`, `DeleteInvite`, and `RemoveMember` should always use the verify-delete-verify pattern above.
3. **Bearer-token auth is supported.** `DeepgramHttpClientOptions` prefers explicit `accessToken` over `apiKey`, then env vars in that order.
4. **Most sub-resources are project-scoped.** Fetch a project ID first via `GetProjects()` before calling key/member/usage methods.

## Example files in this repo

- `examples/manage/projects/Program.cs`
- `examples/manage/models/Program.cs`
- `examples/manage/keys/Program.cs`
- `examples/manage/members/Program.cs`
- `examples/manage/scopes/Program.cs`
- `examples/manage/invitations/Program.cs`
- `examples/manage/usage/Program.cs`
- `examples/manage/balances/Program.cs`
- `examples/auth/grant-token/Program.cs`
- `examples/auth/bearer-token-workflow/Program.cs`

Cross-language product knowledge (API reference, recipes, MCP setup): `npx skills add deepgram/skills`.
