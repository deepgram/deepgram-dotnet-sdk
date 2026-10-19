// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Globalization;

using Deepgram.Abstractions.v2;
using Abstract = Deepgram.Abstractions.v2;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Flux.WebSocket;
using Common = Deepgram.Models.Common.v2.WebSocket;
using Deepgram.Clients.Interfaces.v2;
using Deepgram.Models.Exceptions.v1;

namespace Deepgram.Clients.Flux.WebSocket;

/// <summary>
/// (PREVIEW) Implements the Flux (v2 listen) WebSocket Client for conversational
/// speech-to-text with contextual turn detection.
///
/// Flux differs from classic streaming transcription:
/// results arrive as TurnInfo messages carrying a turn lifecycle event
/// (StartOfTurn, Update, EagerEndOfTurn, TurnResumed, EndOfTurn), and the endpoint accepts
/// only three client control messages: CloseStream, Configure, and ForceEndTurn. There is no
/// KeepAlive and no Finalize; idle connections rely on WebSocket protocol-level ping/pong.
///
/// Note: event handlers are invoked from the receive loop; a slow handler delays delivery of
/// subsequent messages (including EndOfTurn). Keep handlers fast and offload heavy work.
/// <see href="https://developers.deepgram.com/docs/flux/quickstart"/>
/// </summary>
public class Client : AbstractWebSocketClient, IFluxWebSocketClient
{
    #region Fields
    // Guards against CloseStream being sent twice for one connection (a user-initiated Stop()
    // calls SendClose, and the base Stop() would otherwise call it again).
    private int _closeStreamSent = 0;
    #endregion

    /// <param name="apiKey">Required DeepgramApiKey</param>
    /// <param name="options"><see cref="IDeepgramClientOptions"/> for WebSocket Configuration</param>
    public Client(string? apiKey = null, IDeepgramClientOptions? options = null) : base(apiKey, options)
    {
        Log.Verbose("FluxWSClient", "ENTER");
        if (_deepgramClientOptions.KeepAlive)
        {
            // Flux does not support the KeepAlive control message; sending it would terminate
            // the connection with an UNPARSABLE_CLIENT_MESSAGE error. The option is ignored.
            Log.Warning("FluxWSClient", "KeepAlive option is not supported by Flux and will be ignored");
        }
        Log.Verbose("FluxWSClient", "LEAVE");
    }

    #region Event Handlers
    /// <summary>
    /// Fires when an event is received from the Deepgram API
    /// </summary>
    private event EventHandler<ConnectedResponse>? _connectedReceived;
    private event EventHandler<TurnInfoResponse>? _turnInfoReceived;
    private event EventHandler<ConfigureSuccessResponse>? _configureSuccessReceived;
    private event EventHandler<ConfigureFailureResponse>? _configureFailureReceived;
    private event EventHandler<ErrorResponse>? _fluxErrorReceived;
    private event EventHandler<UnhandledResponse>? _fluxUnhandledReceived;
    #endregion

