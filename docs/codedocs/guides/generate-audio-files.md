---
title: "Generate Audio Files"
description: "Convert text to saved audio files with SpeakRESTClient and optionally switch to streaming TTS."
---

This guide starts with the simplest TTS path in `examples/text-to-speech/rest/file/hello-world/Program.cs`, then shows how to move to streaming output when you need low latency instead of a finished file.

<Steps>
<Step>
### Synthesize directly to a file

```csharp
using Deepgram;
using Deepgram.Models.Speak.v1.REST;

Library.Initialize();

var client = ClientFactory.CreateSpeakRESTClient();

var response = await client.ToFile(
    new TextSource("Hello World!"),
    "hello.mp3",
    new SpeakSchema
    {
        Model = "aura-2-thalia-en"
    });

Console.WriteLine($"Saved {response.Filename}");
Library.Terminate();
```

</Step>
<Step>
### Keep the audio in memory instead of writing to disk

```csharp
var streamResponse = await client.ToStream(
    new TextSource("Queue this audio for upload."),
    new SpeakSchema
    {
        Model = "aura-2-thalia-en",
        Container = "mp3"
    });

await File.WriteAllBytesAsync("queued.mp3", streamResponse.Stream!.ToArray());
```

</Step>
<Step>
### Switch to streaming synthesis when latency matters

```csharp
using Deepgram.Models.Speak.v2.WebSocket;

var wsClient = ClientFactory.CreateSpeakWebSocketClient();

await wsClient.Subscribe(new EventHandler<AudioResponse>((_, e) =>
{
    if (e.Stream is not null)
    {
        File.AppendAllBytes("output.raw", e.Stream.ToArray());
    }
}));

await wsClient.Connect(new Deepgram.Models.Speak.v2.WebSocket.SpeakSchema
{
    Encoding = "linear16",
    SampleRate = 48000
});

wsClient.SpeakWithText("Hello from the streaming client.");
wsClient.Flush();
await wsClient.Stop();
```

</Step>
</Steps>

Use REST when you need a final artifact, and use WebSocket when you need audio chunks as soon as they are synthesized.

Practical advice:

- `ToFile` is usually the right default for CLIs, batch jobs, and report-generation pipelines.
- `ToStream` is better when your app uploads the generated audio somewhere else and you do not want an intermediate file on disk.
- Streaming TTS is more interactive, but you need to decide how to wrap raw PCM output into a playable container such as WAV, exactly as the repository examples do.

If you are choosing between these modes for a user-facing feature, start with REST to validate content and voice selection, then move to WebSocket once latency becomes the bottleneck.
