// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Flux.Speak.WebSocket;

namespace Deepgram.Clients.Interfaces.v2;

/// <summary>
/// Implements the Flux (v2 speak) text-to-speech WebSocket Client: stream text in and
/// receive synthesized audio out, turn by turn, for voice-agent pipelines.
///
/// Sends five client messages — Speak, Flush, Interrupt, Configure, and Close. There
/// are intentionally no binary-send members (the client receives audio, it does not send it).
/// </summary>
public interface IFluxSpeakWebSocketClient
{
    #region Connect and Disconnect
    /// <summary>
    /// Connects to the Deepgram Flux (v2 speak) WebSocket API.
    /// </summary>
    public Task<bool> Connect(SpeakSchema options, CancellationTokenSource? cancelToken = null, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null);

    /// <summary>
    /// Gracefully closes the connection: sends a Close message and waits briefly for the server to
    /// drain remaining audio, emit a final SessionMetadata, and close the socket.
    /// </summary>
    public Task<bool> Stop(CancellationTokenSource? cancelToken = null, bool nullByte = false);
    #endregion

    #region Subscribe Event
    /// <summary>
    /// Subscribe to an Open event (raised locally when the connection opens).
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<OpenResponse> eventHandler);

    /// <summary>
    /// Subscribe to a binary Audio event carrying a chunk of synthesized audio.
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<AudioResponse> eventHandler);

    /// <summary>
    /// Subscribe to a Connected event from the Deepgram API.
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<ConnectedResponse> eventHandler);

    /// <summary>
    /// Subscribe to a SpeechStarted event, emitted at the start of each new turn.
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<SpeechStartedResponse> eventHandler);

    /// <summary>
    /// Subscribe to a SpeechMetadata event, carrying per-turn billing and timing after a Flush.
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<SpeechMetadataResponse> eventHandler);

    /// <summary>
    /// Subscribe to a SpeechInterrupted event, sent in response to a client Interrupt.
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<SpeechInterruptedResponse> eventHandler);

    /// <summary>
    /// Subscribe to a Flushed event echoing receipt of a manual Flush.
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<FlushedResponse> eventHandler);

    /// <summary>
    /// Subscribe to a SessionMetadata event, carrying cumulative session totals before close.
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<SessionMetadataResponse> eventHandler);

    /// <summary>
    /// Subscribe to a Warning event. Synthesis continues and the connection is unaffected.
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<WarningResponse> eventHandler);

    /// <summary>
    /// Subscribe to a ConfigureSuccess event, acknowledging a client Configure message.
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<ConfigureSuccessResponse> eventHandler);

    /// <summary>
    /// Subscribe to a ConfigureFailure event, rejecting a client Configure message. Non-fatal —
    /// the connection stays open.
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<ConfigureFailureResponse> eventHandler);

    /// <summary>
    /// Subscribe to a fatal Error event. The server closes the connection after sending it.
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<ErrorResponse> eventHandler);

    /// <summary>
    /// Subscribe to a Close event (raised locally when the connection closes).
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<CloseResponse> eventHandler);

    /// <summary>
    /// Subscribe to an Unhandled event carrying the raw JSON of an unrecognized message type.
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<UnhandledResponse> eventHandler);
    #endregion

    #region Send Functions
    /// <summary>
    /// Sends text to be synthesized into the active turn ({"type":"Speak","text":...}).
    /// </summary>
    public Task SendText(string text);

    /// <summary>
    /// Ends the active turn and generates the remaining audio ({"type":"Flush"}). The server
    /// replies with Flushed, then SpeechMetadata once the turn's audio has been sent.
    /// </summary>
    public Task SendFlush();

    /// <summary>
    /// Cancels the currently active turn (barge-in). The server replies with SpeechInterrupted,
    /// or a Warning if it cannot act on the interrupt.
    /// </summary>
    public Task SendInterrupt(InterruptSchema? interrupt = null);

    /// <summary>
    /// Cancels the currently active turn (barge-in), reporting the given playback offset.
    /// Convenience overload of <see cref="SendInterrupt(InterruptSchema?)"/>.
    /// </summary>
    public Task SendInterrupt(long playbackOffsetMs);

    /// <summary>
    /// Changes the speaking rate mid-session, without reconnecting. The server responds with
    /// ConfigureSuccess or ConfigureFailure.
    /// </summary>
    public Task SendConfigure(ConfigureSchema configure);

    /// <summary>
    /// Sends a Close message ({"type":"Close"}) and waits briefly for the server to drain all
    /// remaining and queued audio, emit a final SessionMetadata, and close the socket.
    /// </summary>
    public Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null);
    #endregion

    #region Helpers
    /// <summary>
    /// Retrieves the connection state of the WebSocket.
    /// </summary>
    public WebSocketState State();

    /// <summary>
    /// Indicates whether the WebSocket is connected.
    /// </summary>
    public bool IsConnected();
    #endregion
}