    /// <summary>
    /// Connect to the Deepgram Flux API WebSocket to begin transcribing audio
    /// </summary>
    /// <remarks>
    /// Open event handlers run before Connect() returns, and the receive loop starts only after they
    /// return, so an Open handler that waits for a message from the server blocks until its own
    /// timeout. A blocking Stop().Wait() inside an Open handler waits out this client's close grace
    /// period (<c>DefaultCloseStreamGracePeriodMs</c>, 3 s). An exception thrown by an Open handler is
    /// logged at Error level and Connect() still returns true. Messages still queued from earlier
    /// sends are sent as soon as the new connection opens; the send methods' remarks say which ones
    /// are kept. If a previous connection is still being released, Connect() waits for that to
    /// finish (within its connect timeout) and then opens a new connection. If another Connect() is
    /// still opening the connection or a Stop() is closing it, Connect() waits for it within the
    /// same timeout and returns true only for an open connection that is not stopping.
    /// A Stop() that begins before Connect() returns (for example, from an Open handler) makes
    /// Connect() return false even after Open fired; Close then follows.
    /// </remarks>
    /// <param name="options">Options to use when transcribing audio, <see cref="FluxSchema.Model"/> is required</param>
    /// <param name="cancelToken">Provide a cancel token to be used for the connect or use the internal one</param>
    /// <param name="addons">Additional query parameters to pass to the API</param>
    /// <param name="headers">Additional HTTP headers to send on the WebSocket upgrade</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task<bool> Connect(FluxSchema options, CancellationTokenSource? cancelToken = null, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null)
    {
        Log.Verbose("FluxWSClient.Connect", "ENTER");

        if (options is null || string.IsNullOrWhiteSpace(options.Model))
        {
            throw new DeepgramException("Flux requires a model to be specified (e.g. \"flux-general-en\").");
        }

        Log.Information("Connect", $"options:\n{JsonSerializer.Serialize(options, JsonSerializeOptions.DefaultOptions)}");
        Log.Debug("Connect", $"addons: {addons}");

        try
        {
            Interlocked.Exchange(ref _closeStreamSent, 0);

            var myURI = GetUri(_deepgramClientOptions, options, addons);
            Log.Debug("Connect", $"uri: {myURI}");
            bool bConnected = await base.Connect(myURI.ToString(), cancelToken, headers);
            if (!bConnected)
            {
                Log.Warning("Connect", "Connect failed");
                Log.Verbose("FluxWSClient.Connect", "LEAVE");
                return false;
            }

            // NOTE: Unlike the classic listen client, no KeepAlive and no AutoFlush background
            // threads are started. Flux does not accept those control messages.

            Log.Debug("Connect", "Connect Succeeded");
            Log.Verbose("FluxWSClient.Connect", "LEAVE");

            return true;
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("Connect", "Connect cancelled.");
            Log.Verbose("Connect", $"Connect cancelled. Info: {ex}");
            Log.Verbose("FluxWSClient.Connect", "LEAVE");

            return false;
        }
        catch (Exception ex)
        {
            Log.Error("Connect", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Connect", $"Exception: {ex}");
            Log.Verbose("FluxWSClient.Connect", "LEAVE");
            throw;
        }
    }

    #region Subscribe Event
    /// <summary>
    /// Subscribe to an Open event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<OpenResponse> eventHandler)
    {
        // Create a new event handler that wraps the original one
        EventHandler<Common.OpenResponse> wrappedHandler = (sender, args) =>
        {
            // Cast the event arguments to the desired type
            var castedArgs = new OpenResponse();
            castedArgs.Copy(args);
            if (castedArgs != null)
            {
                // Invoke the original event handler with the casted arguments
                eventHandler(sender, castedArgs);
            }
        };

        // Pass the new event handler to the base Subscribe method
        return await base.Subscribe(wrappedHandler);
    }

    /// <summary>
    /// Subscribe to a Connected event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<ConnectedResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _connectedReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a TurnInfo event from the Deepgram API. The turn lifecycle
    /// (StartOfTurn, Update, EagerEndOfTurn, TurnResumed, EndOfTurn) is carried in
    /// <see cref="TurnInfoResponse.Event"/> / <see cref="TurnInfoResponse.EventType"/>.
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<TurnInfoResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _turnInfoReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a ConfigureSuccess event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<ConfigureSuccessResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _configureSuccessReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a ConfigureFailure event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<ConfigureFailureResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _configureFailureReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to an Error event from the Deepgram API. Flux fatal errors carry a
    /// sequence_id, code, and description; the connection closes after one is received.
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<ErrorResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _fluxErrorReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

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
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<CloseResponse> eventHandler)
    {
        // Create a new event handler that wraps the original one
        EventHandler<Common.CloseResponse> wrappedHandler = (sender, args) =>
        {
            // Cast the event arguments to the desired type
            var castedArgs = new CloseResponse();
            castedArgs.Copy(args);
            if (castedArgs != null)
            {
                // Invoke the original event handler with the casted arguments
                eventHandler(sender, castedArgs);
            }
        };

        return await base.Subscribe(wrappedHandler);
    }

