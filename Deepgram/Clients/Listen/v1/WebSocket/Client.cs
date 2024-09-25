// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT


using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Listen.v1.WebSocket;
using Deepgram.Clients.Interfaces.v1;

namespace Deepgram.Clients.Listen.v1.WebSocket;

/// <summary>
/// Implements version 1 of the Live Client.
/// </summary>
public class Client : IDisposable, IListenWebSocketClient
{
    #region Fields
    private readonly IDeepgramClientOptions _deepgramClientOptions;

    private ClientWebSocket? _clientWebSocket;
    private CancellationTokenSource? _cancellationTokenSource;

    private DateTime? _lastReceived = null;

    private readonly SemaphoreSlim _mutexSubscribe = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _mutexSend = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _mutexLastDatagram = new SemaphoreSlim(1, 1);
    #endregion

    /// <param name="apiKey">Required DeepgramApiKey</param>
    /// <param name="deepgramClientOptions"><see cref="IDeepgramClientOptions"/> for HttpClient Configuration</param>
    public Client(string? apiKey = null, IDeepgramClientOptions? options = null)
    {
        Log.Verbose("LiveClient", "ENTER");

        options ??= new DeepgramWsClientOptions(apiKey);
        _deepgramClientOptions = options;

        Log.Debug("LiveClient", $"APIVersion: {options.APIVersion}");
        Log.Debug("LiveClient", $"BaseAddress: {options.BaseAddress}");
        Log.Debug("LiveClient", $"KeepAlive: {options.KeepAlive}");
        Log.Debug("LiveClient", $"OnPrem: {options.OnPrem}");
        Log.Debug("LiveClient", $"Autoflush: {options.AutoFlushReplyDelta}");
        Log.Verbose("LiveClient", "LEAVE");
    }

    #region Event Handlers
    /// <summary>
    /// Fires when an event is received from the Deepgram API
    /// </summary>
    private event EventHandler<OpenResponse>? _openReceived;
    private event EventHandler<MetadataResponse>? _metadataReceived;
    private event EventHandler<ResultResponse>? _resultsReceived;
    private event EventHandler<UtteranceEndResponse>? _utteranceEndReceived;
    private event EventHandler<SpeechStartedResponse>? _speechStartedReceived;
    private event EventHandler<CloseResponse>? _closeReceived;
    private event EventHandler<UnhandledResponse>? _unhandledReceived;
    private event EventHandler<ErrorResponse>? _errorReceived;
    #endregion

