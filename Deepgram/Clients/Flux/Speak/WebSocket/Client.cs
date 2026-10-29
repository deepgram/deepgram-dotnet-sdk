// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Globalization;

using Deepgram.Abstractions.v2;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Flux.Speak.WebSocket;
using Common = Deepgram.Models.Common.v2.WebSocket;
using Deepgram.Clients.Interfaces.v2;
using Deepgram.Models.Exceptions.v1;

namespace Deepgram.Clients.Flux.Speak.WebSocket;

/// <summary>
/// Implements the Flux (v2 speak) text-to-speech WebSocket Client: stream text in and
/// receive synthesized audio out, turn by turn, for voice-agent pipelines.
///
/// Sends five client messages — Speak, Flush, Interrupt, Configure, and Close.
/// Audio arrives as interleaved binary chunks (the Audio event) alongside JSON control messages
/// (Connected, SpeechStarted, SpeechMetadata, SpeechInterrupted, Flushed, ConfigureSuccess,
/// ConfigureFailure, SessionMetadata, Warning, Error).
///
/// Note: event handlers are invoked from the receive loop; a slow handler delays delivery of
/// subsequent messages, including audio chunks. Keep handlers fast and offload heavy work.
/// <see href="https://developers.deepgram.com/docs/flux-tts/quickstart"/>
/// </summary>
public class Client : AbstractWebSocketClient, IFluxSpeakWebSocketClient
{
    #region Fields
    // Guards against the Close event being raised twice for one connection, which can happen
    // when a user-initiated Stop() and the receive loop's server-close handling overlap.
    private int _closeEventFired = 0;

    // Guards against the Close message being sent twice for one connection (a user-initiated
    // Stop() calls SendClose, and the base Stop() would otherwise call it again).
    private int _closeSent = 0;
    #endregion

    /// <param name="apiKey">Required DeepgramApiKey</param>
    /// <param name="options"><see cref="IDeepgramClientOptions"/> for WebSocket Configuration</param>
    public Client(string? apiKey = null, IDeepgramClientOptions? options = null) : base(apiKey, options)
    {
        Log.Verbose("FluxSpeakWSClient", "ENTER");
        if (_deepgramClientOptions.KeepAlive)
        {
            // Flux does not support the KeepAlive control message; the option is ignored.
            Log.Warning("FluxSpeakWSClient", "KeepAlive option is not supported by Flux and will be ignored");
        }
        Log.Verbose("FluxSpeakWSClient", "LEAVE");
    }

    #region Event Handlers
    /// <summary>
    /// Fires when an event is received from the Deepgram API
    /// </summary>
    private event EventHandler<AudioResponse>? _audioReceived;
    private event EventHandler<ConnectedResponse>? _connectedReceived;
    private event EventHandler<SpeechStartedResponse>? _speechStartedReceived;
    private event EventHandler<SpeechMetadataResponse>? _speechMetadataReceived;
    private event EventHandler<SpeechInterruptedResponse>? _speechInterruptedReceived;
    private event EventHandler<FlushedResponse>? _flushedReceived;
    private event EventHandler<SessionMetadataResponse>? _sessionMetadataReceived;
    private event EventHandler<WarningResponse>? _warningReceived;
    private event EventHandler<ConfigureSuccessResponse>? _configureSuccessReceived;
    private event EventHandler<ConfigureFailureResponse>? _configureFailureReceived;
    private event EventHandler<ErrorResponse>? _speakErrorReceived;
    private event EventHandler<UnhandledResponse>? _speakUnhandledReceived;
    #endregion

