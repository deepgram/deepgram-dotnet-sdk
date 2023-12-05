using System.Net.WebSockets;
using System.Threading.Channels;
using Deepgram.DeepgramEventArgs;

namespace Deepgram.Abstractions;
public abstract class AbstractWebSocketClient
{
    #region Fields
    readonly string _clientType;
    internal ILogger logger => LogProvider.GetLogger(_clientType);
    internal DeepgramClientOptions _deepgramClientOptions;
    internal string _apiKey;
    internal ClientWebSocket? _clientWebSocket;
    internal CancellationTokenSource? _tokenSource = new();
    internal bool _disposed;

    #endregion

    #region Subscribe Events
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
    #endregion

    /// <summary>
    /// Retrieves the connection state of the WebSocket
    /// </summary>
    /// <returns>Returns the connection state of the WebSocket</returns>
    public WebSocketState State() => _clientWebSocket == null ? WebSocketState.None : _clientWebSocket.State;


    public AbstractWebSocketClient(string? apiKey, DeepgramClientOptions? deepgramClientOptions = null)
    {
        _clientType = this.GetType().Name;
        _deepgramClientOptions = deepgramClientOptions is null ? new DeepgramClientOptions() : deepgramClientOptions;
        _deepgramClientOptions = BaseAddressUtil.GetWss(_deepgramClientOptions);
        _apiKey = ApiKeyUtil.Validate(apiKey, _clientType);
    }

    internal readonly Channel<MessageToSend> _sendChannel = System.Threading.Channels.Channel
        .CreateUnbounded<MessageToSend>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true, });

    internal void EnqueueForSending(MessageToSend message)
    {
        try
        {
            _sendChannel.Writer.TryWrite(message);
        }
        catch (Exception ex)
        {
            if (_disposed)
                Log.SocketDisposed(logger, "Enqueue Message", ex);
            else
                Log.EnqueueFailure(logger);

            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
            throw;
        }
    }
    internal async Task ProcessSenderQueue()
    {
        if (_disposed)
        {
            var ex = new Exception(
                "Attempting to start a sender queue when the WebSocket has been disposed is not allowed.");
            Log.SocketStartError(logger, ex);
            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
            throw ex;
        }

        try
        {
            while (await _sendChannel.Reader.WaitToReadAsync())
            {
                while (_sendChannel.Reader.TryRead(out var message))
                {
                    if (_clientWebSocket!.State != WebSocketState.Open)
                        Log.LiveSendWarning(logger, _clientWebSocket.State);

                    await _clientWebSocket.SendAsync(
                        message.Message,
                        WebSocketMessageType.Text,
                        true,
                        _tokenSource!.Token).ConfigureAwait(false);
                }
            }
        }

        catch (ObjectDisposedException ex)
        {
            if (_disposed)
                Log.SocketDisposed(logger, "Processing send queue", ex);
            else
                Log.SendCancelledError(logger, ex);
            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
        }
        catch (OperationCanceledException ex)
        {
            if (_disposed)
                Log.SocketDisposed(logger, "Processing send queue", ex);
            else
                Log.SendCancelledError(logger, ex);

            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
        }
        catch (Exception ex)
        {
            Log.SendWebSocketError(logger, ex);
            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
        }
    }


    internal async Task Receive()
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
                        result = await _clientWebSocket.ReceiveAsync(buffer, _tokenSource!.Token);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            Log.RequestedSocketClose(logger, result.CloseStatusDescription!);
                            break;
                        }

                        ms.Write(
                            buffer.Array ?? throw new InvalidOperationException("buffer cannot be null"),
                            buffer.Offset,
                            result.Count);
                    } while (!result.EndOfMessage);

                    RaiseTranscriptionReceived(result, ms);
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await StopConnectionAsync();
                    break;
                }
            }
            catch (Exception ex)
            {
                Log.ReceiptError(logger, ex);
                ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
                break;
            }

        }
    }

    private void RaiseTranscriptionReceived(WebSocketReceiveResult result, MemoryStream ms)
    {
        ms.Seek(0, SeekOrigin.Begin);

        if (result.MessageType == WebSocketMessageType.Text)
        {
            if (Encoding.UTF8.GetString(ms.ToArray()) != null)
            {
                var transcript = RequestContentUtil.Deserialize<LiveTranscriptionResponse>(Encoding.UTF8.GetString(ms.ToArray()));
                if (transcript != null)
                    TranscriptReceived?.Invoke(null, new TranscriptReceivedEventArgs(transcript));
            }
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
            if (_clientWebSocket!.CloseStatus.HasValue)
            {
                Log.ClosingSocket(logger);
            }

            if (!_disposed)
            {
                if (_clientWebSocket!.State != WebSocketState.Closed)
                {
                    await _clientWebSocket.CloseOutputAsync(
                        WebSocketCloseStatus.NormalClosure,
                        string.Empty,
                        CancellationToken.None)
                        .ConfigureAwait(false);
                }

                _tokenSource?.Cancel();
            }

            ConnectionClosed?.Invoke(null, new ConnectionClosedEventArgs());
        }
        catch (Exception ex)
        {
            Log.WebSocketCloseError(logger, ex);
            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
        }
    }

    #region Helpers
    internal Uri GetUri(LiveSchema queryParameters)
    {
        var query = QueryParameterUtil.GetParameters(queryParameters);
        return new Uri($"wss://{_deepgramClientOptions.BaseAddress}/{Constants.API_VERSION}/{Constants.LISTEN}?{query}");
    }
    public void KeepAlive()
    {
        var keepAliveMessage = JsonSerializer.Serialize(new { type = "KeepAlive" });
        var keepAliveBytes = Encoding.Default.GetBytes(keepAliveMessage);

        EnqueueForSending(new MessageToSend(keepAliveBytes, WebSocketMessageType.Text));
    }

    public void InvokeConnectionOpen(object? sender = null) => ConnectionOpened?.Invoke(sender, new ConnectionOpenEventArgs());

    public void InvokeConnectionError(Exception ex, object? sender = null) => ConnectionError?.Invoke(sender, new ConnectionErrorEventArgs(ex));

    /// <summary>
    /// Dispose method. Stops the send thread and disposes the WebSocket.
    /// </summary>
    internal void DisposeResources()
    {
        if (_disposed)
        {
            return;
        }

        _tokenSource!.Cancel();
        _tokenSource.Dispose();
        _sendChannel?.Writer.Complete();
        _clientWebSocket?.Dispose();

        _disposed = true;
    }
    #endregion
}
