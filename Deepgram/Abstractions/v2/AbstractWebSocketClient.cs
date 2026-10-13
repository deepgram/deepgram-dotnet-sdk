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

    // Locking model (#455). _lifecycleLock guards every change to which connection the client
    // owns: _connection, the three fields above, _senderTask and _disposed are only written
    // while holding it, except by the legacy DisposeCancellationTokenSource(), which nothing in
    // the SDK calls and which is kept for derived classes. It is a plain lock, held for a few field reads and writes only: never
    // across an await, and never while user code (event handlers, cancellation callbacks) runs.
    // Hot paths (sends, IsConnected(), State()) read the fields without it; each reads a field
    // once and then works on that value or on the connection's own ConnectionState.
    private readonly object _lifecycleLock = new object();

    // The connection the fields above belong to (null when none is published), the most recent
    // connection's sender task, and whether Dispose() has run. See ConnectionState.
    private ConnectionState? _connection;
    private Task? _senderTask;
    private bool _disposed;

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
        var (connected, _) = await ConnectCore(uri, cancelToken, headers).ConfigureAwait(false);
        return connected;
    }

    /// <summary>
    /// Does the work of <see cref="Connect"/> and also returns the connection this call opened.
    /// <c>Owned</c> is null when the call opened nothing: on failure (<c>Connected</c> false) and
    /// when another connection was already published (<c>Connected</c> true, the "already
    /// initialized" result). Subclasses use <c>Owned</c> for their post-connect work, never
    /// <see cref="CurrentConnection"/>, so a concurrent Connect() cannot act on, or release,
    /// a connection it did not open.
    /// </summary>
    private protected async Task<(bool Connected, ConnectionState? Owned)> ConnectCore(string uri, CancellationTokenSource? cancelToken, Dictionary<string, string>? headers)
    {
        Log.Verbose("AbstractWebSocketClient.Connect", "ENTER");
        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.Debug("Connect", $"headers: {headers}");
        }

        // check if the client is already connected
        if (CurrentConnection != null)
        {
            // client has already connected
            var exStr = "Client has already been initialized";
            Log.Error("Connect", exStr);
            Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

            return (true, null);
        }

        // The default connect-timeout source is owned by this call: the using declaration
        // disposes it on every exit path, including when the header setup below throws.
        using var defaultCancelToken = cancelToken == null
            ? new CancellationTokenSource(Constants.DefaultConnectTimeout)
            : null;
        if (defaultCancelToken != null)
        {
            Log.Information("Connect", "Using default connect cancellation token");
            cancelToken = defaultCancelToken;
        }

        // Create and configure the socket as a local, and publish it to _clientWebSocket only
        // once its request headers are set. SetRequestHeader throws ArgumentException for an
        // invalid header name or value; publishing first left that half-built socket assigned,
        // so every later Connect() hit the guard above and returned true without connecting.
        var socket = new ClientWebSocket();
        try
        {
            DeepgramWebSocketException.EnableHttpResponseDetails(socket);

            // set headers
            SetAuthenticationHeader(socket, _deepgramClientOptions);
            if (_deepgramClientOptions.Headers is not null)
            {
                foreach (var header in _deepgramClientOptions.Headers)
                {
                    // header names only: values may be credentials and are never logged

                    Log.Debug("Connect", $"Add Header {header.Key}");
                    socket.Options.SetRequestHeader(header.Key, header.Value);
                }
            }
            if (headers is not null)
            {
                foreach (var header in headers)
                {
                    // header names only: values may be credentials and are never logged

                    Log.Debug("Connect", $"Add Header {header.Key}");
                    socket.Options.SetRequestHeader(header.Key, header.Value);
                }
            }
        }
        catch (Exception ex)
        {
            // the exception message is not logged: it can echo a header value, which may be a credential
            Log.Error("Connect", $"{ex.GetType()} thrown while setting request headers");
            Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

            socket.Dispose();
            throw;
        }

        // The state this connection owns: its socket, its internal cancellation token and its
        // connection id. Its background loops and its teardown use this object, never the
        // fields, so a stale teardown cannot release a newer connection.
        var owned = new ConnectionState(socket, new CancellationTokenSource(), Guid.NewGuid().ToString("N"));

        // Claim the client for this connection and publish its state in one step, under the
        // lifecycle lock. The claim fails if the client is disposed, or if another connection is
        // already published (the existing "already initialized" result: true, nothing opened).
        bool disposed;
        bool claimed;
        lock (_lifecycleLock)
        {
            disposed = _disposed;
            claimed = !disposed && _connection == null;
            if (claimed)
            {
                _clientWebSocket = socket;
                _cancellationTokenSource = owned.TokenSource;
                _connectionId = owned.ConnectionId;
                Volatile.Write(ref _connection, owned);
            }
        }

        if (!claimed)
        {
            if (disposed)
            {
                Log.Warning("Connect", "Client has been disposed. Not connecting.");
            }
            else
            {
                Log.Error("Connect", "Client has already been initialized");
            }
            Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

            owned.TokenSource.Dispose();
            socket.Dispose();
            return (!disposed, null);
        }
        Log.Debug("Connect", "Connection claimed");

        // A connection-scoped correlation id, so every log entry for this connection can be
        // grouped together (#305). The scope is ambient (async-local): the sender and receiver
        // tasks started below inherit it for their whole lifetime. It is disposed when this
        // method returns, on this method's own flow, never from another thread's teardown.
        using var connectionScope = Log.BeginScope("AbstractWebSocketClient",
            new Dictionary<string, object> { ["dg.connection_id"] = owned.ConnectionId });

        try
        {
            var myUri = new Uri(uri);

            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.Debug("Connect", $"uri: {uri}");

                Log.Debug("Connect", "Connecting to Deepgram API...");
            }
            await socket.ConnectAsync(myUri, cancelToken!.Token).ConfigureAwait(false);

            if (owned.IsReleased)
            {
                // Stop() or Dispose() released this connection during the handshake.
                Log.Warning("Connect", "Connection was stopped while connecting");
                Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

                return (false, null);
            }

            if (socket.State != WebSocketState.Open)
            {
                Log.Error("Connect", "Failed to connect to Deepgram API");
                Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

                ResetFailedConnection(owned);
                return (false, null);
            }

            // The send channel is single-reader. A sender from an earlier connection exits once
            // its connection is released (its token is cancelled first), but it may still be
            // finishing; never start a second reader alongside it. Do not await the receive task
            // here: Connect() may be running inside a handler that the old receive loop invoked.
            Task? priorSender;
            lock (_lifecycleLock)
            {
                priorSender = _senderTask;
            }
            if (priorSender != null && !priorSender.IsCompleted)
            {
                Log.Debug("Connect", "Waiting for the previous sender to finish...");
                using var delayCts = new CancellationTokenSource();
                try
                {
                    var finished = await Task.WhenAny(priorSender, Task.Delay(Constants.DefaultDisconnectTimeout, delayCts.Token))
                        .ConfigureAwait(false);
                    if (finished != priorSender)
                    {
                        Log.Warning("Connect", "The previous connection's sender did not finish. Not connecting.");
                        Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

                        ResetFailedConnection(owned);
                        return (false, null);
                    }
                }
                finally
                {
                    delayCts.Cancel();
                }
            }

            // Stop() or Dispose() may have released this connection while it waited. Starting a
            // sender then would add a second reader to the single-reader channel for a dead
            // connection, so report a failed connect instead. The check and the start happen
            // under the lifecycle lock, as is a newer connection's read of _senderTask above:
            // either this sender is started first and the newer connection waits for it, or this
            // connection is already released and starts nothing.
            bool releasedBeforeSender;
            lock (_lifecycleLock)
            {
                releasedBeforeSender = owned.IsReleased;
                if (!releasedBeforeSender)
                {
                    _senderTask = Task.Run(() => ProcessSendQueue(owned));
                }
            }
            if (releasedBeforeSender)
            {
                Log.Warning("Connect", "Connection was stopped while connecting");
                Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

                return (false, null);
            }

            // Logged outside the lock: log providers are user code.
            Log.Debug("Connect", "Sender Thread started");

            Log.Debug("Connect", "Starting Receiver Thread...");
            _ = Task.Run(() => ProcessReceiveQueue(owned));

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

            return (true, owned);
        }
        catch (Exception ex) when (owned.IsReleased)
        {
            // Stop() or Dispose() released this connection while it was connecting, which
            // surfaces here as the released socket's exception. Report a failed connect.
            Log.Warning("Connect", "Connection was stopped while connecting");
            Log.Verbose("Connect", $"Exception: {ex}");
            Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

            return (false, null);
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("Connect", "Connect cancelled.");
            Log.Verbose("Connect", $"Connect cancelled. Info: {ex}");
            Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

            ResetFailedConnection(owned);
            return (false, null);
        }
        catch (WebSocketException ex) when (_deepgramClientOptions is DeepgramWsClientOptions { ThrowDeepgramWebSocketExceptions: true })
        {
            Log.Error("Connect", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Connect", $"Exception: {ex}");
            Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

            var httpStatusCode = DeepgramWebSocketException.GetHttpStatusCode(socket);
            ResetFailedConnection(owned);
            throw new DeepgramWebSocketException(
                "Failed to connect to Deepgram API",
                httpStatusCode,
                ex);
        }
        catch (Exception ex)
        {
            Log.Error("Connect", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Connect", $"Exception: {ex}");
            Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

            ResetFailedConnection(owned);
            throw;
        }
    }

    /// <summary>
    /// Returns the client to its pre-connect state after a failed <see cref="Connect"/>, so the
    /// caller can retry. Without this, the half-initialized socket stays assigned and the next
    /// Connect() call returns true without opening a connection.
    /// </summary>
    private void ResetFailedConnection(ConnectionState owned)
    {
        ReleaseConnection(owned);
        owned.Socket.Dispose();
    }

    /// <summary>
    /// Releases one connection's state. Idempotent: the first caller does the work. In order:
    /// <list type="number">
    /// <item>cancel the connection's token, so its background loops stop (outside the lock:
    /// cancellation runs callbacks);</item>
    /// <item>under the lifecycle lock, if this connection is still the published one, clear
    /// <c>_connection</c>, <see cref="_clientWebSocket"/>, the token source field and the
    /// connection id together, so a stale teardown leaves a newer connection alone and a new
    /// <see cref="Connect"/> can claim the client only after the old token is cancelled;</item>
    /// <item>dispose the token source.</item>
    /// </list>
    /// </summary>
    private protected void ReleaseConnection(ConnectionState owned)
    {
        if (!owned.TryBeginRelease())
        {
            return;
        }

        try
        {
            try
            {
                owned.TokenSource.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // already disposed; nothing to cancel
            }
            catch (AggregateException)
            {
                // a cancellation callback threw; the token is still cancelled
            }

            lock (_lifecycleLock)
            {
                if (ReferenceEquals(_connection, owned))
                {
                    Volatile.Write(ref _connection, null);
                    _clientWebSocket = null;
                    _cancellationTokenSource = null;
                    _connectionId = null;
                }
            }

            owned.TokenSource.Dispose();
        }
        finally
        {
            owned.CompleteRelease();
        }
    }

    /// <summary>
    /// The connection currently published by <see cref="Connect"/>, or null. Teardown entry points
    /// (Stop) snapshot it once on entry. Post-connect work uses the state returned by
    /// <see cref="ConnectCore"/> instead.
    /// </summary>
    private protected ConnectionState? CurrentConnection => Volatile.Read(ref _connection);

    /// <summary>
    /// The state one <see cref="Connect"/> call owns: its socket, its internal cancellation token
    /// source and its connection id.
    /// </summary>
    internal sealed class ConnectionState
    {
        private int _releaseStarted;
        private readonly TaskCompletionSource<bool> _released = new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal ConnectionState(ClientWebSocket socket, CancellationTokenSource tokenSource, string connectionId)
        {
            Socket = socket;
            TokenSource = tokenSource;
            Token = tokenSource.Token;
            ConnectionId = connectionId;
        }

        internal ClientWebSocket Socket { get; }

        internal CancellationTokenSource TokenSource { get; }

        /// <summary>Captured at creation, so it stays readable after the source is disposed.</summary>
        internal CancellationToken Token { get; }

        internal string ConnectionId { get; }

        /// <summary>True once a release has started.</summary>
        internal bool IsReleased => Volatile.Read(ref _releaseStarted) == 1;

        /// <summary>Completes when the release has finished.</summary>
        internal Task ReleaseCompleted => _released.Task;

        /// <summary>True while this connection is not released and its socket is open.</summary>
        internal bool IsOpen
        {
            get
            {
                if (IsReleased)
                {
                    return false;
                }

                try
                {
                    return Socket.State == WebSocketState.Open;
                }
                catch (ObjectDisposedException)
                {
                    return false;
                }
            }
        }

        internal bool TryBeginRelease() => Interlocked.Exchange(ref _releaseStarted, 1) == 0;

        internal void CompleteRelease() => _released.TrySetResult(true);
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
    /// Sends the close message for one connection; <see cref="StopConnection"/> calls it with the
    /// connection it is stopping. The clients in this SDK override it to send on that connection
    /// only. The default calls the public <see cref="SendClose(bool, CancellationTokenSource?)"/>,
    /// which acts on the current connection, so it does so only while that is still this one.
    /// </summary>
    private protected virtual Task SendClose(ConnectionState owned, bool nullByte, CancellationTokenSource? cancelToken) =>
        ReferenceEquals(CurrentConnection, owned) ? SendClose(nullByte, cancelToken) : Task.CompletedTask;

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

    /// <summary>
    /// Sends one message on the given connection's socket, never on a newer connection, holding
    /// the send mutex. Does nothing once that connection is released; callers check the socket
    /// state they need before calling.
    /// </summary>
    internal virtual async Task SendOnConnection(ConnectionState owned, ArraySegment<byte> data, WebSocketMessageType messageType, CancellationToken token)
    {
        if (owned.IsReleased)
        {
            Log.Debug("SendOnConnection", "The connection was released. Exiting...");
            return;
        }

        try
        {
            await _mutexSend.WaitAsync(token);
            try
            {
                await owned.Socket.SendAsync(data, messageType, true, token)
                    .ConfigureAwait(false);
            }
            finally
            {
                _mutexSend.Release();
            }
        }
        catch (ObjectDisposedException ex)
        {
            // A concurrent release disposed the connection's socket or token source.
            Log.Debug("SendOnConnection", "WebSocket was disposed while sending. Exiting...");
            Log.Verbose("SendOnConnection", $"ObjectDisposedException: {ex}");
        }
    }

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

    internal async Task ProcessSendQueue(ConnectionState owned)
    {
        Log.Verbose("AbstractWebSocketClient.ProcessSendQueue", "ENTER");

        try
        {
            // Every read uses this connection's own state, never the fields, so the loop cannot
            // pick up a newer connection's socket or token. Release cancels the token before it
            // clears anything, so a released connection is always observed as cancelled here.
            var token = owned.Token;
            while (await _sendChannel.Reader.WaitToReadAsync(token))
            {
                if (token.IsCancellationRequested)
                {
                    Log.Information("ProcessSendQueue", "ProcessSendQueue cancelled");
                    break;
                }
                if (!owned.IsOpen)
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
                    await SendOnConnection(owned, message.Message, message.MessageType, token);
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

    internal async Task ProcessReceiveQueue(ConnectionState owned)
    {
        Log.Verbose("AbstractWebSocketClient.ProcessReceiveQueue", "ENTER");

        // Every read uses this connection's own state, never the fields, so the loop cannot read
        // from a newer connection's socket or tear it down.
        var token = owned.Token;
        var socket = owned.Socket;
        while (owned.IsOpen)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    Log.Information("ProcessReceiveQueue", "ReceiveThread cancelled");
                    await StopConnection(owned);
                    Log.Verbose("ProcessReceiveQueue", "LEAVE");
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
                    await StopConnection(owned);
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

        // Snapshot the connection on entry: this Stop() tears down only that connection, even if
        // a newer one is published while it runs. See #455.
        var owned = CurrentConnection;
        if (owned == null)
        {
            Log.Information("Stop", "Client has already been disposed");
            Log.Verbose("AbstractWebSocketClient.Stop", "LEAVE");
            return true;
        }

        var result = await StopConnection(owned, cancelToken, nullByte);
        Log.Verbose("AbstractWebSocketClient.Stop", "LEAVE");
        return result;
    }

    /// <summary>
    /// Closes and releases one connection. Returns early (after any release in progress has
    /// finished) when another teardown already released it, so a stale caller never touches a
    /// newer connection.
    /// </summary>
    private protected async Task<bool> StopConnection(ConnectionState owned, CancellationTokenSource? cancelToken = null, bool nullByte = false)
    {
        Log.Verbose("AbstractWebSocketClient.StopConnection", "ENTER");

        if (owned.IsReleased)
        {
            await owned.ReleaseCompleted.ConfigureAwait(false);
            Log.Information("Stop", "Client has already been disposed");
            Log.Verbose("AbstractWebSocketClient.StopConnection", "LEAVE");
            return true;
        }

        // The default disconnect-timeout source is owned by this call: the using declaration
        // disposes it on every exit path.
        using var defaultCancelToken = cancelToken == null
            ? new CancellationTokenSource(Constants.DefaultDisconnectTimeout)
            : null;
        if (defaultCancelToken != null)
        {
            Log.Information("Stop", "Using default disconnect cancellation token");
            cancelToken = defaultCancelToken;
        }

        var socket = owned.Socket;

        try
        {
            // if websocket is open, send a close message on this connection
            if (owned.IsOpen)
            {
                Log.Debug("Stop", "Sending Close message...");
                await SendClose(owned, nullByte, cancelToken);
            }

            // Another teardown may have released this connection while SendClose ran.
            if (owned.IsReleased)
            {
                await owned.ReleaseCompleted.ConfigureAwait(false);
                Log.Debug("Stop", "Connection was released by another teardown");
                Log.Verbose("AbstractWebSocketClient.StopConnection", "LEAVE");
                return true;
            }

            // small delay to wait for any final transcription
            await Task.Delay(100, cancelToken.Token).ConfigureAwait(false);

            if (owned.IsReleased)
            {
                await owned.ReleaseCompleted.ConfigureAwait(false);
                Log.Debug("Stop", "Connection was released by another teardown");
                Log.Verbose("AbstractWebSocketClient.StopConnection", "LEAVE");
                return true;
            }

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
                if (!owned.IsReleased && (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived))
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

            // Release only this connection's state: cancel-before-dispose so its background loops
            // observe cancellation and exit cleanly, and compare-before-clear so a newer
            // connection's fields are left alone. See #390 and #455.
            Log.Debug("Stop", "Releasing the connection...");
            ReleaseConnection(owned);
            await owned.ReleaseCompleted.ConfigureAwait(false);

            Log.Debug("Stop", "Succeeded");
            Log.Verbose("AbstractWebSocketClient.StopConnection", "LEAVE");

            return true;
        }
        catch (OperationCanceledException ex)
        {
            // Covers both Task.Delay's TaskCanceledException and CloseOutputAsync's bare
            // OperationCanceledException if the disconnect-timeout token fires during teardown.
            Log.Debug("Stop", "Stop cancelled.");
            Log.Verbose("Stop", $"Stop cancelled. Info: {ex}");
            Log.Verbose("AbstractWebSocketClient.StopConnection", "LEAVE");

            ReleaseConnection(owned);
            await owned.ReleaseCompleted.ConfigureAwait(false);
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
            Log.Verbose("AbstractWebSocketClient.StopConnection", "LEAVE");

            ReleaseConnection(owned);
            await owned.ReleaseCompleted.ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error("Stop", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Stop", $"Exception: {ex}");
            Log.Verbose("AbstractWebSocketClient.StopConnection", "LEAVE");

            ReleaseConnection(owned);
            await owned.ReleaseCompleted.ConfigureAwait(false);
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
    /// Cancels and disposes whatever internal cancellation token source is current, exactly once
    /// across concurrent callers (Interlocked.Exchange lets one caller claim it; see #390).
    /// Kept for derived classes. Stop() and Dispose() no longer use it: they release one
    /// connection's own state, which this method is not scoped to, so a call can cancel a newer
    /// connection's token.
    /// </summary>
    protected void DisposeCancellationTokenSource()
    {
        var cts = Interlocked.Exchange(ref _cancellationTokenSource, null);
        if (cts == null)
        {
            return;
        }

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
        // Read the field once: a concurrent release can clear it between reads.
        var socket = _clientWebSocket;
        if (socket == null)
        {
            return WebSocketState.None;
        }
        var state = socket.State;
        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.Debug("State", $"WebSocket State: {state}");
        }
        return state;
    }

    /// <summary>
    /// Indicates whether the WebSocket is connected
    /// </summary> 
    /// <returns>Returns true if the WebSocket is connected</returns>
    public bool IsConnected()
    {
        // Read the field once: a concurrent release can clear it between reads.
        var socket = _clientWebSocket;
        if (socket == null)
        {
            return false;
        }

        var state = socket.State;
        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.Debug("State", $"WebSocket State: {state}");
        }
        return state == WebSocketState.Open;
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
        // Mark disposed and take the published connection in one step: a Connect() either claimed
        // the client before this (and its connection is released here) or sees the flag and
        // fails its claim.
        ConnectionState? owned;
        lock (_lifecycleLock)
        {
            _disposed = true;
            owned = _connection;
        }

        // atomic-claim teardown; safe against a concurrent Stop()/Dispose(). See #390.
        if (owned != null)
        {
            ReleaseConnection(owned);
            owned.Socket.Dispose();
        }

        _sendChannel.Writer.TryComplete(); // TryComplete: idempotent under concurrent Dispose() (#390 R3)

        GC.SuppressFinalize(this);
    }
    #endregion
}