    /// <summary>
    /// Connect to a Deepgram API Web Socket to begin transcribing audio
    /// </summary>
    /// <param name="options">Options to use when transcribing audio</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task Connect(LiveSchema options, CancellationTokenSource? cancelToken = null, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null)
    {
        Log.Verbose("LiveClient.Connect", "ENTER");
        Log.Information("Connect", $"options:\n{JsonSerializer.Serialize(options, JsonSerializeOptions.DefaultOptions)}");
        Log.Debug("Connect", $"addons: {addons}");

        // check if the client is disposed
        if (_clientWebSocket != null)
        {
            // client has already connected
            var exStr = "Client has already been initialized";
            Log.Error("Connect", exStr);
            Log.Verbose("LiveClient.Connect", "LEAVE");
            throw new InvalidOperationException(exStr);
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

        // internal cancelation token for internal threads
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var _uri = GetUri(_deepgramClientOptions, options, addons);
            Log.Debug("Connect", $"uri: {_uri}");

            Log.Debug("Connect", "Connecting to Deepgram API...");
            await _clientWebSocket.ConnectAsync(_uri, cancelToken.Token).ConfigureAwait(false);

            Log.Debug("Connect", "Starting Sender Thread...");
            StartSenderBackgroundThread();

            Log.Debug("Connect", "Starting Receiver Thread...");
            StartReceiverBackgroundThread();

            if (_deepgramClientOptions.KeepAlive)
            {
                Log.Debug("Connect", "Starting KeepAlive Thread...");
                StartKeepAliveBackgroundThread();
            }

            if (_deepgramClientOptions.AutoFlushReplyDelta > 0)
            {
                Log.Debug("Connect", "Starting AutoFlush Thread...");
                StartAutoFlushBackgroundThread();
            }

            // send a OpenResponse event
            if (_openReceived != null)
            {
                Log.Debug("Connect", "Sending OpenResponse event...");
                var data = new OpenResponse();
                data.Type = ListenType.Open;
                _openReceived.Invoke(null, data);
            }

            Log.Debug("Connect", "Connect Succeeded");
            Log.Verbose("LiveClient.Connect", "LEAVE");
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("Connect", "Connect cancelled.");
            Log.Verbose("Connect", $"Connect cancelled. Info: {ex}");
            Log.Verbose("LiveClient.Connect", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("Connect", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Connect", $"Excepton: {ex}");
            Log.Verbose("LiveClient.Connect", "LEAVE");
            throw;
        }

        void StartSenderBackgroundThread() => _ = Task.Factory.StartNew(
            _ => ProcessSendQueue(),
                TaskCreationOptions.LongRunning);

        void StartReceiverBackgroundThread() => _ = Task.Factory.StartNew(
                _ => ProcessReceiveQueue(),
                TaskCreationOptions.LongRunning);

        void StartKeepAliveBackgroundThread() => _ = Task.Factory.StartNew(
                _ => ProcessKeepAlive(),
                TaskCreationOptions.LongRunning);

        void StartAutoFlushBackgroundThread() => _ = Task.Factory.StartNew(
                _ => ProcessAutoFlush(),
                TaskCreationOptions.LongRunning);
    }

    #region Subscribe Event
    /// <summary>
    /// Subscribe to an Open event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<OpenResponse> eventHandler)
    {
        lock (_mutexSubscribe)
        {
            _openReceived += (sender, e) => eventHandler(sender, e);
        }

        return true;
    }

    /// <summary>
    /// Subscribe to a Metadata event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<MetadataResponse> eventHandler)
    {
        lock (_mutexSubscribe)
        {
            _metadataReceived += (sender, e) => eventHandler(sender, e);
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a Results event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<ResultResponse> eventHandler)
    {
        lock (_mutexSubscribe)
        {
            _resultsReceived += (sender, e) => eventHandler(sender, e);
        }
        return true;
    }

    /// <summary>
    /// Subscribe to an UtteranceEnd event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<UtteranceEndResponse> eventHandler)
    {
        lock (_mutexSubscribe)
        {
            _utteranceEndReceived += (sender, e) => eventHandler(sender, e);
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a SpeechStarted event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<SpeechStartedResponse> eventHandler)
    {
        _speechStartedReceived += (sender, e) => eventHandler(sender, e);
        return true;
    }

    /// <summary>
    /// Subscribe to a Close event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<CloseResponse> eventHandler)
    {
        lock (_mutexSubscribe)
        {
            _closeReceived += (sender, e) => eventHandler(sender, e);
        }
        return true;
    }

    /// <summary>
    /// Subscribe to an Unhandled event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<UnhandledResponse> eventHandler)
    {
        lock (_mutexSubscribe)
        {
            _unhandledReceived += (sender, e) => eventHandler(sender, e);
        }
        return true;
    }

    /// <summary>
    /// Subscribe to an Error event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<ErrorResponse> eventHandler)
    {
        lock (_mutexSubscribe)
        {
            _errorReceived += (sender, e) => eventHandler(sender, e);
        }
        return true;
    }
    #endregion

    #region Send Functions
    /// <summary>
    /// Sends a KeepAlive message to Deepgram
    /// </summary>
    public void SendKeepAlive()
    {
        Log.Debug("SendKeepAlive", "Sending KeepAlive Message Immediately...");
        byte[] data = Encoding.ASCII.GetBytes("{\"type\": \"KeepAlive\"}");
        SendMessageImmediately(data);
    }

    /// <summary>
    /// Sends a Finalize message to Deepgram
    /// </summary>
    public void SendFinalize()
    {
        Log.Debug("SendFinalize", "Sending Finalize Message Immediately...");
        byte[] data = Encoding.ASCII.GetBytes("{\"type\": \"Finalize\"}");
        SendMessageImmediately(data);
    }

    /// <summary>
    /// Sends a Close message to Deepgram
    /// </summary>
    public void SendClose(bool nullByte = false)
    {
        Log.Debug("SendFinalize", "Sending Close Message Immediately...");
        if (nullByte && _clientWebSocket != null)
        {
            // send a close to Deepgram
            lock (_mutexSend)
            {
                _clientWebSocket.SendAsync(new ArraySegment<byte>([0]), WebSocketMessageType.Binary, true, _cancellationTokenSource.Token)
                    .ConfigureAwait(false);
            }
            return;
        }

        byte[] data = Encoding.ASCII.GetBytes("{\"type\": \"CloseStream\"}");
        SendMessageImmediately(data);
    }

    /// <summary>
    /// Sends a binary message over the WebSocket connection.
    /// </summary>
    /// <param name="data">The data to be sent over the WebSocket.</param>
    public void Send(byte[] data, int length = Constants.UseArrayLengthForSend) => SendBinary(data, length);

    /// <summary>
    /// This method sends a binary message over the WebSocket connection.
    /// </summary>
    /// <param name="data"></param>
    public void SendBinary(byte[] data, int length = Constants.UseArrayLengthForSend) =>
        EnqueueSendMessage(new WebSocketMessage(data, WebSocketMessageType.Binary, length));

    /// <summary>
    /// This method sends a text message over the WebSocket connection.
    /// </summary>
    public void SendMessage(byte[] data, int length = Constants.UseArrayLengthForSend) =>
        EnqueueSendMessage(new WebSocketMessage(data, WebSocketMessageType.Text, length));

    /// <summary>
    /// This method sends a binary message over the WebSocket connection immediately without queueing.
    /// </summary>
    public void SendBinaryImmediately(byte[] data, int length = Constants.UseArrayLengthForSend)
    {
        lock (_mutexSend)
        {
            Log.Verbose("SendBinaryImmediately", "Sending binary message immediately..");  // TODO: dump this message
            if (length == Constants.UseArrayLengthForSend)
            {
                length = data.Length;
            }
            _clientWebSocket.SendAsync(new ArraySegment<byte>(data, 0, length), WebSocketMessageType.Binary, true, _cancellationTokenSource.Token)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// This method sends a text message over the WebSocket connection immediately without queueing.
    /// </summary>
    public void SendMessageImmediately(byte[] data, int length = Constants.UseArrayLengthForSend)
    {
        lock (_mutexSend)
        {
            Log.Verbose("SendBinaryImmediately", "Sending binary message immediately..");  // TODO: dump this message
            if (length == Constants.UseArrayLengthForSend)
            {
                length = data.Length;
            }
            _clientWebSocket.SendAsync(new ArraySegment<byte>(data, 0, length), WebSocketMessageType.Text, true, _cancellationTokenSource.Token)
                .ConfigureAwait(false);
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
            Log.Verbose("EnqueueSendMessage", $"Excepton: {ex}");
        }
    }

    internal async Task ProcessSendQueue()
    {
        Log.Verbose("LiveClient.ProcessSendQueue", "ENTER");

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

                Log.Verbose("ProcessSendQueue", "Reading message of queue...");
                while (_sendChannel.Reader.TryRead(out var message))
                {
                    // TODO: Add logging for message capturing for possible playback
                    Log.Verbose("ProcessSendQueue", "Sending message...");
                    lock (_mutexSend)
                    {
                        _clientWebSocket.SendAsync(message.Message, message.MessageType, true, _cancellationTokenSource.Token)
                            .ConfigureAwait(false);
                    }
                }
            }

            Log.Verbose("ProcessSendQueue", "Exit");
            Log.Verbose("LiveClient.ProcessSendQueue", "LEAVE");
        }
        catch (OperationCanceledException ex)
        {
            Log.Debug("ProcessSendQueue", "SendThread cancelled.");
            Log.Verbose("ProcessSendQueue", $"SendThread cancelled. Info: {ex}");
            Log.Verbose("LiveClient.ProcessSendQueue", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessSendQueue", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessSendQueue", $"Excepton: {ex}");
            Log.Verbose("LiveClient.ProcessSendQueue", "LEAVE");
        }
    }

    internal async void ProcessKeepAlive()
    {
        Log.Verbose("LiveClient.ProcessKeepAlive", "ENTER");

        try
        {
            while (true)
            {
                Log.Verbose("ProcessKeepAlive", "Waiting for KeepAlive...");
                await Task.Delay(5000, _cancellationTokenSource.Token);

                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Log.Information("ProcessKeepAlive", "KeepAliveThread cancelled");
                    break;
                }

                SendKeepAlive();
            }

            Log.Verbose("ProcessKeepAlive", "Exit");
            Log.Verbose("LiveClient.ProcessKeepAlive", "LEAVE");
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("ProcessKeepAlive", "KeepAliveThread cancelled.");
            Log.Verbose("ProcessKeepAlive", $"KeepAliveThread cancelled. Info: {ex}");
            Log.Verbose("LiveClient.ProcessKeepAlive", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessKeepAlive", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessKeepAlive", $"Excepton: {ex}");
            Log.Verbose("LiveClient.ProcessKeepAlive", "LEAVE");
        }
    }


    internal async void ProcessAutoFlush()
    {
        Log.Verbose("LiveClient.ProcessAutoFlush", "ENTER");

        var diffTicks = TimeSpan.FromMilliseconds((double)_deepgramClientOptions.AutoFlushReplyDelta);

        try
        {
            while (true)
            {
                Log.Verbose("ProcessAutoFlush", "Waiting for AutoFlush...");
                await Task.Delay(Constants.DefaultFlushPeriodInMs, _cancellationTokenSource.Token);

                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Log.Information("ProcessAutoFlush", "ProcessAutoFlush cancelled");
                    break;
                }

                lock (_mutexLastDatagram)
                {
                    if (_lastReceived == null)
                    {
                        Log.Debug("ProcessAutoFlush", "No datagram received. Skipping...");
                        continue;
                    }

                    var deltaTicks = DateTime.Now - _lastReceived;
                    if (deltaTicks < diffTicks)
                    {
                        Log.Debug("ProcessAutoFlush", $"AutoFlush delta is less than threshold: {deltaTicks}. Skipping...");
                        continue;
                    }

                    SendFinalize();
                    _lastReceived = null;
                }
            }

            Log.Verbose("ProcessAutoFlush", "Exit");
            Log.Verbose("LiveClient.ProcessAutoFlush", "LEAVE");
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("ProcessAutoFlush", "KeepAliveThread cancelled.");
            Log.Verbose("ProcessAutoFlush", $"KeepAliveThread cancelled. Info: {ex}");
            Log.Verbose("LiveClient.ProcessAutoFlush", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessAutoFlush", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessAutoFlush", $"Excepton: {ex}");
            Log.Verbose("LiveClient.ProcessAutoFlush", "LEAVE");
        }
    }

    internal async Task ProcessReceiveQueue()
    {
        Log.Verbose("LiveClient.ProcessReceiveQueue", "ENTER");

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
                Log.Verbose("LiveClient.ProcessReceiveQueue", "LEAVE");
            }
            catch (Exception ex)
            {
                Log.Error("ProcessReceiveQueue", $"{ex.GetType()} thrown {ex.Message}");
                Log.Verbose("ProcessReceiveQueue", $"Excepton: {ex}");
                Log.Verbose("LiveClient.ProcessReceiveQueue", "LEAVE");
            }
        }
    }

