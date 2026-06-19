---
title: "Library and ClientFactory"
description: "Initialization, teardown, and the factory methods that create Deepgram SDK clients."
---

Source files: `Deepgram/Library.cs`, `Deepgram/ClientFactory.cs`, `Deepgram/Models/Authenticate/v1/DeepgramHttpClientOptions.cs`, `Deepgram/Models/Authenticate/v1/DeepgramWsClientOptions.cs`.

Import paths:

- `using Deepgram;`
- `using Deepgram.Models.Authenticate.v1;`

## Library

Signature:

```csharp
public class Library
{
    public static void Initialize(Logger.LogLevel level = Logger.LogLevel.Default, string? filename = "log.txt");
    public static void Terminate();
}
```

`Initialize` configures the SDK logger through `Log.Initialize`. `Terminate` is currently a no-op for the core package.

## ClientFactory

Primary signatures:

```csharp
public static class ClientFactory
{
    public static IAgentWebSocketClient CreateAgentWebSocketClient(string apiKey = "", DeepgramWsClientOptions? options = null);
    public static IAnalyzeClient CreateAnalyzeClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null);
    public static IListenWebSocketClient CreateListenWebSocketClient(string apiKey = "", DeepgramWsClientOptions? options = null);
    public static IAuthClient CreateAuthClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null);
    public static IManageClient CreateManageClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null);
    public static ISelfHostedClient CreateSelfHostedClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null);
    public static IListenRESTClient CreateListenRESTClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null);
    public static ISpeakRESTClient CreateSpeakRESTClient(string apiKey = "", DeepgramHttpClientOptions? options = null, string? httpId = null);
    public static ISpeakWebSocketClient CreateSpeakWebSocketClient(string apiKey = "", DeepgramWsClientOptions? options = null);
}
```

Version-specific overloads also exist and return `object` for migration scenarios. Those overloads are useful when you need older WebSocket client versions, but new code should use the latest overloads above.

## Constructor and option parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `apiKey` | `string` | `""` | Explicit API key. If omitted, the SDK falls back to environment variables or an access token in the options object. |
| `options` | `DeepgramHttpClientOptions` or `DeepgramWsClientOptions` | `null` | Shared configuration object for auth, base address, headers, addons, keepalive, and autoflush. |
| `httpId` | `string?` | `null` | Optional name for the underlying `HttpClient` instance created by the SDK. |

## Option types

`DeepgramHttpClientOptions` constructor:

```csharp
public DeepgramHttpClientOptions(
    string? apiKey = null,
    string? baseAddress = null,
    bool? onPrem = null,
    Dictionary<string, string>? options = null,
    Dictionary<string, string>? headers = null,
    string? accessToken = null)
```

`DeepgramWsClientOptions` constructor:

```csharp
public DeepgramWsClientOptions(
    string? apiKey = null,
    string? baseAddress = null,
    bool? keepAlive = null,
    bool? onPrem = null,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null,
    string? accessToken = null)
```

Key properties:

| Property | Type | Default | Description |
|-----------|------|---------|-------------|
| `ApiKey` | `string` | resolved | API key credential. |
| `AccessToken` | `string` | resolved | OAuth-style bearer token credential. |
| `BaseAddress` | `string` | `api.deepgram.com` | Base host, normalized by the option constructor. |
| `APIVersion` | `string` | `v1` | API version appended when the base address has no explicit version. |
| `Headers` | `Dictionary<string, string>` | empty | Global headers added to every request. |
| `Addons` | `Dictionary<string, string>` | empty | Global query-like options added to request construction. |
| `OnPrem` | `bool` | `false` | Allows empty credentials in self-hosted scenarios. |
| `KeepAlive` | `bool` | `false` | WebSocket only. Enables periodic keepalive sends. |
| `AutoFlushReplyDelta` | `decimal` | `0` | WebSocket listen only. Enables reply autoflush inspection. |
| `AutoFlushSpeakDelta` | `decimal` | `0` | WebSocket speak only. Enables speak autoflush inspection. |

Helper methods:

```csharp
public void SetApiKey(string apiKey);
public void SetAccessToken(string accessToken);
public void ClearCredentials();
```

## Example

```csharp
using Deepgram;
using Deepgram.Models.Authenticate.v1;

Library.Initialize();

var options = new DeepgramWsClientOptions(
    accessToken: "jwt_here",
    keepAlive: true,
    headers: new Dictionary<string, string>
    {
        ["X-Trace-Id"] = Guid.NewGuid().ToString()
    });

var client = ClientFactory.CreateListenWebSocketClient(options: options);
Library.Terminate();
```

Related pages: [ListenRESTClient](/docs/api-reference/listen-rest-client), [ListenWebSocketClient](/docs/api-reference/listen-websocket-client), [AgentWebSocketClient](/docs/api-reference/agent-websocket-client).
