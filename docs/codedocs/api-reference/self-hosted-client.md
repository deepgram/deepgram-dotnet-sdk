---
title: "Self-Hosted Distribution Credentials"
description: "Manage current self-hosted distribution credentials attached to Deepgram projects."
---

Source files: `Deepgram/SelfHostedDistributionCredentialsClient.cs`, `Deepgram/Clients/Interfaces/v1/ISelfHostedDistributionCredentialsClient.cs`, `Deepgram/Clients/SelfHosted/v1/DistributionCredentialsClient.cs`.

Import paths:

- `using Deepgram;`
- `using Deepgram.Models.SelfHosted.v1;`

Constructor:

```csharp
public SelfHostedDistributionCredentialsClient(
    string apiKey = "",
    DeepgramHttpClientOptions? deepgramClientOptions = null,
    string? httpId = null)
```

## Current Client

```csharp
var client = ClientFactory.CreateSelfHostedDistributionCredentialsClient();
```

```csharp
Task<CredentialsResponse> ListDistributionCredentials(
    string projectId,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)

Task<CredentialResponse> GetDistributionCredentials(
    string projectId,
    string distributionCredentialsId,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)

Task<CredentialResponse> CreateDistributionCredentials(
    string projectId,
    DistributionCredentialsCreateSchema? credentialsSchema = null,
    DistributionCredentialsCreateOptions? options = null,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)

Task<CredentialResponse> DeleteDistributionCredentials(
    string projectId,
    string distributionCredentialsId,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)
```

## Request Types

| Property | Type | Default | Description |
|-----------|------|---------|-------------|
| `DistributionCredentialsCreateSchema.Comment` | `string?` | `null` | Human-readable description in the JSON body. |
| `DistributionCredentialsCreateOptions.Scopes` | `List<string>?` | `null` | Credential scopes, repeated in the query string. |
| `DistributionCredentialsCreateOptions.Provider` | `string?` | `null` | Distribution provider in the query string; the hosted API supports `quay`. |

## Example

```csharp
var client = ClientFactory.CreateSelfHostedDistributionCredentialsClient();

var credential = await client.CreateDistributionCredentials(
    projectId: "project_id",
    credentialsSchema: new DistributionCredentialsCreateSchema
    {
        Comment = "Credential for staging cluster",
    },
    options: new DistributionCredentialsCreateOptions
    {
        Provider = "quay",
        Scopes = new List<string> { "self-hosted:product:api" }
    });
```

Operational notes:

- `ListDistributionCredentials` is the entry point you usually call first because the delete and get operations both depend on a distribution credentials ID.
- `CreateDistributionCredentials` uses the same REST plumbing as the rest of the SDK, so you can still attach custom headers or addons for operational tracing.
- `CreateSelfHostedClient()` remains available for applications using the legacy on-prem route and request shape.

Use the distribution credentials client for new code.

## Legacy Client

`SelfHostedClient` and `Clients.SelfHosted.v1.Client` are frozen legacy clients for the retired
on-prem route. They remain available for source compatibility but are marked obsolete; do not add
new functionality to either surface.

Related pages: [ManageClient](/docs/api-reference/manage-client).
