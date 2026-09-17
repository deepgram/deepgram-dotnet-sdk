---
title: "Build a Voice Agent"
description: "Create a live agent session that listens, thinks with an LLM provider, and speaks back audio."
---

This guide combines the cleaner pieces of `examples/agent/websocket/no_mic/Program.cs` and `examples/agent/websocket/simple/Program.cs`. It starts with injected text for fast iteration, then shows how the same client can accept binary microphone audio.

<Steps>
<Step>
### Configure the agent session

```csharp
using Deepgram;
using Deepgram.Models.Agent.v2.WebSocket;

Deepgram.Library.Initialize();

var client = ClientFactory.CreateAgentWebSocketClient();

var settings = new SettingsSchema();
settings.Agent.Greeting = "Hello, how can I help you today?";
settings.Agent.Listen.Provider.Type = "deepgram";
settings.Agent.Listen.Provider.Model = "nova-3";
settings.Agent.Think.Provider.Type = "open_ai";
settings.Agent.Think.Provider.Model = "gpt-4o-mini";
settings.Agent.Speak.Provider.Type = "deepgram";
settings.Agent.Speak.Provider.Model = "aura-2-thalia-en";
settings.Audio.Input.Encoding = "linear16";
settings.Audio.Input.SampleRate = 24000;
settings.Audio.Output.Encoding = "linear16";
settings.Audio.Output.SampleRate = 24000;
settings.Audio.Output.Container = "none";
```

</Step>
<Step>
### Subscribe to text and audio events

```csharp
await client.Subscribe(new EventHandler<ConversationTextResponse>((_, e) =>
{
    Console.WriteLine(e);
}));

await client.Subscribe(new EventHandler<AudioResponse>((_, e) =>
{
    if (e.Stream is not null)
    {
        File.AppendAllBytes("agent-output.raw", e.Stream.ToArray());
    }
}));
```

</Step>
<Step>
### Connect and start with injected user text

```csharp
var connected = await client.Connect(settings);
if (!connected)
{
    throw new InvalidOperationException("Agent connection failed.");
}

await client.SendInjectUserMessage("Summarize the last three support tickets.");
```

</Step>
<Step>
### Upgrade to live microphone audio

```csharp
using Deepgram.Microphone;

Deepgram.Microphone.Library.Initialize();

var microphone = new Microphone(
    push_callback: (audio, length) =>
    {
        var actual = new byte[length];
        Array.Copy(audio, actual, length);
        client.SendBinary(actual);
    },
    rate: 24000,
    channels: 1);

microphone.Start();
Console.ReadKey();
microphone.Stop();

await client.Stop();
Deepgram.Microphone.Library.Terminate();
Deepgram.Library.Terminate();
```

</Step>
</Steps>

This split workflow matters in practice: injected text is the fastest way to debug prompts and function-calling behavior, while microphone audio is the final transport layer you add once the agent behavior is stable.

Operational tips:

- Start with injected text until your `SettingsSchema` is stable, because it eliminates microphone and playback variables.
- Match `Audio.Input.SampleRate` to the actual microphone capture rate when you move to live audio.
- Keep output in raw or headerless form during debugging, then wrap it into WAV or another container once you know the bytes are correct.

The agent client is the most flexible part of the SDK, but it is also the easiest place to introduce provider-shape mistakes. In practice, small helper methods that build your `SettingsSchema` make these sessions much easier to maintain.
