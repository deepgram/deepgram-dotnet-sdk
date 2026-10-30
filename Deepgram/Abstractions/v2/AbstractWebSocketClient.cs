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
    // the SDK calls and which is kept for derived classes. It is a plain lock, held for a few
    // field reads and writes only: never across an await, and never while user code (event
    // handlers, cancellation callbacks) runs. Hot paths (sends, IsConnected(), State()) read
    // the fields without it; each reads a field once and then works on that value or on the
    // connection's own ConnectionState.
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
    /// <remarks>
    /// Open event handlers run before Connect() returns, and the receive loop starts only after they
    /// return, so an Open handler that waits for a message from the server blocks until its own
    /// timeout. An exception thrown by an Open handler is logged at Error level and Connect() still
    /// returns true. Messages still queued from earlier sends are sent as soon as the new connection
    /// opens; the send methods' remarks say which ones are kept. If a previous connection is still
    /// being released, Connect() waits for that to finish (within its connect timeout) and then
    /// opens a new connection. If another Connect() is still opening the connection or a Stop() is
    /// closing it, Connect() waits for it within the same timeout and returns true only for an open
    /// connection that is not stopping.
    /// A Stop() that begins before Connect() returns (for example, from an Open handler) makes
    /// Connect() return false even after Open fired; Close then follows.
    /// </remarks>
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
    /// when another live connection was already published (<c>Connected</c> true, the "already
    /// initialized" result). Subclasses use <c>Owned</c> for their post-connect work, never
    /// <see cref="CurrentConnection"/>, so a concurrent Connect() cannot act on, or release,
    /// a connection it did not open.
    /// <para>
    /// <paramref name="beforeOpen"/>, when given, runs on the new connection after the handshake,
    /// before its sender starts and before Open is raised, so the frames it sends are the first on
    /// the wire (the Voice Agent sends its Settings message here). It receives the connect token.
    /// If it returns false, the connection is released and Connect() returns false without raising
    /// Open or Close; if it throws, the exception propagates as the raw exception. It is a per-call
    /// delegate, not a virtual, because concurrent Connect() calls can carry different settings.
    /// </para>
    /// </summary>
    private protected async Task<(bool Connected, ConnectionState? Owned)> ConnectCore(string uri, CancellationTokenSource? cancelToken, Dictionary<string, string>? headers,
        Func<ConnectionState, CancellationToken, Task<bool>>? beforeOpen = null)
    {
        Log.Verbose("AbstractWebSocketClient.Connect", "ENTER");
        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.Debug("Connect", $"headers: {headers}");
        }

        // The default connect-timeout source is owned by this call: the using declaration
        // disposes it on every exit path, including when the header setup below throws. It is
        // created before anything else, so one budget covers waiting for a previous connection's
        // release and the handshake. The token is read once, here.
        using var defaultCancelToken = cancelToken == null
            ? new CancellationTokenSource(Constants.DefaultConnectTimeout)
            : null;
        if (defaultCancelToken != null)
        {
            Log.Information("Connect", "Using default connect cancellation token");
            cancelToken = defaultCancelToken;
        }
        var connectToken = cancelToken!.Token;

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
        // lifecycle lock. The claim fails if the client is disposed. If another connection is
        // published, the outcome depends on its state, read outside the lock (#455):
        // - usable (opened, not stopping, socket open): the "already initialized" result, true
        //   with nothing opened;
        // - opened, not stopping, but its socket closed or dropped before any receive loop saw
        //   it: stop it here, as the receive loop would, then try again;
        // - still handshaking or preparing (not yet opened): wait until it opens or is released;
        // - stopping or released: wait for the release to finish.
        // Waits are bounded by the connect token, happen outside the lock and loop back to
        // re-read the state.
        while (true)
        {
            ConnectionState? existing;
            bool disposed;
            lock (_lifecycleLock)
            {
                disposed = _disposed;
                existing = _connection;
                if (existing != null && existing.ReleaseCompleted.IsCompleted)
                {
                    // Defensive: a release that finished without unpublishing (only possible if
                    // it threw first). Waiting on it would spin, so unpublish it here.
                    Volatile.Write(ref _connection, null);
                    _clientWebSocket = null;
                    _cancellationTokenSource = null;
                    _connectionId = null;
                    existing = null;
                }
                if (!disposed && existing == null)
                {
                    _clientWebSocket = socket;
                    _cancellationTokenSource = owned.TokenSource;
                    _connectionId = owned.ConnectionId;
                    Volatile.Write(ref _connection, owned);
                }
            }

            if (disposed)
            {
                Log.Warning("Connect", "Client has been disposed. Not connecting.");
                Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

                owned.TokenSource.Dispose();
                socket.Dispose();
                return (false, null);
            }

            if (existing == null)
            {
                break;
            }

            if (existing.IsUsable)
            {
                Log.Debug("Connect", "Client has already been initialized");
                Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

                owned.TokenSource.Dispose();
                socket.Dispose();
                return (true, null);
            }

            if (existing.IsOpened && !existing.IsStopping && !existing.IsReleased && existing.IsSocketEnded)
            {
                // Nothing else may ever release it: its receive loop starts only after the Open
                // handlers return, and this Connect() may be running inside one of them.
                try
                {
                    Log.Information("Connect", "The current connection is no longer open. Stopping it...");
                    await StopConnection(existing).ConfigureAwait(false);
                }
                catch
                {
                    owned.TokenSource.Dispose();
                    socket.Dispose();
                    throw;
                }
                continue;
            }

            Task waitFor;
            if (existing.IsOpened || existing.IsStopping || existing.IsReleased)
            {
                Log.Debug("Connect", "Waiting for the previous connection to be released...");
                waitFor = existing.ReleaseCompleted;
            }
            else
            {
                Log.Debug("Connect", "Waiting for the connection another Connect() is opening...");
                waitFor = Task.WhenAny(existing.Ready, existing.ReleaseCompleted);
            }
            if (!await WaitOrCancelledAsync(waitFor, connectToken).ConfigureAwait(false))
            {
                Log.Warning("Connect", "The current connection did not open or close in time. Not connecting.");
                Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

                owned.TokenSource.Dispose();
                socket.Dispose();
                return (false, null);
            }
        }
        // A connection-scoped correlation id, so every log entry for this connection can be
        // grouped together (#305). The scope is ambient (async-local): the sender and receiver
        // tasks started below inherit it for their whole lifetime. It is disposed when this
        // method returns, on this method's own flow, never from another thread's teardown.
        // Log providers are user code: if one throws here, release the connection just published
        // so the client is not left holding it.
        IDisposable? scope;
        try
        {
            Log.Debug("Connect", "Connection claimed");
            scope = Log.BeginScope("AbstractWebSocketClient",
                new Dictionary<string, object> { ["dg.connection_id"] = owned.ConnectionId });
        }
        catch
        {
            ResetFailedConnection(owned);
            throw;
        }
        using var connectionScope = scope;

        // True while the pre-open step runs: its failures surface as the raw exception, as the
        // Voice Agent's Settings failures always have, not as a handshake failure.
        var inPreOpenHook = false;
        try
        {
            var myUri = new Uri(uri);

            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.Debug("Connect", $"uri: {uri}");

                Log.Debug("Connect", "Connecting to Deepgram API...");
            }
            await socket.ConnectAsync(myUri, connectToken).ConfigureAwait(false);

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
                if (!await WaitOrCancelledAsync(priorSender, connectToken).ConfigureAwait(false))
                {
                    Log.Warning("Connect", "The previous connection's sender did not finish. Not connecting.");
                    Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

                    ResetFailedConnection(owned);
                    return (false, null);
                }
            }

            // The pre-open step runs before the sender starts and before Open, so what it sends
            // comes first on the wire: nothing queued can overtake it, and immediate sends are
            // refused until the connection is marked opened.
            if (beforeOpen != null)
            {
                inPreOpenHook = true;
                var prepared = await beforeOpen(owned, connectToken).ConfigureAwait(false);
                inPreOpenHook = false;
                if (!prepared)
                {
                    Log.Warning("Connect", "The connection could not be prepared before Open. Not connecting.");
                    Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

                    ResetFailedConnection(owned);
                    return (false, null);
                }
            }

            // Stop() or Dispose() may have released this connection while it waited. Starting a
            // sender then would add a second reader to the single-reader channel for a dead
            // connection, so report a failed connect instead. The check and the start happen
            // under the lifecycle lock, as is a newer connection's read of _senderTask above:
            // either this sender is started first and the newer connection waits for it, or this
            // connection is already released and starts nothing.
            // A Stop() that has begun (Stopping) fails this connect too: starting the sender would
            // only drain the backlog onto a closing socket. A Stop() that begins after this check and
            // before the mark below still fails the connect there; the sender it started then exits
            // with the release.
            bool releasedBeforeSender;
            lock (_lifecycleLock)
            {
                releasedBeforeSender = owned.IsReleased || owned.IsStopping;
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

            // Logged before the mark: log providers are user code, and a throw after the mark but
            // before any Open handler ran would leave an opened connection whose Close has no Open.
            var openHandlers = _openReceived;
            if (openHandlers != null)
            {
                Log.Debug("Connect", "Sending OpenResponse event...");
            }

            // From here on this connection raises Open, so its teardown raises Close. The mark is
            // atomic with the start of a release: if a Stop() or Dispose() released the connection
            // since the check above (and so raised no Close), Open is not raised either. Nothing
            // between the mark and the end of Open delivery may throw.
            if (!owned.TryMarkOpened())
            {
                Log.Warning("Connect", "Connection was stopped while connecting");
                Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

                return (false, null);
            }

            try
            {
                // Send the OpenResponse event. Handlers run one at a time, in subscription order,
                // each in its own try/catch (InvokeEachIsolated): an exception is logged and the
                // next handler still runs. Close is delivered the same way. A throwing handler
                // used to fail Connect() after the background loops had started, leaving them
                // behind.
                if (openHandlers != null)
                {
                    var data = new OpenResponse();
                    data.Type = WebSocketType.Open;
                    InvokeEachIsolated(openHandlers, data, "Connect", "Open");
                }
            }
            finally
            {
                // A Stop() that released this connection while the Open handlers ran left its
                // Close for this point, so Close always follows Open (see TryOweCloseOrClaim).
                if (owned.TryMarkOpenDelivered())
                {
                    RaiseClose();
                }
            }

            // Start reading only after the Open handlers have run, so a server close (and the
            // Close event it raises) cannot overtake Open.
            Log.Debug("Connect", "Starting Receiver Thread...");
            _ = Task.Run(() => RunReceiveLoop(owned));

            // A Stop() or Dispose() (from an Open handler, say) may have released the connection
            // meanwhile, or a Stop() may still be closing it (a Flux STT/TTS close grace period). A
            // Stop() raises Close for it, after Open; Dispose() raises none. Do not report success
            // for it.
            if (owned.IsReleased || owned.IsStopping)
            {
                Log.Warning("Connect", "Connection was stopped while connecting");
                Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

                return (false, null);
            }

            Log.Debug("Connect", "Connect Succeeded");
            Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

            return (true, owned);
        }
        catch (Exception ex) when (owned.IsReleased || owned.IsStopping)
        {
            // Stop() or Dispose() released (or a Stop() began closing) this connection while it was
            // connecting, which surfaces here as the socket's exception. Report a failed connect.
            // Every catch below releases before it logs: log providers are user code and can throw.
            await ReleaseFailedConnect(owned).ConfigureAwait(false);
            Log.Warning("Connect", "Connection was stopped while connecting");
            Log.Verbose("Connect", $"Exception: {ex}");
            Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

            return (false, null);
        }
        catch (TaskCanceledException ex)
        {
            await ReleaseFailedConnect(owned).ConfigureAwait(false);
            Log.Debug("Connect", "Connect cancelled.");
            Log.Verbose("Connect", $"Connect cancelled. Info: {ex}");
            Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

            return (false, null);
        }
        catch (WebSocketException ex) when (_deepgramClientOptions is DeepgramWsClientOptions { ThrowDeepgramWebSocketExceptions: true } && !inPreOpenHook)
        {
            // Read the handshake's status code before the release disposes the socket.
            var httpStatusCode = DeepgramWebSocketException.GetHttpStatusCode(socket);
            await ReleaseFailedConnect(owned).ConfigureAwait(false);
            Log.Error("Connect", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Connect", $"Exception: {ex}");
            Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

            throw new DeepgramWebSocketException(
                "Failed to connect to Deepgram API",
                httpStatusCode,
                ex);
        }
        catch (Exception ex)
        {
            await ReleaseFailedConnect(owned).ConfigureAwait(false);
            Log.Error("Connect", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Connect", $"Exception: {ex}");
            Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

            throw;
        }
    }

    /// <summary>
    /// Releases a connection whose <see cref="ConnectCore"/> failed with an exception. A connection
    /// that already raised Open (a log call after Open threw, say) gets its Close, as I3 requires;
    /// one that did not is released without events. Idempotent.
    /// </summary>
    private async Task ReleaseFailedConnect(ConnectionState owned)
    {
        if (owned.IsOpened)
        {
            await ReleaseAndRaiseClose(owned).ConfigureAwait(false);
        }
        else
        {
            ResetFailedConnection(owned);
        }
    }

    /// <summary>
    /// Waits for <paramref name="task"/> to complete or <paramref name="token"/> to be cancelled.
    /// True when the task completed. netstandard2.0 has no Task.WaitAsync.
    /// </summary>
    private static async Task<bool> WaitOrCancelledAsync(Task task, CancellationToken token)
    {
        if (task.IsCompleted)
        {
            return true;
        }

        var cancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using (token.Register(static state => ((TaskCompletionSource<bool>)state!).TrySetResult(true), cancelled))
        {
            return await Task.WhenAny(task, cancelled.Task).ConfigureAwait(false) == task;
        }
    }

    /// <summary>
    /// Returns the client to its pre-connect state after a failed <see cref="Connect"/>, so the
    /// caller can retry. Without this, the half-initialized socket stays assigned and the next
    /// Connect() call returns true without opening a connection.
    /// </summary>
    private void ResetFailedConnection(ConnectionState owned) => ReleaseConnection(owned);

    /// <summary>
    /// Releases one connection's state. Idempotent: the first caller does the work. In order:
    /// <list type="number">
    /// <item>cancel the connection's token, so its background loops stop (outside the lock:
    /// cancellation runs callbacks);</item>
    /// <item>under the lifecycle lock, if this connection is still the published one, clear
    /// <c>_connection</c>, <see cref="_clientWebSocket"/>, the token source field and the
    /// connection id together, so a stale teardown leaves a newer connection alone and a new
    /// <see cref="Connect"/> can claim the client only after the old token is cancelled;</item>
    /// <item>dispose the token source and the socket.</item>
    /// </list>
    /// </summary>
    /// <returns>True when this call performed the release; false when another caller had already started it.</returns>
    private protected bool ReleaseConnection(ConnectionState owned)
    {
        if (!owned.TryBeginRelease())
        {
            return false;
        }

        try
        {
            ReleaseOwnedState(owned);
        }
        finally
        {
            owned.CompleteRelease();
        }

        return true;
    }

    /// <summary>
    /// The release steps listed on <see cref="ReleaseConnection"/>. Only the caller that won the
    /// release (<see cref="ConnectionState.TryBeginRelease"/>) runs them.
    /// </summary>
    private void ReleaseOwnedState(ConnectionState owned)
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
        owned.Socket.Dispose();
    }

    /// <summary>
    /// Ends a Stop(): releases the connection and, if this call performed the release, raises
    /// the Close event. Close is raised after the release, so a handler that calls Connect()
    /// opens a new connection, and exactly once per stopped connection, whichever teardown wins.
    /// A connection that never reached its Open event (stopped during the handshake) gets no
    /// Close either.
    /// </summary>
    private protected async Task ReleaseAndRaiseClose(ConnectionState owned)
    {
        if (!owned.TryBeginRelease())
        {
            // Another teardown is releasing this connection and raises Close if needed. Wait
            // until it has finished, Close included, so this caller returns after it.
            await owned.ReleaseCompleted.ConfigureAwait(false);
            return;
        }

        try
        {
            ReleaseOwnedState(owned);

            // Close follows Open. If the Open handlers are still running, Close is owed and the
            // Open delivery raises it when they return; nobody waits for that, so a Stop() from
            // inside an Open handler cannot deadlock.
            if (owned.IsOpened && owned.TryOweCloseOrClaim())
            {
                RaiseClose();
            }
        }
        finally
        {
            // Completed after Close when this call raised it: a Stop() that waits for another
            // teardown's release returns only once that Close has been delivered. When Close is
            // owed to a running Open delivery, it completes without waiting for it.
            owned.CompleteRelease();
        }
    }

    private void RaiseClose()
    {
        if (_closeReceived != null)
        {
            try
            {
                Log.Debug("Stop", "Sending CloseResponse event...");
            }
            catch
            {
                // A failing log provider must not suppress Close: it pairs with an Open that fired.
            }
            var data = new CloseResponse();
            data.Type = WebSocketType.Close;

            // One at a time, each handler isolated, as for Open: Close fires once per connection,
            // so every handler must receive it even when an earlier one throws.
            InvokeEachIsolated(_closeReceived, data, "Stop", "Close");
        }
    }

    /// <summary>
    /// Invokes each handler of <paramref name="eventHandler"/> in subscription order, each in its
    /// own try/catch. An exception from one handler is logged and the next handler still runs.
    /// Used for the lifecycle events (Open, Close), which fire once per connection.
    /// </summary>
    internal static void InvokeEachIsolated<T>(EventHandler<T>? eventHandler, T e, string module, string eventName)
    {
        if (eventHandler == null)
        {
            return;
        }

        foreach (var handler in eventHandler.GetInvocationList().Cast<EventHandler<T>>())
        {
            InvokeIsolated(handler, e, module, eventName);
        }
    }

    /// <summary>
    /// Invokes one event handler. An exception from it is logged, never rethrown.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Event handlers are user code; one that throws must not stop delivery to the others.")]
    private static void InvokeIsolated<T>(EventHandler<T> handler, T e, string module, string eventName)
    {
        try
        {
            handler(null, e);
        }
        catch (Exception ex)
        {
            LogHandlerException(module, eventName, ex);
        }
    }

    /// <summary>
    /// Logs an exception thrown by an event handler. Log providers are user code: a provider that
    /// throws here must not stop delivery to the remaining handlers.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Log providers are user code; one that throws must not stop event delivery.")]
    private static void LogHandlerException(string module, string eventName, Exception ex)
    {
        try
        {
            Log.Error(module, $"Exception occurred in {eventName} event handler: {ex.GetType()} {ex.Message}");
            Log.Verbose(module, $"Exception: {ex}");
        }
        catch
        {
            // A failing log provider must not stop event delivery.
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
        // OpenedFlag and ReleasedFlag live in one field so "mark opened" and "begin release" are
        // decided atomically against each other. They stay lock-free CAS loops rather than going
        // under the client's lifecycle lock: they concern this connection only, not which
        // connection the client owns, and a CAS on one int is the whole decision.
        private const int OpenedFlag = 1;
        private const int ReleasedFlag = 2;
        // Close is raised by whichever comes second: the release that ends a Stop(), or the end
        // of the Open delivery. OpenDeliveredFlag records the latter, CloseOwedFlag records a
        // release that came first. Both are set by CAS on _flags, so exactly one party raises.
        private const int OpenDeliveredFlag = 4;
        private const int CloseOwedFlag = 8;
        // Set when a Stop() (or a teardown that goes through StopConnection) begins. The connection
        // is then no longer usable, a Connect() still opening it fails, and it is always released.
        private const int StoppingFlag = 16;
        private int _flags;
        // Set once the Flux STT CloseStream / Flux TTS Close message has been sent on this connection.
        private int _closeMessageSent;
        private readonly TaskCompletionSource<bool> _released = new(TaskCreationOptions.RunContinuationsAsynchronously);
        // Completed when the connection is marked opened. Continuations run asynchronously, so a
        // waiting Connect() never runs inline on the opening thread before Open is delivered.
        private readonly TaskCompletionSource<bool> _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

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
        internal bool IsReleased => (Volatile.Read(ref _flags) & ReleasedFlag) != 0;

        /// <summary>Completes when the release has finished, including its Close event if it raises one.</summary>
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

        /// <summary>True once Connect() has committed to raising Open for this connection.</summary>
        internal bool IsOpened => (Volatile.Read(ref _flags) & OpenedFlag) != 0;

        /// <summary>True once a Stop() of this connection has begun.</summary>
        internal bool IsStopping => (Volatile.Read(ref _flags) & StoppingFlag) != 0;

        /// <summary>
        /// True when another Connect() may report this connection as connected: opened, not
        /// stopping, not released, and its socket open.
        /// </summary>
        internal bool IsUsable => IsOpened && !IsStopping && IsOpen;

        /// <summary>
        /// True when the socket has seen the end of the connection (a close frame, a completed close
        /// or an abort), whether or not a receive loop has noticed it yet.
        /// </summary>
        internal bool IsSocketEnded
        {
            get
            {
                try
                {
                    var state = Socket.State;
                    return state == WebSocketState.CloseReceived || state == WebSocketState.Closed || state == WebSocketState.Aborted;
                }
                catch (ObjectDisposedException)
                {
                    return true;
                }
            }
        }

        /// <summary>Completes when the connection is marked opened.</summary>
        internal Task Ready => _ready.Task;

        /// <summary>
        /// True for the one caller that may send this connection's close message (the Flux STT
        /// CloseStream or the Flux TTS Close), false once it has been claimed.
        /// </summary>
        internal bool TryMarkCloseMessageSent() => Interlocked.Exchange(ref _closeMessageSent, 1) == 0;

        /// <summary>Marks the connection opened; fails once a release or a Stop() has begun.</summary>
        internal bool TryMarkOpened()
        {
            while (true)
            {
                var flags = Volatile.Read(ref _flags);
                if ((flags & (ReleasedFlag | StoppingFlag)) != 0)
                {
                    return false;
                }
                if (Interlocked.CompareExchange(ref _flags, flags | OpenedFlag, flags) == flags)
                {
                    _ready.TrySetResult(true);
                    return true;
                }
            }
        }

        /// <summary>
        /// Marks that a Stop() of this connection has begun. Idempotent: being already stopping
        /// never lets a caller skip its release path.
        /// </summary>
        internal void MarkStopping()
        {
            while (true)
            {
                var flags = Volatile.Read(ref _flags);
                if ((flags & StoppingFlag) != 0
                    || Interlocked.CompareExchange(ref _flags, flags | StoppingFlag, flags) == flags)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Called by the Stop() release winner of an opened connection. True: the Open delivery has
        /// finished, so the caller raises Close now. False: Close is owed to the Open delivery.
        /// </summary>
        internal bool TryOweCloseOrClaim()
        {
            while (true)
            {
                var flags = Volatile.Read(ref _flags);
                if ((flags & OpenDeliveredFlag) != 0)
                {
                    return true;
                }
                if (Interlocked.CompareExchange(ref _flags, flags | CloseOwedFlag, flags) == flags)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Called when the Open delivery finishes. True when a release already owes Close, so the
        /// caller raises it now.
        /// </summary>
        internal bool TryMarkOpenDelivered()
        {
            while (true)
            {
                var flags = Volatile.Read(ref _flags);
                if (Interlocked.CompareExchange(ref _flags, flags | OpenDeliveredFlag, flags) == flags)
                {
                    return (flags & CloseOwedFlag) != 0;
                }
            }
        }

        /// <summary>Begins the release; true for the one caller that does it.</summary>
        internal bool TryBeginRelease()
        {
            while (true)
            {
                var flags = Volatile.Read(ref _flags);
                if ((flags & ReleasedFlag) != 0)
                {
                    return false;
                }
                if (Interlocked.CompareExchange(ref _flags, flags | ReleasedFlag, flags) == flags)
                {
                    return true;
                }
            }
        }

        internal void CompleteRelease() => _released.TrySetResult(true);
    }

    #region Subscribe Event
    /// <summary>
    /// Subscribe to an Open event from the Deepgram API
    /// </summary>
    /// <remarks>
    /// The handler runs before Connect() returns. If it throws, the exception is logged at Error
    /// level and the connection stays open; Connect() still returns true. The receive loop starts
    /// only after the Open handlers return, so a handler that waits for a message from the server
    /// blocks until its own timeout.
    /// </remarks>
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
        var owned = CurrentConnection;
        if (owned == null)
        {
            Log.Debug("Flush", "WebSocket is not connected. Nothing to flush.");
            return;
        }

        await FlushConnection(owned).ConfigureAwait(false);
    }

    /// <summary>
    /// <see cref="Flush"/> for one connection: waits until every message queued before the call has
    /// been written by that connection's sender, or that connection has ended. The marker belongs to
    /// the connection, so a newer connection's sender never reports it as reached (#455).
    /// </summary>
    private protected async Task FlushConnection(ConnectionState owned)
    {
        if (!owned.IsOpen || owned.IsStopping)
        {
            Log.Debug("Flush", "WebSocket is not connected. Nothing to flush.");
            return;
        }

        var flushSignal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        EnqueueSendMessage(WebSocketMessage.CreateFlushMarker(flushSignal, owned));

        // Unblock if the connection is torn down before the marker is reached. Registering on an
        // already-cancelled token runs the callback immediately; if the token's source was disposed
        // by a concurrent teardown between the guard above and here, treat it as "connection gone".
        try
        {
            using (owned.Token.Register(static state => ((TaskCompletionSource<bool>)state!).TrySetResult(false), flushSignal))
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
    /// <remarks>
    /// The message is queued and written by a background sender while the client is connected.
    /// When a connection ends, its sender stops: messages queued around that moment, including
    /// any queued before the sender notices the end, may be dropped. Messages queued while no
    /// sender is running (after it has stopped, or around a failed Connect()) stay queued and are
    /// sent all at once on the next successful Connect(). To keep that backlog from reaching the
    /// server, stop sending while disconnected or use a new client. After Dispose(), messages are
    /// dropped.
    /// </remarks>
    /// <param name="data"></param>
    public virtual void SendBinary(byte[] data, int length = Constants.UseArrayLengthForSend) =>
        EnqueueSendMessage(new WebSocketMessage(data, WebSocketMessageType.Binary, length));

    /// <summary>
    /// This method sends a text message over the WebSocket connection.
    /// </summary>
    /// <remarks>
    /// The message is queued and written by a background sender while the client is connected.
    /// When a connection ends, its sender stops: messages queued around that moment, including
    /// any queued before the sender notices the end, may be dropped. Messages queued while no
    /// sender is running (after it has stopped, or around a failed Connect()) stay queued and are
    /// sent all at once on the next successful Connect(). To keep that backlog from reaching the
    /// server, stop sending while disconnected or use a new client. After Dispose(), messages are
    /// dropped.
    /// </remarks>
    public virtual void SendMessage(byte[] data, int length = Constants.UseArrayLengthForSend) =>
        EnqueueSendMessage(new WebSocketMessage(data, WebSocketMessageType.Text, length));

    /// <summary>
    /// This method sends a binary message over the WebSocket connection immediately without queueing.
    /// </summary>
    /// <remarks>
    /// The message is sent only on a connection that has finished opening (its Open event has been
    /// or is being raised). Otherwise it is dropped, including while Connect() is still in progress
    /// or after Stop() begins.
    /// </remarks>
    public virtual async Task SendBinaryImmediately(byte[] data, int length = Constants.UseArrayLengthForSend, CancellationTokenSource? _cancellationToken = null)
    {
        // Read the current connection once and use its socket and token together, so a concurrent
        // Stop()/Dispose() or reconnect can't pair one connection's socket with another's token.
        // See #390 and #455.
        // A connection that has not been marked opened is still connecting; an immediate send then
        // could precede the pre-open step's frames (the Voice Agent's Settings message).
        var owned = CurrentConnection;
        if (owned == null || !owned.IsOpened || !owned.IsOpen)
        {
            Log.Debug("SendBinaryImmediately", "WebSocket is not connected. Exiting...");
            return;
        }
        await SendBinaryImmediately(owned, data, length, _cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// This method sends a text message over the WebSocket connection immediately without queueing.
    /// </summary>
    /// <remarks>
    /// The message is sent only on a connection that has finished opening (its Open event has been
    /// or is being raised). Otherwise it is dropped, including while Connect() is still in progress
    /// or after Stop() begins.
    /// </remarks>
    public virtual async Task SendMessageImmediately(byte[] data, int length = Constants.UseArrayLengthForSend, CancellationTokenSource? _cancellationToken = null)
    {
        // Read the current connection once and use its socket and token together, so a concurrent
        // Stop()/Dispose() or reconnect can't pair one connection's socket with another's token.
        // See #390 and #455.
        // A connection that has not been marked opened is still connecting; an immediate send then
        // could precede the pre-open step's frames (the Voice Agent's Settings message).
        var owned = CurrentConnection;
        if (owned == null || !owned.IsOpened || !owned.IsOpen)
        {
            Log.Debug("SendMessageImmediately", "WebSocket is not connected. Exiting...");
            return;
        }
        await SendMessageImmediately(owned, data, length, _cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Sends binary data on one opened, non-stopping connection only.</summary>
    private protected async Task SendBinaryImmediately(ConnectionState owned, byte[] data, int length = Constants.UseArrayLengthForSend, CancellationTokenSource? cancellationToken = null)
    {
        if (!owned.IsOpened || !owned.IsOpen || owned.IsStopping)
        {
            Log.Debug("SendBinaryImmediately", "WebSocket is not connected. Exiting...");
            return;
        }
        if (length == Constants.UseArrayLengthForSend)
        {
            length = data.Length;
        }
        Log.Verbose("SendBinaryImmediately", "Sending binary message immediately...");
        await SendOnConnection(owned, new ArraySegment<byte>(data, 0, length), WebSocketMessageType.Binary,
            cancellationToken?.Token ?? owned.Token).ConfigureAwait(false);
    }

    /// <summary>Sends text data on one opened, non-stopping connection only.</summary>
    private protected async Task SendMessageImmediately(ConnectionState owned, byte[] data, int length = Constants.UseArrayLengthForSend, CancellationTokenSource? cancellationToken = null)
    {
        if (!owned.IsOpened || !owned.IsOpen || owned.IsStopping)
        {
            Log.Debug("SendMessageImmediately", "WebSocket is not connected. Exiting...");
            return;
        }
        if (length == Constants.UseArrayLengthForSend)
        {
            length = data.Length;
        }
        Log.Verbose("SendMessageImmediately", "Sending text message immediately...");
        await SendOnConnection(owned, new ArraySegment<byte>(data, 0, length), WebSocketMessageType.Text,
            cancellationToken?.Token ?? owned.Token).ConfigureAwait(false);
    }
    #endregion

    /// <summary>
    /// Sends one message on the given connection's socket, never on a newer connection, holding
    /// the send mutex. Non-teardown sends do nothing once that connection is stopping or released;
    /// Close paths set <paramref name="allowStopping"/> so they can finish teardown.
    /// </summary>
    internal virtual async Task SendOnConnection(ConnectionState owned, ArraySegment<byte> data, WebSocketMessageType messageType, CancellationToken token, bool allowStopping = false)
    {
        if (owned.IsReleased || (owned.IsStopping && !allowStopping))
        {
            Log.Debug("SendOnConnection", "The connection was released. Exiting...");
            return;
        }

        try
        {
            await _mutexSend.WaitAsync(token).ConfigureAwait(false);
            try
            {
                if (owned.IsReleased || (owned.IsStopping && !allowStopping))
                {
                    Log.Debug("SendOnConnection", "The connection is stopping or released. Exiting...");
                    return;
                }
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
        catch (OperationCanceledException ex) when (owned.IsReleased)
        {
            // A concurrent release cancelled the connection's token while this send waited or
            // ran. The connection is gone; nothing to send.
            Log.Debug("SendOnConnection", "The connection was released while sending. Exiting...");
            Log.Verbose("SendOnConnection", $"OperationCanceledException: {ex}");
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
            while (await _sendChannel.Reader.WaitToReadAsync(token).ConfigureAwait(false))
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
                    // An SDK-internal message that belongs to another connection (an autoflush Flush
                    // or Finalize marker from a connection that has ended) is dropped, never sent on
                    // or reported as reached by this one (#455). Checked before the marker branch.
                    if (message.Owner is { } messageOwner && !ReferenceEquals(messageOwner, owned))
                    {
                        Log.Debug("ProcessSendQueue", "Dropping a message that belongs to another connection.");
                        message.FlushSignal?.TrySetResult(false);
                        continue;
                    }

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
                    await SendOnConnection(owned, message.Message, message.MessageType, token).ConfigureAwait(false);
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
                    await StopConnection(owned).ConfigureAwait(false);
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
                    await StopConnection(owned).ConfigureAwait(false);
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

        // The socket stopped being open without a close frame (for example, the TCP connection
        // dropped and the socket aborted). Tear the connection down like a server close, so
        // Close fires and the next Connect() opens a new connection instead of hitting the
        // "already initialized" guard. A no-op when a teardown already released it.
        if (!owned.IsReleased)
        {
            await TearDownDroppedConnection(owned).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Stops a connection whose socket dropped, from the receive loop. Nothing awaits the receive
    /// loop, and StopConnection always releases the connection (and raises Close) before it
    /// rethrows, so a failure here only needs to be logged.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Runs on the fire-and-forget receive task; nothing observes its exceptions.")]
    private async Task TearDownDroppedConnection(ConnectionState owned)
    {
        try
        {
            Log.Information("ProcessReceiveQueue", "WebSocket is no longer open. Tearing down the connection...");
            await StopConnection(owned).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogReceiveLoopException("teardown", ex);
        }
    }

    /// <summary>
    /// Runs <see cref="ProcessReceiveQueue"/> for the fire-and-forget receive task. Nothing awaits
    /// that task, so any exception that reaches here (a throwing log provider in a catch block,
    /// say) is logged instead of becoming an unobserved task exception.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Runs on the fire-and-forget receive task; nothing observes its exceptions.")]
    private async Task RunReceiveLoop(ConnectionState owned)
    {
        try
        {
            await ProcessReceiveQueue(owned).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogReceiveLoopException("loop", ex);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Log providers are user code; one that throws must not fault the fire-and-forget receive task.")]
    private static void LogReceiveLoopException(string stage, Exception ex)
    {
        try
        {
            Log.Error("ProcessReceiveQueue", $"Receive {stage} failed: {ex.GetType()} {ex.Message}");
            Log.Verbose("ProcessReceiveQueue", $"Exception: {ex}");
        }
        catch
        {
            // A failing log provider must not fault the fire-and-forget receive task.
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
    /// <remarks>
    /// Stop() returns after the Close event has been delivered, except when it ends a connection
    /// whose Open handlers are still running: then it returns without waiting for them, and Close
    /// fires as soon as they return. If a new Connect() runs before then, that Close can arrive after
    /// the new connection's Open. Dispose() raises no Close.
    /// </remarks>
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

        var result = await StopConnection(owned, cancelToken, nullByte).ConfigureAwait(false);
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
        // From here on a Connect() still opening this connection fails, and a concurrent Connect()
        // waits for the release instead of reporting this connection as usable.
        owned.MarkStopping();

        try
        {
            return await StopConnectionCore(owned, cancelToken, nullByte).ConfigureAwait(false);
        }
        finally
        {
            // A stopping connection is always released (I6), whatever happened above. Log providers
            // are user code and can throw from any line, a catch block's included, so the release
            // cannot depend on reaching one. Release is idempotent: if the work above (or another
            // teardown) already released the connection, this does nothing.
            if (!owned.IsReleased)
            {
                await ReleaseAndRaiseClose(owned).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// The work of <see cref="StopConnection"/>, run after the connection is marked stopping.
    /// </summary>
    private async Task<bool> StopConnectionCore(ConnectionState owned, CancellationTokenSource? cancelToken, bool nullByte)
    {
        Log.Verbose("AbstractWebSocketClient.StopConnection", "ENTER");

        try
        {
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

            // If the connection is open, send a close message on it. A connection that never raised
            // Open (stopped while connecting) has nothing to drain and gets no close message, which
            // would otherwise precede the Voice Agent's Settings message.
            var wasOpen = owned.IsOpened && owned.IsOpen;
            if (wasOpen)
            {
                Log.Debug("Stop", "Sending Close message...");
                await SendClose(owned, nullByte, cancelToken).ConfigureAwait(false);
            }

            // Another teardown may have released this connection while SendClose ran.
            if (owned.IsReleased)
            {
                await owned.ReleaseCompleted.ConfigureAwait(false);
                Log.Debug("Stop", "Connection was released by another teardown");
                Log.Verbose("AbstractWebSocketClient.StopConnection", "LEAVE");
                return true;
            }

            // Small delay to wait for any final transcription. A socket that was not open on entry
            // (server close, dropped connection, still handshaking) has nothing more to deliver,
            // so release it at once rather than keep a dead socket published.
            if (wasOpen)
            {
                await Task.Delay(100, cancelToken.Token).ConfigureAwait(false);
            }

            if (owned.IsReleased)
            {
                await owned.ReleaseCompleted.ConfigureAwait(false);
                Log.Debug("Stop", "Connection was released by another teardown");
                Log.Verbose("AbstractWebSocketClient.StopConnection", "LEAVE");
                return true;
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
            // observe cancellation and exit cleanly, compare-before-clear so a newer connection's
            // fields are left alone, and dispose the socket. Then raise Close, once. See #390, #455.
            Log.Debug("Stop", "Releasing the connection...");
            await ReleaseAndRaiseClose(owned).ConfigureAwait(false);

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

            await ReleaseAndRaiseClose(owned).ConfigureAwait(false);
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

            await ReleaseAndRaiseClose(owned).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error("Stop", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Stop", $"Exception: {ex}");
            Log.Verbose("AbstractWebSocketClient.StopConnection", "LEAVE");

            await ReleaseAndRaiseClose(owned).ConfigureAwait(false);
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
            // Each handler is isolated: Parallel.ForEach stops starting new iterations once one
            // throws, so an unguarded throwing handler could keep later handlers from receiving
            // the event at all. InvokeIsolated never throws, so the loop has nothing to catch.
            Parallel.ForEach(
                eventHandler.GetInvocationList().Cast<EventHandler<T>>(),
                (handler) => InvokeIsolated(handler, e, "InvokeParallel", typeof(T).Name));
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
        }

        _sendChannel.Writer.TryComplete(); // TryComplete: idempotent under concurrent Dispose() (#390 R3)

        GC.SuppressFinalize(this);
    }
    #endregion
}
