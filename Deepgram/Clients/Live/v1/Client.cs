// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.Tracing;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Live.v1;

namespace Deepgram.Clients.Live.v1;

/// <summary>
/// Implements version 1 of the Live Client.
/// </summary>
public class Client : Attribute, IDisposable
{
    #region Fields
    internal ILogger logger => LogProvider.GetLogger(this.GetType().Name);
    internal readonly DeepgramWsClientOptions _deepgramClientOptions;

    internal ClientWebSocket? _clientWebSocket;
    internal CancellationTokenSource _cancellationTokenSource;
    #endregion

    /// <param name="apiKey">Required DeepgramApiKey</param>
    /// <param name="deepgramClientOptions"><see cref="DeepgramWsClientOptions"/> for HttpClient Configuration</param>
    public Client(string? apiKey = null, DeepgramWsClientOptions? options = null)
    {
        options ??= new DeepgramWsClientOptions(apiKey);
        _deepgramClientOptions = options;
    }

    #region Subscribe Events
    /// <summary>
    /// Fires when an event is received from the Deepgram API
    /// </summary>
    public event EventHandler<OpenResponse>? _openReceived;
    public event EventHandler<MetadataResponse>? _metadataReceived;
    public event EventHandler<ResultResponse>? _resultsReceived;
    public event EventHandler<UtteranceEndResponse>? _utteranceEndReceived;
    public event EventHandler<SpeechStartedResponse>? _speechStartedReceived;
    public event EventHandler<CloseResponse>? _closeReceived;
    public event EventHandler<UnhandledResponse>? _unhandledReceived;
    public event EventHandler<ErrorResponse>? _errorReceived;
    #endregion

    //TODO when a response is received check if it is a transcript(LiveTranscriptionEvent) or metadata (LiveMetadataEvent) response 

