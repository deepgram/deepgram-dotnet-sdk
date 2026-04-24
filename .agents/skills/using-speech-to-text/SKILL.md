---
name: using-speech-to-text
description: Use when writing or reviewing C# code in this repo that calls Deepgram Speech-to-Text for prerecorded or live transcription. Covers `ClientFactory.CreateListenRESTClient()` with `TranscribeUrl` / `TranscribeFile`, and `ClientFactory.CreateListenWebSocketClient()` with `Connect`, `Subscribe`, and `Send`. Use `using-audio-intelligence` for summaries/sentiment/topics overlays, `using-conversational-stt` for Flux-specific work (not fully surfaced in this SDK), and `using-voice-agent` for full-duplex assistants.
---

# Using Deepgram Speech-to-Text (.NET SDK)

Basic transcription for prerecorded audio (REST) or live audio (WebSocket).

## When to use this product

- **REST** — one-shot transcription of a file, stream, or hosted URL.
- **WebSocket** — continuous live transcription for microphone, file streaming, or relay streams.

**Use a different skill when:**
- You want summaries, sentiment, topics, intents, redaction, or diarization guidance on `/listen` → `using-audio-intelligence`.
- You need Flux / conversational turn-taking semantics → `using-conversational-stt`.
- You need STT + LLM + TTS over one socket → `using-voice-agent`.

## Authentication

```bash
dotnet add package Deepgram
```

```csharp
using Deepgram;

Library.Initialize();

// Reads DEEPGRAM_API_KEY by default.
var client = ClientFactory.CreateListenRESTClient();
```

The SDK also accepts `DEEPGRAM_ACCESS_TOKEN` via `DeepgramHttpClientOptions` / `DeepgramWsClientOptions`. In this repo, async methods return `Task<T>` but do **not** use an `Async` suffix.

## Quick start — REST (URL)

```csharp
using Deepgram;
using Deepgram.Models.Listen.v1.REST;

Library.Initialize();

var client = ClientFactory.CreateListenRESTClient();
var response = await client.TranscribeUrl(
    new UrlSource("https://dpgr.am/bueller.wav"),
    new PreRecordedSchema()
    {
        Model = "nova-3",
        SmartFormat = true,
        Punctuate = true,
        Keyterm = new List<string> { "Bueller" },
    });

Console.WriteLine(response.Results.Channels[0].Alternatives[0].Transcript);
```

## Quick start — REST (file)

```csharp
using Deepgram.Models.Listen.v1.REST;

var client = ClientFactory.CreateListenRESTClient();
var audioData = File.ReadAllBytes("audio.wav");

var response = await client.TranscribeFile(
    audioData,
    new PreRecordedSchema()
    {
        Model = "nova-3",
        Punctuate = true,
    });
```

You can also call `TranscribeFile(Stream source, ...)` when you already have a `Stream`.

## Quick start — WebSocket (live)

```csharp
using Deepgram;
using Deepgram.Microphone;
using Deepgram.Models.Listen.v2.WebSocket;

Library.Initialize();

var liveClient = ClientFactory.CreateListenWebSocketClient();

await liveClient.Subscribe(new EventHandler<ResultResponse>((sender, e) =>
{
    var transcript = e.Channel.Alternatives[0].Transcript;
    if (!string.IsNullOrWhiteSpace(transcript))
    {
        Console.WriteLine(transcript);
    }
}));

bool connected = await liveClient.Connect(new LiveSchema()
{
    Model = "nova-3",
    Encoding = "linear16",
    SampleRate = 16000,
    InterimResults = true,
    UtteranceEnd = "1000",
    VadEvents = true,
});

if (connected)
{
    var microphone = new Microphone(liveClient.Send);
    microphone.Start();
    Console.ReadKey();
    microphone.Stop();
    await liveClient.Stop();
}
```

## Key params

REST `PreRecordedSchema`: `Model`, `Language`, `Encoding`, `SampleRate`, `Punctuate`, `SmartFormat`, `Keywords`, `Keyterm`, `Utterances`, `Paragraphs`, `Numerals`, `MultiChannel`, `Replace`, `Search`, `Tag`, `Version`.

WebSocket `LiveSchema`: `Model`, `Encoding`, `SampleRate`, `Channels`, `InterimResults`, `UtteranceEnd`, `VadEvents`, `Punctuate`, `SmartFormat`, `Endpointing`, `Keywords`, `Keyterm`, `NoDelay`.

## API reference (layered)

1. **In-repo source of truth**:
   - `Deepgram/ClientFactory.cs`
   - `Deepgram/Clients/Listen/v1/REST/Client.cs`
   - `Deepgram/Clients/Listen/v2/WebSocket/Client.cs`
   - `Deepgram/Models/Listen/v1/REST/PreRecordedSchema.cs`
   - `Deepgram/Models/Listen/v2/WebSocket/LiveSchema.cs`
2. **Canonical OpenAPI (REST)**: https://developers.deepgram.com/openapi.yaml
3. **Canonical AsyncAPI (streaming / WSS)**: https://developers.deepgram.com/asyncapi.yaml
4. **Context7**:
   - repo mirror: `https://context7.com/deepgram/deepgram-dotnet-sdk`
   - docs corpus: `/llmstxt/developers_deepgram_llms_txt`
5. **Product docs**:
   - https://developers.deepgram.com/reference/speech-to-text/listen-pre-recorded
   - https://developers.deepgram.com/reference/speech-to-text/listen-streaming

## Gotchas

1. **Use the real .NET surface.** This SDK uses `await client.TranscribeUrl(...)`, not `TranscribeUrlAsync(...)`.
2. **REST and streaming use different model namespaces.** REST is `Deepgram.Models.Listen.v1.REST`; live is `Deepgram.Models.Listen.v2.WebSocket`.
3. **Streaming events are subscription-based.** Register `Subscribe(...)` handlers before `Connect(...)`.
4. **Raw audio must match `Encoding` + `SampleRate`.** Wrong declarations produce bad transcripts or server errors.
5. **`Keyterm` is guarded.** `Listen.v2.WebSocket.Client.Connect` throws if you use `Keyterm` with a non-`nova-3` model.
6. **Callback flows are separate methods.** Use `TranscribeUrlCallBack` / `TranscribeFileCallBack`; sync methods reject `CallBack` in `PreRecordedSchema`.
7. **`Deepgram.Microphone` is optional.** It is a helper package/project, not required for file or URL transcription.

## Example files in this repo

- `examples/speech-to-text/rest/url/Program.cs`
- `examples/speech-to-text/rest/file/Program.cs`
- `examples/speech-to-text/websocket/file/Program.cs`
- `examples/speech-to-text/websocket/http/Program.cs`
- `examples/speech-to-text/websocket/microphone/Program.cs`
- `tests/edge_cases/stt_v1_client_example/`
