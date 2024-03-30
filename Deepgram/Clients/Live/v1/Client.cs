// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.Tracing;
using System.Net.WebSockets;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Live.v1;

namespace Deepgram.Clients.Live.v1;

/// <summary>
/// Implements version 1 of the Live Client.
/// </summary>
public class Client : Attribute, IDisposable
{
    #region Fields
    private readonly DeepgramWsClientOptions _deepgramClientOptions;

    private ClientWebSocket? _clientWebSocket;
    private CancellationTokenSource _cancellationTokenSource;
    #endregion

    /// <param name="apiKey">Required DeepgramApiKey</param>
    /// <param name="deepgramClientOptions"><see cref="DeepgramWsClientOptions"/> for HttpClient Configuration</param>
    public Client(string? apiKey = null, DeepgramWsClientOptions? options = null)
    {
        Log.Verbose("LiveClient", "ENTER");

        options ??= new DeepgramWsClientOptions(apiKey);
        _deepgramClientOptions = options;

        Log.Debug("LiveClient", $"APIVersion: {options.APIVersion}");
        Log.Debug("LiveClient", $"BaseAddress: {options.BaseAddress}");
        Log.Debug("LiveClient", $"KeepAlive: {options.KeepAlive}");
        Log.Debug("LiveClient", $"options: {options.OnPrem}");
        Log.Verbose("LiveClient", "LEAVE");
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

    /// <summary>
    /// Connect to a Deepgram API Web Socket to begin transcribing audio
    /// </summary>
    /// <param name="options">Options to use when transcribing audio</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task Connect(LiveSchema options, CancellationTokenSource? cancellationToken = null, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null)
    {
        Log.Verbose("LiveClient.Connect", "ENTER");
        Log.Information("Connect", $"options: {options}");
        Log.Debug("Connect", $"addons: {addons}");

        // check if the client is disposed
        if (_clientWebSocket != null)
        {
            // client has already connected
            var exStr = "Client has already been initialized";
            Log.Error("Connect", exStr);
            throw new InvalidOperationException(exStr);
        }

        // create client
        _clientWebSocket = new ClientWebSocket();

        // set headers
        _clientWebSocket.Options.SetRequestHeader("Authorization", $"token {_deepgramClientOptions.ApiKey}");
        if (_deepgramClientOptions.Headers is not null) {
            foreach (var header in _deepgramClientOptions.Headers)
            {
                var tmp = header.Key.ToLower();
                if (!(tmp.Contains("password") || tmp.Contains("token") || tmp.Contains("authorization") || tmp.Contains("auth")))
                {
                    Log.Debug("PutAsync<S, T>", $"Add Header {header.Key}={header.Value}");
                }
                _clientWebSocket.Options.SetRequestHeader(header.Key, header.Value);
            }
        }
        if (headers is not null)
        {
            foreach (var header in headers)
            {
                var tmp = header.Key.ToLower();
                if (!(tmp.Contains("password") || tmp.Contains("token") || tmp.Contains("authorization") || tmp.Contains("auth")))
                {
                    Log.Debug("PutAsync<S, T>", $"Add Header {header.Key}={header.Value}");
                }
                _clientWebSocket.Options.SetRequestHeader(header.Key, header.Value);
            }
        }

        // cancelation token
        if (cancellationToken != null)
        {
            Log.Information("Connect", "Using provided cancellation token");
            _cancellationTokenSource = cancellationToken;
        } else
        {
            Log.Information("Connect", "Using default cancellation token");
            _cancellationTokenSource = new CancellationTokenSource();
        }

        try
        {
            var _uri = GetUri(_deepgramClientOptions, options, addons);
            Log.Debug("Connect", $"uri: {_uri}");

            Log.Debug("Connect", "Connecting to Deepgram API...");
            await _clientWebSocket.ConnectAsync(_uri, _cancellationTokenSource.Token).ConfigureAwait(false);

            Log.Debug("Connect", "Starting Sender Thread...");
            StartSenderBackgroundThread();

            Log.Debug("Connect", "Starting Receiver Thread...");
            StartReceiverBackgroundThread();

            if (_deepgramClientOptions.KeepAlive)
            {
                Log.Debug("Connect", "Starting KeepAlive Thread...");
                StartKeepAliveBackgroundThread();
            }

            // send a OpenResponse event
            if (_openReceived != null)
            {
                Log.Debug("Connect", "Sending OpenResponse event...");
                var data = new OpenResponse();
                data.Type = LiveType.Open;
                _openReceived.Invoke(null, data);
            }

            Log.Debug("Connect", "Connect Succeeded");
        }
        catch (Exception ex)
        {
            Log.Error("Connect", $"Excepton: {ex}");
        }

        void StartSenderBackgroundThread() => _ = Task.Factory.StartNew(
            _ => ProcessSendQueue(),
                TaskCreationOptions.LongRunning);

        void StartReceiverBackgroundThread() => _ = Task.Factory.StartNew(
                _ => ProcessReceiveQueue(),
                TaskCreationOptions.LongRunning);

        void StartKeepAliveBackgroundThread() => _ = Task.Factory.StartNew(
                _ => ProcessKeepAlive(),
                TaskCreationOptions.LongRunning);

        Log.Verbose("LiveClient.Connect", "LEAVE");
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
            Log.Error("EnqueueSendMessage", $"Excepton: {ex}");
        }
    }

