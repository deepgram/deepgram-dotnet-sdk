---
title: "Stream From Microphone"
description: "Capture live microphone audio with Deepgram.Microphone and send it to ListenWebSocketClient."
---

This guide shows the live path used by `examples/speech-to-text/websocket/microphone/Program.cs`: initialize the core SDK, initialize the microphone helper, subscribe to events, connect the WebSocket, and push captured PCM audio to Deepgram.

<Steps>
<Step>
### Install both packages

```bash
dotnet add package Deepgram
dotnet add package Deepgram.Microphone
```

</Step>
<Step>
### Configure the WebSocket client

```csharp
using Deepgram;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Listen.v2.WebSocket;

Deepgram.Library.Initialize();

var options = new DeepgramWsClientOptions(keepAlive: true);

var client = ClientFactory.CreateListenWebSocketClient(options: options);

await client.Subscribe(new EventHandler<ResultResponse>((_, e) =>
{
    var transcript = e.Channel?.Alternatives?[0].Transcript;
    if (!string.IsNullOrWhiteSpace(transcript))
    {
        Console.WriteLine(transcript);
    }
}));
```

</Step>
<Step>
### Start the stream and microphone

```csharp
using Deepgram.Microphone;

Deepgram.Microphone.Library.Initialize();

var connected = await client.Connect(new LiveSchema
{
    Model = "nova-3",
    Encoding = "linear16",
    SampleRate = 16000,
    Punctuate = true,
    InterimResults = true,
    VadEvents = true,
    UtteranceEnd = "1000"
});

if (!connected)
{
    throw new InvalidOperationException("Could not connect to Deepgram.");
}

var microphone = new Microphone(client.Send, rate: 16000, channels: 1);
microphone.Start();

Console.ReadKey();

microphone.Stop();
await client.Stop();

Deepgram.Microphone.Library.Terminate();
Deepgram.Library.Terminate();
```

</Step>
</Steps>

Operational notes:

- The microphone helper defaults to `linear16`, mono, 16 kHz, which is a natural fit for the example `LiveSchema`.
- `DeepgramWsClientOptions(keepAlive: true)` turns on the background keepalive loop implemented by the listen WebSocket client.
- If your app behaves like push-to-talk, set `auto_flush_reply_delta` in `DeepgramWsClientOptions.Addons` or call `SendFinalize()` yourself.

Troubleshooting:

- If `Microphone.Start()` returns `false`, the SDK likely could not resolve a default input device through PortAudio.
- If you receive empty transcripts, verify that your `LiveSchema.Encoding` and `SampleRate` match the audio produced by the microphone callback.
- If the socket stays open but final transcripts lag, either lower `UtteranceEnd` or enable autoflush through WebSocket addons.

This pattern is the fastest way to stand up a speech-enabled console tool because the microphone helper's callback shape matches `ListenWebSocketClient.Send(byte[] data, int length)` directly.
