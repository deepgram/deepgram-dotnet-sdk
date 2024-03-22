// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Live.v1;

namespace Deepgram.Clients.Live.v1;

/// <summary>
/// Implements version 1 of the Live Client.
/// </summary>
public class Client : IDisposable
{
    #region Fields
    internal ILogger logger => LogProvider.GetLogger(this.GetType().Name);
    internal readonly DeepgramClientOptions _deepgramClientOptions;
    internal readonly string _apiKey;
    internal ClientWebSocket? _clientWebSocket;
    internal CancellationTokenSource _cancellationTokenSource;
    internal bool _isDisposed;
    #endregion

    /// <param name="apiKey">Required DeepgramApiKey</param>
    /// <param name="deepgramClientOptions"><see cref="DeepgramClientOptions"/> for HttpClient Configuration</param>
    public Client(string apiKey = "", DeepgramClientOptions? options = null)
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

    #region Subscribe Events
    /// <summary>
    /// Fires when transcription metadata is received from the Deepgram API
    /// </summary>  
    public event EventHandler<ResponseEventArgs>? EventResponseReceived;
    #endregion

    //TODO when a response is received check if it is a transcript(LiveTranscriptionEvent) or metadata (LiveMetadataEvent) response 

    /// <summary>
    /// Connect to a Deepgram API Web Socket to begin transcribing audio
    /// </summary>
    /// <param name="options">Options to use when transcribing audio</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task Connect(LiveSchema options, CancellationTokenSource? cancellationToken = null, Dictionary<string, string>? addons = null)
    {
        // create client
        _clientWebSocket = new ClientWebSocket();

        // set headers
        _clientWebSocket.Options.SetRequestHeader("Authorization", $"token {_apiKey}");
        if (_deepgramClientOptions.Headers is not null) {
            foreach (var header in _deepgramClientOptions.Headers)
            {
                _clientWebSocket.Options.SetRequestHeader(header.Key, header.Value);
            }
        }

        // cancelation token
        if (cancellationToken != null)
        {
            _cancellationTokenSource = cancellationToken;
        } else
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        try
        {
            await _clientWebSocket.ConnectAsync(GetUri(_deepgramClientOptions, options, addons), _cancellationTokenSource.Token).ConfigureAwait(false);
            StartSenderBackgroundThread();
            StartReceiverBackgroundThread();
        }
        catch (Exception ex)
        {
            ProcessException("Connect", ex);
        }

        void StartSenderBackgroundThread() => _ = Task.Factory.StartNew(
            _ => ProcessSendQueue(),
                TaskCreationOptions.LongRunning);

        void StartReceiverBackgroundThread() => _ = Task.Factory.StartNew(
                _ => ProcessReceiveQueue(),
                TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Sends a binary message over the WebSocket connection.
    /// </summary>
    /// <param name="data">The data to be sent over the WebSocket.</param>
    public void Send(byte[] data) =>
        EnqueueSendMessage(new WebSocketMessage(data, WebSocketMessageType.Binary));

    internal void EnqueueSendMessage(WebSocketMessage message)
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

    internal async Task ProcessSendQueue()
    {
        if (_isDisposed)
        {
            var ex = new Exception(
               "Attempting to start a sender queue when the WebSocket has been disposed is not allowed.");
            ProcessException("ProcessSendQueue", ex);
            throw ex;
        }

        try
        {
            while (await _sendChannel.Reader.WaitToReadAsync(_cancellationTokenSource.Token))
            {
                while (_sendChannel.Reader.TryRead(out var message))
                {
                    await _clientWebSocket.SendAsync(message.Message, message.MessageType, true, _cancellationTokenSource.Token).ConfigureAwait(false);

                }
            }
        }
        catch (Exception ex)
        {
            ProcessException("Process sender queue", ex);
        }
    }

    internal async Task ProcessReceiveQueue()
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
                        result = await _clientWebSocket.ReceiveAsync(buffer, _cancellationTokenSource.Token);
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
                    await Stop();
                    break;
                }
            }
            catch (Exception ex)
            {
                ProcessException("ProcessReceiveQueue", ex);
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
                        case LiveType.SpeechStarted:
                            eventResponse.SpeechStarted = data.Deserialize<SpeechStartedResponse>()!;
                            break;
                    }
                    EventResponseReceived?.Invoke(null, new ResponseEventArgs(eventResponse));
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
    public async Task Stop(CancellationToken? cancellationToken = null)
    {
        // send the close message and flush transcription messages
        if (_clientWebSocket!.State != WebSocketState.Open)
            return;

        await _clientWebSocket.SendAsync(new ArraySegment<byte>([]), WebSocketMessageType.Binary, true, _cancellationTokenSource.Token)
            .ConfigureAwait(false);

        // attempt to stop the connection
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
                        _cancellationTokenSource.Token)
                        .ConfigureAwait(false);
                }

                // Always request cancellation to the local token source, if some function has been called without a token
                _cancellationTokenSource.Cancel();
            }
        }
        catch (Exception ex)
        {
            ProcessException("Stop Connection ", ex);
        }
    }

    #region Helpers
    /// <summary>
    /// Retrieves the connection state of the WebSocket
    /// </summary>
    /// <returns>Returns the connection state of the WebSocket</returns>
    public WebSocketState State() => _clientWebSocket.State;

    internal readonly Channel<WebSocketMessage> _sendChannel = System.Threading.Channels.Channel
       .CreateUnbounded<WebSocketMessage>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true, });

    internal static Uri GetUri(DeepgramClientOptions options, LiveSchema parameter, Dictionary<string, string>? addons = null)
    {
        var baseUrl = GetBaseUrl(options);

        var propertyInfoList = parameter.GetType()
            .GetProperties()
            .Where(v => v.GetValue(parameter) is not null);

        var queryString = QueryParameterUtil.UrlEncode(parameter, propertyInfoList, addons);

        return new Uri($"{baseUrl}/{options.APIVersion}/{UriSegments.LISTEN}?{queryString}");
    }

    internal static string GetBaseUrl(DeepgramClientOptions options)
    {
        string baseAddress = Defaults.DEFAULT_URI;
        if (options.BaseAddress != null)
        {
            baseAddress = options.BaseAddress;
        }

        //checks for ws:// wss:// ws wss - wss:// is include to ensure it is all stripped out and correctly formatted
        Regex regex = new Regex(@"\b(ws:\/\/|wss:\/\/|ws|wss)\b", RegexOptions.IgnoreCase);
        if (!regex.IsMatch(baseAddress))
        {
            //if no protocol in the address then https:// is added
            // TODO: log
            baseAddress = $"wss://{baseAddress}";
        }

        return baseAddress;
    }

    private void ProcessException(string action, Exception ex)
    {
        if (_isDisposed)
            Log.SocketDisposed(logger, action, ex);
        else
            Log.Exception(logger, action, ex);
        EventResponseReceived?.Invoke(null, new ResponseEventArgs(new EventResponse() { Error = ex }));
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
        _sendChannel?.Writer.Complete();
        _clientWebSocket?.Dispose();

        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
    #endregion
}
