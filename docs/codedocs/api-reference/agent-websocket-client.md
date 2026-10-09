---
title: "AgentWebSocketClient"
description: "Configure live agent sessions, subscribe to agent events, and send text or audio into the conversation."
---

Source files: `Deepgram/AgentWebSocketClient.cs`, `Deepgram/AgentWebSocketClientExtensions.cs`, `Deepgram/Clients/Interfaces/v2/IAgentWebSocketClient.cs`, `Deepgram/Clients/Interfaces/v2/IAgentProtocolClient.cs`, `Deepgram/Clients/Agent/v2/Websocket/Client.cs`.

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

`ClientFactory.CreateAgentProtocolClient()` returns `IAgentProtocolClient`, which additionally
provides typed subscriptions for `ListenUpdatedResponse`, `ThinkUpdatedResponse`,
`FunctionCallCancelledResponse`, `LatencyReportResponse`, `AgentWarningResponse`, and
`AgentHistoryResponse`, `FunctionCallResponse`, and `CustomFromThinkProviderResponse`.

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

The following extension methods work with both `IAgentWebSocketClient` and
`IAgentProtocolClient`: `SendUpdateListen`, `SendUpdateThink`, `SendUpdateSpeak`,
`SendUpdatePrompt`, `SendInjectAgentMessage`, `SendFunctionCallResponse`, and
`SendCustomToThinkProvider`.
`SendForceEndTurn` is available on `IAgentProtocolClient` and, as an extension method, on the
`IAgentWebSocketClient` returned by `ClientFactory.CreateAgentWebSocketClient()`; either way the
SDK flushes queued audio before sending the control frame. The extension throws
`NotSupportedException` for an `IAgentWebSocketClient` implementation that does not also
implement `IAgentProtocolClient`.

## Main schema types

| Type | Important fields | Notes |
|-----------|------|-------------|
| `SettingsSchema` | `Experimental`, `Tags`, `MipOptOut`, `Flags`, `Audio`, `Agent` | Initial settings payload sent immediately after connect; set `Flags.History` to control History event reporting. |
| `Agent` | `Language`, `Listen`, `Think`, `Speak`, `Greeting` | Conversation behavior. |
| `Input` | `Encoding`, `SampleRate` | Input audio format. |
| `Output` | `Encoding`, `SampleRate`, `Bitrate`, `Container` | Output audio format. |
| `InjectUserMessageSchema` | `Type`, `Content` | Text injection without microphone audio. |
| `AgentFunctionCallResponseSchema` | `Id`, `Name`, `Content` | Current response message for function-calling flows. |
| `AgentInjectAgentMessageSchema` | `Message`, `Behavior` | Current Agent speech injection message. |
| `AgentCustomToThinkProviderSchema` | `Content` | Experimental arbitrary JSON forwarded to a custom Think provider. |
| `Function` | `DeferUntilEot` | Prevents speculative dispatch of irreversible functions. |

## Example

```csharp
var client = ClientFactory.CreateAgentProtocolClient();

var settings = new SettingsSchema();
settings.Agent.Think.Provider.Type = "open_ai";
settings.Agent.Think.Provider.Model = "gpt-4o-mini";
settings.Agent.Listen.Provider.Type = "deepgram";
settings.Agent.Listen.Provider.Model = "nova-3";
settings.Agent.Speak.Provider.Type = "deepgram";
settings.Agent.Speak.Provider.Model = "aura-2-thalia-en";

await client.Connect(settings);
await client.SendInjectUserMessage("What is still unresolved in our queue?");
await client.SendUpdatePrompt(new AgentUpdatePromptSchema { Prompt = "Answer in one sentence." });
```

## Custom Think providers

Experimental custom Think messages require `settings.Agent.Think.Endpoint.Url` to use `wss://`.
Their content is represented as `JsonElement` and forwarded unchanged, including JSON `null`.

```csharp
await client.Subscribe(new EventHandler<CustomFromThinkProviderResponse>((_, response) =>
{
    Console.WriteLine(response.Content.GetRawText());
}));

using var payload = JsonDocument.Parse("""{"action":"continue"}""");
await client.SendCustomToThinkProvider(new AgentCustomToThinkProviderSchema
{
    Content = payload.RootElement.Clone(),
});
```

Related pages: [Agent Conversations](/docs/agent-conversations), [Guides: Build a Voice Agent](/docs/guides/build-a-voice-agent).
