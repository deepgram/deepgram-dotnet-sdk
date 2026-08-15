---
title: "SpeakRESTClient"
description: "REST text-to-speech methods for returning audio streams, files, or callback jobs."
---

Source files: `Deepgram/SpeakRestClient.cs`, `Deepgram/Clients/Interfaces/v1/ISpeakRESTClient.cs`, `Deepgram/Clients/Speak/v1/REST/Client.cs`.

Import paths:

- `using Deepgram;`
- `using Deepgram.Models.Speak.v1.REST;`

Constructor:

```csharp
public SpeakRESTClient(
    string apiKey = "",
    DeepgramHttpClientOptions? deepgramClientOptions = null,
    string? httpId = null)
```

## Public methods

```csharp
Task<SyncResponse> ToStream(
    TextSource source,
    SpeakSchema? speakSchema,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)

Task<SyncResponse> ToFile(
    TextSource source,
    string filename,
    SpeakSchema? speakSchema,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)

Task<AsyncResponse> StreamCallBack(
    TextSource source,
    string? callBack,
    SpeakSchema? speakSchema,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)
```

## Main schema parameters

| Property | Type | Default | Description |
|-----------|------|---------|-------------|
| `Model` | `string?` | `null` | Voice model such as `aura-2-thalia-en`. |
| `BitRate` | `string?` | `null` | Audio bitrate. |
| `Container` | `string?` | `null` | Audio container format. |
| `Encoding` | `string?` | `null` | Audio encoding. |
| `SampleRate` | `string?` | `null` | Audio sample rate. |
| `CallBack` | `string?` | `null` | Callback URL for asynchronous generation. |
| `CallBackMethod` | `string?` | `null` | Callback method metadata. |

## Return shape

`SyncResponse` exposes:

- `ContentType`
- `RequestId`
- `ModelUUID`
- `ModelName`
- `Characters`
- `TransferEncoding`
- `Date`
- `Stream`
- `Filename`

Implementation detail:

`SpeakRESTClient.ToStream` does not deserialize a JSON body the way transcription and management methods do. Instead, `Deepgram/Clients/Speak/v1/REST/Client.cs` reads response headers like request ID, model metadata, content type, and character count, then combines them with the returned audio stream. That is why `ToFile` is implemented as a thin wrapper around `ToStream` plus a file write.

## Example

```csharp
var client = ClientFactory.CreateSpeakRESTClient();

var response = await client.ToFile(
    new TextSource("Ready for playback."),
    "ready.mp3",
    new SpeakSchema
    {
        Model = "aura-2-thalia-en"
    });
```

The callback variant, `StreamCallBack`, follows the same exclusive-callback rule used by prerecorded transcription and analysis: set the callback URL either in the method argument or in `SpeakSchema.CallBack`, but not in both places.

Related pages: [Speech Synthesis](/docs/speech-synthesis), [SpeakWebSocketClient](/docs/api-reference/speak-websocket-client).
