---
title: "SpeakWebSocketClient"
description: "Streaming TTS connection, subscriptions, text queueing, and control methods."
---

Source files: `Deepgram/SpeakWebSocketClient.cs`, `Deepgram/Clients/Interfaces/v2/ISpeakWebSocketClient.cs`, `Deepgram/Clients/Speak/v2/WebSocket/Client.cs`.

Import paths:

- `using Deepgram;`
- `using Deepgram.Models.Speak.v2.WebSocket;`

Constructor:

```csharp
public SpeakWebSocketClient(
    string apiKey = "",
    DeepgramWsClientOptions? deepgramClientOptions = null)
```

## Public methods

### Connection lifecycle

```csharp
Task<bool> Connect(
    SpeakSchema options,
    CancellationTokenSource? cancelToken = null,
    Dictionary<string, string>? addons = null,
    Dictionary<string, string>? headers = null)

Task<bool> Stop(CancellationTokenSource? cancelToken = null, bool nullByte = false)
```

### Subscriptions

```csharp
Task<bool> Subscribe(EventHandler<OpenResponse> eventHandler)
Task<bool> Subscribe(EventHandler<MetadataResponse> eventHandler)
Task<bool> Subscribe(EventHandler<FlushedResponse> eventHandler)
Task<bool> Subscribe(EventHandler<ClearedResponse> eventHandler)
Task<bool> Subscribe(EventHandler<AudioResponse> eventHandler)
Task<bool> Subscribe(EventHandler<CloseResponse> eventHandler)
Task<bool> Subscribe(EventHandler<WarningResponse> eventHandler)
Task<bool> Subscribe(EventHandler<ErrorResponse> eventHandler)
Task<bool> Subscribe(EventHandler<UnhandledResponse> eventHandler)
```

### Send and control methods

```csharp
void SpeakWithText(string data)
void Flush()
void Clear()
void Close(bool nullByte = false)
Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null)
void Send(byte[] data, int length = Constants.UseArrayLengthForSend)
void SendMessage(byte[] data, int length = Constants.UseArrayLengthForSend)
Task SendMessageImmediately(byte[] data, int length = Constants.UseArrayLengthForSend, CancellationTokenSource? _cancellationToken = null)
WebSocketState State()
bool IsConnected()
```

## Main schema parameters

| Property | Type | Default | Description |
|-----------|------|---------|-------------|
| `Model` | `string?` | `"aura-asteria-en"` | Voice model. |
| `BitRate` | `int?` | `null` | Audio bitrate. |
| `Encoding` | `string?` | `null` | Output encoding. |
| `SampleRate` | `int?` | `null` | Output sample rate. |

Behavior notes from `Deepgram/Clients/Speak/v2/WebSocket/Client.cs`:

- `SpeakWithText` escapes newlines, quotes, and control characters before wrapping the text in the payload object.
- `Flush` tells Deepgram to process the queued text buffer into audio.
- `Clear` drops the queued text buffer on the server side.
- `Close` is a convenience wrapper around `SendClose`.

## Example

```csharp
var client = ClientFactory.CreateSpeakWebSocketClient();

await client.Subscribe(new EventHandler<AudioResponse>((_, e) =>
{
    if (e.Stream is not null)
    {
        File.AppendAllBytes("output.raw", e.Stream.ToArray());
    }
}));

await client.Connect(new SpeakSchema
{
    Encoding = "linear16",
    SampleRate = 48000
});

client.SpeakWithText("This sentence will be synthesized.");
client.Flush();
```

In most applications you subscribe to `AudioResponse` and append the returned bytes to a file, memory buffer, or playback device. If ordering matters, prefer the queued methods rather than immediate send variants for normal text traffic.

Related pages: [Speech Synthesis](/docs/speech-synthesis), [SpeakRESTClient](/docs/api-reference/speak-rest-client).
