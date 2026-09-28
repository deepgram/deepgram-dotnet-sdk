---
title: "AuthClient"
description: "Grant temporary Deepgram tokens using the SDK's auth client."
---

Source files: `Deepgram/AuthClient.cs`, `Deepgram/Clients/Interfaces/v1/IAuthClient.cs`, `Deepgram/Clients/Auth/v1/Client.cs`.

Import paths:

- `using Deepgram;`
- `using Deepgram.Models.Auth.v1;`

Constructor:

```csharp
public AuthClient(
    string apiKey = "",
    DeepgramHttpClientOptions? deepgramClientOptions = null,
    string? httpId = null)
```

## Public methods

```csharp
Task<GrantTokenResponse> GrantToken(
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)

Task<GrantTokenResponse> GrantToken(
    GrantTokenSchema grantTokenSchema,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)
```

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `grantTokenSchema.TtlSeconds` | `int?` | `null` | Token TTL in seconds, validated to the range `1` to `3600`. |

Behavior notes from `Deepgram/Clients/Auth/v1/Client.cs`:

- The parameterless `GrantToken()` posts an empty body to `auth/grant`.
- The overload with `GrantTokenSchema` throws `ArgumentNullException` if you pass `null`.
- Both methods return `GrantTokenResponse`, which is the SDK's typed wrapper around the temporary JWT response.

Use this client when you want short-lived credentials in frontends or worker processes without distributing a long-lived API key to every runtime. The auth client still requires normal SDK authentication to mint the temporary token in the first place.

Response usage pattern:

- Request a token in a backend or trusted worker.
- Hand the short-lived token to a less-trusted runtime that should not hold the main API key.
- Build `DeepgramHttpClientOptions` or `DeepgramWsClientOptions` with `accessToken` instead of `apiKey`.

That pattern lines up with the credential-precedence logic in the options classes, where `AccessToken` always wins over `ApiKey` when both are present.

Example of consuming the minted token:

```csharp
var grant = await client.GrantToken(new GrantTokenSchema { TtlSeconds = 120 });

var wsOptions = new Deepgram.Models.Authenticate.v1.DeepgramWsClientOptions(
    accessToken: grant.AccessToken);

var live = ClientFactory.CreateListenWebSocketClient(options: wsOptions);
```

That flow is especially useful when your application has a trusted service layer that should keep the primary API key private while still enabling downstream live or REST requests.

## Example

```csharp
var client = ClientFactory.CreateAuthClient();

var token = await client.GrantToken(new GrantTokenSchema
{
    TtlSeconds = 300
});

Console.WriteLine(token);
```

Related pages: [Library and ClientFactory](/docs/api-reference/library-and-client-factory).
