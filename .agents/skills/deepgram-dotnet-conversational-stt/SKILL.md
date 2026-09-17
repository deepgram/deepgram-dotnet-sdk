---
name: deepgram-dotnet-conversational-stt
description: "Use when evaluating, extending, or writing C# code for conversational speech-to-text, Flux-style real-time transcription, or turn-taking streaming in the Deepgram .NET SDK. Identifies missing Flux request parameters (language_hint, eot_threshold), maps existing WebSocket response types, provides the closest supported LiveSchema code path, and guides adding TurnInfo models and Flux examples. Use `deepgram-dotnet-speech-to-text` for standard streaming transcription without turn awareness."
---

# Using Deepgram Conversational STT / Flux (.NET SDK)

This repo does **not** currently expose a dedicated Flux / conversational STT API surface comparable to the Python SDK's `listen.v2.connect(...)` + `TurnInfo` flow.

**Use a different skill when:**
- You only need standard streaming transcription without turn awareness → `deepgram-dotnet-speech-to-text`.
- You need a full voice assistant (STT + LLM + TTS) → `deepgram-dotnet-voice-agent`.

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

## Closest supported code path today

```csharp
using Deepgram;
using Deepgram.Models.Listen.v2.WebSocket;

Library.Initialize(); // reads DEEPGRAM_API_KEY env var
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

## Workflow: adding Flux support to the SDK

If the task requires real Flux parity, follow these steps in order:

1. **Add request params** — extend `Deepgram/Models/Listen/v2/WebSocket/LiveSchema.cs` with `LanguageHint`, `EagerEotThreshold`, `EotThreshold`, and other Flux-specific fields. Validate against the AsyncAPI spec.
2. **Add response models** — create `TurnInfo` and any turn-aware event types under `Deepgram/Models/Listen/v2/WebSocket/`. Verify field names match the AsyncAPI spec.
3. **Wire events in the client** — update `Deepgram/Clients/Listen/v2/WebSocket/Client.cs` to deserialize and dispatch new event types.
4. **Write tests** — add unit tests covering serialization of new request params and deserialization of new response types.
5. **Add an example** — create `examples/speech-to-text/websocket/flux/Program.cs` demonstrating a Flux session with turn-taking.

## Gotchas

1. **Flux is not first-class here yet.** Do not invent `TurnInfo`-style .NET models or `ConnectFluxAsync(...)` helpers that are not backed by real implementation.
2. **`Listen.v2.WebSocket` naming is misleading for Python-parity expectations.** It is the newest streaming client, but not a full conversational surface.
3. **`DeepgramWsClientOptions` defaults `APIVersion` to `v1`.** Inspect connection URIs before assuming `/v2/listen` behavior.

## Example files in this repo

- `examples/speech-to-text/websocket/file/Program.cs`
- `examples/speech-to-text/websocket/http/Program.cs`
- `examples/speech-to-text/websocket/microphone/Program.cs`

## References

- In-repo: `Deepgram/Clients/Listen/v2/WebSocket/Client.cs`, `Deepgram/Models/Listen/v2/WebSocket/*.cs`
- AsyncAPI (target spec): https://developers.deepgram.com/asyncapi.yaml
- Product docs: https://developers.deepgram.com/reference/speech-to-text/listen-flux
