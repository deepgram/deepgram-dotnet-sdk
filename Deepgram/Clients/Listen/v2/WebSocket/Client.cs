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
    /// <remarks>
    /// Open event handlers run before Connect() returns, and the receive loop starts only after they
    /// return, so an Open handler that waits for a message from the server blocks until its own
    /// timeout. An exception thrown by an Open handler is logged at Error level and Connect() still
    /// returns true. Messages still queued from earlier sends are sent as soon as the new connection
    /// opens; the send methods' remarks say which ones are kept. If a previous connection is still
    /// being released, Connect() waits for that to finish (within its connect timeout) and then
    /// opens a new connection. If another Connect() is still opening the connection or a Stop() is
    /// closing it, Connect() waits for it within the same timeout and returns true only for an open
    /// connection that is not stopping.
    /// A Stop() that begins before Connect() returns (for example, from an Open handler) makes
    /// Connect() return false even after Open fired; Close then follows.
    /// </remarks>
    /// <param name="options">Options to use when transcribing audio</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task<bool> Connect(LiveSchema options, CancellationTokenSource? cancelToken = null, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null)
    {
        if (options.Model is not null && !options.Model.StartsWith("nova-3") && options.Keyterm?.Count > 0)
        {
            throw new DeepgramException("Keyterm is only supported in Nova 3 models.");
        }
        Log.Verbose("ListenWSClient.Connect", "ENTER");
        Log.Information("Connect", $"options:\n{JsonSerializer.Serialize(options, JsonSerializeOptions.DefaultOptions)}");
        Log.Debug("Connect", $"addons: {addons}");

        try
        {
            var myURI = GetUri(_deepgramClientOptions, options, addons);
            Log.Debug("Connect", $"uri: {myURI}");
            // owned is the connection this call opened. It is null when another connection was
            // already published ("already initialized"): that connection's own Connect() runs
            // its post-connect work, so this call does none (#455).
            var (bConnected, owned) = await ConnectCore(myURI.ToString(), cancelToken, headers);
            if (!bConnected)
            {
                Log.Warning("Connect", "Connect failed");
                Log.Verbose("ListenWSClient.Connect", "LEAVE");
                return false;
            }

            if (_deepgramClientOptions.KeepAlive && owned != null)
            {
                Log.Debug("Connect", "Starting KeepAlive Thread...");
                StartKeepAliveBackgroundThread(owned);
            }

            if (_deepgramClientOptions.AutoFlushReplyDelta > 0 && owned != null)
            {
                Log.Debug("Connect", "Starting AutoFlush Thread...");
                StartAutoFlushBackgroundThread(owned);
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

        void StartKeepAliveBackgroundThread(ConnectionState owned) => Task.Run(async () => await ProcessKeepAlive(owned));

        void StartAutoFlushBackgroundThread(ConnectionState owned) => Task.Run(async () => await ProcessAutoFlush(owned));
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
    /// <remarks>
    /// Close fires once for each connection that raised Open, always after Open, when Stop(), a
    /// server close or a dropped connection ends it. If a Stop() ends the connection while Open
    /// handlers are still running, Close fires as soon as they return; if a new Connect() runs before
    /// then, that Close can arrive after the new connection's Open. Dispose() raises no Close.
    /// A Close handler must not block on a different Stop() call that is still in progress for the
    /// same connection: that Stop() returns only after Close has been delivered, so the handler
    /// waits forever. A Stop() called from inside the handler returns at once, unless a newer
    /// connection has opened since; then it stops that connection.
    /// </remarks>
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
    /// <remarks>
    /// Sent immediately, not queued: dropped while the client is not connected, including while
    /// Connect() is still in progress.
    /// </remarks>
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
    /// <remarks>
    /// Sent immediately, not queued: dropped while the client is not connected, including while
    /// Connect() is still in progress.
    /// </remarks>
    public async Task SendFinalize()
    {
        // Flush any buffered audio first so Finalize can't overtake audio still in the send
        // queue (Finalize is sent immediately and would otherwise race ahead). See #358.
        Log.Debug("SendFinalize", "Flushing buffered audio before Finalize...");
        await Flush();

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
    public override Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null)
    {
        var owned = CurrentConnection;
        if (owned == null)
        {
            Log.Warning("SendClose", "ClientWebSocket is null or not connected. Skipping...");
            return Task.CompletedTask;
        }
        return SendClose(owned, nullByte, _cancellationToken);
    }

    /// <summary>
    /// Sends the close message on one connection's socket only, never on a newer connection (#455).
    /// </summary>
    private protected override async Task SendClose(ConnectionState owned, bool nullByte, CancellationTokenSource? _cancellationToken)
    {
        if (!owned.IsOpen)
        {
            Log.Warning("SendClose", "ClientWebSocket is null or not connected. Skipping...");
            return;
        }

        Log.Debug("SendClose", "Sending Close Message Immediately...");
        if (nullByte)
        {
            // send a close to Deepgram
            var token = _cancellationToken?.Token ?? owned.Token;
            await SendOnConnection(owned, new ArraySegment<byte>(new byte[1] { 0 }), WebSocketMessageType.Binary, token);
            return;
        }

        ControlMessage message = new ControlMessage(Constants.CloseStream);
        byte[] data = Encoding.ASCII.GetBytes(message.ToString());
        // The caller's token bounds the send, so Stop(token) keeps its timeout.
        await SendOnConnection(owned, new ArraySegment<byte>(data), WebSocketMessageType.Text, _cancellationToken?.Token ?? owned.Token);
    }
    #endregion

    /// <summary>SendKeepAlive() for one connection: the keepalive loop sends on its own connection only (#455).</summary>
    private async Task SendKeepAlive(ConnectionState owned)
    {
        Log.Debug("SendKeepAlive", "Sending KeepAlive Message Immediately...");
        ControlMessage message = new ControlMessage(Constants.KeepAlive);
        byte[] data = Encoding.ASCII.GetBytes(message.ToString());
        await SendOnConnection(owned, new ArraySegment<byte>(data), WebSocketMessageType.Text, owned.Token);
    }

    /// <summary>SendFinalize() for one connection: the autoflush loop sends on its own connection only (#455).</summary>
    private async Task SendFinalize(ConnectionState owned)
    {
        // Flush any buffered audio first so Finalize can't overtake audio still in the send
        // queue (Finalize is sent immediately and would otherwise race ahead). See #358.
        Log.Debug("SendFinalize", "Flushing buffered audio before Finalize...");
        await Flush();

        Log.Debug("SendFinalize", "Sending Finalize Message Immediately...");
        ControlMessage message = new ControlMessage(Constants.Finalize);
        byte[] data = Encoding.ASCII.GetBytes(message.ToString());
        await SendOnConnection(owned, new ArraySegment<byte>(data), WebSocketMessageType.Text, owned.Token);
    }

    internal async Task ProcessKeepAlive(ConnectionState owned)
    {
        Log.Verbose("ListenWSClient.ProcessKeepAlive", "ENTER");

        try
        {
            // Use the connection this loop was started for, never the fields, so it stops with that
            // connection and never acts on a newer one (#390, #455).
            var _cancelToken = owned.Token;
            while (true)
            {
                Log.Verbose("ProcessKeepAlive", "Waiting for KeepAlive...");
                await Task.Delay(5000, _cancelToken);

                if (_cancelToken.IsCancellationRequested)
                {
                    Log.Information("ProcessKeepAlive", "KeepAliveThread cancelled");
                    break;
                }
                if (!owned.IsOpen)
                {
                    Log.Debug("ProcessAutoFlush", "WebSocket is not connected. Exiting...");
                    break;
                }

                await SendKeepAlive(owned);
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

    internal async Task ProcessAutoFlush(ConnectionState owned)
    {
        Log.Verbose("ListenWSClient.ProcessAutoFlush", "ENTER");

        var diffTicks = TimeSpan.FromMilliseconds((double)_deepgramClientOptions.AutoFlushReplyDelta);

        try
        {
            // Use the connection this loop was started for, never the fields, so it stops with that
            // connection and never acts on a newer one (#390, #455).
            var _cancelToken = owned.Token;
            while (true)
            {
                Log.Verbose("ProcessAutoFlush", "Waiting for AutoFlush...");
                await Task.Delay(Constants.DefaultFlushPeriodInMs, _cancelToken);

                if (_cancelToken.IsCancellationRequested)
                {
                    Log.Information("ProcessAutoFlush", "ProcessAutoFlush cancelled");
                    break;
                }
                if (!owned.IsOpen)
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

                    await SendFinalize(owned);
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
            if (Log.IsEnabled(LogLevel.Verbose))
            {
                Log.Verbose("ProcessTextMessage", $"raw response: {response}");
            }
            var data = JsonDocument.Parse(response);
            var typeString = data.RootElement.ValueKind == JsonValueKind.Object
                && data.RootElement.TryGetProperty("type", out var type)
                && type.ValueKind == JsonValueKind.String
                ? type.GetString()
                : null;

            if (!Enum.TryParse<ListenType>(typeString, out var val))
            {
                Log.Debug("ProcessTextMessage", "Unknown message type. Calling base.ProcessTextMessage...");
                base.ProcessTextMessage(result, ms);
                return;
            }

            if (Log.IsEnabled(LogLevel.Verbose))
            {
                Log.Verbose("ProcessTextMessage", $"Type: {val}");
            }


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

                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.Debug("ProcessTextMessage", $"Invoking ResultsResponse. event: {resultResponse}");
                    }
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

                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.Debug("ProcessTextMessage", $"Invoking MetadataResponse. event: {metadataResponse}");
                    }
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

                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.Debug("ProcessTextMessage", $"Invoking UtteranceEndResponse. event: {utteranceEndResponse}");
                    }
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

                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.Debug("ProcessTextMessage", $"Invoking SpeechStartedResponse. event: {speechStartedResponse}");
                    }
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
