using Deepgram.Extensions;
using Deepgram.Records.Live;

namespace Deepgram;

public class LiveClient
{
    #region Fields

    internal ILogger logger => LogProvider.GetLogger(this.GetType().Name);
    internal readonly DeepgramClientOptions _deepgramClientOptions;
    internal ClientWebSocket? _clientWebSocket;
    internal readonly CancellationTokenSource _tokenSource = new();
    internal bool _disposed;
    #endregion

    #region Subscribe Events
    /// <summary>
    /// Fires when the WebSocket connection to Deepgram has been opened
    /// </summary>
    public event EventHandler<ConnectionOpenEventArgs>? ConnectionOpened;

    /// <summary>
    /// Fires on any error during sending,receiving or message processing
    /// </summary>
    public event EventHandler<LiveErrorEventArgs>? LiveError;

    /// <summary>
    /// Fires when the WebSocket connection is closed
    /// </summary>
    public event EventHandler<ConnectionClosedEventArgs>? ConnectionClosed;

    /// <summary>
    /// Fires when a transcript is received from the Deepgram API
    /// </summary>
    public event EventHandler<TranscriptReceivedEventArgs>? TranscriptReceived;

    /// <summary>
    /// Fires when transcription metadata is received from the Deepgram API
    /// </summary>
    public event EventHandler<MetadataReceivedEventArgs>? MetadataReceived;

    #endregion

    /// <summary>
    /// Constructor with default Options
    /// </summary>
    /// <param name="apiKey">The key to authenticate with Deepgram</param>

    public LiveClient(string apiKey) : this(new DeepgramClientOptions(apiKey)) { }


    /// <param name="deepgramClientOptions"><see cref="DeepgramClientOptions"/> for HttpClient Configuration</param>
    public LiveClient(DeepgramClientOptions deepgramClientOptions)
    {
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
                GetUri(options, _deepgramClientOptions),
                cancelToken).ConfigureAwait(false);
            StartSenderBackgroundThread();
            StartReceiverBackgroundThread();
            ConnectionOpened?.Invoke(null, new ConnectionOpenEventArgs());
            _disposed = false;
        }
        catch (Exception ex)
        {
            ProcessException("StartConnectionAsync", ex);
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
            ProcessException("Enqueue Message", ex);
        }
    }

    internal async Task ProcessSenderQueue(CancellationToken? cancellationToken = null)
    {
        var cancelToken = cancellationToken ?? _tokenSource.Token;
        if (_disposed)
        {
            var ex = new Exception(
               "Attempting to start a sender queue when the WebSocket has been disposed is not allowed.");
            ProcessException("ProcessSenderQueue", ex);
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
        catch (Exception ex)
        {
            ProcessException("Process sender queue", ex);
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

                    ProcessDataReceived(result, ms);
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await StopConnectionAsync();
                    break;
                }
            }
            catch (Exception ex)
            {
                ProcessException("Receive", ex);
                break;
            }

        }
    }

    internal void ProcessDataReceived(WebSocketReceiveResult result, MemoryStream ms)
    {
        ms.Seek(0, SeekOrigin.Begin);

        if (result.MessageType == WebSocketMessageType.Text)
        {
            var response = Encoding.UTF8.GetString(ms.ToArray());

            if (response != null)
            {
                try
                {
                    var data = JsonDocument.Parse(response);
                    var val = Enum.Parse(typeof(LiveType), data.RootElement.GetProperty("type").GetString());
                    switch (val)
                    {
                        case LiveType.Results:
                            TranscriptReceived?.Invoke(null, new TranscriptReceivedEventArgs(data.Deserialize<LiveTranscriptionResponse>()!));
                            break;
                        case LiveType.Metadata:
                            MetadataReceived?.Invoke(null, new MetadataReceivedEventArgs(data.Deserialize<LiveMetadataResponse>()!));
                            break;

                    }
                }
                catch (Exception ex)
                {
                    ProcessException("Live response received", ex);
                }
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
            ProcessException("Stop Connection ", ex);
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

    internal static Uri GetUri(LiveSchema queryParameters, DeepgramClientOptions deepgramClientOptions)
    {
        var baseUrl = GetBaseUrl(deepgramClientOptions);
        var query = QueryParameterUtil.GetParameters(queryParameters);
        // format of URI cannot be determined if run like --
        // return new Uri(new Uri(baseUrl), new Uri($"{Defaults.API_VERSION}/{UriSegments.LISTEN}?{query}"));
        return new Uri($"{baseUrl}/{Defaults.API_VERSION}/{UriSegments.LISTEN}?{query}");
    }

    private void ProcessException(string action, Exception ex)
    {
        if (_disposed)
            Log.SocketDisposed(logger, action, ex);
        else
            Log.Exception(logger, action, ex);

        LiveError?.Invoke(null, new LiveErrorEventArgs(ex));
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
