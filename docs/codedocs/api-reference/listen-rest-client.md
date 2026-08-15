---
title: "ListenRESTClient"
description: "REST transcription methods, callback variants, and the PreRecordedSchema options they rely on."
---

Source files: `Deepgram/ListenRESTClient.cs`, `Deepgram/Clients/Interfaces/v1/IListenRESTClient.cs`, `Deepgram/Clients/Listen/v1/REST/Client.cs`.

Import paths:

- `using Deepgram;`
- `using Deepgram.Models.Listen.v1.REST;`

Concrete constructor:

```csharp
public ListenRESTClient(
    string apiKey = "",
    DeepgramHttpClientOptions? deepgramClientOptions = null,
    string? httpId = null)
```

## Constructor parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `apiKey` | `string` | `""` | Explicit API key if you are not using environment variables or `DeepgramHttpClientOptions`. |
| `deepgramClientOptions` | `DeepgramHttpClientOptions?` | `null` | REST auth, base address, headers, and addons configuration. |
| `httpId` | `string?` | `null` | Optional `HttpClient` name passed to the SDK's internal factory. |

## Public methods

### TranscribeUrl

```csharp
Task<SyncResponse> TranscribeUrl(
    UrlSource source,
    PreRecordedSchema? prerecordedSchema,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)
```

### TranscribeFile

```csharp
Task<SyncResponse> TranscribeFile(
    byte[] source,
    PreRecordedSchema? prerecordedSchema,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)

Task<SyncResponse> TranscribeFile(
    Stream source,
    PreRecordedSchema? prerecordedSchema,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)
```

### TranscribeUrlCallBack

```csharp
Task<AsyncResponse> TranscribeUrlCallBack(
    UrlSource source,
    string? callBack,
    PreRecordedSchema? prerecordedSchema,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)
```

### TranscribeFileCallBack

```csharp
Task<AsyncResponse> TranscribeFileCallBack(
    byte[] source,
    string? callBack,
    PreRecordedSchema? prerecordedSchema,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)

Task<AsyncResponse> TranscribeFileCallBack(
    Stream source,
    string? callBack,
    PreRecordedSchema? prerecordedSchema,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)
```

## Common schema parameters

`PreRecordedSchema` is large, but these are the most used fields in application code:

| Property | Type | Default | Description |
|-----------|------|---------|-------------|
| `Model` | `string?` | `null` | Recognition model such as `nova-3`. |
| `Language` | `string?` | `null` | Spoken language hint. |
| `Punctuate` | `bool?` | `null` | Add punctuation and capitalization. |
| `SmartFormat` | `bool?` | `null` | Format transcript output for readability. |
| `Diarize` | `bool?` | `null` | Enable speaker diarization. |
| `Keyterm` | `List<string>?` | `null` | Keyterm prompting. |
| `Keywords` | `List<string>?` | `null` | Keyword boosting or suppression. |
| `DetectEntities` | `bool?` | `null` | Enable named entity extraction. |
| `DetectTopics` | `bool?` | `null` | Enable topic detection. |
| `Summarize` | `bool?` | `null` | Enable summarization in supported flows. |
| `CallBack` | `string?` | `null` | Callback URL for asynchronous processing. |

## Returns

- `SyncResponse` for synchronous transcription
- `AsyncResponse` for callback workflows

## Example

```csharp
var client = ClientFactory.CreateListenRESTClient();

var response = await client.TranscribeFile(
    File.ReadAllBytes("call.wav"),
    new PreRecordedSchema
    {
        Model = "nova-3",
        Punctuate = true,
        SmartFormat = true,
        Diarize = true
    });
```

Related pages: [REST Transcription](/docs/rest-transcription), [Types](/docs/types).
