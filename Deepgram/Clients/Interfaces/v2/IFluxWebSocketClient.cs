// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Flux.WebSocket;

namespace Deepgram.Clients.Interfaces.v2;

/// <summary>
/// (PREVIEW) Implements the Flux (v2 listen) WebSocket Client for conversational
/// speech-to-text with contextual turn detection.
///
/// Flux accepts only three client control messages: CloseStream, Configure, and ForceEndTurn.
/// There are intentionally no SendKeepAlive or SendFinalize members: those are v1-only control
/// messages and the v2 endpoint rejects them.
/// </summary>
public interface IFluxWebSocketClient
{
    #region Connect and Disconnect
    /// <summary>
    /// Connects to the Deepgram Flux WebSocket API
    /// </summary>
    /// <remarks>
    /// Open event handlers run before Connect() returns, and the receive loop starts only after they
    /// return, so an Open handler that waits for a message from the server blocks until its own
    /// timeout. A blocking Stop().Wait() inside an Open handler waits out this client's close grace
    /// period (<c>DefaultCloseStreamGracePeriodMs</c>, 3 s). An exception thrown by an Open handler is
    /// logged at Error level and Connect() still returns true. Messages still queued from earlier
    /// sends are sent as soon as the new connection opens; the send methods' remarks say which ones
    /// are kept.
    /// </remarks>
    public Task<bool> Connect(FluxSchema options, CancellationTokenSource? cancelToken = null, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null);

    /// <summary>
    /// Disconnects from the Deepgram Flux WebSocket API
    /// </summary>
    /// <remarks>
    /// Stop() returns after the Close event has been delivered, except when it ends a connection
    /// whose Open handlers are still running: then it returns without waiting for them, and Close
    /// fires as soon as they return. If a new Connect() runs before then, that Close can arrive after
    /// the new connection's Open. Dispose() raises no Close.
    /// </remarks>
    public Task<bool> Stop(CancellationTokenSource? cancelToken = null, bool nullByte = false);
    #endregion

    #region Subscribe Event
    /// <summary>
    /// Subscribe to an Open event from the Deepgram API
    /// </summary>
    /// <remarks>
    /// The handler runs before Connect() returns. If it throws, the exception is logged at Error
    /// level and the connection stays open; Connect() still returns true. The receive loop starts
    /// only after the Open handlers return, so a handler that waits for a message from the server
    /// blocks until its own timeout. A blocking Stop().Wait() inside an Open handler waits out this
    /// client's close grace period (<c>DefaultCloseStreamGracePeriodMs</c>, 3 s).
    /// </remarks>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<OpenResponse> eventHandler);

    /// <summary>
    /// Subscribe to a Connected event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<ConnectedResponse> eventHandler);

