---
title: "ManageClient"
description: "Project, key, invite, member, model, usage, and balance management methods."
---

Source files: `Deepgram/ManageClient.cs`, `Deepgram/Clients/Interfaces/v1/IManageClient.cs`, `Deepgram/Clients/Manage/v1/Client.cs`.

Import paths:

- `using Deepgram;`
- `using Deepgram.Models.Manage.v1;`

Constructor:

```csharp
public ManageClient(
    string apiKey = "",
    DeepgramHttpClientOptions? deepgramClientOptions = null,
    string? httpId = null)
```

## Public methods

Projects and models:

```csharp
Task<ProjectsResponse> GetProjects(...)
Task<ProjectResponse> GetProject(string projectId, ...)
Task<MessageResponse> UpdateProject(string projectId, ProjectSchema updateProjectSchema, ...)
Task<MessageResponse> DeleteProject(string projectId, ...)
Task<MessageResponse> LeaveProject(string projectId, ...)
Task<ModelsResponse> GetProjectModels(string projectId, ModelSchema? modelSchema = null, ...)
Task<ModelResponse> GetProjectModel(string projectId, string modelId, ...)
Task<ModelsResponse> GetModels(ModelSchema? modelSchema = null, ...)
Task<ModelResponse> GetModel(string modelId, ...)
```

Keys, invites, and members:

```csharp
Task<KeysResponse> GetKeys(string projectId, ...)
Task<KeyScopeResponse> GetKey(string projectId, string keyId, ...)
Task<KeyResponse> CreateKey(string projectId, KeySchema keySchema, ...)
Task<MessageResponse> DeleteKey(string projectId, string keyId, ...)
Task<InvitesResponse> GetInvites(string projectId, ...)
Task<MessageResponse> DeleteInvite(string projectId, string email, ...)
Task<MessageResponse> SendInvite(string projectId, InviteSchema inviteSchema, ...)
Task<MembersResponse> GetMembers(string projectId, ...)
Task<MemberScopesResponse> GetMemberScopes(string projectId, string memberId, ...)
Task<MessageResponse> UpdateMemberScope(string projectId, string memberId, MemberScopeSchema scopeSchema, ...)
Task<MessageResponse> RemoveMember(string projectId, string memberId, ...)
```

Usage and balances:

```csharp
Task<UsageRequestsResponse> GetUsageRequests(string projectId, UsageRequestsSchema usageRequestsSchema, ...)
Task<UsageRequestResponse> GetUsageRequest(string projectId, string requestId, ...)
Task<UsageSummaryResponse> GetUsageSummary(string projectId, UsageSummarySchema summarySchema, ...)
Task<UsageFieldsResponse> GetUsageFields(string projectId, UsageFieldsSchema fieldsSchema, ...)
Task<BalancesResponse> GetBalances(string projectId, ...)
Task<BalanceResponse> GetBalance(string projectId, string balanceId, ...)
```

## Common schema parameters

| Schema | Key fields | Notes |
|-----------|------|-------------|
| `ProjectSchema` | `Name`, `Company` | Used by `UpdateProject`. |
| `KeySchema` | `Comment`, `Scopes`, `Tags`, `ExpirationDate`, `TimeToLiveInSeconds` | `CreateKey` throws if both expiration fields are set. |
| `ModelSchema` | query-style filter fields | Used when listing models. |
| `UsageRequestsSchema` / `UsageSummarySchema` / `UsageFieldsSchema` | usage filters | Used for reporting endpoints. |

## Example

```csharp
var client = ClientFactory.CreateManageClient();

var projects = await client.GetProjects();
var projectId = projects.Projects!.First().ProjectId;

var key = await client.CreateKey(
    projectId,
    new KeySchema
    {
        Comment = "Temporary admin key",
        Scopes = new List<string> { "member:read" },
        TimeToLiveInSeconds = 3600
    });
```

Related pages: [Guides: Manage Project Resources](/docs/guides/manage-project-resources), [SelfHostedClient](/docs/api-reference/self-hosted-client).
