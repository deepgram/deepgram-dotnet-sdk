---
title: "Microphone"
description: "PortAudio-backed microphone capture for streaming audio into Deepgram WebSocket clients."
---

Source files: `Deepgram.Microphone/Library.cs`, `Deepgram.Microphone/Microphone.cs`, `Deepgram.Microphone/Constants.cs`.

Import path:

```csharp
using Deepgram.Microphone;
```

## Library

```csharp
public class Library
{
    public static void Initialize();
    public static void Terminate();
}
```

`Deepgram.Microphone.Library.Initialize()` calls `PortAudio.Initialize()`. `Terminate()` calls `PortAudio.Terminate()`.

## Microphone constructor

```csharp
public Microphone(
    Action<byte[], int> push_callback,
    int rate = Defaults.RATE,
    uint chunkSize = Defaults.CHUNK_SIZE,
    int channels = Defaults.CHANNELS,
    int device_index = Defaults.DEVICE_INDEX,
    SampleFormat format = Defaults.SAMPLE_FORMAT)
```

## Constructor parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `push_callback` | `Action<byte[], int>` | — | Called with audio bytes and the actual length captured in each callback. |
| `rate` | `int` | `16000` | Input sample rate. |
| `chunkSize` | `uint` | `8194` | Frames per buffer. |
| `channels` | `int` | `1` | Number of input channels. |
| `device_index` | `int` | `PortAudio.NoDevice` | Input device index. Defaults to the platform default input device. |
| `format` | `SampleFormat` | `SampleFormat.Int16` | PortAudio sample format. |

## Public methods

```csharp
bool Start()
void Mute()
void Unmute()
void Stop()
```

Behavior notes from `Microphone.cs`:

- `Start()` resolves the default device when `device_index` is unset and returns `false` if no input device is available.
- The callback copies raw PCM bytes into a managed buffer and forwards them to `push_callback`.
- When muted, the callback sends zeroed audio instead of live captured bytes.
- `Stop()` cancels the internal token and stops the PortAudio stream.

Default constants from `Deepgram.Microphone/Constants.cs`:

| Constant | Value | Purpose |
|-----------|------|-------------|
| `Defaults.RATE` | `16000` | Default input sample rate. |
| `Defaults.CHANNELS` | `1` | Default mono capture. |
| `Defaults.CHUNK_SIZE` | `8194` | Default capture buffer size. |
| `Defaults.SAMPLE_FORMAT` | `SampleFormat.Int16` | Default PCM sample format. |
| `Defaults.DEVICE_INDEX` | `PortAudio.NoDevice` | Triggers default input device lookup. |

## Example

```csharp
Deepgram.Microphone.Library.Initialize();

var microphone = new Microphone(
    push_callback: (buffer, length) =>
    {
        var audio = new byte[length];
        Array.Copy(buffer, audio, length);
        client.Send(audio);
    },
    rate: 16000,
    channels: 1);

microphone.Start();
Console.ReadKey();
microphone.Stop();

Deepgram.Microphone.Library.Terminate();
```

The helper is intentionally minimal: it captures audio and forwards it to your callback, but it does not own any Deepgram client. That separation is useful because the same `Microphone` instance shape works with live transcription, agent audio input, or any custom WebSocket flow that expects raw PCM bytes.

Related pages: [Streaming Transcription](/docs/streaming-transcription), [ListenWebSocketClient](/docs/api-reference/listen-websocket-client).
