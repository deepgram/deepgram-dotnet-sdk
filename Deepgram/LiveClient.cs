using Deepgram.Extensions;
using Deepgram.Records.Live;

namespace Deepgram;

public class LiveClient
{
    #region Fields
    private readonly string _clientType;
    internal ILogger logger => LogProvider.GetLogger(_clientType);
    internal readonly DeepgramClientOptions _deepgramClientOptions;
    internal ClientWebSocket? _clientWebSocket;
    internal readonly CancellationTokenSource _tokenSource;
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

    public LiveClient(DeepgramClientOptions deepgramClientOptions)
    {
        _tokenSource = new CancellationTokenSource();
        _clientType = this.GetType().Name;
        _deepgramClientOptions = deepgramClientOptions;
    }

    //TODO when a response is recieved check if it is a transcript(LiveTranscriptionEvent) or metadata (LiveMetadataEvent) response 

    /// <summary>
    /// Connect to a Deepgram API Web Socket to begin transcribing audio
    /// </summary>
    /// <param name="options">Options to use when transcribing audio</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task StartConnectionAsync(LiveSchema options, CancellationToken? cancellationToken = null)
    {
        var cancelToken = cancellationToken ?? _tokenSource.Token;
        _clientWebSocket?.Dispose();
        _clientWebSocket = new ClientWebSocket()
            .SetHeaders(_deepgramClientOptions);

        try
        {
            await _clientWebSocket.ConnectAsync(
                GetUri(options),
                cancelToken).ConfigureAwait(false);
            StartSenderBackgroundThread();
            StartReceiverBackgroundThread();
            ConnectionOpened?.Invoke(null, new ConnectionOpenEventArgs());
            _disposed = false;
        }
        catch (Exception ex)
        {
            Log.WebSocketStartError(logger, ex);
            ConnectionError?.Invoke(null, new ConnectionErrorEventArgs(ex));
        }

        void StartSenderBackgroundThread() => _ = Task.Factory.StartNew(
            _ => ProcessSenderQueue(),
                TaskCreationOptions.LongRunning,
                cancelToken);

        void StartReceiverBackgroundThread() => _ = Task.Factory.StartNew(
                _ => Receive(),
            TaskCreationOptions.LongRunning,
            cancelToken);
    }

    /// <summary>
    /// Sends a binary message over the WebSocket connection.
    /// </summary>
    /// <param name="data">The data to be sent over the WebSocket.</param>
    public void SendData(byte[] data) =>
        EnqueueForSending(new MessageToSend(data, WebSocketMessageType.Binary));

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

    internal async Task ProcessSenderQueue(CancellationToken? cancellationToken = null)
    {
        var cancelToken = cancellationToken ?? _tokenSource.Token;
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
            while (await _sendChannel.Reader.WaitToReadAsync(cancelToken))
            {
                while (_sendChannel.Reader.TryRead(out var message))
                {
                    if (_clientWebSocket!.State != WebSocketState.Open)
                        Log.LiveSendWarning(logger, _clientWebSocket.State);

                    await _clientWebSocket.SendAsync(
                        message.Message,
                        WebSocketMessageType.Text,
                        true,
                        cancelToken).ConfigureAwait(false);
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

    internal async Task Receive(CancellationToken? cancellationToken = null)
    {
        var cancelToken = cancellationToken ?? _tokenSource.Token;
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
                        result = await _clientWebSocket.ReceiveAsync(buffer, cancelToken);
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
    public async Task StopConnectionAsync(CancellationToken? cancellationToken = null)
    {
        var cancelToken = cancellationToken ?? _tokenSource.Token;
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
                        cancelToken)
                        .ConfigureAwait(false);
                }

                // Always request cancellation to the local token source, if some function has been called without a token
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

    /// <summary>
    /// Signals to Deepgram that the audio has completed so it can return the final transcription output
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task FinishAsync(CancellationToken? cancellationToken = null)
    {
        var cancelToken = cancellationToken ?? _tokenSource.Token;
        if (_clientWebSocket!.State != WebSocketState.Open)
            return;

        await _clientWebSocket.SendAsync(
            new ArraySegment<byte>([]),
            WebSocketMessageType.Binary,
            true,
            cancelToken)
            .ConfigureAwait(false);
    }

    #region Helpers

    /// <summary>
    /// Retrieves the connection state of the WebSocket
    /// </summary>
    /// <returns>Returns the connection state of the WebSocket</returns>
    public WebSocketState State() => _clientWebSocket == null ? WebSocketState.None : _clientWebSocket.State;

    internal readonly Channel<MessageToSend> _sendChannel = System.Threading.Channels.Channel
       .CreateUnbounded<MessageToSend>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true, });

    internal Uri GetUri(LiveSchema queryParameters)
    {
        var baseUrl = GetBaseUrl(_deepgramClientOptions);
        var query = QueryParameterUtil.GetParameters(queryParameters);

        /* Unmerged change from project 'Deepgram (net6.0)'
        Before:
                return new Uri(new Uri(baseUrl), new Uri($"{Common.Defaults.API_VERSION}/{UriSegments.LISTEN}?{query}"));
        After:
                return new Uri(new Uri(baseUrl), new Uri($"{Defaults.API_VERSION}/{UriSegments.LISTEN}?{query}"));
        */
        return new Uri(new Uri(baseUrl), new Uri($"{Constants.Defaults.API_VERSION}/{UriSegments.LISTEN}?{query}"));
    }

    public void KeepAlive()
    {
        var keepAliveMessage = JsonSerializer.Serialize(new { type = "KeepAlive" });
        var keepAliveBytes = Encoding.Default.GetBytes(keepAliveMessage);

        EnqueueForSending(new MessageToSend(keepAliveBytes, WebSocketMessageType.Text));
    }

    internal static string GetBaseUrl(DeepgramClientOptions deepgramClientOptions)
    {
        var baseAddress = deepgramClientOptions.BaseAddress;
        //checks for ws:// wss:// ws wss - wss:// is include to ensure it is all stripped out and correctly formatted
        Regex regex = new Regex(@"\b(ws:\/\/|wss:\/\/|ws|wss)\b", RegexOptions.IgnoreCase);
        if (regex.IsMatch(baseAddress))
            return regex.Replace(baseAddress, "wss://");
        else
            //if no protocol in the address then https:// is added
            return $"wss://{baseAddress}";
    }

    #endregion

    #region Dispose

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

    #endregion
}
