// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Abstractions.v2;
using Abstract = Deepgram.Abstractions.v2;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Listen.v2.WebSocket;
using Common = Deepgram.Models.Common.v2.WebSocket;
using Deepgram.Clients.Interfaces.v2;
using Deepgram.Models.Exceptions.v1;

namespace Deepgram.Clients.Listen.v2.WebSocket;

/// <summary>
/// Implements version 2 of the Listen WebSocket Client.
/// </summary>
public class Client : AbstractWebSocketClient, IListenWebSocketClient
{
    #region Fields
    private DateTime? _lastReceived = null;
    private readonly SemaphoreSlim _mutexLastDatagram = new SemaphoreSlim(1, 1);
    #endregion

    /// <param name="apiKey">Required DeepgramApiKey</param>
    /// <param name="deepgramClientOptions"><see cref="IDeepgramClientOptions"/> for HttpClient Configuration</param>
    public Client(string? apiKey = null, IDeepgramClientOptions? options = null) : base(apiKey, options)
    {
        Log.Verbose("ListenWSClient", "ENTER");
        Log.Debug("ListenWSClient", $"KeepAlive: {_deepgramClientOptions.KeepAlive}");
        Log.Debug("ListenWSClient", $"Autoflush: {_deepgramClientOptions.AutoFlushReplyDelta}");
        Log.Verbose("ListenWSClient", "LEAVE");
    }

    #region Event Handlers
    /// <summary>
    /// Fires when an event is received from the Deepgram API
    /// </summary>
    private event EventHandler<MetadataResponse>? _metadataReceived;
    private event EventHandler<ResultResponse>? _resultsReceived;
    private event EventHandler<UtteranceEndResponse>? _utteranceEndReceived;
    private event EventHandler<SpeechStartedResponse>? _speechStartedReceived;
    #endregion

