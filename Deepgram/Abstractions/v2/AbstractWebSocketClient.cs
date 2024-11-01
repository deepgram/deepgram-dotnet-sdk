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

        Log.Debug("AbstractWebSocketClient", $"APIVersion: {options.APIVersion}");
        Log.Debug("AbstractWebSocketClient", $"BaseAddress: {options.BaseAddress}");
        Log.Debug("AbstractWebSocketClient", $"OnPrem: {options.OnPrem}");
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
        Log.Debug("Connect", $"headers: {headers}");

        // check if the client is disposed
        if (_clientWebSocket != null)
        {
            // client has already connected
            var exStr = "Client has already been initialized";
            Log.Error("Connect", exStr);
            Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

            return true;
        }

        if (cancelToken == null)
        {
            Log.Information("Connect", "Using default connect cancellation token");
            cancelToken = new CancellationTokenSource(Constants.DefaultConnectTimeout);
        }

        // create client
        _clientWebSocket = new ClientWebSocket();

        // set headers
        _clientWebSocket.Options.SetRequestHeader("Authorization", $"token {_deepgramClientOptions.ApiKey}");
        if (_deepgramClientOptions.Headers is not null) {
            foreach (var header in _deepgramClientOptions.Headers)
            {
                var tmp = header.Key.ToLower();
                if (!(tmp.Contains("password") || tmp.Contains("token") || tmp.Contains("authorization") || tmp.Contains("auth")))
                {
                    Log.Debug("PutAsync<S, T>", $"Add Header {header.Key}={header.Value}");
                }
                _clientWebSocket.Options.SetRequestHeader(header.Key, header.Value);
            }
        }
        if (headers is not null)
        {
            foreach (var header in headers)
            {
                var tmp = header.Key.ToLower();
                if (!(tmp.Contains("password") || tmp.Contains("token") || tmp.Contains("authorization") || tmp.Contains("auth")))
                {
                    Log.Debug("PutAsync<S, T>", $"Add Header {header.Key}={header.Value}");
                }
                _clientWebSocket.Options.SetRequestHeader(header.Key, header.Value);
            }
        }

        // internal cancellation token for internal threads
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var myUri = new Uri(uri);
            Log.Debug("Connect", $"uri: {uri}");

            Log.Debug("Connect", "Connecting to Deepgram API...");
            await _clientWebSocket.ConnectAsync(myUri, cancelToken.Token).ConfigureAwait(false);

            if (!IsConnected())
            {
                Log.Error("Connect", "Failed to connect to Deepgram API");
                Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");

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

            return false;
        }
        catch (Exception ex)
        {
            Log.Error("Connect", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Connect", $"Exception: {ex}");
            Log.Verbose("AbstractWebSocketClient.Connect", "LEAVE");
            throw;
        }

        void StartSenderBackgroundThread() => Task.Run(() => ProcessSendQueue());

        void StartReceiverBackgroundThread() => Task.Run(() => ProcessReceiveQueue());
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
    public virtual Task SendClose(bool nullByte = false)
    {
        throw new DeepgramException("Unimplemented");
    }

    /// <summary>
    /// Sends a binary message over the WebSocket connection.
    /// </summary>
    /// <param name="data">The data to be sent over the WebSocket.</param>
    public virtual void Send(byte[] data, int length = Constants.UseArrayLengthForSend) => SendBinary(data, length);

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
    /// </summary>
    public virtual async Task SendBinaryImmediately(byte[] data, int length = Constants.UseArrayLengthForSend)
    {
        if (!IsConnected())
        {
            Log.Debug("SendBinaryImmediately", "WebSocket is not connected. Exiting...");
            return;
        }

        await _mutexSend.WaitAsync(_cancellationTokenSource.Token);
        try
        {
            Log.Verbose("SendBinaryImmediately", "Sending binary message immediately...");
            if (length == Constants.UseArrayLengthForSend)
            {
                length = data.Length;
            }
            await _clientWebSocket.SendAsync(new ArraySegment<byte>(data, 0, length), WebSocketMessageType.Binary, true, _cancellationTokenSource.Token)
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
    public virtual async Task SendMessageImmediately(byte[] data, int length = Constants.UseArrayLengthForSend)
    {
        if (!IsConnected())
        {
            Log.Debug("SendBinaryImmediately", "WebSocket is not connected. Exiting...");
            return;
        }

        await _mutexSend.WaitAsync(_cancellationTokenSource.Token);
        try
        {
            Log.Verbose("SendMessageImmediately", "Sending text message immediately...");
            if (length == Constants.UseArrayLengthForSend)
            {
                length = data.Length;
            }
            await _clientWebSocket.SendAsync(new ArraySegment<byte>(data, 0, length), WebSocketMessageType.Text, true, _cancellationTokenSource.Token)
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
            while (await _sendChannel.Reader.WaitToReadAsync(_cancellationTokenSource.Token))
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Log.Information("ProcessSendQueue", "ProcessSendQueue cancelled");
                    break;
                }
                if (!IsConnected())
                {
                    Log.Debug("ProcessSendQueue", "WebSocket is not connected. Exiting...");
                    break;
                }

                Log.Verbose("ProcessSendQueue", "Reading message off queue...");
                while (_sendChannel.Reader.TryRead(out var message))
                {
                    // TODO: Add logging for message capturing for possible playback
                    Log.Verbose("ProcessSendQueue", "Sending message...");
                    await _mutexSend.WaitAsync(_cancellationTokenSource.Token);
                    try
                    {
                        await _clientWebSocket.SendAsync(message.Message, message.MessageType, true, _cancellationTokenSource.Token)
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
    }

    internal async Task ProcessReceiveQueue()
    {
        Log.Verbose("AbstractWebSocketClient.ProcessReceiveQueue", "ENTER");

        while (_clientWebSocket?.State == WebSocketState.Open)
        {
            try
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Log.Information("ProcessReceiveQueue", "ReceiveThread cancelled");
                    await Stop();
                    Log.Verbose("ProcessReceiveQueue", "LEAVE");
                    return;
                }
                if (!IsConnected())
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
                        result = await _clientWebSocket.ReceiveAsync(buffer, _cancellationTokenSource.Token);

                        ms.Write(
                            buffer.Array ?? throw new InvalidOperationException("buffer cannot be null"),
                            buffer.Offset,
                            result.Count
                            );
                    } while (!result.EndOfMessage);

                    if (result.MessageType != WebSocketMessageType.Close)
                    {
                        Log.Verbose("ProcessReceiveQueue", $"Received message: {result} / {ms}");
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
            Log.Verbose("ProcessTextMessage", $"raw response: {response}");
            var data = JsonDocument.Parse(response);
            var val = Enum.Parse(typeof(WebSocketType), data.RootElement.GetProperty("type").GetString()!);

            Log.Verbose("ProcessTextMessage", $"Type: {val}");

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

                    Log.Debug("ProcessTextMessage", $"Invoking OpenResponse. event: {openResponse}");
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

                    Log.Debug("ProcessTextMessage", $"Invoking ErrorResponse. event: {errorResponse}");
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

                    Log.Debug("ProcessTextMessage", $"Invoking UnhandledResponse. event: {unhandledResponse}");
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

        // client is already disposed
        if (_clientWebSocket == null)
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
            if (_clientWebSocket!.State == WebSocketState.Open)
            {
                Log.Debug("Stop", "Sending Close message...");
                await SendClose(nullByte);
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

            // attempt to stop the connection
            if (_clientWebSocket!.State != WebSocketState.Closed && _clientWebSocket!.State != WebSocketState.Aborted)
            {
                Log.Debug("Stop", "Closing WebSocket connection...");
                await _clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancelToken.Token)
                    .ConfigureAwait(false);
            }

            // clean up internal token
            if (_cancellationTokenSource != null)
            {
                Log.Debug("Stop", "Disposing internal token...");
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            // release the socket
            Log.Debug("Stop", "Disposing WebSocket socket...");
            _clientWebSocket = null;

            Log.Debug("Stop", "Succeeded");
            Log.Verbose("AbstractWebSocketClient.Stop", "LEAVE");

            return true;
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("Stop", "Stop cancelled.");
            Log.Verbose("Stop", $"Stop cancelled. Info: {ex}");
            Log.Verbose("AbstractWebSocketClient.Stop", "LEAVE");

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
    /// Retrieves the connection state of the WebSocket
    /// </summary>
    /// <returns>Returns the connection state of the WebSocket</returns>
    public WebSocketState State()
    {
        if (_clientWebSocket == null)
        {
            return WebSocketState.None;
        }
        Log.Debug("State", $"WebSocket State: {_clientWebSocket.State}");
        return _clientWebSocket.State;
    }

    /// <summary>
    /// Indicates whether the WebSocket is connected
    /// </summary> 
    /// <returns>Returns true if the WebSocket is connected</returns>
    public bool IsConnected() {
        if (_clientWebSocket == null)
        {
            return false;
        }

        Log.Debug("State", $"WebSocket State: {_clientWebSocket.State}");
        return _clientWebSocket.State == WebSocketState.Open;
    }

    /// <summary>
    /// Handle channel options
    /// </summary> 
    internal readonly Channel<WebSocketMessage> _sendChannel = System.Threading.Channels.Channel
       .CreateUnbounded<WebSocketMessage>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true, });

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
          
        if (_cancellationTokenSource != null)
        {
            if (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        if (_sendChannel != null)
        {
            _sendChannel.Writer.Complete();
        }

        if (_clientWebSocket != null)
        {
            _clientWebSocket.Dispose();
            _clientWebSocket = null;
        }

        GC.SuppressFinalize(this);
    }
    #endregion
}
