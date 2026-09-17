---
title: "Speech Synthesis"
description: "How the SDK handles text-to-speech through file-oriented REST calls and low-latency WebSocket streaming."
---

The SDK exposes Aura and Flux TTS clients. `SpeakRESTClient` and `SpeakWebSocketClient` use the v1 Speak API for Aura voices. `FluxSpeakRESTClient` and `FluxSpeakWebSocketClient` use the v2 Speak API for Flux TTS. Each transport has its own typed `SpeakSchema` model in a separate namespace.

The core files are `Deepgram/Clients/Speak/v1/REST/Client.cs`, `Deepgram/Clients/Speak/v2/WebSocket/Client.cs`, `Deepgram/Clients/Flux/Speak/REST/Client.cs`, `Deepgram/Clients/Flux/Speak/WebSocket/Client.cs`, and their matching models under `Deepgram/Models/Speak` and `Deepgram/Models/Flux/Speak`.

## Why this concept exists

Some applications want a finished MP3 or WAV file for storage, caching, or download. Others want to stream text into the model and receive audio chunks immediately for conversational playback. The SDK keeps those use cases separate so each transport can expose the right behavior without overloading one client with conflicting semantics.

## How it relates to other concepts

- REST synthesis behaves like other REST clients and uses `AbstractRestClient`.
- WebSocket synthesis behaves like live transcription and agent conversations and uses `AbstractWebSocketClient`.
- Agent conversations internally combine listen, think, and speak behavior, but standalone TTS is simpler when you only need speech output.

## How it works internally

`SpeakRESTClient.ToStream` sends a `TextSource` and `SpeakSchema` to the `speak` endpoint through `PostRetrieveLocalFileAsync`. That is different from most REST endpoints because the server returns audio bytes plus useful headers such as content type, request ID, model UUID, model name, character count, transfer encoding, and date. `SpeakRESTClient.ToStream` reads those headers into a typed `SyncResponse`, then attaches the returned `MemoryStream` to `response.Stream`.

`SpeakRESTClient.ToFile` simply calls `ToStream`, writes the resulting bytes to disk, sets `Filename`, and clears the in-memory stream reference. Callback synthesis follows the same pattern as prerecorded transcription: the client enforces that callback configuration appears in exactly one place.

`SpeakWebSocketClient` works differently. `Connect(SpeakSchema, ...)` opens the socket and optionally starts autoflush based on `DeepgramWsClientOptions.AutoFlushSpeakDelta`. Text is usually sent via `SpeakWithText`, which wraps the string in a JSON `TextSource`, escapes newlines and quotes, and then queues a text frame. `Flush`, `Clear`, and `Close` send control messages over the socket, while event subscriptions receive `AudioResponse`, `MetadataResponse`, `FlushedResponse`, `ClearedResponse`, warnings, and errors.

## Basic usage

```csharp
using Deepgram;
using Deepgram.Models.Speak.v1.REST;

var client = ClientFactory.CreateSpeakRESTClient();

var response = await client.ToFile(
    new TextSource("Hello from Deepgram."),
    "hello.mp3",
    new SpeakSchema
    {
        Model = "aura-2-thalia-en"
    });

Console.WriteLine($"Saved {response.Filename} with request {response.RequestId}");
```

## Advanced usage

```csharp
using Deepgram;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Speak.v2.WebSocket;

var options = new DeepgramWsClientOptions(
    addons: new Dictionary<string, string>
    {
        ["auto_flush_speak_delta"] = "1000"
    });

var client = ClientFactory.CreateSpeakWebSocketClient(options: options);
var flushed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

await client.Subscribe(new EventHandler<AudioResponse>((_, e) =>
{
    if (e.Stream is not null)
    {
        File.AppendAllBytes("output.raw", e.Stream.ToArray());
    }
}));

await client.Subscribe(new EventHandler<FlushedResponse>((_, _) => flushed.TrySetResult()));

await client.Connect(new SpeakSchema
{
    Model = "aura-asteria-en",
    Encoding = "linear16",
    SampleRate = 48000
});

client.SpeakWithText("This is the first sentence.");
client.SpeakWithText("This is the second sentence.");
client.Flush();
await flushed.Task.WaitAsync(TimeSpan.FromSeconds(30));
await client.Stop();
```

## Flux TTS

Use `ClientFactory.CreateFluxSpeakRESTClient()` to synthesize a complete Flux TTS response with `POST /v2/speak`, or `ClientFactory.CreateFluxSpeakWebSocketClient()` for turn-based v2 streaming. Flux TTS requires a `flux-*` model, such as `flux-alexis-en`; Aura model names belong on the v1 clients. The REST client supports batch encodings and containers, while the Flux TTS WebSocket emits raw `linear16`, `mulaw`, or `alaw` audio.

For WebSocket turns, `SendText()` adds text to the active turn and `await SendFlush()` ends that turn. The server emits `Flushed`, then `SpeechMetadata` after every audio chunk for the turn has been sent. Wait for `SpeechMetadata` before starting dependent work or closing the connection. See [FluxSpeakRESTClient](/docs/api-reference/flux-speak-rest-client) and [FluxSpeakWebSocketClient](/docs/api-reference/flux-speak-websocket-client).

<Callout type="info">`Flush()` on the Aura streaming client and `SendFlush()` on the Flux TTS streaming client are different APIs. Both finish buffered text, but neither replaces awaiting the corresponding completion event before shutdown.</Callout>

<Callout type="warn">For streaming TTS, prefer `SpeakWithText`, `Flush`, and `Clear` over immediate send methods. `Deepgram/Clients/Speak/v2/WebSocket/Client.cs` comments explicitly call out that these operations should stay queued so text and control messages preserve the intended order.</Callout>

<Accordions>
<Accordion title="REST file generation vs streaming synthesis">
REST synthesis is easier to reason about because one request produces one typed response and, optionally, one output file. It is a strong fit for background jobs, generated prompts, and any place where you want deterministic artifact creation. Streaming synthesis is more interactive and better for voice interfaces, but you have to manage socket lifecycle, buffering, and playback yourself. Choose REST when you want a completed asset; choose streaming when low latency matters more than simple control flow.

```csharp
await rest.ToFile(new TextSource("Ready"), "ready.mp3", new SpeakSchema { Model = "aura-2-thalia-en" });
await ws.Connect(new Deepgram.Models.Speak.v2.WebSocket.SpeakSchema { Encoding = "linear16" });
```

</Accordion>
<Accordion title="In-memory streams vs files on disk">
`ToStream` gives you the most flexibility because you can decide whether to write bytes to disk, upload them somewhere else, or hand them directly to another processing stage. `ToFile` is less flexible but saves boilerplate and matches the common “generate and save a file” workflow. The trade-off is memory pressure versus convenience: large synthesized outputs can be easier to handle when you stream or persist them promptly instead of holding everything in memory. If your service pipeline immediately uploads audio to storage, `ToStream` is usually the better primitive.

```csharp
var streamResponse = await client.ToStream(text, schema);
var fileResponse = await client.ToFile(text, "voice.wav", schema);
```

</Accordion>
</Accordions>
