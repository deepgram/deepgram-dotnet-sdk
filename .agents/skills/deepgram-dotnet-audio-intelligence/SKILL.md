---
name: deepgram-dotnet-audio-intelligence
description: Use when writing or reviewing C# code in this repo that enables Deepgram intelligence overlays on Speech-to-Text requests. Covers `PreRecordedSchema` analytics flags such as `Summarize`, `Topics`, `Intents`, `Sentiment`, `DetectLanguage`, `DetectEntities`, `Diarize`, and `Redact`, plus the smaller live-streaming subset on `LiveSchema`. Use `deepgram-dotnet-speech-to-text` for plain transcription and `deepgram-dotnet-text-intelligence` for analytics on already-transcribed text.
---

# Using Deepgram Audio Intelligence (.NET SDK)

Audio analytics live on top of the same STT clients. Turn features on through schema properties.

## When to use this product

- You have audio and want transcript + analytics in one request.
- REST is the main path for summaries, topics, intents, sentiment, and language detection.

**Use a different skill when:**
- You only need transcript text → `deepgram-dotnet-speech-to-text`.
- You already have text and want `/read` → `deepgram-dotnet-text-intelligence`.

## Authentication

```bash
dotnet add package Deepgram
```

```csharp
using Deepgram;

Library.Initialize();
var client = ClientFactory.CreateListenRESTClient();
```

## Feature availability: REST vs WebSocket

| Feature | REST (`PreRecordedSchema`) | WebSocket (`LiveSchema`) |
|---|---|---|
| `Diarize` | yes | yes |
| `Redact` | yes | yes |
| `DetectEntities` | yes | not surfaced in `LiveSchema` |
| `Summarize` | yes (`string`) | no |
| `Topics` / `Intents` / `Sentiment` | yes | no |
| `DetectLanguage` | yes | no |
| `CustomTopic` / `CustomIntent` | yes | no |

## Quick start — REST

```csharp
using Deepgram.Models.Listen.v1.REST;

var client = ClientFactory.CreateListenRESTClient();

var response = await client.TranscribeUrl(
    new UrlSource("https://dpgr.am/spacewalk.wav"),
    new PreRecordedSchema()
    {
        Model = "nova-3",
        Punctuate = true,
        SmartFormat = true,
        Diarize = true,
        Summarize = "v2",
        Topics = true,
        Intents = true,
        Sentiment = true,
        DetectLanguage = true,
        DetectEntities = true,
        Redact = new List<string> { "pii", "numbers" },
        Language = "en",
    });

Console.WriteLine(response.Results.Channels[0].Alternatives[0].Transcript);
Console.WriteLine(response.Results.Summary);
```

## Quick start — live subset

```csharp
using Deepgram.Models.Listen.v2.WebSocket;

var liveClient = ClientFactory.CreateListenWebSocketClient();

await liveClient.Subscribe(new EventHandler<ResultResponse>((sender, e) =>
{
    Console.WriteLine(e.Channel.Alternatives[0].Transcript);
}));

await liveClient.Connect(new LiveSchema()
{
    Model = "nova-3",
    Diarize = true,
    Redact = new List<string> { "pii" },
    Encoding = "linear16",
    SampleRate = 16000,
});
```

## Key params

REST (`PreRecordedSchema`): `Summarize`, `Topics`, `Intents`, `Sentiment`, `DetectLanguage`, `DetectEntities`, `CustomTopic`, `CustomTopicMode`, `CustomIntent`, `CustomIntentMode`, `Diarize`, `DiarizeVersion`, `Redact`, `Utterances`, plus the regular STT knobs.

Live (`LiveSchema`): `Diarize`, `Redact`, `UtteranceEnd`, `VadEvents`, `Punctuate`, `SmartFormat`, plus normal streaming STT settings.

## API reference (layered)

1. **In-repo source of truth**:
   - `Deepgram/Models/Listen/v1/REST/PreRecordedSchema.cs`
   - `Deepgram/Models/Listen/v2/WebSocket/LiveSchema.cs`
   - `Deepgram/Clients/Listen/v1/REST/Client.cs`
   - `Deepgram/Clients/Listen/v2/WebSocket/Client.cs`
2. **Canonical OpenAPI (REST)**: https://developers.deepgram.com/openapi.yaml
3. **Canonical AsyncAPI (streaming / WSS)**: https://developers.deepgram.com/asyncapi.yaml
4. **Context7**:
   - repo mirror: `https://context7.com/deepgram/deepgram-dotnet-sdk`
   - docs corpus: `/llmstxt/developers_deepgram_llms_txt`
5. **Product docs**:
   - https://developers.deepgram.com/docs/stt-intelligence-feature-overview
   - https://developers.deepgram.com/docs/summarization
   - https://developers.deepgram.com/docs/topic-detection
   - https://developers.deepgram.com/docs/intent-recognition
   - https://developers.deepgram.com/docs/sentiment-analysis
   - https://developers.deepgram.com/docs/language-detection
   - https://developers.deepgram.com/docs/redaction
   - https://developers.deepgram.com/docs/diarization

## Gotchas

1. **`Summarize` on STT is a string.** Use `Summarize = "v2"`, not `true`.
2. **Most intelligence overlays are REST-only.** The live schema does not expose summary/topic/intent/sentiment/language options.
3. **Redaction is a `List<string>`.** This SDK does not expose the Python-style union of string-or-array.
4. **Custom topics and intents need modes.** Set `CustomTopicMode` / `CustomIntentMode` or the model behavior will not match expectations.
5. **Diarization is available in both paths, but the event shapes differ.** Live results expose speaker data word-by-word in streaming responses.

## Example files in this repo

- `examples/speech-to-text/rest/summary/Program.cs`
- `examples/speech-to-text/rest/sentiment/Program.cs`
- `examples/speech-to-text/rest/topic/Program.cs`
- `examples/speech-to-text/rest/intent/Program.cs`
- `examples/speech-to-text/rest/file/Program.cs`

## Central product skills

For cross-language Deepgram product knowledge — the consolidated API reference, documentation finder, focused runnable recipes, third-party integration examples, and MCP setup — install the central skills:

```bash
npx skills add deepgram/skills
```

This SDK ships language-idiomatic code skills; `deepgram/skills` ships cross-language product knowledge (see `api`, `docs`, `recipes`, `examples`, `starters`, `setup-mcp`).
