---
title: "FluxWebSocketClient"
description: "Flux STT v2 Listen WebSocket lifecycle, turn events, and controls."
---

Source files: `Deepgram/FluxWebSocketClient.cs`, `Deepgram/Clients/Interfaces/v2/IFluxWebSocketClient.cs`, and `Deepgram/Clients/Flux/WebSocket/Client.cs`.

Imports:

- `using Deepgram;`
- `using Deepgram.Models.Flux.WebSocket;`

## Public Methods

```csharp
Task<bool> Connect(FluxSchema options, CancellationTokenSource? cancelToken = null,
    Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
Task<bool> Stop(CancellationTokenSource? cancelToken = null, bool nullByte = false)
Task SendConfigure(ConfigureSchema configure)
Task SendClose(bool nullByte = false, CancellationTokenSource? cancellationToken = null)
void Send(byte[] data, int length = Constants.UseArrayLengthForSend)
void SendBinary(byte[] data, int length = Constants.UseArrayLengthForSend)
```

Subscribe to `ConnectedResponse`, `TurnInfoResponse`, `ConfigureSuccessResponse`, `ConfigureFailureResponse`, `ErrorResponse`, `CloseResponse`, or `UnhandledResponse` before connecting. `TurnInfoResponse.EventType` converts the wire event to `TurnEvent?`; preserve unknown events safely because Flux can add event names.

`Stop()` sends `CloseStream` and waits for the server to emit final turn results and close. Flux does not accept `Finalize` or `KeepAlive`; those controls only apply to the Nova Listen WebSocket client.

Related pages: [Flux STT](/docs/flux-transcription), [ClientFactory](/docs/api-reference/library-and-client-factory).
