using Deepgram.Extensions;
using Deepgram.Models.Live.v1;
using Deepgram.Models.Shared.v1;

namespace Deepgram;

/// <param name="deepgramClientOptions"><see cref="DeepgramClientOptions"/> for HttpClient Configuration</param>
public class LiveClient : IDisposable
{
    public LiveClient(string apiKey, DeepgramClientOptions? deepgramClientOptions = null)
    {
        _apiKey = apiKey;
        if (deepgramClientOptions is null)
        {
            _deepgramClientOptions = new DeepgramClientOptions();
        }
    }
    #region Fields

    internal ILogger logger => LogProvider.GetLogger(this.GetType().Name);
    internal string _apiKey;
    internal readonly DeepgramClientOptions _deepgramClientOptions;
    internal ClientWebSocket? _clientWebSocket;
    internal bool _isDisposed;
    #endregion

    #region Subscribe Events
    /// <summary>
    /// Fires when transcription metadata is received from the Deepgram API
    /// </summary>  
    public event EventHandler<LiveResponseReceivedEventArgs>? LiveResponseReceived;
    #endregion

    //TODO when a response is received check if it is a transcript(LiveTranscriptionEvent) or metadata (LiveMetadataEvent) response 

    /// <summary>
    /// Connect to a Deepgram API Web Socket to begin transcribing audio
    /// </summary>
    /// <param name="options">Options to use when transcribing audio</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task StartConnectionAsync(LiveSchema options, CancellationToken cancellationToken = default)
    {
        _clientWebSocket = new ClientWebSocket()
           .SetHeaders(_apiKey, _deepgramClientOptions);

        try
        {
            await _clientWebSocket.ConnectAsync(
                GetUri(options, _deepgramClientOptions),
                cancellationToken).ConfigureAwait(false);
            StartSenderBackgroundThread();
            StartReceiverBackgroundThread();

        }
        catch (Exception ex)
        {
            ProcessException("StartConnectionAsync", ex);
        }

        void StartSenderBackgroundThread() => _ = Task.Factory.StartNew(
            _ => ProcessSenderQueue(),
                TaskCreationOptions.LongRunning,
                cancellationToken);

        void StartReceiverBackgroundThread() => _ = Task.Factory.StartNew(
                _ => Receive(),
            TaskCreationOptions.LongRunning,
            cancellationToken);
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

    internal async Task ProcessSenderQueue(CancellationToken cancellationToken = default)
    {
        if (_isDisposed)
        {
            var ex = new Exception(
               "Attempting to start a sender queue when the WebSocket has been disposed is not allowed.");
            ProcessException("ProcessSenderQueue", ex);
            throw ex;
        }

        try
        {
            while (await _sendChannel.Reader.WaitToReadAsync(cancellationToken))
            {
                while (_sendChannel.Reader.TryRead(out var message))
                {
                    await _clientWebSocket.SendAsync(message.Message, message.MessageType, true, cancellationToken).ConfigureAwait(false);

                }
            }
        }
        catch (Exception ex)
        {
            ProcessException("Process sender queue", ex);
        }
    }

    internal async Task Receive(CancellationToken cancellationToken = default)
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
                        result = await _clientWebSocket.ReceiveAsync(buffer, cancellationToken);
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
                    await StopConnectionAsync(cancellationToken);
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
                    var liveResponse = new LiveResponse();
                    var data = JsonDocument.Parse(response);
                    var val = Enum.Parse(typeof(LiveType), data.RootElement.GetProperty("type").GetString()!);

                    switch (val)
                    {
                        case LiveType.Results:
                            liveResponse.Transcription = data.Deserialize<LiveTranscriptionResponse>()!;
                            break;
                        case LiveType.Metadata:
                            liveResponse.MetaData = data.Deserialize<LiveMetadataResponse>()!;
                            break;
                        case LiveType.UtteranceEnd:
                            liveResponse.UtteranceEnd = data.Deserialize<LiveUtteranceEndResponse>()!;
                            break;
                    }
                    LiveResponseReceived?.Invoke(null, new LiveResponseReceivedEventArgs(liveResponse));
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
    public async Task StopConnectionAsync(CancellationToken cancellationToken = default)
    {

        try
        {
            if (_clientWebSocket!.CloseStatus.HasValue)
            {
                Log.ClosingSocket(logger);
            }

            if (!_isDisposed)
            {
                if (_clientWebSocket!.State != WebSocketState.Closed)
                {
                    await _clientWebSocket.CloseOutputAsync(
                        WebSocketCloseStatus.NormalClosure,
                        string.Empty,
                        cancellationToken)
                        .ConfigureAwait(false);
                }
            }
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
    public async Task FinishAsync(CancellationToken cancellationToken = default)
    {

        if (_clientWebSocket!.State != WebSocketState.Open)
            return;

        await _clientWebSocket.SendAsync(
            new ArraySegment<byte>([]),
            WebSocketMessageType.Binary,
            true,
            cancellationToken)
            .ConfigureAwait(false);
    }

    #region Helpers

    /// <summary>
    /// Retrieves the connection state of the WebSocket
    /// </summary>
    /// <returns>Returns the connection state of the WebSocket</returns>
    public WebSocketState State() => _clientWebSocket.State;

    internal readonly Channel<MessageToSend> _sendChannel = System.Threading.Channels.Channel
       .CreateUnbounded<MessageToSend>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true, });

    internal static Uri GetUri(LiveSchema queryParameters, DeepgramClientOptions? deepgramClientOptions)
    {
        var baseUrl = GetBaseUrl(deepgramClientOptions);
        var query = QueryParameterUtil.GetParameters(queryParameters);
        // format of URI cannot be determined if run like --
        // return new Uri(new Uri(baseUrl), new Uri($"{Defaults.API_VERSION}/{UriSegments.LISTEN}?{query}"));
        return new Uri($"{baseUrl}/{deepgramClientOptions.APIVersion}/{UriSegments.LISTEN}?{query}");
    }

    private void ProcessException(string action, Exception ex)
    {
        if (_isDisposed)
            Log.SocketDisposed(logger, action, ex);
        else
            Log.Exception(logger, action, ex);
        LiveResponseReceived?.Invoke(null, new LiveResponseReceivedEventArgs(new LiveResponse() { Error = ex }));

    }



    internal static string GetBaseUrl(DeepgramClientOptions? deepgramClientOptions)
    {
        string baseAddress = Defaults.DEFAULT_URI;
        if (deepgramClientOptions is not null && deepgramClientOptions.BaseAddress is not null)
        {
            baseAddress = deepgramClientOptions.BaseAddress;
        }


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
        if (_isDisposed)
            return;

        _sendChannel?.Writer.Complete();
        _clientWebSocket?.Dispose();
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }

    #endregion
}
