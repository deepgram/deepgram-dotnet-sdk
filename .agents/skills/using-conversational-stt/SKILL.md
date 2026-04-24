---
name: using-conversational-stt
description: Use when evaluating or extending conversational / Flux-style streaming STT support in this C# SDK. The repo has a latest WebSocket listen client under `Deepgram.Models.Listen.v2.WebSocket`, but it does not currently expose Flux-specific request params or turn-aware response types like `TurnInfo`. Use this skill to document that gap honestly and to guide work on the closest supported surface.
---

# Using Deepgram Conversational STT / Flux (.NET SDK)

This repo does **not** currently expose a dedicated Flux / conversational STT API surface comparable to the Python SDK's `listen.v2.connect(...)` + `TurnInfo` flow.

## When to use this product

- Use this skill when someone asks for **Flux**, **conversational STT**, **turn-taking**, or `/v2/listen` support in the .NET SDK.
- Use it when deciding whether the current `Listen.v2.WebSocket` client is sufficient for the task.

**Use a different skill when:**
- You only need standard streaming transcription with `ResultResponse`, `SpeechStartedResponse`, and `UtteranceEndResponse` → `using-speech-to-text`.
- You need a full voice assistant instead of transcription-only → `using-voice-agent`.

## Current repo status

What exists:
- `ClientFactory.CreateListenWebSocketClient()` returns the latest WebSocket listen client.
- Request model: `Deepgram.Models.Listen.v2.WebSocket.LiveSchema`.
- Event models: `OpenResponse`, `MetadataResponse`, `ResultResponse`, `SpeechStartedResponse`, `UtteranceEndResponse`, `CloseResponse`, `ErrorResponse`, `UnhandledResponse`.
- Control helpers: `SendKeepAlive()`, `SendFinalize()`, `SendClose()`, `Send(...)`.

What is **not** present in the current repo search:
- No `flux` model constants or examples.
- No `TurnInfo` / turn-aware event models.
- No `language_hint`, `eager_eot_threshold`, `eot_threshold`, or similar Flux request properties.
- No README/examples that mention conversational STT explicitly.

## Authentication

```bash
dotnet add package Deepgram
```

```csharp
using Deepgram;

Library.Initialize();
var client = ClientFactory.CreateListenWebSocketClient();
```

## Closest supported code path today

```csharp
using Deepgram.Models.Listen.v2.WebSocket;

var liveClient = ClientFactory.CreateListenWebSocketClient();

await liveClient.Subscribe(new EventHandler<ResultResponse>((sender, e) =>
{
    var transcript = e.Channel.Alternatives[0].Transcript;
    if (!string.IsNullOrWhiteSpace(transcript))
    {
        Console.WriteLine(transcript);
    }
}));

await liveClient.Subscribe(new EventHandler<UtteranceEndResponse>((sender, e) =>
{
    Console.WriteLine(e.Type);
}));

await liveClient.Connect(new LiveSchema()
{
    Model = "nova-3",
    Encoding = "linear16",
    SampleRate = 16000,
    InterimResults = true,
    UtteranceEnd = "1000",
    VadEvents = true,
});
```

Treat this as **standard live STT**, not true Flux parity.

## Key params currently available

On `LiveSchema`: `Model`, `Encoding`, `SampleRate`, `InterimResults`, `UtteranceEnd`, `VadEvents`, `Endpointing`, `NoDelay`, `Punctuate`, `SmartFormat`, `Keywords`, `Keyterm`, `Diarize`, `Redact`.

## API reference (layered)

1. **In-repo source of truth**:
   - `Deepgram/ClientFactory.cs`
   - `Deepgram/Clients/Listen/v2/WebSocket/Client.cs`
   - `Deepgram/Models/Listen/v2/WebSocket/LiveSchema.cs`
   - `Deepgram/Models/Listen/v2/WebSocket/*.cs`
2. **Canonical OpenAPI**: not the primary reference for this streaming gap
3. **Canonical AsyncAPI (what the product can support, even if this SDK is incomplete)**: https://developers.deepgram.com/asyncapi.yaml
4. **Context7**:
   - repo mirror: `https://context7.com/deepgram/deepgram-dotnet-sdk`
   - docs corpus: `/llmstxt/developers_deepgram_llms_txt`
5. **Product docs**:
   - https://developers.deepgram.com/reference/speech-to-text/listen-flux
   - https://developers.deepgram.com/docs/flux/quickstart
   - https://developers.deepgram.com/docs/flux/language-prompting

## Gotchas

1. **Be honest: Flux is not first-class here yet.** Do not invent `TurnInfo`-style .NET models or `ConnectFluxAsync(...)` helpers.
2. **The repo's `Listen.v2.WebSocket` naming is misleading if you expect Python parity.** It is the newest streaming client, but not a full conversational surface.
3. **Default API version still resolves from shared options.** `DeepgramWsClientOptions` defaults `APIVersion` to `v1`, so inspect connection URIs before assuming `/v2/listen` behavior.
4. **If the task requires real Flux parity, start by adding models, request params, examples, and tests.** Do not paper over the gap in docs or code.

## Example files in this repo

- `examples/speech-to-text/websocket/file/Program.cs`
- `examples/speech-to-text/websocket/http/Program.cs`
- `examples/speech-to-text/websocket/microphone/Program.cs`

## Central product skills

For cross-language Deepgram product knowledge — the consolidated API reference, documentation finder, focused runnable recipes, third-party integration examples, and MCP setup — install the central skills:

```bash
npx skills add deepgram/skills
```

This SDK ships language-idiomatic code skills; `deepgram/skills` ships cross-language product knowledge (see `api`, `docs`, `recipes`, `examples`, `starters`, `setup-mcp`).
