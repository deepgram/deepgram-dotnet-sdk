---
title: "FluxSpeakRESTClient"
description: "Flux TTS v2 Speak REST methods for complete audio files, streams, and callbacks."
---

Source files: `Deepgram/FluxSpeakRESTClient.cs`, `Deepgram/Clients/Interfaces/v2/IFluxSpeakRESTClient.cs`, and `Deepgram/Clients/Flux/Speak/REST/Client.cs`.

Imports:

- `using Deepgram;`
- `using Deepgram.Models.Flux.Speak.REST;`

## Public Methods

```csharp
Task<SyncResponse> ToStream(TextSource source, SpeakSchema? speakSchema,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
Task<SyncResponse> ToFile(TextSource source, string filename, SpeakSchema? speakSchema,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
Task<AsyncResponse> StreamCallBack(TextSource source, string? callBack, SpeakSchema? speakSchema,
    CancellationTokenSource? cancellationToken = default,
    Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
```

`SpeakSchema.Model` is required and must be a Flux TTS model such as `flux-alexis-en`. Batch requests support `linear16`, `flac`, `mulaw`, `alaw`, `mp3`, `opus`, and `aac`, plus compatible `container`, `sample_rate`, and `bit_rate` options. `Speed` accepts 0.85 through 1.15 in 0.05 increments; `Expressivity` is beta and accepts -2 through 2.

```csharp
var client = ClientFactory.CreateFluxSpeakRESTClient();
var response = await client.ToFile(
    new TextSource("Your appointment is confirmed for 3pm tomorrow."),
    "confirmation.mp3",
    new SpeakSchema
    {
        Model = "flux-alexis-en",
        Encoding = "mp3",
        BitRate = 48000,
        Speed = 1.05
    });

Console.WriteLine($"Wrote {response.Filename}: {response.RequestId}");
```

Related pages: [FluxSpeakWebSocketClient](/docs/api-reference/flux-speak-websocket-client), [Speech Synthesis](/docs/speech-synthesis).
