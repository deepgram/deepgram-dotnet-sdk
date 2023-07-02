using System.Threading.Channels;

namespace Deepgram.Clients;

internal class LiveTranscriptionClient : ILiveTranscriptionClient, IDisposable
{
    const string LOGGER_CATEGORY = "Deepgram.Transcription.LiveTranscriptionClient";

    readonly CleanCredentials _credentials;
    internal ClientWebSocket _clientWebSocket { get; set; }
    private CancellationTokenSource _tokenSource = new();
    private bool _disposed;

    private readonly Channel<MessageToSend> _sendChannel = System.Threading.Channels.Channel.CreateUnbounded<MessageToSend>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
        });

    public LiveTranscriptionClient(CleanCredentials credentials)
    {
        _credentials = credentials;
    }

    /// <summary>
    /// Fires when the WebSocket connection to Deepgram has been opened
    /// </summary>
    public event EventHandler<ConnectionOpenEventArgs> ConnectionOpened;

    /// <summary>
    /// Fires on any error in the connection, sending or receiving
    /// </summary>
    public event EventHandler<ConnectionErrorEventArgs> ConnectionError;

    /// <summary>
    /// Fires when the WebSocket connection is closed
    /// </summary>
    public event EventHandler<ConnectionClosedEventArgs> ConnectionClosed;

    /// <summary>
    /// Fires when a transcript is received from the Deepgram API
    /// </summary>
    public event EventHandler<TranscriptReceivedEventArgs> TranscriptReceived;

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
    /// Connect to a Deepgram API Web Socket to begin transcribing audio
    /// </summary>
    /// <param name="options">Options to use when transcribing audio</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task StartConnectionAsync(LiveTranscriptionOptions options)
    {
        _clientWebSocket?.Dispose();

        _clientWebSocket = new ClientWebSocket();
        _clientWebSocket.Options.SetRequestHeader("Authorization", $"token {_credentials.ApiKey}");

        _tokenSource = new CancellationTokenSource();
        try
        {
            var wssUri = GetWSSUriWithQuerystring("listen", options);
            await _clientWebSocket.ConnectAsync(wssUri, CancellationToken.None).ConfigureAwait(false);
            StartSenderBackgroundThread();
            StartReceiverBackgroundThread();
            ConnectionOpened?.Invoke(null, new ConnectionOpenEventArgs());
        }
        catch (Exception ex)
        {
            var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
            logger.LogDebug(ex, $"StartConnectionAsync: {ex.Message}");
            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
        }

        void StartSenderBackgroundThread()
        {
            _ = Task.Factory.StartNew(_ => ProcessSenderQueue(), TaskCreationOptions.LongRunning, _tokenSource.Token);
        }

        void StartReceiverBackgroundThread()
        {
            _ = Task.Factory.StartNew(_ => Receive(), TaskCreationOptions.LongRunning, _tokenSource.Token);
        }
    }

    /// <summary>
    /// Closes the Web Socket connection to the Deepgram API
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task StopConnectionAsync()
    {
        try
        {
            if (_clientWebSocket.CloseStatus.HasValue)
            {
                var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
                logger.LogDebug("Closing websocket.");
            }

            if (!_disposed)
            {
                if (_clientWebSocket?.State != WebSocketState.Closed)
                {
                    await _clientWebSocket.CloseOutputAsync(
                        WebSocketCloseStatus.NormalClosure,
                        string.Empty,
                        CancellationToken.None).ConfigureAwait(false);
                }

                _tokenSource?.Cancel();
            }

            ConnectionClosed?.Invoke(null, new ConnectionClosedEventArgs());
        }
        catch (ObjectDisposedException ex)
        {
            var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
            logger.LogDebug("Error stopping connection. WebSocket was disposed.");
            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
        }
        catch (Exception ex)
        {
            var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
            logger.LogDebug("Error stopping connection.");
            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
        }
    }

    /// <summary>
    /// Signals to Deepgram that the audio has completed so it can return
    /// the final transcription output
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task FinishAsync()
    {
        if (_clientWebSocket.State != WebSocketState.Open)
        {
            var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
            logger.LogWarning($"Trying to finish when the socket is {_clientWebSocket.State}. Ack for this message will fail shortly.");
            return;
        }

        await _clientWebSocket.SendAsync(new ArraySegment<byte>(Array.Empty<byte>()), WebSocketMessageType.Binary, true, CancellationToken.None).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a binary message over the websocket connection.
    /// </summary>
    /// <param name="data">The data to be sent over the websocket.</param>
    public void SendData(byte[] data)
    {
        EnqueueForSending(new MessageToSend(data));
    }

    private Uri GetWSSUriWithQuerystring(string uriSegment, LiveTranscriptionOptions queryParameters) =>
    UriUtil.ResolveUri(
       _credentials.ApiUrl, uriSegment,
       Convert.ToBoolean(_credentials.RequireSSL) ? "wss" : "ws",
       queryParameters);


    private async Task ProcessSenderQueue()
    {
        if (_disposed)
        {
            var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
            var ex = new Exception("Attempting to start a sender queue when the WebSocket has been disposed is not allowed.");
            logger.LogError(ex, "Attempting to start a sender queue when the WebSocket has been disposed is not allowed.");
            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
            throw ex;
        }

        try
        {
            while (await _sendChannel.Reader.WaitToReadAsync())
            {
                while (_sendChannel.Reader.TryRead(out var message))
                {
                    await Send(message.Message, _tokenSource.Token).ConfigureAwait(false);
                }
            }
        }
        catch (ObjectDisposedException e)
        {
            var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
            logger.LogDebug(e, _disposed ? "Connection has been disposed." : "WebSocket send operation cancelled.");
            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(e));
        }
        catch (OperationCanceledException e)
        {
            var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
            logger.LogDebug(e, _disposed ? "Connection has been disposed." : "WebSocket send operation cancelled.");
            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(e));
        }
        catch (Exception e)
        {
            var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
            logger.LogError(e, "Error Sending to WebSocket");
            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(e));
        }
    }

    private async Task Receive()
    {
        while (_clientWebSocket?.State == WebSocketState.Open)
        {
            try
            {
                var buffer = new ArraySegment<byte>(new byte[1024 * 16]); // Default receive buffer size
                WebSocketReceiveResult result;
                using (var ms = new MemoryStream())
                {
                    do
                    {
                        result = await _clientWebSocket.ReceiveAsync(buffer, _tokenSource.Token);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            Console.WriteLine(result.CloseStatusDescription);
                            break;
                        }

                        ms.Write(buffer.Array ?? throw new InvalidOperationException("buffer cannot be null"), buffer.Offset, result.Count);
                    }
                    while (!result.EndOfMessage);

                    ms.Seek(0, SeekOrigin.Begin);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var text = Encoding.UTF8.GetString(ms.ToArray());
                        if (text != null)
                        {
                            var transcript = JsonConvert.DeserializeObject<LiveTranscriptionResult>(text);
                            if (transcript != null)
                            {
                                TranscriptReceived?.Invoke(null, new TranscriptReceivedEventArgs(transcript));
                            }
                        }
                    }
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await StopConnectionAsync();
                    break;
                }
            }
            catch (Exception ex)
            {
                var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
                logger.LogError(ex, $"Receipt error");
                ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
                break;
            }
        }
    }

    private void EnqueueForSending(MessageToSend message)
    {
        try
        {
            var writeResult = _sendChannel.Writer.TryWrite(message);
            if (writeResult == false)
            {
                var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
                logger.LogWarning("Failed to enqueue message to WebSocket connection. The connection is being disposed.");
            }
        }
        catch (Exception e)
        {
            var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
            logger.LogError(_disposed ? "Failed to enqueue message to WebSocket connection. The connection has been disposed." : "Failed to enqueue message to WebSocket connection.");
            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(e));
            throw;
        }
    }

    private async Task Send(ArraySegment<byte> data, CancellationToken token)
    {
        if (_clientWebSocket.State != WebSocketState.Open)
        {
            var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
            logger.LogWarning($"Trying to send message when the socket is {_clientWebSocket.State}. Ack for this message will fail shortly.");
            return;
        }

        await _clientWebSocket.SendAsync(data, WebSocketMessageType.Binary, true, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Dispose method. Stops the send thread and disposes the websocket.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _tokenSource.Cancel();
        _tokenSource.Dispose();
        _sendChannel?.Writer.Complete();
        _clientWebSocket?.Dispose();

        _disposed = true;
    }
}