    internal void ProcessDataReceived(WebSocketReceiveResult result, MemoryStream ms)
    {
        Log.Verbose("LiveClient.ProcessDataReceived", "ENTER");

        ms.Seek(0, SeekOrigin.Begin);

        if (result.MessageType != WebSocketMessageType.Text)
        {
            Log.Warning("ProcessDataReceived", "Received a text message. This is not supported.");
            Log.Verbose("LiveClient.ProcessDataReceived", "LEAVE");
            return;
        }

        var response = Encoding.UTF8.GetString(ms.ToArray());
        if (response == null)
        {
            Log.Warning("ProcessDataReceived", "Response is null");
            Log.Verbose("LiveClient.ProcessDataReceived", "LEAVE");
            return;
        }

        try
        {
            Log.Verbose("ProcessDataReceived", $"raw response: {response}");
            var data = JsonDocument.Parse(response);
            var val = Enum.Parse(typeof(ListenType), data.RootElement.GetProperty("type").GetString()!);

            Log.Verbose("ProcessDataReceived", $"Type: {val}");


            if (_deepgramClientOptions.InspectListenMessage())
            {
                Log.Debug("ProcessDataReceived", "Call InspectMessage...");
                InspectMessage(val, data);
            }

            switch (val)
            {
                case ListenType.Open:
                    var openResponse = data.Deserialize<OpenResponse>();
                    if (_openReceived == null)
                    {
                        Log.Debug("ProcessDataReceived", "_openReceived has no listeners");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }
                    if (openResponse == null)
                    {
                        Log.Warning("ProcessDataReceived", "OpenResponse is invalid");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessDataReceived", $"Invoking OpenResponse. event: {openResponse}");
                    InvokeParallel(_openReceived, openResponse);
                    break;
                case ListenType.Results:
                    var resultResponse = data.Deserialize<ResultResponse>();
                    if (_resultsReceived == null)
                    {
                        Log.Debug("ProcessDataReceived", "_resultsReceived has no listeners");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }
                    if (resultResponse == null)
                    {
                        Log.Warning("ProcessDataReceived", "ResultResponse is invalid");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessDataReceived", $"Invoking ResultsResponse. event: {resultResponse}");
                    InvokeParallel(_resultsReceived, resultResponse);
                    break;
                case ListenType.Metadata:
                    var metadataResponse = data.Deserialize<MetadataResponse>();
                    if (_metadataReceived == null)
                    {
                        Log.Debug("ProcessDataReceived", "_metadataReceived has no listeners");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }
                    if (metadataResponse == null)
                    {
                        Log.Warning("ProcessDataReceived", "MetadataResponse is invalid");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessDataReceived", $"Invoking MetadataResponse. event: {metadataResponse}");
                    InvokeParallel(_metadataReceived, metadataResponse);
                    break;
                case ListenType.UtteranceEnd:
                    var utteranceEndResponse = data.Deserialize<UtteranceEndResponse>();
                    if (_utteranceEndReceived == null)
                    {
                        Log.Debug("ProcessDataReceived", "_utteranceEndReceived has no listeners");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }
                    if (utteranceEndResponse == null)
                    {
                        Log.Warning("ProcessDataReceived", "UtteranceEndResponse is invalid");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessDataReceived", $"Invoking UtteranceEndResponse. event: {utteranceEndResponse}");
                    InvokeParallel(_utteranceEndReceived, utteranceEndResponse);
                    break;
                case ListenType.SpeechStarted:
                    var speechStartedResponse = data.Deserialize<SpeechStartedResponse>();
                    if (_speechStartedReceived == null)
                    {
                        Log.Debug("ProcessDataReceived", "_speechStartedReceived has no listeners");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }
                    if (speechStartedResponse == null)
                    {
                        Log.Warning("ProcessDataReceived", "SpeechStartedResponse is invalid");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessDataReceived", $"Invoking SpeechStartedResponse. event: {speechStartedResponse}");
                    InvokeParallel(_speechStartedReceived, speechStartedResponse);
                    break;
                case ListenType.Close:
                    var closeResponse = data.Deserialize<CloseResponse>();
                    if (_closeReceived == null)
                    {
                        Log.Debug("ProcessDataReceived", "_closeReceived has no listeners");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }
                    if (closeResponse == null)
                    {
                        Log.Warning("ProcessDataReceived", "CloseResponse is invalid");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessDataReceived", $"Invoking CloseResponse. event: {closeResponse}");
                    InvokeParallel(_closeReceived, closeResponse);
                    break;
                case ListenType.Error:
                    var errorResponse = data.Deserialize<ErrorResponse>();
                    if (_errorReceived == null)
                    {
                        Log.Debug("ProcessDataReceived", "_errorReceived has no listeners");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }
                    if (errorResponse == null)
                    {
                        Log.Warning("ProcessDataReceived", "ErrorResponse is invalid");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessDataReceived", $"Invoking ErrorResponse. event: {errorResponse}");
                    InvokeParallel(_errorReceived, errorResponse);
                    break;
                default:
                    if (_unhandledReceived == null)
                    {
                        Log.Debug("ProcessDataReceived", "_unhandledReceived has no listeners");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }

                    var unhandledResponse = new UnhandledResponse();
                    unhandledResponse.Type = ListenType.Unhandled;
                    unhandledResponse.Raw = response;

                    Log.Debug("ProcessDataReceived", $"Invoking UnhandledResponse. event: {unhandledResponse}");
                    InvokeParallel(_unhandledReceived, unhandledResponse);
                    break;
            }

            Log.Debug("ProcessDataReceived", "Succeeded");
            Log.Verbose("LiveClient.ProcessDataReceived", "LEAVE");
        }
        catch (JsonException ex)
        {
            Log.Error("ProcessDataReceived", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessDataReceived", $"Excepton: {ex}");
            Log.Verbose("LiveClient.ProcessDataReceived", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessDataReceived", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessDataReceived", $"Excepton: {ex}");
            Log.Verbose("LiveClient.ProcessDataReceived", "LEAVE");
        }
    }

    /// <summary>
    /// Closes the Web Socket connection to the Deepgram API
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task Stop(CancellationTokenSource? cancelToken = null)
    {
        Log.Verbose("LiveClient.Stop", "ENTER");

        // client is already disposed
        if (_clientWebSocket == null)
        {
            Log.Information("Stop", "Client has already been disposed");
            Log.Verbose("LiveClient.Stop", "LEAVE");
            return;
        }

        if (cancelToken == null)
        {
            Log.Information("Stop", "Using default disconnect cancellation token");
            cancelToken = new CancellationTokenSource(Constants.DefaultDisconnectTimeout);
        }

        try
        {
            // cancel the internal token to stop all threads
            if (_cancellationTokenSource != null)
            {
                Log.Debug("Stop", "Cancelling native token...");
                _cancellationTokenSource.Cancel();
            }

            // if websocket is open, send a close message
            if (_clientWebSocket!.State == WebSocketState.Open)
            {
                Log.Debug("Stop", "Sending Close message...");
                // send a close to Deepgram
                lock (_mutexSend)
                {
                    _clientWebSocket.SendAsync(new ArraySegment<byte>([0]), WebSocketMessageType.Binary, true, cancelToken.Token)
                        .ConfigureAwait(false);
                }
            }

            // send a CloseResponse event
            if (_closeReceived != null)
            {
                Log.Debug("Stop", "Sending CloseResponse event...");
                var data = new CloseResponse();
                data.Type = ListenType.Close;
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
            Log.Verbose("LiveClient.Stop", "LEAVE");
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("Stop", "Stop cancelled.");
            Log.Verbose("Stop", $"Stop cancelled. Info: {ex}");
            Log.Verbose("LiveClient.Stop", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("Stop", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Stop", $"Excepton: {ex}");
            Log.Verbose("LiveClient.Stop", "LEAVE");
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
            
        return _clientWebSocket.State == WebSocketState.Open;
    }

    /// <summary>
    /// Handle channel options
    /// </summary> 
    internal readonly Channel<WebSocketMessage> _sendChannel = System.Threading.Channels.Channel
       .CreateUnbounded<WebSocketMessage>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true, });

    /// <summary>
    /// Get the URI for the WebSocket connection
    /// </summary> 
    internal static Uri GetUri(IDeepgramClientOptions options, LiveSchema parameter, Dictionary<string, string>? addons = null)
    {
        var propertyInfoList = parameter.GetType()
            .GetProperties()
            .Where(v => v.GetValue(parameter) is not null);

        var queryString = QueryParameterUtil.UrlEncode(parameter, propertyInfoList, addons);

        return new Uri($"{options.BaseAddress}/{UriSegments.LISTEN}?{queryString}");
    }

    private void InvokeParallel<T>(EventHandler<T> eventHandler, T e)
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

    private void InspectMessage(object type, JsonDocument data)
    {
        Log.Verbose("InspectMessage", "ENTER");

        try
        {
            switch (type)
            {
                case ListenType.Results:
                    var resultResponse = data.Deserialize<ResultResponse>();
                    if (resultResponse == null)
                    {
                        Log.Warning("InspectMessage", "ResultResponse is invalid");
                        Log.Verbose("InspectMessage", "LEAVE");
                        return;
                    }

                    var sentence = resultResponse.Channel.Alternatives[0].Transcript;

                    if (resultResponse.Channel.Alternatives.Count == 0 || sentence == "") {
                        Log.Verbose("InspectMessage", $"resultResponse has empty message");
                        Log.Verbose("InspectMessage", "LEAVE");
                        return;
                    }

                    if (_deepgramClientOptions.AutoFlushReplyDelta > 0)
                    {
                        if ((bool)resultResponse.IsFinal)
                        {
                            var now = DateTime.Now;
                            Log.Debug("InspectMessage", $"AutoFlush IsFinal received. Time: {now}");
                            lock(_mutexLastDatagram)
                            {
                                _lastReceived = null;
                            }
                        } else {
                            var now = DateTime.Now;
                            Log.Debug("InspectMessage", $"AutoFlush Interim received. Time: {now}");
                            lock (_mutexLastDatagram)
                            {
                                _lastReceived = now;
                            }
                        }
                    }
                    break;
            }

            Log.Debug("InspectMessage", "Succeeded");
            Log.Verbose("InspectMessage", "LEAVE");
        }
        catch (JsonException ex)
        {
            Log.Error("InspectMessage", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("InspectMessage", $"Excepton: {ex}");
            Log.Verbose("LiveClient.InspectMessage", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("InspectMessage", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("InspectMessage", $"Excepton: {ex}");
            Log.Verbose("LiveClient.InspectMessage", "LEAVE");
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