    /// <summary>
    /// Connect to a Deepgram API Web Socket to begin transcribing audio
    /// </summary>
    /// <param name="options">Options to use when transcribing audio</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task<bool> Connect(LiveSchema options, CancellationTokenSource? cancelToken = null, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null)
    {
        if (!options.Model.StartsWith("nova-3") && options.Keyterms?.Count > 0)
        {
            throw new DeepgramException("Keyterms is only supported in Nova 3 models.");
        }
        Log.Verbose("ListenWSClient.Connect", "ENTER");
        Log.Information("Connect", $"options:\n{JsonSerializer.Serialize(options, JsonSerializeOptions.DefaultOptions)}");
        Log.Debug("Connect", $"addons: {addons}");

        try
        {
            var myURI = GetUri(_deepgramClientOptions, options, addons);
            Log.Debug("Connect", $"uri: {myURI}");
            bool bConnected = await base.Connect(myURI.ToString(), cancelToken, headers);
            if (!bConnected)
            {
                Log.Warning("Connect", "Connect failed");
                Log.Verbose("ListenWSClient.Connect", "LEAVE");
                return false;
            }

            if (_deepgramClientOptions.KeepAlive)
            {
                Log.Debug("Connect", "Starting KeepAlive Thread...");
                StartKeepAliveBackgroundThread();
            }

            if (_deepgramClientOptions.AutoFlushReplyDelta > 0)
            {
                Log.Debug("Connect", "Starting AutoFlush Thread...");
                StartAutoFlushBackgroundThread();
            }

            Log.Debug("Connect", "Connect Succeeded");
            Log.Verbose("ListenWSClient.Connect", "LEAVE");

            return true;
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("Connect", "Connect cancelled.");
            Log.Verbose("Connect", $"Connect cancelled. Info: {ex}");
            Log.Verbose("ListenWSClient.Connect", "LEAVE");

            return false;
        }
        catch (Exception ex)
        {
            Log.Error("Connect", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Connect", $"Exception: {ex}");
            Log.Verbose("ListenWSClient.Connect", "LEAVE");
            throw;
        }

        void StartKeepAliveBackgroundThread() => Task.Run(async () => await ProcessKeepAlive());

        void StartAutoFlushBackgroundThread() => Task.Run(async () => await ProcessAutoFlush());
    }

    #region Subscribe Event
    /// <summary>
    /// Subscribe to an Open event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<OpenResponse> eventHandler)
    {
        // Create a new event handler that wraps the original one
        EventHandler<Common.OpenResponse> wrappedHandler = (sender, args) =>
        {
            // Cast the event arguments to the desired type
            var castedArgs = new OpenResponse();
            castedArgs.Copy(args);
            if (castedArgs != null)
            {
                // Invoke the original event handler with the casted arguments
                eventHandler(sender, castedArgs);
            }
        };

        // Pass the new event handler to the base Subscribe method
        return await base.Subscribe(wrappedHandler);
    }

    /// <summary>
    /// Subscribe to a Metadata event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<MetadataResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _metadataReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a Results event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<ResultResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _resultsReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to an UtteranceEnd event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<UtteranceEndResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _utteranceEndReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a SpeechStarted event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<SpeechStartedResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _speechStartedReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to an Close event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<CloseResponse> eventHandler)
    {
        // Create a new event handler that wraps the original one
        EventHandler<Common.CloseResponse> wrappedHandler = (sender, args) =>
        {
            // Cast the event arguments to the desired type
            var castedArgs = new CloseResponse();
            castedArgs.Copy(args);
            if (castedArgs != null)
            {
                // Invoke the original event handler with the casted arguments
                eventHandler(sender, castedArgs);
            }
        };

        return await base.Subscribe(wrappedHandler);
    }

    /// <summary>
    /// Subscribe to an Error event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<ErrorResponse> eventHandler)
    {
        // Create a new event handler that wraps the original one
        EventHandler<Common.ErrorResponse> wrappedHandler = (sender, args) =>
        {
            // Cast the event arguments to the desired type
            var castedArgs = new ErrorResponse();
            castedArgs.Copy(args);
            if (castedArgs != null)
            {
                // Invoke the original event handler with the casted arguments
                eventHandler(sender, castedArgs);
            }
        };

        return await base.Subscribe(wrappedHandler);
    }

    /// <summary>
    /// Subscribe to an Unhandled event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<UnhandledResponse> eventHandler)
    {
        // Create a new event handler that wraps the original one
        EventHandler<Common.UnhandledResponse> wrappedHandler = (sender, args) =>
        {
            // Cast the event arguments to the desired type
            var castedArgs = new UnhandledResponse();
            castedArgs.Copy(args);
            if (castedArgs != null)
            {
                // Invoke the original event handler with the casted arguments
                eventHandler(sender, castedArgs);
            }
        };

        return await base.Subscribe(wrappedHandler);
    }
    #endregion

    #region Send Functions
    /// <summary>
    /// Sends a KeepAlive message to Deepgram
    /// </summary>
    public async Task SendKeepAlive()
    {
        Log.Debug("SendKeepAlive", "Sending KeepAlive Message Immediately...");
        ControlMessage message = new ControlMessage(Constants.KeepAlive);
        byte[] data = Encoding.ASCII.GetBytes(message.ToString());
        await SendMessageImmediately(data);
    }

    /// <summary>
    /// Sends a Finalize message to Deepgram
    /// </summary>
    public async Task SendFinalize()
    {
        Log.Debug("SendFinalize", "Sending Finalize Message Immediately...");
        ControlMessage message = new ControlMessage(Constants.Finalize);
        byte[] data = Encoding.ASCII.GetBytes(message.ToString());
        await SendMessageImmediately(data);
    }

    /// <summary>
    /// Sends a binary message over the WebSocket connection.
    /// </summary>
    /// <param name="data">The data to be sent over the WebSocket.</param>
    public void Send(byte[] data, int length = Abstract.Constants.UseArrayLengthForSend) => SendBinary(data, length);

    /// <summary>
    /// Sends a Close message to Deepgram
    /// </summary>
    public override async Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null)
    {
        if (_clientWebSocket == null || !IsConnected())
        {
            Log.Warning("SendClose", "ClientWebSocket is null or not connected. Skipping...");
            return;
        }

        // provide a cancellation token, or use the one in the class
        var _cancelToken = _cancellationToken ?? _cancellationTokenSource;

        Log.Debug("SendClose", "Sending Close Message Immediately...");
        if (nullByte)
        {
            // send a close to Deepgram
            await _mutexSend.WaitAsync(_cancelToken.Token);
            try
            {
                await _clientWebSocket.SendAsync(new ArraySegment<byte>(new byte[1] { 0 }), WebSocketMessageType.Binary, true, _cancellationTokenSource.Token)
                    .ConfigureAwait(false);
            }
            finally
            {
                _mutexSend.Release();
            }
            return;
        }

        ControlMessage message = new ControlMessage(Constants.CloseStream);
        byte[] data = Encoding.ASCII.GetBytes(message.ToString());
        await SendMessageImmediately(data);
    }
    #endregion

