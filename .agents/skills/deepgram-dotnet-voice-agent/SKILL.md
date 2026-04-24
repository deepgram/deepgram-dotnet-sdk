---
name: deepgram-dotnet-voice-agent
description: Use when writing or reviewing C# code in this repo that builds an interactive Deepgram Voice Agent over WebSocket. Covers `ClientFactory.CreateAgentWebSocketClient()`, `SettingsSchema`, event subscriptions, microphone audio streaming, injected user messages, and function-call-related message types. Use `deepgram-dotnet-text-to-speech` for one-way synthesis and STT skills for transcription-only flows.
---

# Using Deepgram Voice Agent (.NET SDK)

Full-duplex voice agent sessions over a single WebSocket.

## When to use this product

- You want a live assistant that listens, thinks, and speaks.
- You want Deepgram-managed orchestration for STT + LLM + TTS.
- You may need injected user messages or function-call message handling.

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
using Deepgram.Models.Authenticate.v1;

Deepgram.Library.Initialize();

var options = new DeepgramWsClientOptions(null, null, true);
var agentClient = ClientFactory.CreateAgentWebSocketClient(apiKey: "", options: options);
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
        using (var writer = new BinaryWriter(File.Open("output.wav", FileMode.Append)))
        {
            writer.Write(e.Stream.ToArray());
        }
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
if (connected)
{
    await agentClient.SendInjectUserMessage("Say hello in one sentence.");
}
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

## API reference (layered)

1. **In-repo source of truth**:
   - `Deepgram/ClientFactory.cs`
   - `Deepgram/Clients/Agent/v2/Websocket/Client.cs`
   - `Deepgram/Clients/Interfaces/v2/IAgentWebSocketClient.cs`
   - `Deepgram/Models/Agent/v2/WebSocket/*.cs`
   - `Deepgram.Microphone/Microphone.cs`
2. **Canonical OpenAPI (REST)**: not applicable for the runtime socket in this repo
3. **Canonical AsyncAPI (WSS)**: https://developers.deepgram.com/asyncapi.yaml
4. **Context7**:
   - repo mirror: `https://context7.com/deepgram/deepgram-dotnet-sdk`
   - docs corpus: `/llmstxt/developers_deepgram_llms_txt`
5. **Product docs**:
   - https://developers.deepgram.com/reference/voice-agent/voice-agent
   - https://developers.deepgram.com/docs/voice-agent
   - https://developers.deepgram.com/docs/configure-voice-agent
   - https://developers.deepgram.com/docs/voice-agent-message-flow

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

## Central product skills

For cross-language Deepgram product knowledge — the consolidated API reference, documentation finder, focused runnable recipes, third-party integration examples, and MCP setup — install the central skills:

```bash
npx skills add deepgram/skills
```

This SDK ships language-idiomatic code skills; `deepgram/skills` ships cross-language product knowledge (see `api`, `docs`, `recipes`, `examples`, `starters`, `setup-mcp`).
