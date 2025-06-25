﻿// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT


using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Exceptions.v1;
using Deepgram.Models.Speak.v1.WebSocket;
using Deepgram.Clients.Interfaces.v1;

namespace Deepgram.Clients.Speak.v1.WebSocket;

/// <summary>
// *********** WARNING ***********
// Implements version 1 of the Speak WebSocket Client
//
// Deprecated: This class is deprecated. Use the `v2` of the client instead.
// This will be removed in a future release.
//
// This class is frozen and no new functionality will be added.
// *********** WARNING ***********
/// </summary>
[Obsolete("Please use Deepgram.Clients.Speak.v2.WebSocket instead", false)]
public class Client : IDisposable, ISpeakWebSocketClient
{
    #region Fields
    private readonly IDeepgramClientOptions _deepgramClientOptions;

    private ClientWebSocket? _clientWebSocket;
    private CancellationTokenSource? _cancellationTokenSource;

    private DateTime? _lastReceived = null;
    private int _flushCount = 0;

    private readonly SemaphoreSlim _mutexSubscribe = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _mutexSend = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _mutexLastDatagram = new SemaphoreSlim(1, 1);
    #endregion

    /// <param name="apiKey">Required DeepgramApiKey</param>
    /// <param name="deepgramClientOptions"><see cref="IDeepgramClientOptions"/> for HttpClient Configuration</param>
    public Client(string? apiKey = null, IDeepgramClientOptions? options = null)
    {
        Log.Verbose("SpeakWSClient", "ENTER");

        options ??= new DeepgramWsClientOptions(apiKey);
        _deepgramClientOptions = options;

        Log.Debug("SpeakWSClient", $"APIVersion: {options.APIVersion}");
        Log.Debug("SpeakWSClient", $"BaseAddress: {options.BaseAddress}");
        Log.Debug("SpeakWSClient", $"options: {options.OnPrem}");
        Log.Debug("SpeakWSClient", $"Autoflush: {options.AutoFlushSpeakDelta}");
        Log.Verbose("SpeakWSClient", "LEAVE");
    }

    #region Event Handlers
    /// <summary>
    /// Fires when an event is received from the Deepgram API
    /// </summary>
    private event EventHandler<OpenResponse>? _openReceived;
    private event EventHandler<MetadataResponse>? _metadataReceived;
    private event EventHandler<FlushedResponse>? _flushedReceived;
    private event EventHandler<ClearedResponse>? _clearedReceived;
    private event EventHandler<AudioResponse>? _audioReceived;
    private event EventHandler<CloseResponse>? _closeReceived;
    private event EventHandler<UnhandledResponse>? _unhandledReceived;
    private event EventHandler<WarningResponse>? _warningReceived;
    private event EventHandler<ErrorResponse>? _errorReceived;
    #endregion