    /// <summary>
    /// Subscribe to a TurnInfo event from the Deepgram API. The turn lifecycle
    /// (StartOfTurn, Update, EagerEndOfTurn, TurnResumed, EndOfTurn) is carried in the
    /// TurnInfoResponse Event property.
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<TurnInfoResponse> eventHandler);

    /// <summary>
    /// Subscribe to a ConfigureSuccess event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<ConfigureSuccessResponse> eventHandler);

    /// <summary>
    /// Subscribe to a ConfigureFailure event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<ConfigureFailureResponse> eventHandler);

    /// <summary>
    /// Subscribe to an Error event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<ErrorResponse> eventHandler);

    /// <summary>
    /// Subscribe to a Close event from the Deepgram API
    /// </summary>
    /// <remarks>
    /// Close fires once for each connection that raised Open, always after Open, when Stop(), a
    /// server close or a dropped connection ends it. If a Stop() ends the connection while Open
    /// handlers are still running, Close fires as soon as they return; if a new Connect() runs before
    /// then, that Close can arrive after the new connection's Open. Dispose() raises no Close.
    /// A Close handler must not block on a different Stop() call that is still in progress for the
    /// same connection: that Stop() returns only after Close has been delivered, so the handler
    /// waits forever. A Stop() called from inside the handler returns at once, unless a newer
    /// connection has opened since; then it stops that connection.
    /// </remarks>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<CloseResponse> eventHandler);

    /// <summary>
    /// Subscribe to an Unhandled event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<UnhandledResponse> eventHandler);
    #endregion

    #region Send Functions
    /// <summary>
    /// Sends a Configure message to Deepgram to update end-of-turn thresholds, keyterms, or
    /// language hints mid-stream
    /// </summary>
    /// <remarks>
    /// Sent immediately, not queued: dropped while the client is not connected, including while
    /// Connect() is still in progress.
    /// </remarks>
    public Task SendConfigure(ConfigureSchema configure);

    // NOTE: SendForceEndTurn intentionally lives on the concrete
    // Deepgram.Clients.Flux.WebSocket.Client only, not on this interface. This interface
    // shipped in 7.0.0, and adding a member would source-break third-party implementations
    // in a 7.x release; the member will be added here in 8.0.

    /// <summary>
    /// Sends a CloseStream message to Deepgram and waits briefly for the final results
    /// </summary>
    public Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null);

    /// <summary>
    /// Sends a binary audio message over the WebSocket connection.
    /// </summary>
    /// <remarks>
    /// Audio is queued for a background sender. When a connection ends (a Stop(), a server close
    /// or a dropped connection), audio queued around that moment may be dropped. Audio queued
    /// while no sender is running (after it has stopped, or around a failed Connect()) stays
    /// queued and is sent all at once on the next successful Connect(). Stop sending while
    /// disconnected, or use a new client, to keep stale audio from the server.
    /// </remarks>
    /// <param name="data">The audio to be sent over the WebSocket.</param>
    public void Send(byte[] data, int length = Constants.UseArrayLengthForSend);

    /// <summary>
    /// This method sends a binary message over the WebSocket connection.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="length">The number of bytes from the data to send. Use `Constants.UseArrayLengthForSend` to send the entire array.</param>
    public void SendBinary(byte[] data, int length = Constants.UseArrayLengthForSend);

    /// <summary>
    /// This method sends a text message over the WebSocket connection.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="length">The number of bytes from the data to send. Use `Constants.UseArrayLengthForSend` to send the entire array.</param>
    public void SendMessage(byte[] data, int length = Constants.UseArrayLengthForSend);

    /// <summary>
    /// This method sends a binary message over the WebSocket connection immediately without queueing.
    /// </summary>
    /// <remarks>
    /// The message is sent only on a connection that has finished opening (its Open event has been
    /// or is being raised). Otherwise it is dropped, including while Connect() is still in progress.
    /// </remarks>
    /// <param name="data"></param>
    /// <param name="length">The number of bytes from the data to send. Use `Constants.UseArrayLengthForSend` to send the entire array.</param>
    /// <param name="_cancellationToken">Provide a cancel token to be used for the send function or use the internal one</param>
    public Task SendBinaryImmediately(byte[] data, int length = Constants.UseArrayLengthForSend, CancellationTokenSource? _cancellationToken = null);

    /// <summary>
    /// This method sends a text message over the WebSocket connection immediately without queueing.
    /// </summary>
    /// <remarks>
    /// The message is sent only on a connection that has finished opening (its Open event has been
    /// or is being raised). Otherwise it is dropped, including while Connect() is still in progress.
    /// </remarks>
    /// <param name="data"></param>
    /// <param name="length">The number of bytes from the data to send. Use `Constants.UseArrayLengthForSend` to send the entire array.</param>
    /// <param name="_cancellationToken">Provide a cancel token to be used for the send function or use the internal one</param>
    public Task SendMessageImmediately(byte[] data, int length = Constants.UseArrayLengthForSend, CancellationTokenSource? _cancellationToken = null);
    #endregion

    #region Helpers
    /// <summary>
    /// Retrieves the connection state of the WebSocket
    /// </summary>
    /// <returns>Returns the connection state of the WebSocket</returns>
    public WebSocketState State();

    /// <summary>
    /// Indicates whether the WebSocket is connected
    /// </summary>
    /// <returns>Returns true if the WebSocket is connected</returns>
    public bool IsConnected();
    #endregion
}
