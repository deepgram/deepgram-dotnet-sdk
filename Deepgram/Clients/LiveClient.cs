namespace Deepgram.Clients;
public class LiveClient
{
    //const string LOGGER_CATEGORY = "Deepgram.Transcription.LiveTranscriptionClient";
    //internal HttpClientOptions _clientOptions;
    //internal ClientWebSocket? ClientWebSocket { get; set; }
    //private CancellationTokenSource? _tokenSource = new();
    //private bool _disposed;

    //public LiveClient(HttpClientOptions clientOptions)
    //{
    //        this._clientOptions = clientOptions;
    //}

    //private readonly Channel<MessageToSend> _sendChannel = System.Threading.Channels.Channel.CreateUnbounded<MessageToSend>(
    //    new UnboundedChannelOptions
    //    {
    //        SingleReader = true,
    //        SingleWriter = true,
    //    });

    ///// <summary>
    ///// Fires when the WebSocket connection to Deepgram has been opened
    ///// </summary>
    //public event EventHandler<ConnectionOpenEventArgs>? ConnectionOpened;

    ///// <summary>
    ///// Fires on any error in the connection, sending or receiving
    ///// </summary>
    //public event EventHandler<ConnectionErrorEventArgs>? ConnectionError;

    ///// <summary>
    ///// Fires when the WebSocket connection is closed
    ///// </summary>
    //public event EventHandler<ConnectionClosedEventArgs>? ConnectionClosed;

    ///// <summary>
    ///// Fires when a transcript is received from the Deepgram API
    ///// </summary>
    //public event EventHandler<TranscriptReceivedEventArgs>? TranscriptReceived;

    ///// <summary>
    ///// Retrieves the connection state of the WebSocket
    ///// </summary>
    ///// <returns>Returns the connection state of the WebSocket</returns>
    //public WebSocketState State()
    //{
    //    if (ClientWebSocket == null)
    //    {
    //        return WebSocketState.None;
    //    }
    //    return ClientWebSocket.State;
    //}

    ///// <summary>
    ///// Connect to a Deepgram API Web Socket to begin transcribing audio
    ///// </summary>
    ///// <param name="options">Options to use when transcribing audio</param>
    ///// <returns>The task object representing the asynchronous operation.</returns>
    //public async Task StartConnectionAsync(LiveTranscriptionOptions options)
    //{
    //    if (ClientWebSocket != null)
    //    {
    //        ClientWebSocket.Dispose();
    //    }

    //    ClientWebSocket = new ClientWebSocket();
    //    ClientWebSocket.Options.SetRequestHeader("Authorization", $"token {_clientOptions.ApiKey}");

    //    _tokenSource = new CancellationTokenSource();
    //    try
    //    {
    //        var wssUri = GetWSSUriWithQuerystring(options);
    //        await ClientWebSocket.ConnectAsync(wssUri, CancellationToken.None).ConfigureAwait(false);
    //        StartSenderBackgroundThread();
    //        StartReceiverBackgroundThread();
    //        ConnectionOpened?.Invoke(null, new ConnectionOpenEventArgs());
    //    }
    //    catch (Exception ex)
    //    {
    //        var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
    //        logger.LogDebug(ex, $"StartConnectionAsync: {ex.Message}");
    //        ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
    //    }

    //    void StartSenderBackgroundThread()
    //    {
    //        _ = Task.Factory.StartNew(_ => ProcessSenderQueue(), TaskCreationOptions.LongRunning, _tokenSource.Token);
    //    }

    //    void StartReceiverBackgroundThread()
    //    {
    //        _ = Task.Factory.StartNew(_ => Receive(), TaskCreationOptions.LongRunning, _tokenSource.Token);
    //    }
    //}

    ///// <summary>
    ///// Closes the Web Socket connection to the Deepgram API
    ///// </summary>
    ///// <returns>The task object representing the asynchronous operation.</returns>
    //public async Task StopConnectionAsync()
    //{
    //    try
    //    {
    //        if (ClientWebSocket!.CloseStatus.HasValue)
    //        {
    //            var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
    //            logger.LogDebug("Closing websocket.");
    //        }

    //        if (!_disposed)
    //        {
    //            if (ClientWebSocket!.State != WebSocketState.Closed)
    //            {
    //                await ClientWebSocket.CloseOutputAsync(
    //                    WebSocketCloseStatus.NormalClosure,
    //                    string.Empty,
    //                    CancellationToken.None).ConfigureAwait(false);
    //            }

    //            _tokenSource?.Cancel();
    //        }

    //        ConnectionClosed?.Invoke(null, new ConnectionClosedEventArgs());
    //    }
    //    catch (ObjectDisposedException ex)
    //    {
    //        ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
    //    }
    //    catch (Exception ex)
    //    {
    //        ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
    //    }
    //}

    ///// <summary>
    ///// Signals to Deepgram that the audio has completed so it can return
    ///// the final transcription output
    ///// </summary>
    ///// <returns>The task object representing the asynchronous operation.</returns>
    //public async Task FinishAsync()
    //{
    //    if (ClientWebSocket!.State != WebSocketState.Open)
    //    {
    //        return;
    //    }

    //    await ClientWebSocket.SendAsync(new ArraySegment<byte>(Array.Empty<byte>()), WebSocketMessageType.Binary, true, CancellationToken.None).ConfigureAwait(false);
    //}