    /// <summary>
    /// Connect to a Deepgram API Web Socket to begin transcribing audio
    /// </summary>
    /// <param name="options">Options to use when transcribing audio</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task Connect(SpeakSchema options, CancellationTokenSource? cancelToken = null, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null)
    {
        Log.Verbose("SpeakWSClient.Connect", "ENTER");
        Log.Information("Connect", $"options:\n{JsonSerializer.Serialize(options, JsonSerializeOptions.DefaultOptions)}");
        Log.Debug("Connect", $"addons: {addons}");

        // check if the client is disposed
        if (_clientWebSocket != null)
        {
            // client has already connected
            var exStr = "Client has already been initialized";
            Log.Error("Connect", exStr);
            Log.Verbose("SpeakWSClient.Connect", "LEAVE");
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
        SetAuthenticationHeader(_clientWebSocket, _deepgramClientOptions);
        if (_deepgramClientOptions.Headers is not null)
        {
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

            if (!IsConnected())
            {
                Log.Error("Connect", "Failed to connect to Deepgram API");
                Log.Verbose("SpeakWSClient.Connect", "LEAVE");
                throw new DeepgramWebSocketException("Failed to connect to Deepgram API");
            }

            Log.Debug("Connect", "Starting Sender Thread...");
            StartSenderBackgroundThread();

            Log.Debug("Connect", "Starting Receiver Thread...");
            StartReceiverBackgroundThread();

            if (_deepgramClientOptions.AutoFlushSpeakDelta > 0)
            {
                Log.Debug("Connect", "Starting AutoFlush Thread...");
                StartAutoFlushBackgroundThread();
            }

            // send a OpenResponse event
            if (_openReceived != null)
            {
                Log.Debug("Connect", "Sending OpenResponse event...");
                var data = new OpenResponse();
                data.Type = SpeakType.Open;
                _openReceived.Invoke(null, data);
            }

            Log.Debug("Connect", "Connect Succeeded");
            Log.Verbose("SpeakWSClient.Connect", "LEAVE");
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("Connect", "Connect cancelled.");
            Log.Verbose("Connect", $"Connect cancelled. Info: {ex}");
            Log.Verbose("SpeakWSClient.Connect", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("Connect", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Connect", $"Excepton: {ex}");
            Log.Verbose("SpeakWSClient.Connect", "LEAVE");
            throw;
        }

        void StartSenderBackgroundThread() => _ = Task.Factory.StartNew(
            _ => ProcessSendQueue(),
                TaskCreationOptions.LongRunning);

        void StartReceiverBackgroundThread() => _ = Task.Factory.StartNew(
                _ => ProcessReceiveQueue(),
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
    /// Subscribe to a Flushed event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<FlushedResponse> eventHandler)
    {
        lock (_mutexSubscribe)
        {
            _flushedReceived += (sender, e) => eventHandler(sender, e);
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a Cleared event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<ClearedResponse> eventHandler)
    {
        lock (_mutexSubscribe)
        {
            _clearedReceived += (sender, e) => eventHandler(sender, e);
        }
        return true;
    }

    /// <summary>
    /// Subscribe to an Audio event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<AudioResponse> eventHandler)
    {
        lock (_mutexSubscribe)
        {
            _audioReceived += (sender, e) => eventHandler(sender, e);
        }
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
    /// Subscribe to an Warning event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<WarningResponse> eventHandler)
    {
        lock (_mutexSubscribe)
        {
            _warningReceived += (sender, e) => eventHandler(sender, e);
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
    /// This method sends a string to Deepgram for conversion to audio.
    /// This is a convenience functions that will wrap the provided string in a TextSource object.
    /// </summary>
    /// <param name="text">The string of text you want to be converted to audio.</param>
    public void SpeakWithText(string text)
    {
        TextSource textSource = new TextSource(text);
        byte[] byteArray = Encoding.UTF8.GetBytes(textSource.ToString());
        Send(byteArray);
    }

    /// <summary>
    ///  This method Flushes the text buffer on Deepgram to be converted to audio.
    /// </summary>
    public void Flush()
    {
        ControlMessage controlMessage = new ControlMessage(Constants.Flush);
        byte[] byteArray = Encoding.UTF8.GetBytes(controlMessage.ToString());
        Send(byteArray);
    }

    /// <summary>
    ///  This method Clears the text buffer on Deepgram to be converted to audio
    /// </summary>
    public void Clear()
    {
        ControlMessage controlMessage = new ControlMessage(Constants.Clear);
        byte[] byteArray = Encoding.UTF8.GetBytes(controlMessage.ToString());
        Send(byteArray);
    }

    /// <summary>
    ///  This method tells Deepgram to initiate the close server-side.
    /// </summary>
    public void Close(bool nullByte = false)
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

        ControlMessage controlMessage = new ControlMessage(Constants.Close);
        byte[] byteArray = Encoding.UTF8.GetBytes(controlMessage.ToString());
        SendMessageImmediately(byteArray);
    }

    /// <summary>
    /// Sends a binary message over the WebSocket connection.
    /// </summary>
    /// <param name="data">The data to be sent over the WebSocket.</param>
    public void Send(byte[] data, int length = Constants.UseArrayLengthForSend) => SendMessage(data, length);

    ///// <summary>
    ///// This method sends a binary message over the WebSocket connection.
    ///// Currently, this method has no use.
    ///// </summary>
    ///// <param name="data"></param>
    //public void SendBinary(byte[] data) =>
    //    EnqueueSendMessage(new WebSocketMessage(data, WebSocketMessageType.Binary));

    /// <summary>
    /// This method sends a text message over the WebSocket connection.
    /// </summary>
    public void SendMessage(byte[] data, int length = Constants.UseArrayLengthForSend)
    {
        // auto flush
        if (_deepgramClientOptions.InspectSpeakMessage())
        {
            string type = GetMessageType(data);
            Log.Debug("SendMessage", $"Inspecting Message: Sending {type}");
            switch (type)
            {
                case Constants.Flush:
                    lock (_mutexLastDatagram)
                    {
                        _flushCount += 1;
                        Log.Debug("SendMessage", $"Increment Flush count: {_flushCount}");
                    }
                    break;
                case Constants.Speak:
                    InspectMessage();
                    break;
            }
        }

        // send message
        EnqueueSendMessage(new WebSocketMessage(data, WebSocketMessageType.Text, length));
    }
    ///// <summary>
    ///// This method sends a binary message over the WebSocket connection immediately without queueing.
    ///// Currently, this method has no use.
    ///// </summary>
    //public void SendBinaryImmediately(byte[] data)
    //{
    //    lock (_mutexSend)
    //    {
    //        Log.Verbose("SendBinaryImmediately", "Sending binary message immediately..");  // TODO: dump this message
    //        _clientWebSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, _cancellationTokenSource.Token)
    //            .ConfigureAwait(false);
    //    }
    //}

    /// <summary>
    /// This method sends a text message over the WebSocket connection immediately without queueing.
    /// </summary>
    public void SendMessageImmediately(byte[] data, int length = Constants.UseArrayLengthForSend)
    {
        // auto flush
        if (_deepgramClientOptions.InspectSpeakMessage())
        {
            string type = GetMessageType(data);
            Log.Debug("SendMessage", $"Inspecting Message: Sending {type}");
            switch (type)
            {
                case Constants.Flush:
                    lock (_mutexLastDatagram)
                    {
                        _flushCount += 1;
                        Log.Debug("SendMessage", $"Increment Flush count: {_flushCount}");
                    }
                    break;
                case Constants.Speak:
                    InspectMessage();
                    break;
            }
        }

        lock (_mutexSend)
        {
            Log.Verbose("SendBinaryImmediately", "Sending text message immediately..");  // TODO: dump this message
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
        Log.Verbose("SpeakWSClient.ProcessSendQueue", "ENTER");

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
            Log.Verbose("SpeakWSClient.ProcessSendQueue", "LEAVE");
        }
        catch (OperationCanceledException ex)
        {
            Log.Debug("ProcessSendQueue", "SendThread cancelled.");
            Log.Verbose("ProcessSendQueue", $"SendThread cancelled. Info: {ex}");
            Log.Verbose("SpeakWSClient.ProcessSendQueue", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessSendQueue", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessSendQueue", $"Excepton: {ex}");
            Log.Verbose("SpeakWSClient.ProcessSendQueue", "LEAVE");
        }
    }

    internal async void ProcessAutoFlush()
    {
        Log.Verbose("LiveClient.ProcessAutoFlush", "ENTER");

        var diffTicks = TimeSpan.FromMilliseconds((double)_deepgramClientOptions.AutoFlushSpeakDelta);

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

                    Log.Debug("ProcessAutoFlush", $"AutoFlush delta exceeded threshold: {deltaTicks}. Skipping...");
                    Flush();
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
        Log.Verbose("SpeakWSClient.ProcessReceiveQueue", "ENTER");

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
                Log.Verbose("SpeakWSClient.ProcessReceiveQueue", "LEAVE");
            }
            catch (Exception ex)
            {
                Log.Error("ProcessReceiveQueue", $"{ex.GetType()} thrown {ex.Message}");
                Log.Verbose("ProcessReceiveQueue", $"Excepton: {ex}");
                Log.Verbose("SpeakWSClient.ProcessReceiveQueue", "LEAVE");
            }
        }
    }

    internal void ProcessDataReceived(WebSocketReceiveResult result, MemoryStream ms)
    {
        Log.Verbose("SpeakWSClient.ProcessDataReceived", "ENTER");

        ms.Seek(0, SeekOrigin.Begin);

        try
        {
            if (result.MessageType == WebSocketMessageType.Binary)
            {
                Log.Debug("ProcessDataReceived", "Received WebSocketMessageType.Binary");

                if (_audioReceived == null)
                {
                    Log.Debug("ProcessDataReceived", "_audioReceived has no listeners");
                    Log.Verbose("ProcessDataReceived", "LEAVE");
                    return;
                }

                var audioResponse = new AudioResponse()
                {
                    Stream = ms
                };

                Log.Debug("ProcessDataReceived", "Invoking AudioResponse");
                InvokeParallel(_audioReceived, audioResponse);

            }
            else if (result.MessageType == WebSocketMessageType.Text)
            {
                Log.Debug("ProcessDataReceived", "Received WebSocketMessageType.Text");

                var response = Encoding.UTF8.GetString(ms.ToArray());
                if (response == null)
                {
                    Log.Warning("ProcessDataReceived", "Response is null");
                    Log.Verbose("SpeakWSClient.ProcessDataReceived", "LEAVE");
                    return;
                }

                Log.Verbose("ProcessDataReceived", $"raw response: {response}");
                var data = JsonDocument.Parse(response);
                var val = Enum.Parse(typeof(SpeakType), data.RootElement.GetProperty("type").GetString()!);

                Log.Verbose("ProcessDataReceived", $"Type: {val}");

                switch (val)
                {
                    case SpeakType.Open:
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
                    case SpeakType.Metadata:
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
                    case SpeakType.Flushed:
                        var flushedResponse = data.Deserialize<FlushedResponse>();
                        if (_flushedReceived == null)
                        {
                            Log.Debug("ProcessDataReceived", "_flushedReceived has no listeners");
                            Log.Verbose("ProcessDataReceived", "LEAVE");
                            return;
                        }
                        if (flushedResponse == null)
                        {
                            Log.Warning("ProcessDataReceived", "FlushedResponse is invalid");
                            Log.Verbose("ProcessDataReceived", "LEAVE");
                            return;
                        }

                        // auto flush
                        if (_deepgramClientOptions.InspectSpeakMessage())
                        {
                            lock (_mutexLastDatagram)
                            {
                                _flushCount -= 1;
                                Log.Debug("ProcessDataReceived", $"Decrement Flush count: {_flushCount}");
                            }
                        }

                        Log.Debug("ProcessDataReceived", $"Invoking FlushedResponse. event: {flushedResponse}");
                        InvokeParallel(_flushedReceived, flushedResponse);
                        break;
                    case SpeakType.Cleared:
                        var clearResponse = data.Deserialize<ClearedResponse>();
                        if (_clearedReceived == null)
                        {
                            Log.Debug("ProcessDataReceived", "_clearedReceived has no listeners");
                            Log.Verbose("ProcessDataReceived", "LEAVE");
                            return;
                        }
                        if (clearResponse == null)
                        {
                            Log.Warning("ProcessDataReceived", "ClearedResponse is invalid");
                            Log.Verbose("ProcessDataReceived", "LEAVE");
                            return;
                        }

                        Log.Debug("ProcessDataReceived", $"Invoking ClearedResponse. event: {clearResponse}");
                        InvokeParallel(_clearedReceived, clearResponse);
                        break;
                    case SpeakType.Close:
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
                    case SpeakType.Warning:
                        var warningResponse = data.Deserialize<WarningResponse>();
                        if (_warningReceived == null)
                        {
                            Log.Debug("ProcessDataReceived", "_warningReceived has no listeners");
                            Log.Verbose("ProcessDataReceived", "LEAVE");
                            return;
                        }
                        if (warningResponse == null)
                        {
                            Log.Warning("ProcessDataReceived", "WarningResponse is invalid");
                            Log.Verbose("ProcessDataReceived", "LEAVE");
                            return;
                        }

                        Log.Debug("ProcessDataReceived", $"Invoking WarningResponse. event: {warningResponse}");
                        InvokeParallel(_warningReceived, warningResponse);
                        break;
                    case SpeakType.Error:
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
                        unhandledResponse.Type = SpeakType.Unhandled;
                        unhandledResponse.Raw = response;

                        Log.Debug("ProcessDataReceived", $"Invoking UnhandledResponse. event: {unhandledResponse}");
                        InvokeParallel(_unhandledReceived, unhandledResponse);
                        break;
                }
            }
            else
            {
                Log.Error("ProcessDataReceived", $"Received WebSocketMessageType.{result.MessageType.ToString()}");
                Log.Error("ProcessDataReceived", $"Data: {ms.ToString()}");
            }

            Log.Debug("ProcessDataReceived", "Succeeded");
            Log.Verbose("SpeakWSClient.ProcessDataReceived", "LEAVE");
        }
        catch (JsonException ex)
        {
            Log.Error("ProcessDataReceived", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessDataReceived", $"Excepton: {ex}");
            Log.Verbose("SpeakWSClient.ProcessDataReceived", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessDataReceived", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessDataReceived", $"Excepton: {ex}");
            Log.Verbose("SpeakWSClient.ProcessDataReceived", "LEAVE");
        }
    }

    /// <summary>
    /// Closes the Web Socket connection to the Deepgram API
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task Stop(CancellationTokenSource? cancelToken = null, bool nullByte = false)
    {
        Log.Verbose("SpeakWSClient.Stop", "ENTER");

        // client is already disposed
        if (_clientWebSocket == null)
        {
            Log.Information("Stop", "Client has already been disposed");
            Log.Verbose("SpeakWSClient.Stop", "LEAVE");
            return;
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
                Close(nullByte);
            }

            // small delay to wait for any final transcription
            await Task.Delay(100, cancelToken.Token).ConfigureAwait(false);

            // send a CloseResponse event
            if (_closeReceived != null)
            {
                Log.Debug("Stop", "Sending CloseResponse event...");
                var data = new CloseResponse();
                data.Type = SpeakType.Close;
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
            Log.Verbose("SpeakWSClient.Stop", "LEAVE");
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("Stop", "Stop cancelled.");
            Log.Verbose("Stop", $"Stop cancelled. Info: {ex}");
            Log.Verbose("SpeakWSClient.Stop", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("Stop", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Stop", $"Excepton: {ex}");
            Log.Verbose("SpeakWSClient.Stop", "LEAVE");
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
    public bool IsConnected()
    {
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

    /// <summary>
    /// Get the URI for the WebSocket connection
    /// </summary> 
    internal static Uri GetUri(IDeepgramClientOptions options, SpeakSchema parameter, Dictionary<string, string>? addons = null)
    {
        var propertyInfoList = parameter.GetType()
            .GetProperties()
            .Where(v => v.GetValue(parameter) is not null);

        var queryString = QueryParameterUtil.UrlEncode(parameter, propertyInfoList, addons);

        return new Uri($"{options.BaseAddress}/{UriSegments.SPEAK}?{queryString}");
    }

    internal void InvokeParallel<T>(EventHandler<T> eventHandler, T e)
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

    private void InspectMessage()
    {
        Log.Verbose("InspectMessage", "ENTER");

        if (_deepgramClientOptions.AutoFlushSpeakDelta > 0)
        {
            var now = DateTime.Now;
            Log.Debug("InspectMessage", $"AutoFlush last received. Time: {now}");
            lock (_mutexLastDatagram)
            {
                _lastReceived = now;
            }
        }

        Log.Debug("InspectMessage", "Succeeded");
        Log.Verbose("InspectMessage", "LEAVE");
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

    internal string GetMessageType(byte[] msg)
    {
        // Convert the byte array to a string
        string response = Encoding.UTF8.GetString(msg);
        if (response == null)
        {
            return "";
        }

        Log.Verbose("ProcessDataReceived", $"raw response: {response}");
        var data = JsonDocument.Parse(response);

        string val = data.RootElement.GetProperty("type").GetString() ?? "";
        Log.Debug("ProcessDataReceived", $"Type: {val}");

        return val;
    }
}
#endregion
