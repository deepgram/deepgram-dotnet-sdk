---
title: "AnalyzeClient"
description: "Text analysis methods for intent, sentiment, topic, and summary workflows."
---

Source files: `Deepgram/AnalyzeClient.cs`, `Deepgram/Clients/Interfaces/v1/IAnalyzeClient.cs`, `Deepgram/Clients/Analyze/v1/Client.cs`.

Import paths:

- `using Deepgram;`
- `using Deepgram.Models.Analyze.v1;`

Constructor:

```csharp
public AnalyzeClient(
    string apiKey = "",
    DeepgramHttpClientOptions? deepgramClientOptions = null,
    string? httpId = null)
```

## Public methods

```csharp
Task<SyncResponse> AnalyzeUrl(UrlSource source, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
Task<SyncResponse> AnalyzeText(TextSource source, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
Task<SyncResponse> AnalyzeFile(byte[] source, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
Task<SyncResponse> AnalyzeFile(Stream source, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
Task<AsyncResponse> AnalyzeFileCallBack(byte[] source, string? callBack, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
Task<AsyncResponse> AnalyzeFileCallBack(Stream source, string? callBack, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
Task<AsyncResponse> AnalyzeUrlCallBack(UrlSource source, string? callBack, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
Task<AsyncResponse> AnalyzeTextCallBack(TextSource source, string? callBack, AnalyzeSchema? analyzeSchema, CancellationTokenSource? cancellationToken = default, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
```

## Main schema parameters

| Property | Type | Default | Description |
|-----------|------|---------|-------------|
| `Language` | `string?` | `null` | Input language. Current schema comments document English support. |
| `Summarize` | `bool?` | `null` | Generate a summary. |
| `Sentiment` | `bool?` | `null` | Detect sentiment shifts. |
| `Topics` | `bool?` | `null` | Detect topics. |
| `Intents` | `bool?` | `null` | Detect intent segments. |
| `CustomIntent` | `List<string>?` | `null` | Custom intents. |
| `CustomTopic` | `List<string>?` | `null` | Custom topics. |
| `CallBack` | `string?` | `null` | Async callback URL. |

## Example

```csharp
var client = ClientFactory.CreateAnalyzeClient();

var response = await client.AnalyzeText(
    new TextSource("The customer sounds frustrated but still wants to renew."),
    new AnalyzeSchema
    {
        Language = "en",
        Sentiment = true,
        Intents = true,
        Summarize = true
    });
```

Related pages: [Client Factory and Options](/docs/client-factory-and-options), [Types](/docs/types).