    internal async Task ProcessSendQueue()
    {
        Log.Verbose("ProcessSendQueue", "ENTER");

        if (_clientWebSocket == null)
        {
            var exStr = "Attempting to start a sender queue when the WebSocket has been disposed is not allowed.";
            Log.Error("EnqueueSendMessage", exStr);
            Log.Verbose("ProcessSendQueue", "LEAVE");

            throw new InvalidOperationException(exStr);
        }

        try
        {
            while (await _sendChannel.Reader.WaitToReadAsync(_cancellationTokenSource.Token))
            {
                Log.Verbose("ProcessSendQueue", "Reading message of queue...");
                while (_sendChannel.Reader.TryRead(out var message))
                {
                    Log.Verbose("ProcessSendQueue", $"Sending message..."); // TODO: dump this message
                    await _clientWebSocket.SendAsync(message.Message, message.MessageType, true, _cancellationTokenSource.Token).ConfigureAwait(false);
                }
            }

            Log.Verbose("ProcessSendQueue", "Succeeded");
            Log.Verbose("ProcessSendQueue", "LEAVE");
            return;
        }
        catch (OperationCanceledException ex)
        {
            Log.Debug("ProcessSendQueue", $"SendThread cancelled.");
            Log.Verbose("ProcessSendQueue", $"SendThread cancelled. Info: {ex}");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessSendQueue", $"Excepton: {ex}");
            Log.Verbose("ProcessSendQueue", "LEAVE");
            throw;
        }

        Log.Verbose("ProcessSendQueue", "LEAVE");
    }

    internal async void ProcessKeepAlive()
    {
        Log.Verbose("ProcessKeepAlive", "ENTER");

        try
        {
            while(true)
            {
                Log.Verbose("ProcessKeepAlive", "Waiting for KeepAlive...");
                await Task.Delay(5000, _cancellationTokenSource.Token);

                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Log.Information("ProcessKeepAlive", "KeepAliveThread cancelled");
                    break;
                }

                Log.Debug("ProcessKeepAlive", "Sending KeepAlive");
                byte[] array = Encoding.ASCII.GetBytes("{\"type\": \"KeepAlive\"}");
                await _clientWebSocket.SendAsync(new ArraySegment<byte>(array), WebSocketMessageType.Text, true, _cancellationTokenSource.Token)
                    .ConfigureAwait(false);
            }

            Log.Verbose("ProcessKeepAlive", "Exiting");
            Log.Verbose("ProcessKeepAlive", "LEAVE");
            return;
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("ProcessKeepAlive", $"KeepAliveThread cancelled.");
            Log.Verbose("ProcessKeepAlive", $"KeepAliveThread cancelled. Info: {ex}");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessKeepAlive", $"Excepton: {ex}");
            Log.Verbose("ProcessKeepAlive", "LEAVE");
            throw;
        }

