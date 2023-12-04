using System.Net.WebSockets;
using System.Threading.Channels;
using Deepgram.CustomEventArgs;
using Deepgram.Logger;

namespace Deepgram;
public class LiveClient
{
    internal string _loggerName;
    internal DeepgramClientOptions _deepgramClientOptions;
    internal string ApiKey;
    internal ClientWebSocket? ClientWebSocket { get; set; }
    private CancellationTokenSource? _tokenSource = new();
    private bool _disposed;

    public LiveClient(string apiKey, DeepgramClientOptions? deepgramClientOptions = null)
    {
        _loggerName = this.GetType().Name;
        _deepgramClientOptions = deepgramClientOptions is null ? new DeepgramClientOptions() : deepgramClientOptions;
        ApiKey = ApiKeyUtil.Validate(apiKey, this.GetType().Name);

        // add logger to  log collection

    }

    private readonly Channel<MessageToSend> _sendChannel = System.Threading.Channels.Channel.CreateUnbounded<MessageToSend>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true,
        });

    /// <summary>
    /// Fires when the WebSocket connection to Deepgram has been opened
    /// </summary>
    public event EventHandler<ConnectionOpenEventArgs>? ConnectionOpened;

    /// <summary>
    /// Fires on any error in the connection, sending or receiving
    /// </summary>
    public event EventHandler<ConnectionErrorEventArgs>? ConnectionError;

    /// <summary>
    /// Fires when the WebSocket connection is closed
    /// </summary>
    public event EventHandler<ConnectionClosedEventArgs>? ConnectionClosed;

    /// <summary>
    /// Fires when a transcript is received from the Deepgram API
    /// </summary>
    public event EventHandler<TranscriptReceivedEventArgs>? TranscriptReceived;

    /// <summary>
    /// Retrieves the connection state of the WebSocket
    /// </summary>
    /// <returns>Returns the connection state of the WebSocket</returns>
    public WebSocketState State() => ClientWebSocket == null ? WebSocketState.None : ClientWebSocket.State;

    /// <summary>
    /// Connect to a Deepgram API Web Socket to begin transcribing audio
    /// </summary>
    /// <param name="options">Options to use when transcribing audio</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task StartConnectionAsync(LiveSchema options)
    {
        ClientWebSocket?.Dispose();

        ClientWebSocket = new ClientWebSocket();
        ClientWebSocket.Options.SetRequestHeader("Authorization", $"token {ApiKey}");

        _tokenSource = new CancellationTokenSource();
        try
        {
            await ClientWebSocket.ConnectAsync(GetUri(options), CancellationToken.None).ConfigureAwait(false);
            StartSenderBackgroundThread();
            StartReceiverBackgroundThread();
            ConnectionOpened?.Invoke(null, new ConnectionOpenEventArgs());
        }
        catch (Exception ex)
        {
            Log.StartSocketError(LogProvider.GetLogger(_loggerName), ex);
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

    private Uri GetUri(LiveSchema queryParameters)
    {
        var query = QueryParameterUtil.GetParameters(_loggerName, queryParameters);
        return new Uri($"wss/{_deepgramClientOptions.BaseAddress}/{Constants.API_VERSION}/{Constants.LISTEN}?{query}");
    }



    /// <summary>
    /// Closes the Web Socket connection to the Deepgram API
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task StopConnectionAsync()
    {
        try
        {
            if (ClientWebSocket!.CloseStatus.HasValue)
            {
                Log.ClosingSocket(LogProvider.GetLogger(_loggerName));
            }

            if (!_disposed)
            {
                if (ClientWebSocket!.State != WebSocketState.Closed)
                {
                    await ClientWebSocket.CloseOutputAsync(
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
            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
        }
        catch (Exception ex)
        {
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
        if (ClientWebSocket!.State != WebSocketState.Open)
            return;

        await ClientWebSocket.SendAsync(new ArraySegment<byte>([]), WebSocketMessageType.Binary, true, CancellationToken.None).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void KeepAlive()
    {
        var keepAliveMessage = JsonSerializer.Serialize(new { type = "KeepAlive" });
        var keepAliveBytes = Encoding.Default.GetBytes(keepAliveMessage);

        EnqueueForSending(new MessageToSend(keepAliveBytes, WebSocketMessageType.Text));
    }
    /// <summary>
    /// Sends a binary message over the WebSocket connection.
    /// </summary>
    /// <param name="data">The data to be sent over the WebSocket.</param>
    public virtual void SendData(byte[] data)
    {
        EnqueueForSending(new MessageToSend(data, WebSocketMessageType.Binary));
    }

    private async Task ProcessSenderQueue()
    {
        if (_disposed)
        {
            var ex = new Exception("Attempting to start a sender queue when the WebSocket has been disposed is not allowed.");
            Log.SocketStartError(LogProvider.GetLogger(_loggerName), ex);
            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
            throw ex;
        }

        try
        {
            while (await _sendChannel.Reader.WaitToReadAsync())
            {
                while (_sendChannel.Reader.TryRead(out var message))
                {
                    await Send(message, _tokenSource!.Token).ConfigureAwait(false);
                }
            }
        }
        catch (ObjectDisposedException ex)
        {
            var logger = LogProvider.GetLogger(_loggerName);
            if (_disposed)
                Log.SocketDisposingWithException(logger, "Processing send queue", ex);
            else
                Log.SendOperationCancelledError(logger, ex);
            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
        }
        catch (OperationCanceledException ex)
        {
            var logger = LogProvider.GetLogger(_loggerName);
            if (_disposed)
                Log.SocketDisposingWithException(logger, "Processing send queue", ex);
            else
                Log.SendOperationCancelledError(logger, ex);

            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
        }
        catch (Exception ex)
        {
            Log.SendWebSocketError(LogProvider.GetLogger(_loggerName), ex);
            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
        }
    }

    private async Task Receive()
    {
        while (ClientWebSocket?.State == WebSocketState.Open)
        {
            try
            {
                var buffer = new ArraySegment<byte>(new byte[1024 * 16]); // Default receive buffer size
                WebSocketReceiveResult result;
                using (var ms = new MemoryStream())
                {
                    do
                    {
                        result = await ClientWebSocket.ReceiveAsync(buffer, _tokenSource!.Token);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            Log.RequestedSocketClose(LogProvider.GetLogger(_loggerName), result.CloseStatusDescription);

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
                            var transcript = RequestContentUtil.Deserialize<LiveTranscriptionResponse>(_loggerName, text);
                            if (transcript != null)
                                TranscriptReceived?.Invoke(null, new TranscriptReceivedEventArgs(transcript));
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
                Log.ReceiptError(LogProvider.GetLogger(_loggerName), ex);
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
                Log.SocketDisposing(LogProvider.GetLogger(_loggerName), "Enqueue Message");

        }
        catch (Exception ex)
        {
            if (_disposed)
                Log.SocketDisposing(LogProvider.GetLogger(_loggerName), "Enqueue Message");
            else
                Log.EnqueueFailure(LogProvider.GetLogger(_loggerName));

            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
            throw;
        }
    }

    private async Task Send(MessageToSend message, CancellationToken token)
    {
        if (ClientWebSocket!.State != WebSocketState.Open)
        {
            Log.LiveSendWarning(LogProvider.GetLogger(_loggerName), ClientWebSocket.State);
            return;
        }

        await ClientWebSocket.SendAsync(message.Message, WebSocketMessageType.Text, true, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Dispose method. Stops the send thread and disposes the WebSocket.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _tokenSource!.Cancel();
        _tokenSource.Dispose();
        _sendChannel?.Writer.Complete();
        ClientWebSocket?.Dispose();

        _disposed = true;
    }
}
internal readonly struct MessageToSend
{
    public MessageToSend(byte[] message, WebSocketMessageType type)
    {
        Message = new ArraySegment<byte>(message);
        MessageType = type;
    }

    public ArraySegment<byte> Message { get; }

    public WebSocketMessageType MessageType { get; }
}
