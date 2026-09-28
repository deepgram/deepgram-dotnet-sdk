---
title: "Flux STT"
description: "Turn-aware conversational transcription with the Flux v2 Listen WebSocket client."
---

Flux STT is the SDK's conversational speech-to-text client for `wss://api.deepgram.com/v2/listen`. Create it with `ClientFactory.CreateFluxWebSocketClient()` and configure it with `Deepgram.Models.Flux.WebSocket.FluxSchema`.

Flux STT is distinct from Nova live transcription. It sends `TurnInfoResponse` events whose `EventType` is `StartOfTurn`, `Update`, `EagerEndOfTurn`, `TurnResumed`, or `EndOfTurn`. Handle `StartOfTurn` even when `Transcript` is empty, and treat `EndOfTurn` as the final transcript for that turn.

## Lifecycle

Subscribe before connecting. After sending the final audio chunk, call `Stop()`. Flux STT sends `CloseStream`, waits briefly for final turn events, and then closes. Do not send Nova's `SendFinalize()` or `SendKeepAlive()` controls: the v2 endpoint rejects them.

```csharp
using Deepgram;
using Deepgram.Models.Flux.WebSocket;

var client = ClientFactory.CreateFluxWebSocketClient();
var finalTurn = new TaskCompletionSource<TurnInfoResponse>(
    TaskCreationOptions.RunContinuationsAsynchronously);

await client.Subscribe(new EventHandler<TurnInfoResponse>((_, e) =>
{
    switch (e.EventType)
    {
        case TurnEvent.StartOfTurn:
            Console.WriteLine("Turn started.");
            break;
        case TurnEvent.Update:
            Console.WriteLine(e.Transcript);
            break;
        case TurnEvent.EndOfTurn:
            Console.WriteLine($"Final: {e.Transcript}");
            finalTurn.TrySetResult(e);
            break;
    }
}));

var connected = await client.Connect(new FluxSchema
{
    Model = "flux-general-en",
    Encoding = "linear16",
    SampleRate = 16000,
    EotThreshold = 0.7
});

if (!connected)
{
    throw new InvalidOperationException("Could not connect to Flux STT.");
}

client.Send(audioChunk);
await client.Stop();
await finalTurn.Task.WaitAsync(TimeSpan.FromSeconds(30));
```

## Configuration And Controls

`FluxSchema` requires `Model`. Use `flux-general-en` for English or `flux-general-multi` for multilingual audio. For raw audio, also set `Encoding` and `SampleRate`. `EotThreshold`, `EagerEotThreshold`, and `EotTimeoutMs` tune turn detection. `LanguageHint` is valid only with `flux-general-multi`.

Flux accepts `Configure`, `ForceEndTurn`, and `CloseStream` client controls. `IFluxWebSocketClient` exposes `SendConfigure()` and `SendClose()`; `SendForceEndTurn()` is currently on the concrete `FluxWebSocketClient`, not the interface, to avoid breaking third-party interface implementations in 7.x.

Related pages: [Streaming Transcription](/docs/streaming-transcription), [FluxWebSocketClient](/docs/api-reference/flux-websocket-client), and [the Flux example](../../examples/speech-to-text/websocket/flux/).