    ///// <inheritdoc />
    //public void KeepAlive()
    //{
    //    var keepAliveMessage = JsonSerializer.Serialize(new { type = "KeepAlive" });
    //    var keepAliveBytes = Encoding.Default.GetBytes(keepAliveMessage);

    //    EnqueueForSending(new MessageToSend(keepAliveBytes, WebSocketMessageType.Text));
    //}
    ///// <summary>
    ///// Sends a binary message over the websocket connection.
    ///// </summary>
    ///// <param name="data">The data to be sent over the websocket.</param>
    //public virtual void SendData(byte[] data)
    //{
    //    EnqueueForSending(new MessageToSend(data, WebSocketMessageType.Binary));
    //}

    //private Uri GetWSSUriWithQuerystring(LiveTranscriptionOptions queryParameters)
    //{
    //    var protocol = Convert.ToBoolean(_clientOptions.RequireSSL) ? "wss" : "ws";
    //    var startUri = $@"{protocol}://{_clientOptions.Uri}/v1/listen";
    //    var parameters = QueryParameterUtil.GetParameters(queryParameters);
    //    return new Uri($"{startUri}?{parameters}");
    //}


    //private async Task ProcessSenderQueue()
    //{
    //    if (_disposed)
    //    {
    //        var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
    //        var ex = new Exception("Attempting to start a sender queue when the WebSocket has been disposed is not allowed.");
    //        logger.LogError(ex, "Attempting to start a sender queue when the WebSocket has been disposed is not allowed.");
    //        ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
    //        throw ex;
    //    }

    //    try
    //    {
    //        while (await _sendChannel.Reader.WaitToReadAsync())
    //        {
    //            while (_sendChannel.Reader.TryRead(out var message))
    //            {
    //                await Send(message, _tokenSource!.Token).ConfigureAwait(false);
    //            }
    //        }
    //    }
    //    catch (ObjectDisposedException e)
    //    {
    //        var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
    //        logger.LogDebug(e, _disposed ? "Connection has been disposed." : "WebSocket send operation cancelled.");
    //        ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(e));
    //    }
    //    catch (OperationCanceledException e)
    //    {
    //        var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
    //        logger.LogDebug(e, _disposed ? "Connection has been disposed." : "WebSocket send operation cancelled.");
    //        ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(e));
    //    }
    //    catch (Exception e)
    //    {
    //        var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
    //        logger.LogError(e, "Error Sending to WebSocket");
    //        ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(e));
    //    }
    //}

    //private async Task Receive()
    //{
    //    while (ClientWebSocket?.State == WebSocketState.Open)
    //    {
    //        try
    //        {
    //            var buffer = new ArraySegment<byte>(new byte[1024 * 16]); // Default receive buffer size
    //            WebSocketReceiveResult result;
    //            using (var ms = new MemoryStream())
    //            {
    //                do
    //                {
    //                    result = await ClientWebSocket.ReceiveAsync(buffer, _tokenSource!.Token);
    //                    if (result.MessageType == WebSocketMessageType.Close)
    //                    {
    //                        Console.WriteLine(result.CloseStatusDescription);
    //                        break;
    //                    }

    //                    ms.Write(buffer.Array ?? throw new InvalidOperationException("buffer cannot be null"), buffer.Offset, result.Count);
    //                }
    //                while (!result.EndOfMessage);

    //                ms.Seek(0, SeekOrigin.Begin);

    //                if (result.MessageType == WebSocketMessageType.Text)
    //                {
    //                    var text = Encoding.UTF8.GetString(ms.ToArray());
    //                    if (text != null)
    //                    {
    //                        var transcript = JsonSerializer.Deserialize<LiveTranscriptionResult>(text);
    //                        if (transcript != null)
    //                        {
    //                            TranscriptReceived?.Invoke(null, new TranscriptReceivedEventArgs(transcript));
    //                        }
    //                    }
    //                }
    //            }

    //            if (result.MessageType == WebSocketMessageType.Close)
    //            {
    //                await StopConnectionAsync();
    //                break;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
    //            logger.LogError(ex, $"Receipt error");

    //            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
    //            break;
    //        }
    //    }
    //}

    //private void EnqueueForSending(MessageToSend message)
    //{
    //    try
    //    {
    //        var writeResult = _sendChannel.Writer.TryWrite(message);
    //        if (writeResult == false)
    //        {
    //            var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
    //            logger.LogWarning("Failed to enqueue message to WebSocket connection. The connection is being disposed.");
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
    //        logger.LogError(_disposed ? "Failed to enqueue message to WebSocket connection. The connection has been disposed." : "Failed to enqueue message to WebSocket connection.");
    //        ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(e));
    //        throw;
    //    }
    //}

    //private async Task Send(MessageToSend message, CancellationToken token)
    //{
    //    if (ClientWebSocket!.State != WebSocketState.Open)
    //    {
    //        var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
    //        logger.LogWarning($"Trying to send message when the socket is {ClientWebSocket.State}. Ack for this message will fail shortly.");
    //        return;
    //    }

    //    await ClientWebSocket.SendAsync(message.Message, message.MessageType, true, token).ConfigureAwait(false);
    //}

    ///// <summary>
    ///// Dispose method. Stops the send thread and disposes the websocket.
    ///// </summary>
    //public void Dispose()
    //{
    //    if (_disposed)
    //    {
    //        return;
    //    }

    //    _tokenSource!.Cancel();
    //    _tokenSource.Dispose();
    //    _sendChannel?.Writer.Complete();
    //    ClientWebSocket?.Dispose();

    //    _disposed = true;
    //}
}
