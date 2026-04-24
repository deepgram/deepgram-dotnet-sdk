---
name: using-management-api
description: Use when writing or reviewing C# code in this repo that calls Deepgram Management APIs for projects, models, keys, members, invitations, usage, balances, and auth token grants. Covers `ClientFactory.CreateManageClient()` and `ClientFactory.CreateAuthClient()`. Unlike some other SDKs, this repo does not currently expose reusable Voice Agent configuration management endpoints.
---

# Using Deepgram Management API (.NET SDK)

Administrative REST endpoints for projects, models, API keys, members, invites, usage, balances, and auth token grants.

## When to use this product

- List or mutate projects.
- List models or per-project model availability.
- Manage keys, members, invites, scopes, usage, balances.
- Mint short-lived bearer tokens with `GrantToken()`.

**Use a different skill when:**
- You want to run a live agent session → `using-voice-agent`.
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

## API reference (layered)

1. **In-repo source of truth**:
   - `Deepgram/ClientFactory.cs`
   - `Deepgram/Clients/Manage/v1/Client.cs`
   - `Deepgram/Clients/Auth/v1/Client.cs`
   - `Deepgram/Models/Manage/v1/*.cs`
   - `Deepgram/Models/Auth/v1/*.cs`
2. **Canonical OpenAPI (REST)**: https://developers.deepgram.com/openapi.yaml
3. **AsyncAPI**: not applicable for these admin endpoints
4. **Context7**:
   - repo mirror: `https://context7.com/deepgram/deepgram-dotnet-sdk`
   - docs corpus: `/llmstxt/developers_deepgram_llms_txt`
5. **Product docs**:
   - https://developers.deepgram.com/reference/manage/projects/list
   - https://developers.deepgram.com/reference/manage/models/list
   - https://developers.deepgram.com/reference/auth/grant-token

## Gotchas

1. **This repo does not currently expose Voice Agent configuration CRUD.** Do not copy Python `client.voice_agent.configurations.*` examples into C#.
2. **Management and auth are separate clients.** Use `CreateManageClient()` for admin APIs and `CreateAuthClient()` for `GrantToken()`.
3. **Destructive methods are real.** `DeleteProject`, `DeleteKey`, `DeleteInvite`, and `RemoveMember` should stay commented or guarded in examples/tests unless you mean it.
4. **Bearer-token auth is supported.** `DeepgramHttpClientOptions` prefers explicit `accessToken` over `apiKey`, then env vars in that order.
5. **Current examples often fetch a project ID first.** Many sub-resources are project-scoped.

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

## Central product skills

For cross-language Deepgram product knowledge — the consolidated API reference, documentation finder, focused runnable recipes, third-party integration examples, and MCP setup — install the central skills:

```bash
npx skills add deepgram/skills
```

This SDK ships language-idiomatic code skills; `deepgram/skills` ships cross-language product knowledge (see `api`, `docs`, `recipes`, `examples`, `starters`, `setup-mcp`).
