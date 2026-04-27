---
name: deepgram-dotnet-text-to-speech
description: Use when writing or reviewing C# code in this repo that calls Deepgram Text-to-Speech. Covers `ClientFactory.CreateSpeakRESTClient()` with `ToStream` / `ToFile`, and `ClientFactory.CreateSpeakWebSocketClient()` with `Connect`, `SpeakWithText`, `Flush`, and streaming `AudioResponse` events. Use `deepgram-dotnet-voice-agent` for full-duplex assistants instead of one-way synthesis.
---

# Using Deepgram Text-to-Speech (.NET SDK)

Convert text to audio via REST or low-latency streaming WebSocket synthesis.

## When to use this product

- **REST** — synthesize complete text and save or process the returned audio.
- **WebSocket** — stream text into Deepgram and receive audio chunks back incrementally.

**Use a different skill when:**
- You need an agent that listens, thinks, and speaks in one session → `deepgram-dotnet-voice-agent`.

## Authentication

```bash
dotnet add package Deepgram
```

```csharp
using Deepgram;

Library.Initialize();
var client = ClientFactory.CreateSpeakRESTClient();
```

The SDK reads `DEEPGRAM_API_KEY` by default and also supports bearer access tokens through `DeepgramHttpClientOptions` / `DeepgramWsClientOptions`.

## Quick start — REST

```csharp
using Deepgram;
using Deepgram.Models.Speak.v1.REST;

Library.Initialize();

var client = ClientFactory.CreateSpeakRESTClient();
var response = await client.ToFile(
    new TextSource("Hello World!"),
    "output.mp3",
    new SpeakSchema()
    {
        Model = "aura-2-thalia-en",
    });

Console.WriteLine(response);
```

If you want the bytes in memory, call `ToStream(...)` and read `response.Stream`.

## Quick start — WebSocket

```csharp
using Deepgram;
using Deepgram.Models.Speak.v2.WebSocket;

Library.Initialize();

var speakClient = ClientFactory.CreateSpeakWebSocketClient();

await speakClient.Subscribe(new EventHandler<AudioResponse>((sender, e) =>
{
    if (e.Stream != null)
    {
        // Streaming Speak (Encoding = "linear16") delivers raw PCM — no WAV header.
        // Save as .raw, or prepend a valid WAV header (see examples/text-to-speech/websocket/simple/Program.cs).
        using (var writer = new BinaryWriter(File.Open("output.raw", FileMode.Append)))
        {
            writer.Write(e.Stream.ToArray());
        }
    }
}));

bool connected = await speakClient.Connect(new SpeakSchema()
{
    Encoding = "linear16",
    SampleRate = 48000,
});

if (connected)
{
    speakClient.SpeakWithText("Hello World!");
    speakClient.Flush();
    Console.ReadKey();
    await speakClient.Stop();
}
```

## Key params

REST `SpeakSchema`: `Model`, `BitRate`, `CallBack`, `CallBackMethod`, `Container`, `Encoding`, `SampleRate`.

WebSocket `SpeakSchema`: `Model`, `BitRate`, `Encoding`, `SampleRate`.

Streaming controls: `SpeakWithText`, `Flush`, `Clear`, `Close`, `SendMessageImmediately`.

## API reference (layered)

1. **In-repo source of truth**:
   - `Deepgram/ClientFactory.cs`
   - `Deepgram/Clients/Speak/v1/REST/Client.cs`
   - `Deepgram/Clients/Speak/v2/WebSocket/Client.cs`
   - `Deepgram/Models/Speak/v1/REST/SpeakSchema.cs`
   - `Deepgram/Models/Speak/v2/WebSocket/SpeakSchema.cs`
2. **Canonical OpenAPI (REST)**: https://developers.deepgram.com/openapi.yaml
3. **Canonical AsyncAPI (streaming / WSS)**: https://developers.deepgram.com/asyncapi.yaml
4. **Context7**:
   - repo mirror: `https://context7.com/deepgram/deepgram-dotnet-sdk`
   - docs corpus: `/llmstxt/developers_deepgram_llms_txt`
5. **Product docs**:
   - https://developers.deepgram.com/reference/text-to-speech/speak-request
   - https://developers.deepgram.com/reference/text-to-speech/speak-streaming
   - https://developers.deepgram.com/docs/tts-models

## Gotchas

1. **Methods are `ToStream` / `ToFile`, not `GenerateAsync`.** Use the actual .NET names.
2. **REST returns audio metadata plus a `MemoryStream`.** `ToFile` writes the file for you and then clears `response.Stream`.
3. **WebSocket output arrives as `AudioResponse.Stream`.** You must write or play the bytes yourself.
4. **Flush matters.** If you never call `Flush()`, you may wait indefinitely for the buffered text to synthesize.
5. **Autoflush is configurable.** `DeepgramWsClientOptions.AutoFlushSpeakDelta` can flush automatically for token-by-token input.
6. **Match output format to your sink.** If you request `linear16` + `48000`, your WAV header / playback path must match.
7. **Callback flows are separate.** Use `StreamCallBack(...)` for async REST callback processing.

## Example files in this repo

- `examples/text-to-speech/rest/file/hello-world/Program.cs`
- `examples/text-to-speech/rest/file/woodchuck/Program.cs`
- `examples/text-to-speech/websocket/simple/Program.cs`
- `tests/edge_cases/tts_v1_client_example/`

## Central product skills

For cross-language Deepgram product knowledge — the consolidated API reference, documentation finder, focused runnable recipes, third-party integration examples, and MCP setup — install the central skills:

```bash
npx skills add deepgram/skills
```

This SDK ships language-idiomatic code skills; `deepgram/skills` ships cross-language product knowledge (see `api`, `docs`, `recipes`, `examples`, `starters`, `setup-mcp`).