    /// <summary>
    /// Connect to the Deepgram Flux (v2 speak) WebSocket to begin synthesizing text
    /// </summary>
    /// <param name="options">Options to use when synthesizing, <see cref="SpeakSchema.Model"/> is required</param>
    /// <param name="cancelToken">Provide a cancel token to be used for the connect or use the internal one</param>
    /// <param name="addons">Additional query parameters to pass to the API</param>
    /// <param name="headers">Additional HTTP headers to send on the WebSocket upgrade</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task<bool> Connect(SpeakSchema options, CancellationTokenSource? cancelToken = null, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null)
    {
        Log.Verbose("FluxSpeakWSClient.Connect", "ENTER");

        if (options is null || string.IsNullOrWhiteSpace(options.Model))
        {
            throw new DeepgramException("Flux TTS requires a model to be specified (e.g. \"flux-alexis-en\").");
        }

        Log.Information("Connect", $"options:\n{JsonSerializer.Serialize(options, JsonSerializeOptions.DefaultOptions)}");
        Log.Debug("Connect", $"addons: {addons}");

        try
        {
            Interlocked.Exchange(ref _closeEventFired, 0);
            Interlocked.Exchange(ref _closeSent, 0);

            var myURI = GetUri(_deepgramClientOptions, options, addons);
            Log.Debug("Connect", $"uri: {myURI}");
            bool bConnected = await base.Connect(myURI.ToString(), cancelToken, headers);
            if (!bConnected)
            {
                Log.Warning("Connect", "Connect failed");
                Log.Verbose("FluxSpeakWSClient.Connect", "LEAVE");
                return false;
            }

            // NOTE: no KeepAlive and no AutoFlush background threads are started; Flux does not
            // accept those control messages.

            Log.Debug("Connect", "Connect Succeeded");
            Log.Verbose("FluxSpeakWSClient.Connect", "LEAVE");

            return true;
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("Connect", "Connect cancelled.");
            Log.Verbose("Connect", $"Connect cancelled. Info: {ex}");
            Log.Verbose("FluxSpeakWSClient.Connect", "LEAVE");

            return false;
        }
        catch (Exception ex)
        {
            Log.Error("Connect", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Connect", $"Exception: {ex}");
            Log.Verbose("FluxSpeakWSClient.Connect", "LEAVE");
            throw;
        }
    }

    #region Subscribe Event
    /// <summary>
    /// Subscribe to an Open event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<OpenResponse> eventHandler)
    {
        EventHandler<Common.OpenResponse> wrappedHandler = (sender, args) =>
        {
            var castedArgs = new OpenResponse();
            castedArgs.Copy(args);
            if (castedArgs != null)
            {
                eventHandler(sender, castedArgs);
            }
        };

        return await base.Subscribe(wrappedHandler);
    }

    /// <summary>
    /// Subscribe to a binary Audio event carrying a chunk of synthesized audio.
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<AudioResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _audioReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a Connected event from the Deepgram API
    /// </summary>
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
    /// Subscribe to a SpeechStarted event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<SpeechStartedResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _speechStartedReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a SpeechMetadata event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<SpeechMetadataResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _speechMetadataReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a SpeechInterrupted event from the Deepgram API, sent in response to a client
    /// Interrupt.
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<SpeechInterruptedResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _speechInterruptedReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a Flushed event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<FlushedResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _flushedReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a SessionMetadata event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<SessionMetadataResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _sessionMetadataReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a Warning event from the Deepgram API. Synthesis continues and the connection
    /// is unaffected.
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<WarningResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _warningReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a ConfigureSuccess event from the Deepgram API, acknowledging a client
    /// Configure message.
    /// </summary>
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
    /// Subscribe to a ConfigureFailure event from the Deepgram API, rejecting a client
    /// Configure message. Non-fatal — the connection stays open.
    /// </summary>
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
    /// Subscribe to a fatal Error event from the Deepgram API. The wire type is "Error"; the
    /// server closes the connection after sending it.
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<ErrorResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _speakErrorReceived += (sender, e) => eventHandler(sender, e);
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
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<CloseResponse> eventHandler)
    {
        EventHandler<Common.CloseResponse> wrappedHandler = (sender, args) =>
        {
            // A user-initiated Stop() and the receive loop's handling of the server close can
            // overlap and both raise the base Close event; deliver only the first to subscribers.
            if (Interlocked.Exchange(ref _closeEventFired, 1) == 1)
            {
                Log.Debug("CloseEvent", "Close event already delivered for this connection. Skipping...");
                return;
            }

            var castedArgs = new CloseResponse();
            castedArgs.Copy(args);
            if (castedArgs != null)
            {
                eventHandler(sender, castedArgs);
            }
        };

        return await base.Subscribe(wrappedHandler);
    }