    internal async Task ProcessKeepAlive()
    {
        Log.Verbose("ListenWSClient.ProcessKeepAlive", "ENTER");

        try
        {
            while (true)
            {
                Log.Verbose("ProcessKeepAlive", "Waiting for KeepAlive...");
                await Task.Delay(5000, _cancellationTokenSource.Token);

                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Log.Information("ProcessKeepAlive", "KeepAliveThread cancelled");
                    break;
                }
                if (!IsConnected())
                {
                    Log.Debug("ProcessAutoFlush", "WebSocket is not connected. Exiting...");
                    break;
                }

                await SendKeepAlive();
            }

            Log.Verbose("ProcessKeepAlive", "Exit");
            Log.Verbose("ListenWSClient.ProcessKeepAlive", "LEAVE");
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("ProcessKeepAlive", "KeepAliveThread cancelled.");
            Log.Verbose("ProcessKeepAlive", $"KeepAliveThread cancelled. Info: {ex}");
            Log.Verbose("ListenWSClient.ProcessKeepAlive", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessKeepAlive", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessKeepAlive", $"Exception: {ex}");
            Log.Verbose("ListenWSClient.ProcessKeepAlive", "LEAVE");
        }
    }

    internal async Task ProcessAutoFlush()
    {
        Log.Verbose("ListenWSClient.ProcessAutoFlush", "ENTER");

        var diffTicks = TimeSpan.FromMilliseconds((double)_deepgramClientOptions.AutoFlushReplyDelta);

        try
        {
            while (true)
            {
                Log.Verbose("ProcessAutoFlush", "Waiting for AutoFlush...");
                await Task.Delay(Constants.DefaultFlushPeriodInMs, _cancellationTokenSource.Token);

                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Log.Information("ProcessAutoFlush", "ProcessAutoFlush cancelled");
                    break;
                }
                if (!IsConnected())
                {
                    Log.Debug("ProcessAutoFlush", "WebSocket is not connected. Exiting...");
                    break;
                }

                await _mutexLastDatagram.WaitAsync();
                try
                {
                    if (_lastReceived == null)
                    {
                        Log.Debug("ProcessAutoFlush", "No datagram received. Skipping...");
                        continue;
                    }

                    var deltaTicks = DateTime.Now - _lastReceived;
                    if (deltaTicks < diffTicks)
                    {
                        Log.Debug("ProcessAutoFlush", $"AutoFlush delta is less than threshold: {deltaTicks}. Skipping...");
                        continue;
                    }

                    await SendFinalize();
                    _lastReceived = null;
                }
                finally
                {
                    _mutexLastDatagram.Release();
                }
            }

            Log.Verbose("ProcessAutoFlush", "Exit");
            Log.Verbose("ListenWSClient.ProcessAutoFlush", "LEAVE");
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("ProcessAutoFlush", "ProcessAutoFlush cancelled.");
            Log.Verbose("ProcessAutoFlush", $"ProcessAutoFlush cancelled. Info: {ex}");
            Log.Verbose("ListenWSClient.ProcessAutoFlush", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessAutoFlush", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessAutoFlush", $"Exception: {ex}");
            Log.Verbose("ListenWSClient.ProcessAutoFlush", "LEAVE");
        }
    }

    internal override void ProcessTextMessage(WebSocketReceiveResult result, MemoryStream ms)
    {
        Log.Verbose("ListenWSClient.ProcessTextMessage", "ENTER");

        ms.Seek(0, SeekOrigin.Begin);

        var response = Encoding.UTF8.GetString(ms.ToArray());
        if (response == null)
        {
            Log.Warning("ProcessTextMessage", "Response is null");
            Log.Verbose("ListenWSClient.ProcessTextMessage", "LEAVE");
            return;
        }

        try
        {
            Log.Verbose("ProcessTextMessage", $"raw response: {response}");
            var data = JsonDocument.Parse(response);
            var val = Enum.Parse(typeof(ListenType), data.RootElement.GetProperty("type").GetString()!);

            Log.Verbose("ProcessTextMessage", $"Type: {val}");


            if (_deepgramClientOptions.InspectListenMessage())
            {
                Log.Debug("ProcessTextMessage", "Call InspectMessage...");
                InspectMessage(val, data).Wait();
            }

            switch (val)
            {
                case ListenType.Open:
                case ListenType.Close:
                case ListenType.Error:
                    Log.Debug("ProcessTextMessage", "Calling base.ProcessTextMessage...");
                    base.ProcessTextMessage(result, ms);
                    break;
                case ListenType.Results:
                    var resultResponse = data.Deserialize<ResultResponse>();
                    if (_resultsReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_resultsReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (resultResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "ResultResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessTextMessage", $"Invoking ResultsResponse. event: {resultResponse}");
                    InvokeParallel(_resultsReceived, resultResponse);
                    break;
                case ListenType.Metadata:
                    var metadataResponse = data.Deserialize<MetadataResponse>();
                    if (_metadataReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_metadataReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (metadataResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "MetadataResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessTextMessage", $"Invoking MetadataResponse. event: {metadataResponse}");
                    InvokeParallel(_metadataReceived, metadataResponse);
                    break;
                case ListenType.UtteranceEnd:
                    var utteranceEndResponse = data.Deserialize<UtteranceEndResponse>();
                    if (_utteranceEndReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_utteranceEndReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (utteranceEndResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "UtteranceEndResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessTextMessage", $"Invoking UtteranceEndResponse. event: {utteranceEndResponse}");
                    InvokeParallel(_utteranceEndReceived, utteranceEndResponse);
                    break;
                case ListenType.SpeechStarted:
                    var speechStartedResponse = data.Deserialize<SpeechStartedResponse>();
                    if (_speechStartedReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_speechStartedReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (speechStartedResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "SpeechStartedResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessTextMessage", $"Invoking SpeechStartedResponse. event: {speechStartedResponse}");
                    InvokeParallel(_speechStartedReceived, speechStartedResponse);
                    break;
                default:
                    Log.Debug("ProcessTextMessage", "Calling base.ProcessTextMessage...");
                    base.ProcessTextMessage(result, ms);
                    break;
            }

            Log.Debug("ProcessTextMessage", "Succeeded");
            Log.Verbose("ListenWSClient.ProcessTextMessage", "LEAVE");
        }
        catch (JsonException ex)
        {
            Log.Error("ProcessTextMessage", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessTextMessage", $"Exception: {ex}");
            Log.Verbose("ListenWSClient.ProcessTextMessage", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessTextMessage", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessTextMessage", $"Exception: {ex}");
            Log.Verbose("ListenWSClient.ProcessTextMessage", "LEAVE");
        }
    }

    #region Helpers
    /// <summary>
    /// Get the URI for the WebSocket connection
    /// </summary> 
    internal static Uri GetUri(IDeepgramClientOptions options, LiveSchema parameter, Dictionary<string, string>? addons = null)
    {
        var propertyInfoList = parameter.GetType()
            .GetProperties()
            .Where(v => v.GetValue(parameter) is not null);

        var queryString = QueryParameterUtil.UrlEncode(parameter, propertyInfoList, addons);

        return new Uri($"{options.BaseAddress}/{UriSegments.LISTEN}?{queryString}");
    }

    private async Task InspectMessage(object type, JsonDocument data)
    {
        Log.Verbose("InspectMessage", "ENTER");

        try
        {
            switch (type)
            {
                case ListenType.Results:
                    var resultResponse = data.Deserialize<ResultResponse>();
                    if (resultResponse == null)
                    {
                        Log.Warning("InspectMessage", "ResultResponse is invalid");
                        Log.Verbose("InspectMessage", "LEAVE");
                        return;
                    }

                    var sentence = resultResponse.Channel.Alternatives[0].Transcript;

                    if (resultResponse.Channel.Alternatives.Count == 0 || sentence == "") {
                        Log.Verbose("InspectMessage", $"resultResponse has empty message");
                        Log.Verbose("InspectMessage", "LEAVE");
                        return;
                    }

                    if (_deepgramClientOptions.AutoFlushReplyDelta > 0)
                    {
                        if ((bool)resultResponse.IsFinal)
                        {
                            var now = DateTime.Now;
                            Log.Debug("InspectMessage", $"AutoFlush IsFinal received. Time: {now}");
                            await _mutexLastDatagram.WaitAsync();
                            try
                            {
                                _lastReceived = null;
                            }
                            finally
                            {
                                _mutexLastDatagram.Release();
                            }
                        }
                        else
                        {
                            var now = DateTime.Now;
                            Log.Debug("InspectMessage", $"AutoFlush Interim received. Time: {now}");
                            await _mutexLastDatagram.WaitAsync();
                            try
                            {
                                _lastReceived = now;
                            }
                            finally
                            {
                                _mutexLastDatagram.Release();
                            }
                        }
                    }
                    break;
            }

            Log.Debug("InspectMessage", "Succeeded");
            Log.Verbose("InspectMessage", "LEAVE");
        }
        catch (JsonException ex)
        {
            Log.Error("InspectMessage", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("InspectMessage", $"Exception: {ex}");
            Log.Verbose("ListenWSClient.InspectMessage", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("InspectMessage", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("InspectMessage", $"Exception: {ex}");
            Log.Verbose("ListenWSClient.InspectMessage", "LEAVE");
        }
    }
    #endregion
}