    /// <summary>
    /// Connect to a Deepgram API Web Socket to begin transcribing audio
    /// </summary>
    /// <param name="options">Options to use when transcribing audio</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task Connect(LiveSchema options, CancellationTokenSource? cancellationToken = null, Dictionary<string, string>? addons = null, Dictionary<string, string>? headers = null)
    {
        // check if the client is disposed
        if (_clientWebSocket != null)
        {
            // client has already connected
            // TODO: logging
            var ex = new Exception("Client has already been initialized");
            ProcessException("Connect", ex);
            throw ex;
        }

        // create client
        _clientWebSocket = new ClientWebSocket();

        // set headers
        
        _clientWebSocket.Options.SetRequestHeader("Authorization", $"token {_deepgramClientOptions.ApiKey}");
        if (_deepgramClientOptions.Headers is not null) {
            foreach (var header in _deepgramClientOptions.Headers)
            {
                _clientWebSocket.Options.SetRequestHeader(header.Key, header.Value);
            }
        }
        if (headers is not null)
        {
            foreach (var header in headers)
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
            var _uri = GetUri(_deepgramClientOptions, options, addons);
            Console.WriteLine(_uri); // TODO: logging
            await _clientWebSocket.ConnectAsync(_uri, _cancellationTokenSource.Token).ConfigureAwait(false);
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

    //// TODO: convienence method for subscribing to events
    //public void On<T>(T e, EventHandler<T> eventHandler) {
    //    switch (e)
    //       {
    //        case OpenResponse open:
    //            OpenReceived += (sender, e) => eventHandler;
    //            break;
    //        case MetadataResponse metadata:
    //            MetadataReceived += (sender, e) => eventHandler;
    //            break;
    //        case ResultResponse result:
    //            ResultsReceived += (sender, e) => eventHandler;
    //            break;
    //        case UtteranceEndResponse utteranceEnd:
    //            UtteranceEndReceived += (sender, e) => eventHandler;
    //            break;
    //        case SpeechStartedResponse speechStarted:
    //            SpeechStartedReceived += (sender, e) => eventHandler;
    //            break;
    //        case CloseResponse close:
    //            CloseReceived += (sender, e) => eventHandler;
    //            break;
    //        case UnhandledResponse unhandled:
    //            UnhandledReceived += (sender, e) => eventHandler;
    //            break;
    //        case ErrorResponse error:
    //            ErrorReceived += (sender, e) => eventHandler;
    //            break;
    //    }
    //}

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
        if (_clientWebSocket == null)
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
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    // TODO: logging
                    break;
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

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Log.RequestedSocketClose(logger, result.CloseStatusDescription!);
                        break;
                    }

                    // TODO: replace with fine grained event handling
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

                    var data = JsonDocument.Parse(response);
                    var val = Enum.Parse(typeof(LiveType), data.RootElement.GetProperty("type").GetString()!);

                    switch (val)
                    {         
                        case LiveType.Open:
                            //var openResponse = new ResponseEvent<OpenResponse>(data.Deserialize<OpenResponse>());
                            var openResponse = data.Deserialize<OpenResponse>();
                            if (_openReceived == null || openResponse == null)
                            {
                                return;
                            }
                            _openReceived.Invoke(null, data.Deserialize<OpenResponse>());
                            //InvokeResponseReceived(_openReceived, openResponse);
                            break;
                        case LiveType.Results:
                            //var eventResponse = new ResponseEvent<ResultResponse>(data.Deserialize<ResultResponse>());
                            var eventResponse = data.Deserialize<ResultResponse>();
                            if (_resultsReceived == null || eventResponse == null)
                            {
                                return;
                            }
                            _resultsReceived.Invoke(null, data.Deserialize<ResultResponse>());
                            //InvokeResponseReceived(_resultsReceived, eventResponse);
                            break;
                        case LiveType.Metadata:
                            //var metadataResponse = new ResponseEvent<MetadataResponse>(data.Deserialize<MetadataResponse>());
                            var metadataResponse = data.Deserialize<MetadataResponse>();
                            if (_metadataReceived == null || metadataResponse == null)
                            {
                                return;
                            }
                            _metadataReceived.Invoke(null, data.Deserialize<MetadataResponse>());
                            //InvokeResponseReceived(_metadataReceived, metadataResponse);
                            break;
                        case LiveType.UtteranceEnd:
                            //var utteranceEndResponse = new ResponseEvent<UtteranceEndResponse>(data.Deserialize<UtteranceEndResponse>());
                            var utteranceEndResponse = data.Deserialize<UtteranceEndResponse>();
                            if (_utteranceEndReceived == null || utteranceEndResponse == null)
                            {
                                return;
                            }
                            _utteranceEndReceived.Invoke(null, data.Deserialize<UtteranceEndResponse>());
                            //InvokeResponseReceived(_utteranceEndReceived, utteranceEndResponse);
                            break;
                        case LiveType.SpeechStarted:
                            //var speechStartedResponse = new ResponseEvent<SpeechStartedResponse>(data.Deserialize<SpeechStartedResponse>());
                            var speechStartedResponse = data.Deserialize<SpeechStartedResponse>();
                            if (_speechStartedReceived == null || speechStartedResponse == null)
                            {
                                return;
                            }
                            _speechStartedReceived.Invoke(null, data.Deserialize<SpeechStartedResponse>());
                            //InvokeResponseReceived(_speechStartedReceived, speechStartedResponse);
                            break;
                        case LiveType.Close:
                            //var closeResponse = new ResponseEvent<CloseResponse>(data.Deserialize<CloseResponse>());
                            var closeResponse = data.Deserialize<CloseResponse>();
                            if (_closeReceived == null || closeResponse == null)
                            {
                                return;
                            }
                            _closeReceived.Invoke(null, data.Deserialize<CloseResponse>());
                            //InvokeResponseReceived(_closeReceived, closeResponse);
                            break;
                        case LiveType.Error:
                            //var errorResponse = new ResponseEvent<ErrorResponse>(data.Deserialize<ErrorResponse>());
                            var errorResponse = data.Deserialize<ErrorResponse>();
                            if (_errorReceived == null || errorResponse == null)
                            {
                                return;
                            }
                            _errorReceived.Invoke(null, data.Deserialize<ErrorResponse>());
                            //InvokeResponseReceived(_errorReceived, errorResponse);
                            break;
                        default:
                            if (_unhandledReceived == null)
                            {
                                return;
                            }
                            //var unhandledResponse = new ResponseEvent<UnhandledResponse>(data.Deserialize<UnhandledResponse>());
                            var unhandledResponse = new UnhandledResponse();
                            unhandledResponse.Type = LiveType.Unhandled;
                            unhandledResponse.Raw = response;

                            _unhandledReceived.Invoke(null, unhandledResponse);
                            //InvokeResponseReceived(_unhandledReceived, unhandledResponse);
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
    public async Task Stop(CancellationTokenSource? cancellationToken = null)
    {
        var cancelToken = _cancellationTokenSource.Token;
        if (cancellationToken != null)
        {
            cancelToken = cancellationToken.Token;
        } 

        // client is already disposed
        if (_clientWebSocket == null)
        {
            // TODO: logging
            return;
        }

        // send the close message and flush transcription message
        if (_clientWebSocket!.State == WebSocketState.Open)
        {
            await _clientWebSocket.SendAsync(new ArraySegment<byte>([]), WebSocketMessageType.Binary, true, cancelToken)
            .ConfigureAwait(false);
        }

        // attempt to stop the connection
        try
        {
            if (_clientWebSocket!.CloseStatus.HasValue)
            {
                Log.ClosingSocket(logger);
            }

            if (_clientWebSocket!.State != WebSocketState.Closed)
            {
                await _clientWebSocket.CloseOutputAsync(
                    WebSocketCloseStatus.NormalClosure,
                    string.Empty,
                    cancelToken)
                    .ConfigureAwait(false);
            }

            // Always request cancellation to the local token source, if some function has been called without a token
            if (cancellationToken != null)
            {
                cancellationToken.Cancel();
            }
            _cancellationTokenSource.Cancel();
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

    /// <summary>
    /// Indicates whether the WebSocket is connected
    /// </summary> 
    /// <returns>Returns true if the WebSocket is connected</returns>
    public bool IsConnected() => _clientWebSocket.State == WebSocketState.Open;

    /// <summary>
    /// TODO
    /// </summary> 
    internal readonly Channel<WebSocketMessage> _sendChannel = System.Threading.Channels.Channel
       .CreateUnbounded<WebSocketMessage>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true, });

    /// <summary>
    /// TODO
    /// </summary> 
    internal static Uri GetUri(DeepgramWsClientOptions options, LiveSchema parameter, Dictionary<string, string>? addons = null)
    {
        var propertyInfoList = parameter.GetType()
            .GetProperties()
            .Where(v => v.GetValue(parameter) is not null);

        var queryString = QueryParameterUtil.UrlEncode(parameter, propertyInfoList, addons);

        return new Uri($"{options.BaseAddress}/{UriSegments.LISTEN}?{queryString}");
    }

    /// <summary>
    /// TODO
    /// </summary> 
    private void ProcessException(string action, Exception ex)
    {
        if (_clientWebSocket == null)
            Log.SocketDisposed(logger, action, ex);
        else
            Log.Exception(logger, action, ex);
        //EventResponseReceived?.Invoke(null, new ResponseEventArgs(new EventResponse() { Error = ex }));
    }
    #endregion

    #region Dispose
    /// <summary>
    /// TODO
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
        }

        if (_sendChannel != null)
        {
            _sendChannel.Writer.Complete();
        }

        _clientWebSocket.Dispose();
        GC.SuppressFinalize(this);
    }

    //internal void InvokeResponseReceived<T>(EventHandler<T> eventHandler, ResponseEvent<T> e)
    //{
    //    if (eventHandler != null)
    //    {
    //        Parallel.ForEach(
    //            eventHandler.GetInvocationList().Cast<EventHandler>(),
    //            (handler) =>
    //                handler(null, e));
    //    }
    //}
    #endregion
}
