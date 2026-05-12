---
name: deepgram-dotnet-voice-agent
description: "Use when writing or reviewing C# code in this repo that builds an interactive Deepgram Voice Agent over WebSocket. Covers `ClientFactory.CreateAgentWebSocketClient()`, `SettingsSchema`, event subscriptions, microphone audio streaming, injected user messages, and function-call-related message types. Use `deepgram-dotnet-text-to-speech` for one-way synthesis and STT skills for transcription-only flows."
---

# Using Deepgram Voice Agent (.NET SDK)

Full-duplex voice agent sessions over a single WebSocket.

**Use a different skill when:**
- One-way transcription → `deepgram-dotnet-speech-to-text` or `deepgram-dotnet-conversational-stt`.
- One-way synthesis → `deepgram-dotnet-text-to-speech`.
- Admin APIs → `deepgram-dotnet-management-api`.

## Authentication

```bash
dotnet add package Deepgram
dotnet add package Deepgram.Microphone   # only if you need local mic capture
```

```csharp
using Deepgram;

Deepgram.Library.Initialize();
// Uses DEEPGRAM_API_KEY env var; pass apiKey: "..." to override
var agentClient = ClientFactory.CreateAgentWebSocketClient();
```

## Quick start

```csharp
using Deepgram.Models.Agent.v2.WebSocket;

var agentClient = ClientFactory.CreateAgentWebSocketClient();

await agentClient.Subscribe(new EventHandler<ConversationTextResponse>((sender, e) =>
{
    Console.WriteLine(e);
}));

await agentClient.Subscribe(new EventHandler<AudioResponse>((sender, e) =>
{
    if (e.Stream != null)
    {
        // Raw linear16 PCM — see examples/agent/websocket/no_mic/Program.cs for WAV header.
        using var writer = new BinaryWriter(File.Open("output.raw", FileMode.Append));
        writer.Write(e.Stream.ToArray());
    }
}));

var settings = new SettingsSchema();
settings.Agent.Think.Provider.Type = "open_ai";
settings.Agent.Think.Provider.Model = "gpt-4o-mini";
settings.Agent.Greeting = "Hello! How can I help you today?";
settings.Agent.Listen.Provider.Type = "deepgram";
settings.Agent.Listen.Provider.Model = "nova-3";
settings.Agent.Speak.Provider.Type = "deepgram";
settings.Agent.Speak.Provider.Model = "aura-2-thalia-en";
settings.Audio.Input.Encoding = "linear16";
settings.Audio.Input.SampleRate = 24000;
settings.Audio.Output.Encoding = "linear16";
settings.Audio.Output.SampleRate = 24000;

bool connected = await agentClient.Connect(settings);
if (!connected)
{
    Console.Error.WriteLine("Agent WebSocket connection failed — check API key and network.");
    return;
}

await agentClient.SendInjectUserMessage("Say hello in one sentence.");

// Cleanup when done
Console.ReadKey();
await agentClient.Stop();
```

## Streaming microphone audio

```csharp
var microphone = new Microphone(
    push_callback: (audioData, length) =>
    {
        byte[] chunk = new byte[length];
        Array.Copy(audioData, chunk, length);
        agentClient.SendBinary(chunk);
    },
    rate: 24000,
    channels: 1,
    format: SampleFormat.Int16);

microphone.Start();

// Cleanup
Console.ReadKey();
microphone.Stop();
await agentClient.Stop();
```

## Key params and events

Settings models:
- `SettingsSchema`
- `Audio`, `Input`, `Output`
- `Agent`, `Listen`, `Think`, `Speak`
- `Provider` (dynamic extra properties supported)

Important events:
- `ConversationTextResponse`
- `AudioResponse`
- `AgentStartedSpeakingResponse`
- `AgentAudioDoneResponse`
- `AgentThinkingResponse`
- `UserStartedSpeakingResponse`
- `FunctionCallRequestResponse`
- `SettingsAppliedResponse`

Send helpers:
- `SendInjectUserMessage(string)`
- `SendInjectUserMessage(InjectUserMessageSchema)`
- `SendBinary(...)`
- `SendBinaryImmediately(...)`
- `SendKeepAlive()`

## References

- In-repo: `Deepgram/Clients/Agent/v2/Websocket/Client.cs`, `Deepgram/Clients/Interfaces/v2/IAgentWebSocketClient.cs`, `Deepgram/Models/Agent/v2/WebSocket/*.cs`, `Deepgram.Microphone/Microphone.cs`
- AsyncAPI (WSS): https://developers.deepgram.com/asyncapi.yaml
- Product docs: https://developers.deepgram.com/docs/voice-agent, https://developers.deepgram.com/docs/voice-agent-message-flow

## Gotchas

1. **Use the actual event/model names in this repo.** `SettingsSchema`, `ConversationTextResponse`, etc. — not the Python names.
2. **`Provider` is dynamic.** Extra provider-specific properties are stored through `JsonExtensionData`; set them carefully.
3. **Function call support is partial.** `FunctionCallRequestResponse` is marked `TODO: this needs to be defined`, so inspect raw payload behavior before relying on typed fields.
4. **There is no convenience `SendFunctionCallResponse(...)` helper on the public interface.** If you need it, send serialized `FunctionCallResponseSchema` manually via the generic send path.
5. **Audio formats must match.** The examples align both input and output around `linear16` / `24000`.
6. **`Deepgram.Microphone` depends on PortAudio.** Local microphone examples need the helper project/package and a working PortAudio environment.

## Example files in this repo

- `examples/agent/websocket/simple/Program.cs`
- `examples/agent/websocket/no_mic/Program.cs`
- `examples/agent/websocket/arbitrary_keys/Program.cs`

Cross-language product knowledge (API reference, recipes, MCP setup): `npx skills add deepgram/skills`.