        Log.Verbose("ProcessKeepAlive", "LEAVE");
    }

    internal async Task ProcessReceiveQueue()
    {
        Log.Verbose("ProcessReceiveQueue", "ENTER");

        while (_clientWebSocket?.State == WebSocketState.Open)
        {
            try
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Log.Information("ProcessReceiveQueue", "ReceiveThread cancelled");
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
                        Log.Information("ProcessReceiveQueue", "Received Close message");
                        break;
                    }

                    Log.Verbose("ProcessReceiveQueue", $"Received message: {result} / {ms}");
                    ProcessDataReceived(result, ms);
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Log.Information("ProcessReceiveQueue", "Received WebSocket Close");
                    await Stop();
                    break;
                }
            }
            catch (TaskCanceledException ex)
            {
                Log.Debug("ProcessReceiveQueue", $"ReceiveThread cancelled.");
                Log.Verbose("ProcessReceiveQueue", $"ReceiveThread cancelled. Info: {ex}");
            }
            catch (Exception ex)
            {
                Log.Error("ProcessReceiveQueue", $"Excepton: {ex}");
                Log.Verbose("ProcessReceiveQueue", "LEAVE");
                throw;
            }
        }

        Log.Verbose("ProcessReceiveQueue", "Succeeded");
        Log.Verbose("ProcessReceiveQueue", "LEAVE");
    }

    internal void ProcessDataReceived(WebSocketReceiveResult result, MemoryStream ms)
    {
        Log.Verbose("ProcessDataReceived", "ENTER");

        ms.Seek(0, SeekOrigin.Begin);

        if (result.MessageType != WebSocketMessageType.Text)
        {
            Log.Warning("ProcessDataReceived", "Received a text message. This is not supported.");
            Log.Verbose("ProcessDataReceived", "LEAVE");
            return;
        }

        var response = Encoding.UTF8.GetString(ms.ToArray());
        if (response == null)
        {
            Log.Warning("ProcessDataReceived", "Response is null");
            Log.Verbose("ProcessDataReceived", "LEAVE");
            return;
        }

        try
        {
            Log.Verbose("ProcessDataReceived", $"raw response: {response}");
            var data = JsonDocument.Parse(response);
            var val = Enum.Parse(typeof(LiveType), data.RootElement.GetProperty("type").GetString()!);

            Log.Verbose("ProcessDataReceived", $"Type: {val}");
            switch (val)
            {         
                case LiveType.Open:
                    //var openResponse = new ResponseEvent<OpenResponse>(data.Deserialize<OpenResponse>());
                    var openResponse = data.Deserialize<OpenResponse>();
                    if (_openReceived == null)
                    {
                        Log.Debug("ProcessDataReceived", "_openReceived has no listeners");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }
                    if (openResponse == null)
                    {
                        Log.Warning("ProcessDataReceived", "OpenResponse is invalid");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessDataReceived", $"Invoking OpenResponse. event: {openResponse}");
                    _openReceived.Invoke(null, openResponse);
                    //InvokeResponseReceived(_openReceived, openResponse);
                    break;
                case LiveType.Results:
                    //var eventResponse = new ResponseEvent<ResultResponse>(data.Deserialize<ResultResponse>());
                    var resultResponse = data.Deserialize<ResultResponse>();
                    if (_resultsReceived == null)
                    {
                        Log.Debug("ProcessDataReceived", "_resultsReceived has no listeners");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }
                    if ( resultResponse == null)
                    {
                        Log.Warning("ProcessDataReceived", "ResultResponse is invalid");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessDataReceived", $"Invoking ResultsResponse. event: {resultResponse}");
                    _resultsReceived.Invoke(null, resultResponse);
                    //InvokeResponseReceived(_resultsReceived, eventResponse);
                    break;
                case LiveType.Metadata:
                    //var metadataResponse = new ResponseEvent<MetadataResponse>(data.Deserialize<MetadataResponse>());
                    var metadataResponse = data.Deserialize<MetadataResponse>();
                    if (_metadataReceived == null)
                    {
                        Log.Debug("ProcessDataReceived", "_metadataReceived has no listeners");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }
                    if (metadataResponse == null)
                    {
                        Log.Warning("ProcessDataReceived", "MetadataResponse is invalid");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessDataReceived", $"Invoking MetadataResponse. event: {metadataResponse}");
                    _metadataReceived.Invoke(null, metadataResponse);
                    //InvokeResponseReceived(_metadataReceived, metadataResponse);
                    break;
                case LiveType.UtteranceEnd:
                    //var utteranceEndResponse = new ResponseEvent<UtteranceEndResponse>(data.Deserialize<UtteranceEndResponse>());
                    var utteranceEndResponse = data.Deserialize<UtteranceEndResponse>();
                    if (_utteranceEndReceived == null)
                    {
                        Log.Debug("ProcessDataReceived", "_utteranceEndReceived has no listeners");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }
                    if ( utteranceEndResponse == null)
                    {
                        Log.Warning("ProcessDataReceived", "UtteranceEndResponse is invalid");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessDataReceived", $"Invoking UtteranceEndResponse. event: {utteranceEndResponse}");
                    _utteranceEndReceived.Invoke(null, utteranceEndResponse);
                    //InvokeResponseReceived(_utteranceEndReceived, utteranceEndResponse);
                    break;
                case LiveType.SpeechStarted:
                    //var speechStartedResponse = new ResponseEvent<SpeechStartedResponse>(data.Deserialize<SpeechStartedResponse>());
                    var speechStartedResponse = data.Deserialize<SpeechStartedResponse>();
                    if (_speechStartedReceived == null)
                    {
                        Log.Debug("ProcessDataReceived", "_speechStartedReceived has no listeners");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }
                    if (speechStartedResponse == null)
                    {
                        Log.Warning("ProcessDataReceived", "SpeechStartedResponse is invalid");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessDataReceived", $"Invoking SpeechStartedResponse. event: {speechStartedResponse}");
                    _speechStartedReceived.Invoke(null, speechStartedResponse);
                    //InvokeResponseReceived(_speechStartedReceived, speechStartedResponse);
                    break;
                case LiveType.Close:
                    //var closeResponse = new ResponseEvent<CloseResponse>(data.Deserialize<CloseResponse>());
                    var closeResponse = data.Deserialize<CloseResponse>();
                    if (_closeReceived == null)
                    {
                        Log.Debug("ProcessDataReceived", "_closeReceived has no listeners");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }
                    if (closeResponse == null)
                    {
                        Log.Warning("ProcessDataReceived", "CloseResponse is invalid");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessDataReceived", $"Invoking CloseResponse. event: {closeResponse}");
                    _closeReceived.Invoke(null, closeResponse);
                    //InvokeResponseReceived(_closeReceived, closeResponse);
                    break;
                case LiveType.Error:
                    //var errorResponse = new ResponseEvent<ErrorResponse>(data.Deserialize<ErrorResponse>());
                    var errorResponse = data.Deserialize<ErrorResponse>();
                    if (_errorReceived == null)
                    {
                        Log.Debug("ProcessDataReceived", "_errorReceived has no listeners");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }
                    if (errorResponse == null)
                    {
                        Log.Warning("ProcessDataReceived", "ErrorResponse is invalid");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessDataReceived", $"Invoking ErrorResponse. event: {errorResponse}");
                    _errorReceived.Invoke(null, errorResponse);
                    //InvokeResponseReceived(_errorReceived, errorResponse);
                    break;
                default:
                    if (_unhandledReceived == null)
                    {
                        Log.Debug("ProcessDataReceived", "_unhandledReceived has no listeners");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }

                    //var unhandledResponse = new ResponseEvent<UnhandledResponse>(data.Deserialize<UnhandledResponse>());
                    var unhandledResponse = new UnhandledResponse();
                    unhandledResponse.Type = LiveType.Unhandled;
                    unhandledResponse.Raw = response;

                    Log.Debug("ProcessDataReceived", $"Invoking UnhandledResponse. event: {unhandledResponse}");
                    _unhandledReceived.Invoke(null, unhandledResponse);
                    //InvokeResponseReceived(_unhandledReceived, unhandledResponse);
                    break;
            }

            Log.Debug("ProcessDataReceived", "Succeeded");
            Log.Verbose("ProcessDataReceived", "LEAVE");
            return;
        }
        catch (JsonException ex)
        {
            Log.Error("ProcessDataReceived", $"JsonException: {ex}");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessDataReceived", $"Excepton: {ex}");
        }

        Log.Verbose("ProcessDataReceived", "LEAVE");
    }

    /// <summary>
    /// Closes the Web Socket connection to the Deepgram API
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task Stop(CancellationTokenSource? cancellationToken = null)
    {
        Log.Verbose("Stop", "ENTER");

        var cancelToken = _cancellationTokenSource.Token;
        if (cancellationToken != null)
        {
            Log.Verbose("Stop", "Using provided cancellation token");
            cancelToken = cancellationToken.Token;
        } 

        // client is already disposed
        if (_clientWebSocket == null)
        {
            Log.Warning("Stop", "Client has already been disposed");
            Log.Verbose("Stop", "LEAVE");
            return;
        }

        // send the close message and flush transcription message
        try {
            if (_clientWebSocket!.State == WebSocketState.Open)
            {
                Log.Debug("Stop", "Sending Close message...");

                // send a close to Deepgram
                await _clientWebSocket.SendAsync(new ArraySegment<byte>([]), WebSocketMessageType.Binary, true, cancelToken)
                    .ConfigureAwait(false);

                // send a CloseResponse event
                if (_closeReceived != null)
                {
                    Log.Debug("Stop", "Sending CloseResponse event...");
                    var data = new CloseResponse();
                    data.Type = LiveType.Close;
                    _closeReceived.Invoke(null, data);
                }
            }

            // attempt to stop the connection
            if (_clientWebSocket!.State != WebSocketState.Closed)
            {
                Log.Debug("Stop", "Closing WebSocket connection...");
                await _clientWebSocket.CloseOutputAsync(
                    WebSocketCloseStatus.NormalClosure,
                    string.Empty,
                    cancelToken)
                    .ConfigureAwait(false);
            }

            // Always request cancellation to the local token source, if some function has been called without a token
            if (cancellationToken != null)
            {
                Log.Debug("Stop", "Cancelling provided cancellation token...");
                cancellationToken.Cancel();
            }

            Log.Debug("Stop", "Disposing WebSocket connection...");
            _cancellationTokenSource.Cancel();

            Log.Debug("Stop", "Succeeded");
            Log.Verbose("Stop", "LEAVE");
            return;
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("Stop", $"Stop cancelled.");
            Log.Verbose("Stop", $"Stop cancelled. Info: {ex}");
        }
        catch (Exception ex)
        {
            Log.Error("Stop", $"Excepton: {ex}");
        }

        _clientWebSocket = null;
        Log.Verbose("Stop", "LEAVE");
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
    /// Handle channel options
    /// </summary> 
    internal readonly Channel<WebSocketMessage> _sendChannel = System.Threading.Channels.Channel
       .CreateUnbounded<WebSocketMessage>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true, });

    /// <summary>
    /// Get the URI for the WebSocket connection
    /// </summary> 
    internal static Uri GetUri(DeepgramWsClientOptions options, LiveSchema parameter, Dictionary<string, string>? addons = null)
    {
        var propertyInfoList = parameter.GetType()
            .GetProperties()
            .Where(v => v.GetValue(parameter) is not null);

        var queryString = QueryParameterUtil.UrlEncode(parameter, propertyInfoList, addons);

        return new Uri($"{options.BaseAddress}/{UriSegments.LISTEN}?{queryString}");
    }
    #endregion

    #region Dispose
    /// <summary>
    /// Disposes of the resources used by the client
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
