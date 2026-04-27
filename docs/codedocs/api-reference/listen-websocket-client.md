---
title: "ListenWebSocketClient"
description: "Live transcription connection lifecycle, event subscriptions, and audio send methods."
---

Source files: `Deepgram/ListenWebSocketClient.cs`, `Deepgram/Clients/Interfaces/v2/IListenWebSocketClient.cs`, `Deepgram/Clients/Listen/v2/WebSocket/Client.cs`.

Import paths:

- `using Deepgram;`
- `using Deepgram.Models.Listen.v2.WebSocket;`

Constructor:

```csharp
public ListenWebSocketClient(
    string apiKey = "",
    DeepgramWsClientOptions? deepgramClientOptions = null)
```

## Constructor parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `apiKey` | `string` | `""` | Explicit API key if not supplied through options or environment variables. |
| `deepgramClientOptions` | `DeepgramWsClientOptions?` | `null` | WebSocket auth, keepalive, autoflush, headers, and addons configuration. |

## Public methods

### Connection lifecycle

```csharp
Task<bool> Connect(
    LiveSchema options,
    CancellationTokenSource? cancelToken = null,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)

Task<bool> Stop(CancellationTokenSource? cancelToken = null, bool nullByte = false)
```

### Subscriptions

```csharp
Task<bool> Subscribe(EventHandler<OpenResponse> eventHandler)
Task<bool> Subscribe(EventHandler<MetadataResponse> eventHandler)
Task<bool> Subscribe(EventHandler<ResultResponse> eventHandler)
Task<bool> Subscribe(EventHandler<UtteranceEndResponse> eventHandler)
Task<bool> Subscribe(EventHandler<SpeechStartedResponse> eventHandler)
Task<bool> Subscribe(EventHandler<CloseResponse> eventHandler)
Task<bool> Subscribe(EventHandler<UnhandledResponse> eventHandler)
Task<bool> Subscribe(EventHandler<ErrorResponse> eventHandler)
```

### Send and control methods

```csharp
Task SendKeepAlive()
Task SendFinalize()
Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null)
void Send(byte[] data, int length = Constants.UseArrayLengthForSend)
void SendBinary(byte[] data, int length = Constants.UseArrayLengthForSend)
void SendMessage(byte[] data, int length = Constants.UseArrayLengthForSend)
Task SendBinaryImmediately(byte[] data, int length = Constants.UseArrayLengthForSend, CancellationTokenSource? _cancellationToken = null)
Task SendMessageImmediately(byte[] data, int length = Constants.UseArrayLengthForSend, CancellationTokenSource? _cancellationToken = null)
WebSocketState State()
bool IsConnected()
```

## Key `LiveSchema` fields

| Property | Type | Default | Description |
|-----------|------|---------|-------------|
| `Model` | `string?` | `null` | Streaming recognition model. |
| `Encoding` | `string?` | `null` | Raw audio encoding such as `linear16`. |
| `SampleRate` | `int?` | `null` | Raw audio sample rate. |
| `InterimResults` | `bool?` | `null` | Emit provisional transcripts. |
| `Punctuate` | `bool?` | `null` | Enable punctuation. |
| `SmartFormat` | `bool?` | `null` | Enable formatting. |
| `UtteranceEnd` | `string?` | `null` | End-of-utterance delay in milliseconds. |
| `VadEvents` | `bool?` | `null` | Emit voice activity events. |
| `Keyterm` | `List<string>?` | `null` | Keyterm prompting, restricted by model check in the client. |

## Example

```csharp
var client = ClientFactory.CreateListenWebSocketClient();

await client.Subscribe(new EventHandler<ResultResponse>((_, e) =>
{
    Console.WriteLine(e.Channel?.Alternatives?[0].Transcript);
}));

await client.Connect(new LiveSchema
{
    Model = "nova-3",
    Encoding = "linear16",
    SampleRate = 16000,
    InterimResults = true
});
```

Related pages: [Streaming Transcription](/docs/streaming-transcription), [Microphone](/docs/api-reference/microphone).
