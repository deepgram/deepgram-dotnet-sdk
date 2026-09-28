---
title: "AgentWebSocketClient"
description: "Configure live agent sessions, subscribe to agent events, and send text or audio into the conversation."
---

Source files: `Deepgram/AgentWebSocketClient.cs`, `Deepgram/Clients/Interfaces/v2/IAgentWebSocketClient.cs`, `Deepgram/Clients/Agent/v2/Websocket/Client.cs`.

Import paths:

- `using Deepgram;`
- `using Deepgram.Models.Agent.v2.WebSocket;`

Constructor:

```csharp
public AgentWebSocketClient(
    string apiKey = "",
    DeepgramWsClientOptions? deepgramClientOptions = null)
```

## Public methods

### Connection lifecycle

```csharp
Task<bool> Connect(
    SettingsSchema options,
    CancellationTokenSource? cancelToken = null,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)

Task<bool> Stop(CancellationTokenSource? cancelToken = null, bool nullByte = false)
```

### Subscriptions

```csharp
Task<bool> Subscribe(EventHandler<OpenResponse> eventHandler)
Task<bool> Subscribe(EventHandler<AudioResponse> eventHandler)
Task<bool> Subscribe(EventHandler<AgentAudioDoneResponse> eventHandler)
Task<bool> Subscribe(EventHandler<AgentStartedSpeakingResponse> eventHandler)
Task<bool> Subscribe(EventHandler<AgentThinkingResponse> eventHandler)
Task<bool> Subscribe(EventHandler<ConversationTextResponse> eventHandler)
Task<bool> Subscribe(EventHandler<FunctionCallRequestResponse> eventHandler)
Task<bool> Subscribe(EventHandler<UserStartedSpeakingResponse> eventHandler)
Task<bool> Subscribe(EventHandler<WelcomeResponse> eventHandler)
Task<bool> Subscribe(EventHandler<CloseResponse> eventHandler)
Task<bool> Subscribe(EventHandler<UnhandledResponse> eventHandler)
Task<bool> Subscribe(EventHandler<ErrorResponse> eventHandler)
Task<bool> Subscribe(EventHandler<SettingsAppliedResponse> eventHandler)
Task<bool> Subscribe(EventHandler<InjectionRefusedResponse> eventHandler)
Task<bool> Subscribe(EventHandler<PromptUpdatedResponse> eventHandler)
Task<bool> Subscribe(EventHandler<SpeakUpdatedResponse> eventHandler)
```

### Send and helper methods

```csharp
Task SendKeepAlive()
Task SendInjectUserMessage(string content)
Task SendInjectUserMessage(InjectUserMessageSchema injectUserMessageSchema)
Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null)
void SendBinary(byte[] data, int length = Constants.UseArrayLengthForSend)
void SendMessage(byte[] data, int length = Constants.UseArrayLengthForSend)
Task SendBinaryImmediately(byte[] data, int length = Constants.UseArrayLengthForSend, CancellationTokenSource? _cancellationToken = null)
Task SendMessageImmediately(byte[] data, int length = Constants.UseArrayLengthForSend, CancellationTokenSource? _cancellationToken = null)
WebSocketState State()
bool IsConnected()
```

## Main schema types

| Type | Important fields | Notes |
|-----------|------|-------------|
| `SettingsSchema` | `Experimental`, `Tags`, `MipOptOut`, `Audio`, `Agent` | Initial settings payload sent immediately after connect. |
| `Agent` | `Language`, `Listen`, `Think`, `Speak`, `Greeting` | Conversation behavior. |
| `Input` | `Encoding`, `SampleRate` | Input audio format. |
| `Output` | `Encoding`, `SampleRate`, `Bitrate`, `Container` | Output audio format. |
| `InjectUserMessageSchema` | `Type`, `Content` | Text injection without microphone audio. |
| `FunctionCallResponseSchema` | `FunctionCallId`, `Output` | Response message for function-calling flows. |

## Example

```csharp
var client = ClientFactory.CreateAgentWebSocketClient();

var settings = new SettingsSchema();
settings.Agent.Think.Provider.Type = "open_ai";
settings.Agent.Think.Provider.Model = "gpt-4o-mini";
settings.Agent.Listen.Provider.Type = "deepgram";
settings.Agent.Listen.Provider.Model = "nova-3";
settings.Agent.Speak.Provider.Type = "deepgram";
settings.Agent.Speak.Provider.Model = "aura-2-thalia-en";

await client.Connect(settings);
await client.SendInjectUserMessage("What is still unresolved in our queue?");
```

Related pages: [Agent Conversations](/docs/agent-conversations), [Guides: Build a Voice Agent](/docs/guides/build-a-voice-agent).
