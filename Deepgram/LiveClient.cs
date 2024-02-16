// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Live.v1;

namespace Deepgram;

/// <param name="deepgramClientOptions"><see cref="DeepgramClientOptions"/> for HttpClient Configuration</param>
public class LiveClient : IDisposable
{
    public LiveClient(string apiKey = "", DeepgramClientOptions? options = null)
    {
        options ??= new DeepgramClientOptions();

        // user provided takes precedence
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            // then try the environment variable
            // TODO: log
            apiKey = Environment.GetEnvironmentVariable(variable: Defaults.DEEPGRAM_API_KEY) ?? "";
            if (string.IsNullOrEmpty(apiKey))
            {
                // TODO: log
            }
        }
        if (!options.OnPrem && string.IsNullOrEmpty(apiKey))
        {
            // TODO: log
            throw new ArgumentException("Deepgram API Key is invalid");
        }

        // housekeeping
        _deepgramClientOptions = options;
        _apiKey = apiKey;
    }
    #region Fields

    internal ILogger logger => LogProvider.GetLogger(this.GetType().Name);
    internal readonly DeepgramClientOptions _deepgramClientOptions;
    internal readonly string _apiKey;
    internal ClientWebSocket? _clientWebSocket;
    internal readonly CancellationTokenSource _tokenSource = new();
    internal bool _isDisposed;
    #endregion

    #region Subscribe Events
    /// <summary>
    /// Fires when transcription metadata is received from the Deepgram API
    /// </summary>  
    public event EventHandler<ResponseReceivedEventArgs>? EventResponseReceived;
    #endregion

    //TODO when a response is received check if it is a transcript(LiveTranscriptionEvent) or metadata (LiveMetadataEvent) response 

    /// <summary>
    /// Connect to a Deepgram API Web Socket to begin transcribing audio
    /// </summary>
    /// <param name="options">Options to use when transcribing audio</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task StartConnectionAsync(LiveSchema options, CancellationToken? cancellationToken = null)
    {
        // create client
        _clientWebSocket = new ClientWebSocket();

        // set headers
        _clientWebSocket.Options.SetRequestHeader("Authorization", $"token {_apiKey}");
        foreach (var header in _deepgramClientOptions.Headers)
        {
            _clientWebSocket.Options.SetRequestHeader(header.Key, header.Value);
        }

        // cancelation token
        var cancelToken = cancellationToken ?? _tokenSource.Token;

        try
        {
            await _clientWebSocket.ConnectAsync(GetUri(options, _deepgramClientOptions),cancelToken).ConfigureAwait(false);
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
                    var eventResponse = new EventResponse();
                    var data = JsonDocument.Parse(response);
                    var val = Enum.Parse(typeof(LiveType), data.RootElement.GetProperty("type").GetString()!);

                    switch (val)
                    {
                        case LiveType.Results:
                            eventResponse.Transcription = data.Deserialize<TranscriptionResponse>()!;
                            break;
                        case LiveType.Metadata:
                            eventResponse.MetaData = data.Deserialize<MetadataResponse>()!;
                            break;
                        case LiveType.UtteranceEnd:
                            eventResponse.UtteranceEnd = data.Deserialize<UtteranceEndResponse>()!;
                            break;
                    }
                    EventResponseReceived?.Invoke(null, new ResponseReceivedEventArgs(eventResponse));
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

            if (!_isDisposed)
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
    public WebSocketState State() => _clientWebSocket.State;

    internal readonly Channel<MessageToSend> _sendChannel = System.Threading.Channels.Channel
       .CreateUnbounded<MessageToSend>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true, });

    internal static Uri GetUri(LiveSchema queryParameters, DeepgramClientOptions options)
    {
        var baseUrl = GetBaseUrl(options);
        var query = QueryParameterUtil.GetParameters(queryParameters);

        return new Uri($"{baseUrl}/{options.APIVersion}/{UriSegments.LISTEN}?{query}");
    }

    internal static string GetBaseUrl(DeepgramClientOptions options)
    {
        string baseAddress = Defaults.DEFAULT_URI;
        if (options.BaseAddress is not null)
        {
            baseAddress = options.BaseAddress;
        }

        //checks for ws:// wss:// ws wss - wss:// is include to ensure it is all stripped out and correctly formatted
        Regex regex = new Regex(@"\b(ws:\/\/|wss:\/\/|ws|wss)\b", RegexOptions.IgnoreCase);
        if (!regex.IsMatch(baseAddress))
            //if no protocol in the address then https:// is added
            // TODO: log
            baseAddress = $"wss://{baseAddress}";

        return baseAddress;
    }

    private void ProcessException(string action, Exception ex)
    {
        if (_isDisposed)
            Log.SocketDisposed(logger, action, ex);
        else
            Log.Exception(logger, action, ex);
        EventResponseReceived?.Invoke(null, new ResponseReceivedEventArgs(new EventResponse() { Error = ex }));
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _tokenSource.Cancel();
        _tokenSource.Dispose();
        _sendChannel?.Writer.Complete();
        _clientWebSocket?.Dispose();

        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
    #endregion
}
