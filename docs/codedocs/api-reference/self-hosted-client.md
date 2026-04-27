---
title: "SelfHostedClient"
description: "Manage self-hosted credentials attached to Deepgram projects."
---

Source files: `Deepgram/SelfHostedClient.cs`, `Deepgram/Clients/Interfaces/v1/ISelfHostedClient.cs`, `Deepgram/Clients/SelfHosted/v1/Client.cs`.

Import paths:

- `using Deepgram;`
- `using Deepgram.Models.SelfHosted.v1;`

Constructor:

```csharp
public SelfHostedClient(
    string apiKey = "",
    DeepgramHttpClientOptions? deepgramClientOptions = null,
    string? httpId = null)
```

## Public methods

```csharp
Task<CredentialsResponse> ListCredentials(
    string projectId,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)

Task<CredentialResponse> GetCredentials(
    string projectId,
    string credentialsId,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)

Task<MessageResponse> DeleteCredentials(
    string projectId,
    string credentialsId,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)

Task<CredentialResponse> CreateCredentials(
    string projectId,
    CredentialsSchema credentialsSchema,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)
```

## Credentials schema

| Property | Type | Default | Description |
|-----------|------|---------|-------------|
| `Comment` | `string?` | `null` | Human-readable description. |
| `Scopes` | `List<string>?` | `null` | Credential scopes. |
| `Provider` | `string?` | `null` | Backing provider label. |

## Example

```csharp
var client = ClientFactory.CreateSelfHostedClient();

var credential = await client.CreateCredentials(
    projectId: "project_id",
    credentialsSchema: new CredentialsSchema
    {
        Comment = "Credential for staging cluster",
        Provider = "aws",
        Scopes = new List<string> { "read", "write" }
    });
```

Operational notes:

- `ListCredentials` is the entry point you usually call first because the delete and get operations both depend on a `credentialsId`.
- `CreateCredentials` uses the same REST plumbing as the rest of the SDK, so you can still attach custom headers or addons for operational tracing.
- The public wrapper `OnPremClient` is deprecated in favor of `SelfHostedClient`, and the source code marks that older wrapper as frozen.

If you are migrating from older code that referenced `OnPremClient`, this page's method set is the one to standardize on going forward.

Related pages: [ManageClient](/docs/api-reference/manage-client).