    /// <summary>
    /// Subscribe to an Unhandled event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<UnhandledResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _fluxUnhandledReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }
    #endregion

    #region Send Functions
    /// <summary>
    /// Sends a Configure message to Deepgram to update end-of-turn thresholds, keyterms, or
    /// language hints mid-stream. Parameters left null keep their current values. The API
    /// responds with a ConfigureSuccess or ConfigureFailure message.
    /// </summary>
    /// <remarks>
    /// Sent immediately, not queued: dropped while the client is not connected, including while
    /// Connect() is still in progress.
    /// </remarks>
    /// <param name="configure">The configuration update to send</param>
    public async Task SendConfigure(ConfigureSchema configure)
    {
        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }
        configure.Type ??= Constants.Configure;

        Log.Debug("SendConfigure", $"Sending Configure Message Immediately... {configure}");
        byte[] data = Encoding.UTF8.GetBytes(configure.ToString());
        await SendMessageImmediately(data);
    }

    /// <summary>
    /// Sends a ForceEndTurn message to Deepgram to end the current turn immediately, on your
    /// own signal (push-to-talk release, DTMF, your own VAD). Flux ends the turn on the audio
    /// transcribed so far and emits a standard EndOfTurn TurnInfo with
    /// <see cref="TurnInfoResponse.Trigger"/> set to "manual". When no turn is active, the
    /// server ignores the message and replies with a Warning (code
    /// FORCE_END_TURN_NO_ACTIVE_TURN), which this SDK surfaces via the Unhandled event.
    /// Set <see cref="FluxSchema.EotThreshold"/> to 1.0 to suppress Flux's native end-of-turn
    /// detection entirely and drive every turn ending yourself.
    /// Audio queued via <see cref="Send"/> before this call is flushed to the socket first, so
    /// the turn always ends on the audio you actually sent.
    /// <see href="https://developers.deepgram.com/docs/flux/force-end-turn"/>
    /// </summary>
    /// <remarks>
    /// Sent immediately, not queued: dropped while the client is not connected, including while
    /// Connect() is still in progress.
    /// </remarks>
    public async Task SendForceEndTurn()
    {
        // Flush any buffered audio first so ForceEndTurn can't overtake audio still in the send
        // queue (it is sent immediately and would otherwise race ahead, ending the turn before
        // the final chunks reach Flux). Mirrors the SendFinalize fix in the v1 listen client.
        Log.Debug("SendForceEndTurn", "Flushing buffered audio before ForceEndTurn...");
        await Flush();

        ControlMessage message = new ControlMessage(Constants.ForceEndTurn);

        Log.Debug("SendForceEndTurn", "Sending ForceEndTurn Message Immediately...");
        byte[] data = Encoding.ASCII.GetBytes(message.ToString());
        await SendMessageImmediately(data);
    }

    /// <summary>
    /// Sends a binary audio message over the WebSocket connection. Flux works best with
    /// audio sent in chunks of roughly 80ms.
    /// </summary>
    /// <param name="data">The audio to be sent over the WebSocket.</param>
    /// <param name="length">The number of bytes from the data to send. Use `Constants.UseArrayLengthForSend` to send the entire array.</param>
    public void Send(byte[] data, int length = Abstract.Constants.UseArrayLengthForSend) => SendBinary(data, length);

    /// <summary>
    /// Sends a CloseStream message to Deepgram, then waits (up to
    /// <see cref="Constants.DefaultCloseStreamGracePeriodMs"/>) for the server to deliver the
    /// final turn results and close the connection, so the end of the conversation is not lost.
    /// </summary>
    /// <param name="nullByte">Send a null byte instead of the CloseStream message</param>
    /// <param name="_cancellationToken">Provide a cancel token to be used for the send or use the internal one</param>
    public override Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null)
    {
        var owned = CurrentConnection;
        if (owned == null)
        {
            Log.Warning("SendClose", "ClientWebSocket is null or not connected. Skipping...");
            return Task.CompletedTask;
        }
        return SendClose(owned, nullByte, _cancellationToken);
    }

    /// <summary>
    /// SendClose() for one connection. Stop() passes the connection it snapshotted on entry, so
    /// the CloseStream message and the grace-period wait never reach a newer connection (#455).
    /// </summary>
    private protected override async Task SendClose(ConnectionState owned, bool nullByte, CancellationTokenSource? _cancellationToken)
    {
        if (!owned.IsOpen)
        {
            Log.Warning("SendClose", "ClientWebSocket is null or not connected. Skipping...");
            return;
        }

        // The caller's token, or this connection's own token (#390, #455).
        var token = _cancellationToken?.Token ?? owned.Token;

        Log.Debug("SendClose", "Sending Close Message Immediately...");
        if (nullByte)
        {
            // send a close to Deepgram, on this connection's socket only
            await SendOnConnection(owned, new ArraySegment<byte>(new byte[1] { 0 }), WebSocketMessageType.Binary, token);
            return;
        }

        if (Interlocked.Exchange(ref _closeStreamSent, 1) == 1)
        {
            Log.Debug("SendClose", "CloseStream already sent for this connection. Skipping...");
            return;
        }

        ControlMessage message = new ControlMessage(Constants.CloseStream);
        byte[] data = Encoding.ASCII.GetBytes(message.ToString());
        // The caller's token bounds the send, so Stop(token) keeps its timeout.
        await SendOnConnection(owned, new ArraySegment<byte>(data), WebSocketMessageType.Text, token);

        // Flux delivers the final turn results AFTER CloseStream and then closes the connection
        // from its side. Give it a grace period instead of tearing the socket down immediately.
        try
        {
            var deadline = Constants.DefaultCloseStreamGracePeriodMs;
            // Watch this connection only: IsOpen turns false once the server closes it or a
            // teardown releases it (#390, #455).
            while (deadline > 0 && owned.IsOpen)
            {
                await Task.Delay(50, token).ConfigureAwait(false);
                deadline -= 50;
            }
            Log.Debug("SendClose", deadline > 0 ? "Server closed the connection" : "Grace period elapsed");
        }
        catch (OperationCanceledException)
        {
            Log.Debug("SendClose", "Grace period cancelled.");
        }
        catch (Exception ex)
        {
            Log.Verbose("SendClose", $"Ignoring exception while waiting for server close: {ex}");
        }
    }
    #endregion

    /// <summary>
    /// Closes the connection to the Deepgram Flux API: sends CloseStream, waits briefly for the
    /// server to deliver the final turn results and close from its side, then completes the
    /// teardown. When the server closes during that wait, the receive loop performs the full
    /// teardown concurrently; this method coordinates with it instead of faulting on the
    /// already-disposed socket.
    /// </summary>
    /// <remarks>
    /// Stop() returns after the Close event has been delivered, except when it ends a connection
    /// whose Open handlers are still running: then it returns without waiting for them, and Close
    /// fires as soon as they return. If a new Connect() runs before then, that Close can arrive after
    /// the new connection's Open. Dispose() raises no Close.
    /// </remarks>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public new async Task<bool> Stop(CancellationTokenSource? cancelToken = null, bool nullByte = false)
    {
        Log.Verbose("FluxWSClient.Stop", "ENTER");

        // Snapshot the connection on entry: this Stop() tears down only that connection, even if
        // a newer one is published while it waits (#455).
        var owned = CurrentConnection;
        if (owned == null)
        {
            Log.Information("Stop", "Client has already been disposed");
            Log.Verbose("FluxWSClient.Stop", "LEAVE");
            return true;
        }

        // From here on a Connect() still opening this connection fails, and a concurrent Connect()
        // waits for this Stop() instead of reporting the connection as usable.
        owned.MarkStopping();

        try
        {
            return await StopOwnedConnection(owned, cancelToken, nullByte).ConfigureAwait(false);
        }
        finally
        {
            // A stopping connection is always released (I6), even if a log provider (user code)
            // threw inside a catch block below. Idempotent with the releases below.
            if (!owned.IsReleased)
            {
                await ReleaseAndRaiseClose(owned).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// The work of <see cref="Stop"/> for the connection it snapshotted, after it is marked stopping.
    /// </summary>
    private async Task<bool> StopOwnedConnection(ConnectionState owned, CancellationTokenSource? cancelToken, bool nullByte)
    {
        try
        {
            // A connection that never raised Open (stopped while connecting) gets no close message.
            if (owned.IsOpened && owned.IsOpen)
            {
                // Sends {"type":"CloseStream"} and waits (up to the grace period) for the
                // server-side close.
                await SendClose(owned, nullByte, cancelToken);
            }

            // If the server closed during the grace period, the receive loop is performing the
            // full teardown (including raising the Close event). Give it a moment to finish.
            var waited = 0;
            while (waited < 2000)
            {
                if (owned.IsReleased)
                {
                    await owned.ReleaseCompleted.ConfigureAwait(false);
                    Log.Debug("Stop", "Receive loop completed the teardown");
                    Log.Verbose("FluxWSClient.Stop", "LEAVE");
                    return true;
                }
                try
                {
                    var state = owned.Socket.State;
                    if (state != WebSocketState.CloseReceived && state != WebSocketState.CloseSent)
                    {
                        break;
                    }
                }
                catch (ObjectDisposedException)
                {
                    // torn down concurrently; treat like the released case on the next pass
                }
                await Task.Delay(50).ConfigureAwait(false);
                waited += 50;
            }

            if (owned.IsReleased)
            {
                await owned.ReleaseCompleted.ConfigureAwait(false);
                Log.Verbose("FluxWSClient.Stop", "LEAVE");
                return true;
            }

            // Server did not close (or teardown stalled): fall back to the base teardown.
            // SendClose will not re-send CloseStream (guarded by _closeStreamSent).
            Log.Debug("Stop", "Falling back to base Stop...");
            // Stop this connection only; base.Stop() would act on whatever is current by now.
            var result = await StopConnection(owned, cancelToken, nullByte);
            Log.Verbose("FluxWSClient.Stop", "LEAVE");
            return result;
        }
        catch (Exception ex) when (ex is WebSocketException || ex is ObjectDisposedException || ex is NullReferenceException
            || ex is OperationCanceledException)
        {
            // The receive loop tore the connection down while we were stopping - the shutdown
            // already happened, just not by us - or the caller's token cancelled the close
            // handshake. Finish the teardown and report success, as the base Stop() does.
            Log.Debug("Stop", $"Connection was torn down concurrently: {ex.GetType()}");
            // Release only this connection: cancel before dispose, and leave any newer
            // connection's fields alone. Raise Close if this call did the release (#455).
            await ReleaseAndRaiseClose(owned);
            Log.Verbose("FluxWSClient.Stop", "LEAVE");
            return true;
        }
        catch (Exception ex)
        {
            // Anything else: still release this connection (raising Close if this call did the
            // release), so a Stop() never leaves a stopping connection published, then rethrow.
            Log.Error("Stop", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Stop", $"Exception: {ex}");
            Log.Verbose("FluxWSClient.Stop", "LEAVE");

            await ReleaseAndRaiseClose(owned).ConfigureAwait(false);
            throw;
        }
    }

    internal override void ProcessBinaryMessage(WebSocketReceiveResult result, MemoryStream ms)
    {
        // The Flux API is not expected to send binary frames; ignore them rather than fault
        // the receive loop.
        Log.Warning("ProcessBinaryMessage", "Received unexpected binary message from the Flux API. Ignoring...");
    }

    internal override void ProcessTextMessage(WebSocketReceiveResult result, MemoryStream ms)
    {
        Log.Verbose("FluxWSClient.ProcessTextMessage", "ENTER");

        ms.Seek(0, SeekOrigin.Begin);

        var response = Encoding.UTF8.GetString(ms.ToArray());
        if (response == null)
        {
            Log.Warning("ProcessTextMessage", "Response is null");
            Log.Verbose("FluxWSClient.ProcessTextMessage", "LEAVE");
            return;
        }

        try
        {
            if (Log.IsEnabled(LogLevel.Verbose))
            {
                Log.Verbose("ProcessTextMessage", $"raw response: {response}");
            }

            var data = JsonDocument.Parse(response);
            if (!data.RootElement.TryGetProperty("type", out var typeElement) ||
                typeElement.ValueKind != JsonValueKind.String ||
                !Enum.TryParse<FluxType>(typeElement.GetString(), out var val))
            {
                Log.Debug("ProcessTextMessage", "Message type is missing or unknown. Invoking UnhandledResponse...");
                InvokeUnhandled(response);
                Log.Verbose("FluxWSClient.ProcessTextMessage", "LEAVE");
                return;
            }

            if (Log.IsEnabled(LogLevel.Verbose))
            {
                Log.Verbose("ProcessTextMessage", $"Type: {val}");
            }

            switch (val)
            {
                case FluxType.Open:
                case FluxType.Close:
                    Log.Debug("ProcessTextMessage", "Calling base.ProcessTextMessage...");
                    base.ProcessTextMessage(result, ms);
                    break;
                case FluxType.Connected:
                    var connectedResponse = data.Deserialize<ConnectedResponse>();
                    if (_connectedReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_connectedReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (connectedResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "ConnectedResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.Debug("ProcessTextMessage", $"Invoking ConnectedResponse. event: {connectedResponse}");
                    }
                    InvokeParallel(_connectedReceived, connectedResponse);
                    break;
                case FluxType.TurnInfo:
                    var turnInfoResponse = data.Deserialize<TurnInfoResponse>();
                    if (_turnInfoReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_turnInfoReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (turnInfoResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "TurnInfoResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.Debug("ProcessTextMessage", $"Invoking TurnInfoResponse. event: {turnInfoResponse}");
                    }
                    InvokeParallel(_turnInfoReceived, turnInfoResponse);
                    break;
                case FluxType.ConfigureSuccess:
                    var configureSuccessResponse = data.Deserialize<ConfigureSuccessResponse>();
                    if (_configureSuccessReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_configureSuccessReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (configureSuccessResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "ConfigureSuccessResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.Debug("ProcessTextMessage", $"Invoking ConfigureSuccessResponse. event: {configureSuccessResponse}");
                    }
                    InvokeParallel(_configureSuccessReceived, configureSuccessResponse);
                    break;
                case FluxType.ConfigureFailure:
                    var configureFailureResponse = data.Deserialize<ConfigureFailureResponse>();
                    if (_configureFailureReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_configureFailureReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (configureFailureResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "ConfigureFailureResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.Debug("ProcessTextMessage", $"Invoking ConfigureFailureResponse. event: {configureFailureResponse}");
                    }
                    InvokeParallel(_configureFailureReceived, configureFailureResponse);
                    break;
                case FluxType.Error:
                    // Flux fatal errors use the wire type "Error" but carry a different shape
                    // (sequence_id/code/description) than the common error message, so they are
                    // handled here rather than by the base class.
                    var errorResponse = data.Deserialize<ErrorResponse>();
                    if (_fluxErrorReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_fluxErrorReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (errorResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "ErrorResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.Debug("ProcessTextMessage", $"Invoking ErrorResponse. event: {errorResponse}");
                    }
                    InvokeParallel(_fluxErrorReceived, errorResponse);
                    break;
                default:
                    Log.Debug("ProcessTextMessage", "Unknown message type. Invoking UnhandledResponse...");
                    InvokeUnhandled(response);
                    break;
            }

            Log.Debug("ProcessTextMessage", "Succeeded");
            Log.Verbose("FluxWSClient.ProcessTextMessage", "LEAVE");
        }
        catch (JsonException ex)
        {
            Log.Error("ProcessTextMessage", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessTextMessage", $"Exception: {ex}");
            Log.Verbose("FluxWSClient.ProcessTextMessage", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessTextMessage", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessTextMessage", $"Exception: {ex}");
            Log.Verbose("FluxWSClient.ProcessTextMessage", "LEAVE");
        }
    }

    #region Helpers
    private void InvokeUnhandled(string response)
    {
        if (_fluxUnhandledReceived == null)
        {
            Log.Debug("InvokeUnhandled", "_fluxUnhandledReceived has no listeners");
            return;
        }

        var unhandledResponse = new UnhandledResponse();
        unhandledResponse.Type = Common.WebSocketType.Unhandled;
        unhandledResponse.Raw = response;

        InvokeParallel(_fluxUnhandledReceived, unhandledResponse);
    }

    /// <summary>
    /// Get the URI for the WebSocket connection
    /// </summary>
    internal static Uri GetUri(IDeepgramClientOptions options, FluxSchema parameter, Dictionary<string, string>? addons = null)
    {
        var baseAddress = options.BaseAddress;

        // If the base address carries an API version (the SDK default appends /v1), remove it.
        // Flux lives on /v2/listen; the version is part of UriSegments.LISTEN_V2. This follows
        // the same pattern the Agent client uses for its own endpoint.
        Regex regex = new Regex(@"\b(\/v[0-9]+)\b", RegexOptions.IgnoreCase);
        if (regex.IsMatch(baseAddress))
        {
            Log.Information("GetUri", $"BaseAddress contains API version: {baseAddress}");
            baseAddress = regex.Replace(baseAddress, "");
            Log.Debug("GetUri", $"BaseAddress: {baseAddress}");
        }
        baseAddress = baseAddress.TrimEnd('/');

        var queryString = BuildQueryString(parameter, addons);

        return new Uri($"{baseAddress}/{UriSegments.LISTEN_V2}?{queryString}");
    }

    /// <summary>
    /// Builds the query string for the Flux connection. Numbers are formatted with the
    /// invariant culture so threshold values like 0.7 are never sent as "0,7" on machines
    /// with a comma decimal separator, and list values are repeated (keyterm=a&amp;keyterm=b).
    /// </summary>
    internal static string BuildQueryString(FluxSchema parameter, Dictionary<string, string>? addons = null)
    {
        var sb = new StringBuilder();

        var propertyInfoList = parameter.GetType()
            .GetProperties()
            .Where(v => v.GetValue(parameter) is not null);

        foreach (var pInfo in propertyInfoList)
        {
            var name = pInfo.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
            if (name is null)
            {
                continue;
            }
            var pValue = pInfo.GetValue(parameter);

            switch (pValue)
            {
                case bool boolean:
                    Append(name, boolean ? "true" : "false");
                    break;
                case System.Collections.IList list:
                    foreach (var value in list)
                    {
                        Append(name, Convert.ToString(value, CultureInfo.InvariantCulture) ?? "");
                    }
                    break;
                case IFormattable formattable:
                    Append(name, formattable.ToString(null, CultureInfo.InvariantCulture));
                    break;
                default:
                    Append(name, pValue?.ToString() ?? "");
                    break;
            }
        }

        if (addons != null)
        {
            foreach (var kvp in addons)
            {
                Append(kvp.Key, kvp.Value);
            }
        }

        return sb.ToString().TrimEnd('&');

        void Append(string name, string value) =>
            sb.Append($"{name}={HttpUtility.UrlEncode(value)}&");
    }
    #endregion
}
