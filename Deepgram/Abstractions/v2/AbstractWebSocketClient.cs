// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT


using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Exceptions.v1;
using Deepgram.Models.Common.v2.WebSocket;

namespace Deepgram.Abstractions.v2;

/// <summary>
/// Implements version 1 of the Live Client.
/// </summary>
public abstract class AbstractWebSocketClient : IDisposable
{
    #region Fields
    protected readonly IDeepgramClientOptions _deepgramClientOptions;

    protected ClientWebSocket? _clientWebSocket;
    protected CancellationTokenSource? _cancellationTokenSource;

    // A per-connection correlation id. All log entries emitted for a single WebSocket
    // connection carry it (via a logging scope), so they can be grouped and later tied
    // back to the server's request_id.
    protected string? _connectionId;
    private IDisposable? _connectionScope;
    private readonly object _connectionLifecycleLock = new();

    protected readonly SemaphoreSlim _mutexSubscribe = new SemaphoreSlim(1, 1);
    protected readonly SemaphoreSlim _mutexSend = new SemaphoreSlim(1, 1);
    #endregion

    /// <param name="apiKey">Required DeepgramApiKey</param>
    /// <param name="deepgramClientOptions"><see cref="IDeepgramClientOptions"/> for HttpClient Configuration</param>
    public AbstractWebSocketClient(string? apiKey = null, IDeepgramClientOptions? options = null)
    {
        Log.Verbose("AbstractWebSocketClient", "ENTER");

        options ??= new DeepgramWsClientOptions(apiKey);
        _deepgramClientOptions = options;

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.Debug("AbstractWebSocketClient", $"APIVersion: {options.APIVersion}");
            Log.Debug("AbstractWebSocketClient", $"BaseAddress: {options.BaseAddress}");
            Log.Debug("AbstractWebSocketClient", $"OnPrem: {options.OnPrem}");
        }
        Log.Verbose("AbstractWebSocketClient", "LEAVE");
    }

    #region Event Handlers
    /// <summary>
    /// Fires when an event is received from the Deepgram API
    /// </summary>
    protected event EventHandler<OpenResponse>? _openReceived;
    protected event EventHandler<CloseResponse>? _closeReceived;
    protected event EventHandler<UnhandledResponse>? _unhandledReceived;
    protected event EventHandler<ErrorResponse>? _errorReceived;
    #endregion

    /// <summary>
    /// Connect to a Deepgram API Web Socket to begin transcribing audio
    /// </summary>
    /// <param name="options">Options to use when transcribing audio</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task<bool> Connect(string uri, CancellationTokenSource? cancelToken = null, Dictionary<string, string>? headers = null)
    {
        Log.Verbose("AbstractWebSocketClient.Connect", "ENTER");
        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.Debug("Connect", $"headers: {headers}");
        }

        // check if the client is disposed
        lock (_connectionLifecycleLock)
        {
            if (_clientWebSocket != null)
            {
                // client has already connected
                var exStr = "Client has already been initialized";
                Log.Error("Connect", exStr);
                Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

                return true;
            }
        }

        using var defaultCancelToken = cancelToken == null
            ? new CancellationTokenSource(Constants.DefaultConnectTimeout)
            : null;
        if (defaultCancelToken != null)
        {
            Log.Information("Connect", "Using default connect cancellation token");
            cancelToken = defaultCancelToken;
        }

        var socket = new ClientWebSocket();
        var internalCancellationTokenSource = new CancellationTokenSource();
        var connectionId = Guid.NewGuid().ToString("N");
        IDisposable? connectionScope = null;
        var callbackFailed = false;

        try
        {
            // Configure a local socket first. Header failures must not publish a half-built
            // connection that makes a later retry report success without opening a socket.
            DeepgramWebSocketException.EnableHttpResponseDetails(socket);
            SetAuthenticationHeader(socket, _deepgramClientOptions);
            if (_deepgramClientOptions.Headers is not null)
            {
                foreach (var header in _deepgramClientOptions.Headers)
                {
                    Log.Debug("Connect", $"Add Header {header.Key}");
                    socket.Options.SetRequestHeader(header.Key, header.Value);
                }
            }
            if (headers is not null)
            {
                foreach (var header in headers)
                {
                    Log.Debug("Connect", $"Add Header {header.Key}");
                    socket.Options.SetRequestHeader(header.Key, header.Value);
                }
            }

            if (_deepgramClientOptions is DeepgramWsClientOptions { ConfigureClientWebSocket: { } configureClientWebSocket })
            {
                try
                {
                    configureClientWebSocket(socket.Options);
                }
                catch (Exception ex)
                {
                    callbackFailed = true;
                    Log.Error("Connect", $"ConfigureClientWebSocket threw {ex.GetType()}");
                    socket.Dispose();
                    throw;
                }
            }

            connectionScope = Log.BeginScope("AbstractWebSocketClient",
                new Dictionary<string, object> { ["dg.connection_id"] = connectionId });
            lock (_connectionLifecycleLock)
            {
                if (_clientWebSocket != null)
                {
                    connectionScope.Dispose();
                    DisposeCancellationTokenSource(internalCancellationTokenSource);
                    socket.Dispose();
                    return true;
                }

                _clientWebSocket = socket;
                _cancellationTokenSource = internalCancellationTokenSource;
                _connectionId = connectionId;
                _connectionScope = connectionScope;
            }

            var myUri = new Uri(uri);

            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.Debug("Connect", $"uri: {uri}");

                Log.Debug("Connect", "Connecting to Deepgram API...");
            }
            await socket.ConnectAsync(myUri, cancelToken.Token).ConfigureAwait(false);

            if (socket.State != WebSocketState.Open)
            {
                Log.Error("Connect", "Failed to connect to Deepgram API");
                Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

                ResetFailedConnection(socket, internalCancellationTokenSource, connectionScope);
                return false;
            }

            Log.Debug("Connect", "Starting Sender Thread...");
            StartSenderBackgroundThread();

            Log.Debug("Connect", "Starting Receiver Thread...");
            StartReceiverBackgroundThread();

            // send an OpenResponse event
            if (_openReceived != null)
            {
                Log.Debug("Connect", "Sending OpenResponse event...");
                var data = new OpenResponse();
                data.Type = WebSocketType.Open;
                _openReceived.Invoke(null, data);
            }

            Log.Debug("Connect", "Connect Succeeded");
            Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

            return true;
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("Connect", "Connect cancelled.");
            Log.Verbose("Connect", $"Connect cancelled. Info: {ex}");
            Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

            ResetFailedConnection(socket, internalCancellationTokenSource, connectionScope);
            return false;
        }
        catch (WebSocketException ex) when (_deepgramClientOptions is DeepgramWsClientOptions { ThrowDeepgramWebSocketExceptions: true })
        {
            Log.Error("Connect", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Connect", $"Exception: {ex}");
            Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

            var httpStatusCode = DeepgramWebSocketException.GetHttpStatusCode(socket);
            ResetFailedConnection(socket, internalCancellationTokenSource, connectionScope);
            throw new DeepgramWebSocketException(
                "Failed to connect to Deepgram API",
                httpStatusCode,
                ex);
        }
        catch (Exception ex)
        {
            Log.Error("Connect", callbackFailed
                ? $"ConfigureClientWebSocket threw {ex.GetType()}"
                : $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Connect", $"Exception: {ex}");
            Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

            ResetFailedConnection(socket, internalCancellationTokenSource, connectionScope);
            throw;
        }

        void StartSenderBackgroundThread() => Task.Run(() => ProcessSendQueue());

        void StartReceiverBackgroundThread() => Task.Run(() => ProcessReceiveQueue());
    }

    /// <summary>
    /// Returns the client to its pre-connect state after a failed <see cref="Connect"/>, so the
    /// caller can retry. Without this, the half-initialized socket stays assigned and the next
    /// Connect() call returns true without opening a connection.
    /// </summary>
    private void ResetFailedConnection(ClientWebSocket socket, CancellationTokenSource internalCancellationTokenSource,
        IDisposable? connectionScope)
        => ReleaseConnection(socket, internalCancellationTokenSource, connectionScope);

    /// <summary>
    /// Releases resources for one connection attempt only. A newer connection may have been
    /// published while a stale Connect or Stop was awaiting, so shared fields are changed only
    /// when they still reference this attempt.
    /// </summary>
    private void ReleaseConnection(ClientWebSocket socket, CancellationTokenSource? internalCancellationTokenSource,
        IDisposable? connectionScope)
    {
        IDisposable? scopeToDispose = null;
        lock (_connectionLifecycleLock)
        {
            // A retry cannot publish between clearing this attempt's socket and scope. If this
            // attempt was already released, its fields no longer match and a newer connection is
            // left untouched.
            if (ReferenceEquals(_clientWebSocket, socket))
            {
                _clientWebSocket = null;
                if (ReferenceEquals(_cancellationTokenSource, internalCancellationTokenSource))
                {
                    _cancellationTokenSource = null;
                }
                if (ReferenceEquals(_connectionScope, connectionScope))
                {
                    _connectionScope = null;
                    scopeToDispose = connectionScope;
                }
                _connectionId = null;
            }
        }

        scopeToDispose?.Dispose();
        if (internalCancellationTokenSource != null)
        {
            DisposeCancellationTokenSource(internalCancellationTokenSource);
        }
        socket.Dispose();
    }

    /// <summary>
    /// Closes the per-connection logging scope opened in <see cref="Connect"/>, if any.
    /// atomic-claim teardown; safe against a concurrent Stop()/Dispose() racing on the field.
    /// Interlocked.Exchange lets exactly one caller claim the instance; the loser gets null. See #390.
    /// </summary>
    private void CloseConnectionScope()
    {
        var scope = Interlocked.Exchange(ref _connectionScope, null);
        _connectionId = null;
        scope?.Dispose();
    }

    #region Subscribe Event
    /// <summary>
    /// Subscribe to an Open event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<OpenResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _openReceived += (sender, e) => eventHandler(sender, e);
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
        await _mutexSubscribe.WaitAsync();
        try
        {
            _closeReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
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
            _unhandledReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to an Error event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<ErrorResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _errorReceived += (sender, e) => eventHandler(sender, e);
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
    /// Sends a Close message to Deepgram
    /// </summary>
    public virtual Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null)
    {
        throw new DeepgramException("Unimplemented");
    }

    /// <summary>
    /// Waits until every message currently queued via <see cref="SendBinary"/> /
    /// <see cref="SendMessage"/> has been written to the socket. Use this to guarantee buffered
    /// audio has been flushed before sending a control message (for example Finalize or Close).
    /// Returns immediately if the client is not connected.
    /// </summary>
    public async Task Flush()
    {
        var cts = _cancellationTokenSource;
        if (!IsConnected() || cts == null)
        {
            Log.Debug("Flush", "WebSocket is not connected. Nothing to flush.");
            return;
        }

        var flushSignal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        EnqueueSendMessage(WebSocketMessage.CreateFlushMarker(flushSignal));

        // Unblock if the connection is torn down before the marker is reached. Registering on an
        // already-cancelled token runs the callback immediately; if the token was disposed by a
        // concurrent teardown between the guard above and here, treat it as "connection gone".
        try
        {
            using (cts.Token.Register(static state => ((TaskCompletionSource<bool>)state!).TrySetResult(false), flushSignal))
            {
                await flushSignal.Task.ConfigureAwait(false);
            }
        }
        catch (ObjectDisposedException)
        {
            Log.Debug("Flush", "Connection was torn down while flushing. Nothing to flush.");
        }
    }

    /// <summary>
    /// This method sends a binary message over the WebSocket connection.
    /// </summary>
    /// <param name="data"></param>
    public virtual void SendBinary(byte[] data, int length = Constants.UseArrayLengthForSend) =>
        EnqueueSendMessage(new WebSocketMessage(data, WebSocketMessageType.Binary, length));

    /// <summary>
    /// This method sends a text message over the WebSocket connection.
    /// </summary>
    public virtual void SendMessage(byte[] data, int length = Constants.UseArrayLengthForSend) =>
        EnqueueSendMessage(new WebSocketMessage(data, WebSocketMessageType.Text, length));

    /// <summary>
    /// This method sends a binary message over the WebSocket connection immediately without queueing.
    /// </summary>, 
    public virtual async Task SendBinaryImmediately(byte[] data, int length = Constants.UseArrayLengthForSend, CancellationTokenSource? _cancellationToken = null)
    {
        // Snapshot the socket so a concurrent Stop()/Dispose() nulling the field can't NRE the
        // SendAsync below (this runs on the Stop -> SendClose path). See #390.
        var socket = _clientWebSocket;
        if (socket == null || !IsConnected())
        {
            Log.Debug("SendBinaryImmediately", "WebSocket is not connected. Exiting...");
            return;
        }

        // provide a cancellation token, or use the teardown-safe one in the class
        var token = _cancellationToken?.Token ?? GetInternalCancellationToken();

        await _mutexSend.WaitAsync(token);
        try
        {
            Log.Verbose("SendBinaryImmediately", "Sending binary message immediately...");
            if (length == Constants.UseArrayLengthForSend)
            {
                length = data.Length;
            }
            await socket.SendAsync(new ArraySegment<byte>(data, 0, length), WebSocketMessageType.Binary, true, token)
                .ConfigureAwait(false);
        }
        finally
        {
            _mutexSend.Release();
        }
    }

    /// <summary>
    /// This method sends a text message over the WebSocket connection immediately without queueing.
    /// </summary>
    public virtual async Task SendMessageImmediately(byte[] data, int length = Constants.UseArrayLengthForSend, CancellationTokenSource? _cancellationToken = null)
    {
        // Snapshot the socket so a concurrent Stop()/Dispose() nulling the field can't NRE the
        // SendAsync below (this runs on the Stop -> SendClose path). See #390.
        var socket = _clientWebSocket;
        if (socket == null || !IsConnected())
        {
            Log.Debug("SendMessageImmediately", "WebSocket is not connected. Exiting...");
            return;
        }

        // provide a cancellation token, or use the teardown-safe one in the class
        var token = _cancellationToken?.Token ?? GetInternalCancellationToken();

        await _mutexSend.WaitAsync(token);
        try
        {
            Log.Verbose("SendMessageImmediately", "Sending text message immediately...");
            if (length == Constants.UseArrayLengthForSend)
            {
                length = data.Length;
            }
            await socket.SendAsync(new ArraySegment<byte>(data, 0, length), WebSocketMessageType.Text, true, token)
                .ConfigureAwait(false);
        }
        finally
        {
            _mutexSend.Release();
        }
    }
    #endregion

    internal void EnqueueSendMessage(WebSocketMessage message)
    {
        try
        {
            _sendChannel.Writer.TryWrite(message);
        }
        catch (Exception ex)
        {
            Log.Error("EnqueueSendMessage", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("EnqueueSendMessage", $"Exception: {ex}");
        }
    }

    internal async Task ProcessSendQueue()
    {
        Log.Verbose("AbstractWebSocketClient.ProcessSendQueue", "ENTER");

        if (_clientWebSocket == null)
        {
            var exStr = "Attempting to start a sender queue when the WebSocket has been disposed is not allowed.";
            Log.Error("EnqueueSendMessage", exStr);
            Log.Verbose("ProcessSendQueue", "LEAVE");

            throw new InvalidOperationException(exStr);
        }

        try
        {
            // snapshot the token to avoid a teardown race nulling the field mid-loop (#390);
            // cancel-before-dispose guarantees a torn-down source is observed as cancelled here.
            var token = GetInternalCancellationToken();
            while (await _sendChannel.Reader.WaitToReadAsync(token))
            {
                token = GetInternalCancellationToken();
                if (token.IsCancellationRequested)
                {
                    Log.Information("ProcessSendQueue", "ProcessSendQueue cancelled");
                    break;
                }
                var socket = _clientWebSocket;
                if (socket == null || !IsConnected())
                {
                    Log.Debug("ProcessSendQueue", "WebSocket is not connected. Exiting...");
                    break;
                }

                Log.Verbose("ProcessSendQueue", "Reading message off queue...");
                while (_sendChannel.Reader.TryRead(out var message))
                {
                    // A flush marker carries no payload; reaching it in queue order means every
                    // message enqueued before it has been sent, so just signal and move on.
                    if (message.FlushSignal is not null)
                    {
                        Log.Verbose("ProcessSendQueue", "Reached flush marker; signaling flush.");
                        message.FlushSignal.TrySetResult(true);
                        continue;
                    }

                    // TODO: Add logging for message capturing for possible playback
                    Log.Verbose("ProcessSendQueue", "Sending message...");
                    await _mutexSend.WaitAsync(token);
                    try
                    {
                        await socket.SendAsync(message.Message, message.MessageType, true, token)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        _mutexSend.Release();
                    }
                }
            }

            Log.Verbose("ProcessSendQueue", "Exit");
            Log.Verbose("AbstractWebSocketClient.ProcessSendQueue", "LEAVE");
        }
        catch (OperationCanceledException ex)
        {
            Log.Debug("ProcessSendQueue", "SendThread cancelled.");
            Log.Verbose("ProcessSendQueue", $"SendThread cancelled. Info: {ex}");
            Log.Verbose("AbstractWebSocketClient.ProcessSendQueue", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessSendQueue", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessSendQueue", $"Exception: {ex}");
            Log.Verbose("AbstractWebSocketClient.ProcessSendQueue", "LEAVE");
        }
        finally
        {
            // The sender thread is stopping; unblock any callers awaiting a flush marker that
            // will now never be reached, so Flush() cannot hang after a disconnect/teardown.
            while (_sendChannel.Reader.TryRead(out var pending))
            {
                pending.FlushSignal?.TrySetResult(false);
            }
        }
    }

    internal async Task ProcessReceiveQueue()
    {
        Log.Verbose("AbstractWebSocketClient.ProcessReceiveQueue", "ENTER");

        while (_clientWebSocket?.State == WebSocketState.Open)
        {
            try
            {
                // snapshot token and socket to avoid a teardown race nulling the fields mid-loop (#390)
                var token = GetInternalCancellationToken();
                var socket = _clientWebSocket;

                if (token.IsCancellationRequested)
                {
                    Log.Information("ProcessReceiveQueue", "ReceiveThread cancelled");
                    await Stop();
                    Log.Verbose("ProcessReceiveQueue", "LEAVE");
                    return;
                }
                if (socket == null || !IsConnected())
                {
                    Log.Debug("ProcessReceiveQueue", "WebSocket is not connected. Exiting...");
                    return;
                }

                var buffer = new ArraySegment<byte>(new byte[Constants.BufferSize]);
                WebSocketReceiveResult result;

                using (var ms = new MemoryStream())
                {
                    do
                    {
                        // get the result of the receive operation
                        result = await socket.ReceiveAsync(buffer, token);

                        ms.Write(
                            buffer.Array ?? throw new InvalidOperationException("buffer cannot be null"),
                            buffer.Offset,
                            result.Count
                            );
                    } while (!result.EndOfMessage);

                    if (result.MessageType != WebSocketMessageType.Close)
                    {
                        if (Log.IsEnabled(LogLevel.Verbose))
                        {
                            Log.Verbose("ProcessReceiveQueue", $"Received message: {result} / {ms}");
                        }
                        ProcessDataReceived(result, ms);
                    }
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Log.Information("ProcessReceiveQueue", "Received WebSocket Close. Trigger cancel...");
                    await Stop();
                    Log.Verbose("ProcessReceiveQueue", "LEAVE");
                    return;
                }
            }
            catch (TaskCanceledException ex)
            {
                Log.Debug("ProcessReceiveQueue", "ReceiveThread cancelled.");
                Log.Verbose("ProcessReceiveQueue", $"ReceiveThread cancelled. Info: {ex}");
                Log.Verbose("AbstractWebSocketClient.ProcessReceiveQueue", "LEAVE");
            }
            catch (Exception ex)
            {
                Log.Error("ProcessReceiveQueue", $"{ex.GetType()} thrown {ex.Message}");
                Log.Verbose("ProcessReceiveQueue", $"Exception: {ex}");
                Log.Verbose("AbstractWebSocketClient.ProcessReceiveQueue", "LEAVE");
            }
        }
    }

    internal virtual void ProcessDataReceived(WebSocketReceiveResult result, MemoryStream ms)
    {
        Log.Verbose("AbstractWebSocketClient.ProcessDataReceived", "ENTER");

        ms.Seek(0, SeekOrigin.Begin);

        if (result.MessageType == WebSocketMessageType.Binary)
        {
            ProcessBinaryMessage(result, ms);
        }
        else
        {
            ProcessTextMessage(result, ms);
        }
    }

    internal virtual void ProcessBinaryMessage(WebSocketReceiveResult result, MemoryStream ms)
    {
        throw new DeepgramException("Unimplemented");
    }

    internal virtual void ProcessTextMessage(WebSocketReceiveResult result, MemoryStream ms)
    {
        Log.Verbose("AbstractWebSocketClient.ProcessTextMessage", "ENTER");

        ms.Seek(0, SeekOrigin.Begin);

        var response = Encoding.UTF8.GetString(ms.ToArray());
        if (response == null)
        {
            Log.Warning("ProcessTextMessage", "Response is null");
            Log.Verbose("AbstractWebSocketClient.ProcessTextMessage", "LEAVE");
            return;
        }

        try
        {
            if (Log.IsEnabled(LogLevel.Verbose))
            {
                Log.Verbose("ProcessTextMessage", $"raw response: {response}");
            }
            var data = JsonDocument.Parse(response);
            // Frames without a string "type" fall through to Unhandled instead of throwing.
            var typeString = data.RootElement.ValueKind == JsonValueKind.Object
                && data.RootElement.TryGetProperty("type", out var type)
                && type.ValueKind == JsonValueKind.String
                ? type.GetString()
                : null;
            // Use TryParse so message types unknown to this SDK version are surfaced as
            // Unhandled instead of throwing. This keeps the SDK forward-compatible with new
            // server message types. See #395.
            if (!Enum.TryParse<WebSocketType>(typeString, out var val))
            {
                Log.Debug("ProcessTextMessage", $"Unknown message type '{typeString}'. Treating as Unhandled.");
                val = WebSocketType.Unhandled;
            }

            if (Log.IsEnabled(LogLevel.Verbose))
            {
                Log.Verbose("ProcessTextMessage", $"Type: {val}");
            }

            switch (val)
            {
                case WebSocketType.Open:
                    var openResponse = data.Deserialize<OpenResponse>();
                    if (_openReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_openReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (openResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "OpenResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.Debug("ProcessTextMessage", $"Invoking OpenResponse. event: {openResponse}");
                    }
                    InvokeParallel(_openReceived, openResponse);
                    break;
                case WebSocketType.Error:
                    var errorResponse = data.Deserialize<ErrorResponse>();
                    if (_errorReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_errorReceived has no listeners");
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
                    InvokeParallel(_errorReceived, errorResponse);
                    break;
                default:
                    if (_unhandledReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_unhandledReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    var unhandledResponse = new UnhandledResponse();
                    unhandledResponse.Type = WebSocketType.Unhandled;
                    unhandledResponse.Raw = response;

                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.Debug("ProcessTextMessage", $"Invoking UnhandledResponse. event: {unhandledResponse}");
                    }
                    InvokeParallel(_unhandledReceived, unhandledResponse);
                    break;
            }

            Log.Debug("ProcessTextMessage", "Succeeded");
            Log.Verbose("AbstractWebSocketClient.ProcessTextMessage", "LEAVE");
        }
        catch (JsonException ex)
        {
            Log.Error("ProcessTextMessage", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessTextMessage", $"Exception: {ex}");
            Log.Verbose("AbstractWebSocketClient.ProcessTextMessage", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessTextMessage", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessTextMessage", $"Exception: {ex}");
            Log.Verbose("AbstractWebSocketClient.ProcessTextMessage", "LEAVE");
        }
    }

    /// <summary>
    /// Closes the Web Socket connection to the Deepgram API
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task<bool> Stop(CancellationTokenSource? cancelToken = null, bool nullByte = false)
    {
        Log.Verbose("AbstractWebSocketClient.Stop", "ENTER");

        ClientWebSocket? socket;
        CancellationTokenSource? internalCancellationTokenSource;
        IDisposable? connectionScope;
        lock (_connectionLifecycleLock)
        {
            socket = _clientWebSocket;
            internalCancellationTokenSource = _cancellationTokenSource;
            connectionScope = _connectionScope;
        }
        if (socket == null)
        {
            Log.Information("Stop", "Client has already been disposed");
            Log.Verbose("AbstractWebSocketClient.Stop", "LEAVE");
            return true;
        }

        if (cancelToken == null)
        {
            Log.Information("Stop", "Using default disconnect cancellation token");
            cancelToken = new CancellationTokenSource(Constants.DefaultDisconnectTimeout);
        }

        try
        {
            // if websocket is open, send a close message
            if (socket.State == WebSocketState.Open)
            {
                Log.Debug("Stop", "Sending Close message...");
                await SendClose(nullByte, cancelToken);
            }

            // small delay to wait for any final transcription
            await Task.Delay(100, cancelToken.Token).ConfigureAwait(false);

            // send a CloseResponse event
            if (_closeReceived != null)
            {
                Log.Debug("Stop", "Sending CloseResponse event...");
                var data = new CloseResponse();
                data.Type = WebSocketType.Close;
                InvokeParallel(_closeReceived, data);
            }

            // attempt to stop the connection — serialize via the send mutex and re-check state so a
            // concurrent Stop (user Stop racing the server-close-triggered Stop) can't issue two
            // overlapping CloseOutputAsync calls on the same socket (ClientWebSocket allows only one
            // outstanding send). See #390.
            await _mutexSend.WaitAsync(cancelToken.Token).ConfigureAwait(false);
            try
            {
                // CloseOutputAsync is only valid from Open/CloseReceived; a positive check avoids an
                // InvalidOperationException if Stop() runs while Connect() is still handshaking
                // (socket in Connecting/None). See #390 (R1).
                if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
                {
                    Log.Debug("Stop", "Closing WebSocket connection...");
                    await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancelToken.Token)
                        .ConfigureAwait(false);
                }
            }
            finally
            {
                _mutexSend.Release();
            }

            ReleaseConnection(socket, internalCancellationTokenSource, connectionScope);

            Log.Debug("Stop", "Succeeded");
            Log.Verbose("AbstractWebSocketClient.Stop", "LEAVE");

            return true;
        }
        catch (OperationCanceledException ex)
        {
            // Covers both Task.Delay's TaskCanceledException and CloseOutputAsync's bare
            // OperationCanceledException if the disconnect-timeout token fires during teardown.
            Log.Debug("Stop", "Stop cancelled.");
            Log.Verbose("Stop", $"Stop cancelled. Info: {ex}");
            Log.Verbose("AbstractWebSocketClient.Stop", "LEAVE");

            ReleaseConnection(socket, internalCancellationTokenSource, connectionScope);
            return true;
        }
        catch (Exception ex) when (ex is ObjectDisposedException || ex is WebSocketException)
        {
            // A concurrent Stop()/Dispose() already tore down the socket (disposed stream, or the
            // remote closed without a handshake). The connection is gone, which is the goal of
            // Stop, so treat it as an already-completed stop rather than a teardown-race error.
            // Mirrors the Flux client's own Stop() catch set. See #390.
            Log.Debug("Stop", "Stop raced with another teardown; already stopped.");
            Log.Verbose("Stop", $"{ex.GetType().Name}: {ex}");
            Log.Verbose("AbstractWebSocketClient.Stop", "LEAVE");

            ReleaseConnection(socket, internalCancellationTokenSource, connectionScope);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error("Stop", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Stop", $"Exception: {ex}");
            Log.Verbose("AbstractWebSocketClient.Stop", "LEAVE");
            throw;
        }
    }

    #region Helpers
    /// <summary>
    /// Returns the current internal cancellation token in a way that is safe against
    /// teardown races. Stop()/Dispose() null and dispose <see cref="_cancellationTokenSource"/>,
    /// which previously caused NullReference/ObjectDisposedException in long-running background
    /// loops (keepalive/autoflush). If the source has been torn down, an already-cancelled token
    /// is returned so callers exit cleanly. See #390.
    /// </summary>
    protected CancellationToken GetInternalCancellationToken()
    {
        // Snapshot the reference to avoid it being nulled between the check and the read.
        var cts = _cancellationTokenSource;
        if (cts == null)
        {
            return new CancellationToken(true);
        }

        try
        {
            return cts.Token;
        }
        catch (ObjectDisposedException)
        {
            return new CancellationToken(true);
        }
    }

    /// <summary>
    /// Cancels and disposes the internal cancellation token source exactly once, safely against
    /// concurrent Stop()/Dispose() calls. A plain "if (field != null) { field.Cancel(); ... }" is a
    /// check-then-act race: a second caller can null the field between the re-reads, throwing
    /// NullReferenceException, or dispose it first, throwing ObjectDisposedException from .Token.
    /// Interlocked.Exchange lets exactly one caller claim the instance; the loser gets null. See #390.
    /// </summary>
    protected void DisposeCancellationTokenSource()
    {
        var cts = Interlocked.Exchange(ref _cancellationTokenSource, null);
        if (cts == null)
        {
            return;
        }

        DisposeCancellationTokenSource(cts);
    }

    private static void DisposeCancellationTokenSource(CancellationTokenSource cts)
    {
        try
        {
            if (!cts.IsCancellationRequested)
            {
                cts.Cancel();
            }
        }
        catch (ObjectDisposedException)
        {
            // already disposed by a concurrent teardown; nothing to cancel
        }
        catch (AggregateException)
        {
            // a cancellation callback threw; the token is still cancelled, so dispose anyway
        }
        finally
        {
            cts.Dispose();
        }
    }

    /// <summary>
    /// Retrieves the connection state of the WebSocket
    /// </summary>
    /// <returns>Returns the connection state of the WebSocket</returns>
    public WebSocketState State()
    {
        if (_clientWebSocket == null)
        {
            return WebSocketState.None;
        }
        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.Debug("State", $"WebSocket State: {_clientWebSocket.State}");
        }
        return _clientWebSocket.State;
    }

    /// <summary>
    /// Indicates whether the WebSocket is connected
    /// </summary> 
    /// <returns>Returns true if the WebSocket is connected</returns>
    public bool IsConnected()
    {
        if (_clientWebSocket == null)
        {
            return false;
        }

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.Debug("State", $"WebSocket State: {_clientWebSocket.State}");
        }
        return _clientWebSocket.State == WebSocketState.Open;
    }

    /// <summary>
    /// Handle channel options
    /// </summary> 
    // SingleWriter is false: audio can be enqueued (SendBinary) concurrently with a flush marker
    // (Flush/SendFinalize) from a different thread. A single background reader drains the queue.
    internal readonly Channel<WebSocketMessage> _sendChannel = System.Threading.Channels.Channel
       .CreateUnbounded<WebSocketMessage>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false, });

    internal void InvokeParallel<T>(EventHandler<T>? eventHandler, T e)
    {
        if (eventHandler != null)
        {
            try
            {
                Parallel.ForEach(
                    eventHandler.GetInvocationList().Cast<EventHandler<T>>(),
                    (handler) =>
                        handler(null, e));
            }
            catch (AggregateException ae)
            {
                Log.Error("InvokeParallel", $"AggregateException occurred in one or more event handlers: {ae}");
            }
            catch (Exception ex)
            {
                Log.Error("InvokeParallel", $"Exception occurred in event handler: {ex}");
            }
        }
    }

    /// <summary>
    /// Sets the appropriate authentication header for WebSocket connections.
    /// Priority: AccessToken (Bearer) > ApiKey (Token)
    /// </summary>
    /// <param name="webSocket">ClientWebSocket to configure</param>
    /// <param name="options">Client options containing credentials</param>
    private void SetAuthenticationHeader(ClientWebSocket webSocket, IDeepgramClientOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.AccessToken))
        {
            // Use Bearer token authentication (highest priority)
            webSocket.Options.SetRequestHeader("Authorization", $"Bearer {options.AccessToken}");
        }
        else if (!string.IsNullOrWhiteSpace(options.ApiKey))
        {
            // Use API key authentication (fallback)
            webSocket.Options.SetRequestHeader("Authorization", $"token {options.ApiKey}");
        }
        else
        {
            // No authentication credentials available
            Log.Warning("SetAuthenticationHeader", "No authentication credentials provided. WebSocket connection may fail.");
        }
    }
    #endregion

    #region Dispose
    /// <summary>
    /// Disposes of the resources used by the client
    /// </summary> 
    public void Dispose()
    {
        if (_clientWebSocket == null)
        {
            return;
        }

        // atomic-claim teardown; safe against a concurrent Stop()/Dispose(). See #390.
        DisposeCancellationTokenSource();

        if (_sendChannel != null)
        {
            _sendChannel.Writer.TryComplete(); // TryComplete: idempotent under concurrent Dispose() (#390 R3)
        }

        if (_clientWebSocket != null)
        {
            _clientWebSocket.Dispose();
            _clientWebSocket = null;
        }

        CloseConnectionScope();

        GC.SuppressFinalize(this);
    }
    #endregion
}
