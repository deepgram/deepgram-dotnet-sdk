---
title: "Client Factory and Options"
description: "How the SDK resolves credentials, chooses client versions, and configures REST or WebSocket behavior."
---

`ClientFactory` is the main entry point for application code. It hides the versioned namespaces under `Deepgram.Clients.*` and returns interfaces such as `IListenRESTClient`, `IAnalyzeClient`, `IListenWebSocketClient`, and `IAgentWebSocketClient`. The companion option types, `DeepgramHttpClientOptions` and `DeepgramWsClientOptions`, control authentication, base addresses, global headers, global addons, keepalive, and autoflush behavior.

The implementation lives in `Deepgram/ClientFactory.cs`, `Deepgram/Models/Authenticate/v1/DeepgramHttpClientOptions.cs`, and `Deepgram/Models/Authenticate/v1/DeepgramWsClientOptions.cs`.

## Why this concept exists

Without the factory, callers would need to know the current versioned client namespaces and construct them directly. Without the option classes, every client would need to repeat credential loading, environment-variable resolution, base-address normalization, and header setup. The factory plus option objects let the SDK change internals while keeping the public construction story stable.

## How it relates to other concepts

- Every REST concept in this SDK starts with `DeepgramHttpClientOptions`.
- Every streaming concept in this SDK starts with `DeepgramWsClientOptions`.
- `ClientFactory` decides which wrapper class you get, but the option object decides how that wrapper authenticates and where it connects.
- `Library.Initialize()` is independent from the factory; it only configures logging and does not create clients.

## How it works internally

`ClientFactory.CreateListenRESTClient()` returns `new ListenRESTClient(apiKey, options, httpId)`, and that wrapper inherits the current versioned implementation from `Deepgram/Clients/Listen/v1/REST/Client.cs`. The same pattern appears for analyze, manage, auth, self-hosted, speak REST, speak WebSocket, and agent WebSocket clients.

The version-specific overloads in `ClientFactory.cs` exist mainly for compatibility. For example, `CreateListenWebSocketClient(int version, ...)` can still return `Deepgram.Clients.Listen.v1.WebSocket.Client` or `Deepgram.Clients.Listen.v2.WebSocket.Client`, while the zero-version overload always returns the latest interface. Deprecated public wrappers such as `LiveClient`, `PreRecordedClient`, and `OnPremClient` still exist, but `ClientFactory` steers new code toward `ListenWebSocketClient`, `ListenRESTClient`, and `SelfHostedClient`.

`DeepgramHttpClientOptions` and `DeepgramWsClientOptions` resolve credentials in this order:

1. Explicit `accessToken`
2. Explicit `apiKey`
3. `DEEPGRAM_ACCESS_TOKEN`
4. `DEEPGRAM_API_KEY`

If neither credential exists and `OnPrem` is `false`, both option constructors throw `ArgumentException`. After credentials are resolved, the constructors normalize the base address. HTTP options append `/v1` when no API version is present and add `https://` when no protocol is present. WebSocket options append `/v1`, strip `http` or `https` if needed, and then ensure a `wss://` prefix.

## Basic usage

```csharp
using Deepgram;
using Deepgram.Models.Authenticate.v1;

Library.Initialize();

var options = new DeepgramHttpClientOptions(
    apiKey: "dg_api_key",
    baseAddress: "api.deepgram.com",
    headers: new Dictionary<string, string>
    {
        ["X-Correlation-Id"] = Guid.NewGuid().ToString()
    });

var client = ClientFactory.CreateManageClient(options: options);
var projects = await client.GetProjects();

Console.WriteLine($"Projects: {projects.Projects?.Count}");
Library.Terminate();
```

## Advanced usage

```csharp
using Deepgram;
using Deepgram.Models.Authenticate.v1;

Library.Initialize();

var wsOptions = new DeepgramWsClientOptions(
    accessToken: "short_lived_jwt",
    keepAlive: true,
    addons: new Dictionary<string, string>
    {
        ["auto_flush_reply_delta"] = "1500"
    },
    headers: new Dictionary<string, string>
    {
        ["X-Debug-Session"] = "live-demo"
    });

wsOptions.SetAccessToken("rotated_token");

var client = ClientFactory.CreateListenWebSocketClient(options: wsOptions);
Console.WriteLine(client.IsConnected());
Library.Terminate();
```

<Callout type="warn">Do not pass both an API key and an access token and then assume both will be used. The option classes intentionally clear one credential when the other is set, and `HttpClientFactory.ConfigureDeepgram` always prefers `AccessToken` over `ApiKey`.</Callout>

<Accordions>
<Accordion title="Factory convenience vs direct client construction">
Using `ClientFactory` keeps your application aligned with the SDK's preferred entry points and current versions. That matters because the wrapper classes in the `Deepgram` namespace are stable while the actual implementation namespaces are explicitly versioned and can shift between major releases. Direct construction is still useful when you want to target a specific legacy client for migration testing, but it increases the amount of SDK structure your application needs to know about. If you want forward compatibility, prefer `ClientFactory`; if you want deliberate version pinning for a transition period, use the versioned overloads and isolate that choice in one place.

```csharp
var latest = ClientFactory.CreateListenWebSocketClient();
var legacy = ClientFactory.CreateListenWebSocketClient(1);
```

</Accordion>
<Accordion title="Global options vs per-call headers and addons">
Global `Headers` and `Addons` on the option objects are useful when every request in a client should carry the same metadata. Per-call `headers` and `addons` parameters are better when only one operation needs extra flags like `smart_format=true` or custom correlation metadata. The trade-off is predictability: global values reduce call-site noise, but it becomes harder to understand why a request behaves a certain way just by reading one method invocation. A practical pattern is to keep credentials and invariant headers in options, then pass endpoint-specific query parameters per call.

```csharp
var response = await client.TranscribeUrl(
    new UrlSource(url),
    new PreRecordedSchema { Model = "nova-3" },
    addons: new Dictionary<string, string> { ["smart_format"] = "true" });
```

</Accordion>
</Accordions>
