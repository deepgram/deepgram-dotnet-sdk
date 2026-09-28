---
title: "FluxSpeakWebSocketClient"
description: "Flux TTS v2 Speak WebSocket lifecycle, completion events, and turn controls."
---

Source files: `Deepgram/FluxSpeakWebSocketClient.cs`, `Deepgram/Clients/Interfaces/v2/IFluxSpeakWebSocketClient.cs`, and `Deepgram/Clients/Flux/Speak/WebSocket/Client.cs`.

Imports:

- `using Deepgram;`
- `using Deepgram.Models.Flux.Speak.WebSocket;`

## Lifecycle And Flush

`Connect()` requires a `SpeakSchema` with a Flux TTS `Model`. Use `SendText()` to add text to the active turn. `await SendFlush()` ends that turn and asks Deepgram to generate the remaining audio; it does not close the session. The server emits `Flushed`, then `SpeechMetadata` after all audio for that turn has been sent. Subscribe before connecting and await `SpeechMetadataResponse` before closing or consuming a completed file.

```csharp
var client = ClientFactory.CreateFluxSpeakWebSocketClient();
using var audio = new FileStream("output.raw", FileMode.Create);
var turnComplete = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

await client.Subscribe(new EventHandler<AudioResponse>((_, e) =>
{
    e.Stream?.CopyTo(audio);
}));
await client.Subscribe(new EventHandler<SpeechMetadataResponse>((_, _) =>
{
    turnComplete.TrySetResult();
}));

var connected = await client.Connect(new SpeakSchema
{
    Model = "flux-alexis-en",
    Encoding = "linear16",
    SampleRate = 24000
});

if (!connected)
{
    throw new InvalidOperationException("Could not connect to Flux TTS.");
}

await client.SendText("Hello from Flux TTS.");
await client.SendFlush();
await turnComplete.Task.WaitAsync(TimeSpan.FromSeconds(30));
await client.Stop();
```

`Stop()` sends `Close`, waits briefly for remaining audio and a final `SessionMetadataResponse`, then tears down the socket. Use `SendInterrupt()` for barge-in and `SendConfigure(new ConfigureSchema { Speed = 1.1 })` to change speed mid-session. `SendInterrupt(long)` requires the actual cumulative playback offset in milliseconds, not bytes received from the network.

The WebSocket emits raw, non-containerized `linear16`, `mulaw`, or `alaw` audio. Use the REST client when you need compressed audio or a container.

Related pages: [FluxSpeakRESTClient](/docs/api-reference/flux-speak-rest-client), [Speech Synthesis](/docs/speech-synthesis).