    /// <summary>
    /// Subscribe to an Unhandled event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<UnhandledResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _speakUnhandledReceived += (sender, e) => eventHandler(sender, e);
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
    /// Sends a Speak message ({"type":"Speak","text":...}) to synthesize text into the active turn.
    /// </summary>
    /// <param name="text">The text to synthesize</param>
    public async Task SendText(string text)
    {
        if (text is null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        var message = new SpeakMessage(text);
        Log.Debug("SendText", "Sending Speak Message Immediately...");
        byte[] data = Encoding.UTF8.GetBytes(message.ToString());
        await SendMessageImmediately(data);
    }

    /// <summary>
    /// Sends a Flush message ({"type":"Flush"}) to end the active turn and generate the remaining
    /// audio. The server replies with Flushed, then SpeechMetadata once the turn's audio is sent.
    /// </summary>
    public async Task SendFlush()
    {
        var message = new ControlMessage(Constants.Flush);
        Log.Debug("SendFlush", "Sending Flush Message Immediately...");
        byte[] data = Encoding.UTF8.GetBytes(message.ToString());
        await SendMessageImmediately(data);
    }

    /// <summary>
    /// Sends an Interrupt message to cancel the currently active turn (barge-in). Stopping local
    /// audio playback is the caller's responsibility - Interrupt is for reconciling the server's
    /// record of what was spoken, not for stopping audio. The server replies with
    /// SpeechInterrupted, or a Warning if it cannot act on the interrupt (no audio generated yet,
    /// an earlier interrupt still in flight, a non-advancing offset, or no active turn).
    /// </summary>
    /// <param name="interrupt">
    /// The interrupt to send. Include <see cref="InterruptSchema.PlaybackOffset"/> so the server
    /// can compute the text_spoken/text_remaining split; omit it (or pass null for a bare
    /// interrupt) and the turn still cancels, but that split comes back empty.
    /// </param>
    public async Task SendInterrupt(InterruptSchema? interrupt = null)
    {
        interrupt ??= new InterruptSchema();
        interrupt.Type ??= Constants.Interrupt;

        Log.Debug("SendInterrupt", $"Sending Interrupt Message Immediately... {interrupt}");
        byte[] data = Encoding.UTF8.GetBytes(interrupt.ToString());
        await SendMessageImmediately(data);
    }

    /// <summary>
    /// Sends an Interrupt message carrying a playback offset. Convenience overload of
    /// <see cref="SendInterrupt(InterruptSchema?)"/> for the common case.
    /// </summary>
    /// <param name="playbackOffsetMs">
    /// Milliseconds of audio played, cumulative from the start of the session (not the current
    /// turn). Each interrupt's offset must advance past the position established by the previous
    /// interrupt. Report your audio player's actual position - not bytes received, which arrive
    /// much faster than realtime.
    /// </param>
    public Task SendInterrupt(long playbackOffsetMs) =>
        SendInterrupt(new InterruptSchema { PlaybackOffset = new PlaybackOffset(playbackOffsetMs) });

    /// <summary>
    /// Sends a Configure message to change the speaking rate mid-session, without reconnecting.
    /// The server responds with ConfigureSuccess (echoing what it applied) or ConfigureFailure
    /// (e.g. code SPEED_OUT_OF_RANGE) - speed is not range-checked client-side.
    /// </summary>
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
    /// Sends a Close message ({"type":"Close"}), then waits (up to
    /// <see cref="Constants.DefaultCloseGracePeriodMs"/>) for the server to drain all remaining
    /// and queued audio, emit a final SessionMetadata, and close the connection, so the end of the
    /// output is not lost.
    /// </summary>
    /// <param name="nullByte">Send a null byte instead of the Close message</param>
    /// <param name="_cancellationToken">Provide a cancel token to be used for the send or use the internal one</param>
    public override async Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null)
    {
        if (_clientWebSocket == null || !IsConnected())
        {
            Log.Warning("SendClose", "ClientWebSocket is null or not connected. Skipping...");
            return;
        }

        // Use the teardown-safe token: a concurrent Stop()/Dispose() nulls _cancellationTokenSource
        // outside _mutexSend, so dereferencing the raw field after an await could throw NRE - which
        // is not in Stop()'s benign catch set and would escape (#390).
        var token = _cancellationToken?.Token ?? GetInternalCancellationToken();

        Log.Debug("SendClose", "Sending Close Message Immediately...");
        if (nullByte)
        {
            // Snapshot the socket too - a concurrent Dispose() can null it while we hold the mutex.
            var socket = _clientWebSocket;
            if (socket == null || !IsConnected())
            {
                Log.Debug("SendClose", "WebSocket is not connected. Exiting...");
                return;
            }

            await _mutexSend.WaitAsync(token);
            try
            {
                await socket.SendAsync(new ArraySegment<byte>(new byte[1] { 0 }), WebSocketMessageType.Binary, true, token)
                    .ConfigureAwait(false);
            }
            finally
            {
                _mutexSend.Release();
            }
            return;
        }

        if (Interlocked.Exchange(ref _closeSent, 1) == 1)
        {
            Log.Debug("SendClose", "Close already sent for this connection. Skipping...");
            return;
        }

        ControlMessage message = new ControlMessage(Constants.Close);
        byte[] data = Encoding.ASCII.GetBytes(message.ToString());
        await SendMessageImmediately(data);

        // Flux drains remaining and queued audio and emits SessionMetadata AFTER Close, then
        // closes the connection from its side. Give it a grace period instead of tearing the
        // socket down immediately.
        try
        {
            var deadline = Constants.DefaultCloseGracePeriodMs;
            // Re-snapshot each iteration so a concurrent Dispose() nulling the field can't turn the
            // condition into a check-then-act NRE (see #390).
            var socket = _clientWebSocket;
            while (deadline > 0 && socket != null && socket.State == WebSocketState.Open)
            {
                await Task.Delay(50, token).ConfigureAwait(false);
                deadline -= 50;
                socket = _clientWebSocket;
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
    /// Closes the connection to the Deepgram Flux (v2 speak) API: sends Close, waits briefly for
    /// the server to drain remaining audio and close from its side, then completes the teardown.
    /// When the server closes during that wait, the receive loop performs the full teardown
    /// concurrently; this method coordinates with it instead of faulting on the disposed socket.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public new async Task<bool> Stop(CancellationTokenSource? cancelToken = null, bool nullByte = false)
    {
        Log.Verbose("FluxSpeakWSClient.Stop", "ENTER");

        if (_clientWebSocket == null)
        {
            Log.Information("Stop", "Client has already been disposed");
            Log.Verbose("FluxSpeakWSClient.Stop", "LEAVE");
            return true;
        }

        try
        {
            if (IsConnected())
            {
                // Sends {"type":"Close"} and waits (up to the grace period) for the server-side close.
                await SendClose(nullByte, cancelToken);
            }

            // If the server closed during the grace period, the receive loop is performing the
            // full teardown (including raising the Close event). Give it a moment to finish.
            var waited = 0;
            while (waited < 2000)
            {
                var socket = _clientWebSocket;
                if (socket == null)
                {
                    Log.Debug("Stop", "Receive loop completed the teardown");
                    Log.Verbose("FluxSpeakWSClient.Stop", "LEAVE");
                    return true;
                }
                try
                {
                    if (socket.State != WebSocketState.CloseReceived && socket.State != WebSocketState.CloseSent)
                    {
                        break;
                    }
                }
                catch (ObjectDisposedException)
                {
                    // torn down concurrently; treat like the socket-null case on the next pass
                }
                await Task.Delay(50).ConfigureAwait(false);
                waited += 50;
            }

            if (_clientWebSocket == null)
            {
                Log.Verbose("FluxSpeakWSClient.Stop", "LEAVE");
                return true;
            }

            // Server did not close (or teardown stalled): fall back to the base teardown.
            // SendClose will not re-send Close (guarded by _closeSent).
            Log.Debug("Stop", "Falling back to base Stop...");
            var result = await base.Stop(cancelToken, nullByte);
            Log.Verbose("FluxSpeakWSClient.Stop", "LEAVE");
            return result;
        }
        catch (Exception ex) when (ex is WebSocketException || ex is ObjectDisposedException || ex is NullReferenceException)
        {
            // The receive loop tore the connection down while we were stopping - the shutdown
            // already happened, just not by us. Finish any remaining cleanup and report success.
            Log.Debug("Stop", $"Connection was torn down concurrently: {ex.GetType()}");
            try
            {
                _cancellationTokenSource?.Dispose();
            }
            catch
            {
                // already disposed by the concurrent teardown
            }
            _cancellationTokenSource = null;
            _clientWebSocket = null;
            Log.Verbose("FluxSpeakWSClient.Stop", "LEAVE");
            return true;
        }
    }

    internal override void ProcessBinaryMessage(WebSocketReceiveResult result, MemoryStream ms)
    {
        Log.Verbose("FluxSpeakWSClient.ProcessBinaryMessage", "ENTER");

        if (_audioReceived == null)
        {
            Log.Debug("ProcessBinaryMessage", "_audioReceived has no listeners");
            Log.Verbose("FluxSpeakWSClient.ProcessBinaryMessage", "LEAVE");
            return;
        }

        // Copy the bytes into a stream the subscriber owns; the receive loop reuses/disposes ms.
        ms.Seek(0, SeekOrigin.Begin);
        var audioStream = new MemoryStream();
        ms.CopyTo(audioStream);
        audioStream.Seek(0, SeekOrigin.Begin);

        var audioResponse = new AudioResponse { Stream = audioStream, Type = SpeakType.Audio };
        Log.Debug("ProcessBinaryMessage", $"Invoking AudioResponse. bytes: {audioStream.Length}");
        InvokeParallel(_audioReceived, audioResponse);

        Log.Verbose("FluxSpeakWSClient.ProcessBinaryMessage", "LEAVE");
    }

    internal override void ProcessTextMessage(WebSocketReceiveResult result, MemoryStream ms)
    {
        Log.Verbose("FluxSpeakWSClient.ProcessTextMessage", "ENTER");

        ms.Seek(0, SeekOrigin.Begin);

        var response = Encoding.UTF8.GetString(ms.ToArray());
        if (response == null)
        {
            Log.Warning("ProcessTextMessage", "Response is null");
            Log.Verbose("FluxSpeakWSClient.ProcessTextMessage", "LEAVE");
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
                !Enum.TryParse<SpeakType>(typeElement.GetString(), out var val))
            {
                Log.Debug("ProcessTextMessage", "Message type is missing or unknown. Invoking UnhandledResponse...");
                InvokeUnhandled(response);
                Log.Verbose("FluxSpeakWSClient.ProcessTextMessage", "LEAVE");
                return;
            }

            if (Log.IsEnabled(LogLevel.Verbose))
            {
                Log.Verbose("ProcessTextMessage", $"Type: {val}");
            }

            switch (val)
            {
                case SpeakType.Open:
                case SpeakType.Close:
                    Log.Debug("ProcessTextMessage", "Calling base.ProcessTextMessage...");
                    base.ProcessTextMessage(result, ms);
                    break;
                case SpeakType.Connected:
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
                    InvokeParallel(_connectedReceived, connectedResponse);
                    break;
                case SpeakType.SpeechStarted:
                    var speechStartedResponse = data.Deserialize<SpeechStartedResponse>();
                    if (_speechStartedReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_speechStartedReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (speechStartedResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "SpeechStartedResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    InvokeParallel(_speechStartedReceived, speechStartedResponse);
                    break;
                case SpeakType.SpeechMetadata:
                    var speechMetadataResponse = data.Deserialize<SpeechMetadataResponse>();
                    if (_speechMetadataReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_speechMetadataReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (speechMetadataResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "SpeechMetadataResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    InvokeParallel(_speechMetadataReceived, speechMetadataResponse);
                    break;
                case SpeakType.SpeechInterrupted:
                    var speechInterruptedResponse = data.Deserialize<SpeechInterruptedResponse>();
                    if (_speechInterruptedReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_speechInterruptedReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (speechInterruptedResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "SpeechInterruptedResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    InvokeParallel(_speechInterruptedReceived, speechInterruptedResponse);
                    break;
                case SpeakType.Flushed:
                    var flushedResponse = data.Deserialize<FlushedResponse>();
                    if (_flushedReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_flushedReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (flushedResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "FlushedResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    InvokeParallel(_flushedReceived, flushedResponse);
                    break;
                case SpeakType.SessionMetadata:
                    var sessionMetadataResponse = data.Deserialize<SessionMetadataResponse>();
                    if (_sessionMetadataReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_sessionMetadataReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (sessionMetadataResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "SessionMetadataResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    InvokeParallel(_sessionMetadataReceived, sessionMetadataResponse);
                    break;
                case SpeakType.Warning:
                    var warningResponse = data.Deserialize<WarningResponse>();
                    if (_warningReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_warningReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (warningResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "WarningResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    InvokeParallel(_warningReceived, warningResponse);
                    break;
                case SpeakType.ConfigureSuccess:
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
                    InvokeParallel(_configureSuccessReceived, configureSuccessResponse);
                    break;
                case SpeakType.ConfigureFailure:
                    // Non-fatal - like Warning, NOT the error path. The connection stays open.
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
                    InvokeParallel(_configureFailureReceived, configureFailureResponse);
                    break;
                case SpeakType.Error:
                    // Flux fatal errors use the wire type "Error" but carry a different shape
                    // (code/description) than the common error message, so they are handled here.
                    var errorResponse = data.Deserialize<ErrorResponse>();
                    if (_speakErrorReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_speakErrorReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (errorResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "ErrorResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    InvokeParallel(_speakErrorReceived, errorResponse);
                    break;
                default:
                    Log.Debug("ProcessTextMessage", "Unknown message type. Invoking UnhandledResponse...");
                    InvokeUnhandled(response);
                    break;
            }

            Log.Debug("ProcessTextMessage", "Succeeded");
            Log.Verbose("FluxSpeakWSClient.ProcessTextMessage", "LEAVE");
        }
        catch (JsonException ex)
        {
            Log.Error("ProcessTextMessage", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessTextMessage", $"Exception: {ex}");
            Log.Verbose("FluxSpeakWSClient.ProcessTextMessage", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessTextMessage", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessTextMessage", $"Exception: {ex}");
            Log.Verbose("FluxSpeakWSClient.ProcessTextMessage", "LEAVE");
        }
    }

    #region Helpers
    private void InvokeUnhandled(string response)
    {
        if (_speakUnhandledReceived == null)
        {
            Log.Debug("InvokeUnhandled", "_speakUnhandledReceived has no listeners");
            return;
        }

        var unhandledResponse = new UnhandledResponse();
        unhandledResponse.Type = Common.WebSocketType.Unhandled;
        unhandledResponse.Raw = response;

        InvokeParallel(_speakUnhandledReceived, unhandledResponse);
    }

    /// <summary>
    /// Get the URI for the WebSocket connection
    /// </summary>
    internal static Uri GetUri(IDeepgramClientOptions options, SpeakSchema parameter, Dictionary<string, string>? addons = null)
    {
        var baseAddress = options.BaseAddress;

        // If the base address carries an API version (the SDK default appends /v1), remove it.
        // Flux TTS lives on /v2/speak; the version is part of UriSegments.SPEAK_V2.
        Regex regex = new Regex(@"\b(\/v[0-9]+)\b", RegexOptions.IgnoreCase);
        if (regex.IsMatch(baseAddress))
        {
            Log.Information("GetUri", $"BaseAddress contains API version: {baseAddress}");
            baseAddress = regex.Replace(baseAddress, "");
            Log.Debug("GetUri", $"BaseAddress: {baseAddress}");
        }
        baseAddress = baseAddress.TrimEnd('/');

        var queryString = BuildQueryString(parameter, addons);

        return new Uri($"{baseAddress}/{UriSegments.SPEAK_V2}?{queryString}");
    }

    /// <summary>
    /// Builds the query string for the Flux TTS connection. Numbers are formatted with the
    /// invariant culture so a sample rate like 44100 is never sent with a decimal separator, and
    /// list values are repeated (tag=a&amp;tag=b).
    /// </summary>
    internal static string BuildQueryString(SpeakSchema parameter, Dictionary<string, string>? addons = null)
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
