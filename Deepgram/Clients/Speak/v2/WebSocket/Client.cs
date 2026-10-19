// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Abstractions.v2;
using Abstract = Deepgram.Abstractions.v2;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Speak.v2.WebSocket;
using Common = Deepgram.Models.Common.v2.WebSocket;
using Deepgram.Clients.Interfaces.v2;

namespace Deepgram.Clients.Speak.v2.WebSocket;

/// <summary>
/// Implements version 2 of the Speak WebSocket Client.
/// </summary>
public class Client : AbstractWebSocketClient, ISpeakWebSocketClient
{
    #region Fields
    private DateTime? _lastReceived = null;
    private int _flushCount = 0;
    private readonly SemaphoreSlim _mutexLastDatagram = new SemaphoreSlim(1, 1);
    #endregion

    /// <param name="apiKey">Required DeepgramApiKey</param>
    /// <param name="deepgramClientOptions"><see cref="IDeepgramClientOptions"/> for HttpClient Configuration</param>
    public Client(string? apiKey = null, IDeepgramClientOptions? options = null) : base(apiKey, options)
    {
        Log.Verbose("SpeakWSClient", "ENTER");
        Log.Debug("SpeakWSClient", $"Autoflush: {_deepgramClientOptions.AutoFlushSpeakDelta}");
        Log.Verbose("SpeakWSClient", "LEAVE");
    }

    #region Event Handlers
    /// <summary>
    /// Fires when an event is received from the Deepgram API
    /// </summary>
    private event EventHandler<MetadataResponse>? _metadataReceived;
    private event EventHandler<FlushedResponse>? _flushedReceived;
    private event EventHandler<ClearedResponse>? _clearedReceived;
    private event EventHandler<AudioResponse>? _audioReceived;
    private event EventHandler<WarningResponse>? _warningReceived;
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
    public async Task<bool> Connect(SpeakSchema options, CancellationTokenSource? cancelToken = null, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null)
    {
        Log.Verbose("SpeakWSClient.Connect", "ENTER");
        Log.Information("Connect", $"options:\n{JsonSerializer.Serialize(options, JsonSerializeOptions.DefaultOptions)}");
        Log.Debug("Connect", $"addons: {addons}");

        try
        {
            var myURI = GetUri(_deepgramClientOptions, options, addons);
            Log.Debug("Connect", $"uri: {myURI}");
            // owned is the connection this call opened. It is null when another connection was
            // already published ("already initialized"): that connection's own Connect() runs
            // its post-connect work, so this call does none (#455).
            var (bConnected, owned) = await ConnectCore(myURI.ToString(), cancelToken, headers).ConfigureAwait(false);
            if (!bConnected)
            {
                Log.Warning("Connect", "Connect failed");
                Log.Verbose("SpeakWSClient.Connect", "LEAVE");
                return false;
            }

            if (_deepgramClientOptions.AutoFlushSpeakDelta > 0 && owned != null)
            {
                Log.Debug("Connect", "Starting AutoFlush Thread...");
                StartAutoFlushBackgroundThread(owned);
            }

            Log.Debug("Connect", "Connect Succeeded");
            Log.Verbose("SpeakWSClient.Connect", "LEAVE");

            return true;
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("Connect", "Connect cancelled.");
            Log.Verbose("Connect", $"Connect cancelled. Info: {ex}");
            Log.Verbose("SpeakWSClient.Connect", "LEAVE");

            return false;
        }
        catch (Exception ex)
        {
            Log.Error("Connect", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Connect", $"Excepton: {ex}");
            Log.Verbose("SpeakWSClient.Connect", "LEAVE");
            throw;
        }

        void StartAutoFlushBackgroundThread(ConnectionState owned) => Task.Run(async () => await ProcessAutoFlush(owned).ConfigureAwait(false));
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
    /// Subscribe to a Flushed event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<FlushedResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _flushedReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a Cleared event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<ClearedResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _clearedReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to an Audio event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<AudioResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _audioReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a Close event from the Deepgram API
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
    /// Subscribe to an Warning event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<WarningResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _warningReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to an Error event from the Deepgram API
    /// </summary>
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
    /// This method sends a string to Deepgram for conversion to audio.
    /// This is a convenience functions that will wrap the provided string in a TextSource object.
    /// NOTE: These should never use the SendImmediately methods because they would by-pass the flow of text messages queued.
    /// </summary>
    /// <param name="text">The string of text you want to be converted to audio.</param>
    public void SpeakWithText(string text)
    {
        TextSource textSource = new TextSource(text.Replace("\r\n", "\\n")
                                             .Replace("\n", "\\n")
                                             .Replace("\"", "\\\"")
                                             .Replace("\b", "\\b")
                                             .Replace("\f", "\\f")
                                             .Replace("\t", "\\t"));
        byte[] byteArray = Encoding.UTF8.GetBytes(textSource.ToString());
        SendMessage(byteArray);
    }

    /// <summary>
    ///  This method Flushes the text buffer on Deepgram to be converted to audio.
    ///  NOTE: These should never use the SendImmediately methods because they would by-pass the flow of text messages queued.
    /// </summary>
    public new void Flush()
    {
        ControlMessage controlMessage = new ControlMessage(Constants.Flush);
        byte[] byteArray = Encoding.UTF8.GetBytes(controlMessage.ToString());
        SendMessage(byteArray);
    }

    /// <summary>
    ///  This method Clears the text buffer on Deepgram to be converted to audio
    ///  NOTE: These should never use the SendImmediately methods because they would by-pass the flow of text messages queued.
    /// </summary>
    public void Clear()
    {
        ControlMessage controlMessage = new ControlMessage(Constants.Clear);
        byte[] byteArray = Encoding.UTF8.GetBytes(controlMessage.ToString());
        SendMessage(byteArray);
    }

    /// <summary>
    ///  This method tells Deepgram to initiate the close server-side.
    ///  NOTE: This is fine to use the SendImmediately methods because you want to shutdown the websocket ASAP.
    /// </summary>
    public void Close(bool nullByte = false)
    {
        SendClose(nullByte).Wait();
    }

    /// <summary>
    /// This method sends a close message over the WebSocket connection.
    /// NOTE: This is fine to use the SendImmediately methods because you want to shutdown the websocket ASAP.
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
            await SendOnConnection(owned, new ArraySegment<byte>(new byte[1] { 0 }), WebSocketMessageType.Binary, token).ConfigureAwait(false);
            return;
        }

        ControlMessage message = new ControlMessage(Constants.Close);
        byte[] data = Encoding.UTF8.GetBytes(message.ToString());
        await SendOnConnection(owned, new ArraySegment<byte>(data), WebSocketMessageType.Text, _cancellationToken?.Token ?? owned.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// The SendMessage function needs to be overridden to handle the auto flush feature.
    /// </summary>
    public override void SendMessage(byte[] data, int length = Constants.UseArrayLengthForSend) =>
        SendMessage(data, length, null);

    /// <summary>
    /// <see cref="SendMessage(byte[], int)"/> with the connection an SDK-internal message belongs
    /// to; null for user messages.
    /// </summary>
    private void SendMessage(byte[] data, int length, ConnectionState? owner)
    {
        // auto flush
        if (_deepgramClientOptions.InspectSpeakMessage())
        {
            string type = GetMessageType(data);
            Log.Debug("SendMessage", $"Inspecting Message: Sending {type}");
            switch (type)
            {
                case Constants.Flush:
                    IncrementCounter().Wait();
                    break;
                case Constants.Speak:
                    InspectMessage();
                    break;
            }
        }

        // send message
        EnqueueSendMessage(new WebSocketMessage(data, WebSocketMessageType.Text, length, owner));
    }

    /// <summary>
    /// The autoflush loop's Flush(): the same message, tagged with the loop's connection, so a
    /// Flush still queued when that connection ends is dropped instead of reaching a newer one (#455).
    /// </summary>
    internal void SendAutoFlush(ConnectionState owned)
    {
        ControlMessage controlMessage = new ControlMessage(Constants.Flush);
        byte[] byteArray = Encoding.UTF8.GetBytes(controlMessage.ToString());
        SendMessage(byteArray, Constants.UseArrayLengthForSend, owned);
    }

    /// <summary>
    /// We need to override the Send function to use the SendMessage function. This is different than STT
    /// because we only deal in text messages for TTS where STT is sending binary (or audio) messages.
    /// </summary>
    public void Send(byte[] data, int length = Abstract.Constants.UseArrayLengthForSend) => SendMessage(data, length);
    #endregion

    internal async Task ProcessAutoFlush(ConnectionState owned)
    {
        Log.Verbose("LiveClient.ProcessAutoFlush", "ENTER");

        var diffTicks = TimeSpan.FromMilliseconds((double)_deepgramClientOptions.AutoFlushSpeakDelta);

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
                    return;
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

                    // Flush goes through the send queue on purpose, so it stays behind the queued
                    // text. Enqueue it only while this loop's connection is still open; it is
                    // tagged with that connection, so if the connection ends before it is sent, a
                    // newer connection's sender drops it (#455).
                    if (!owned.IsOpen)
                    {
                        Log.Debug("ProcessAutoFlush", "WebSocket is not connected. Exiting...");
                        return;
                    }

                    Log.Debug("ProcessAutoFlush", $"AutoFlush delta exceeded threshold: {deltaTicks}. Skipping...");
                    SendAutoFlush(owned);
                    _lastReceived = null;
                }
                finally
                {
                    _mutexLastDatagram.Release();
                }
            }

            Log.Verbose("ProcessAutoFlush", "Exit");
            Log.Verbose("LiveClient.ProcessAutoFlush", "LEAVE");
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("ProcessAutoFlush", "KeepAliveThread cancelled.");
            Log.Verbose("ProcessAutoFlush", $"KeepAliveThread cancelled. Info: {ex}");
            Log.Verbose("LiveClient.ProcessAutoFlush", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessAutoFlush", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessAutoFlush", $"Excepton: {ex}");
            Log.Verbose("LiveClient.ProcessAutoFlush", "LEAVE");
        }
    }

    internal override void ProcessBinaryMessage(WebSocketReceiveResult result, MemoryStream ms)
    {
        try
        {
            Log.Debug("ProcessBinaryMessage", "Received WebSocketMessageType.Binary");

            if (_audioReceived == null)
            {
                Log.Debug("ProcessBinaryMessage", "_audioReceived has no listeners");
                Log.Verbose("ProcessBinaryMessage", "LEAVE");
                return;
            }

            var audioResponse = new AudioResponse()
            {
                Stream = ms
            };

            Log.Debug("ProcessBinaryMessage", "Invoking AudioResponse");
            InvokeParallel(_audioReceived, audioResponse);
        }
        catch (JsonException ex)
        {
            Log.Error("ProcessDataReceived", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessDataReceived", $"Exception: {ex}");
            Log.Verbose("SpeakWSClient.ProcessDataReceived", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessDataReceived", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessDataReceived", $"Excepton: {ex}");
            Log.Verbose("SpeakWSClient.ProcessDataReceived", "LEAVE");
        }
    }

    internal override void ProcessTextMessage(WebSocketReceiveResult result, MemoryStream ms)
    {
        try
        {
            Log.Debug("ProcessDataReceived", "Received WebSocketMessageType.Text");

            var response = Encoding.UTF8.GetString(ms.ToArray());
            if (response == null)
            {
                Log.Warning("ProcessDataReceived", "Response is null");
                Log.Verbose("SpeakWSClient.ProcessDataReceived", "LEAVE");
                return;
            }

            Log.Verbose("ProcessDataReceived", $"raw response: {response}");
            var data = JsonDocument.Parse(response);
            var typeString = data.RootElement.TryGetProperty("type", out var typeProperty) &&
                typeProperty.ValueKind == JsonValueKind.String
                ? typeProperty.GetString()
                : null;
            if (!Enum.TryParse<SpeakType>(typeString, out var val))
            {
                val = SpeakType.Unhandled;
            }

            Log.Verbose("ProcessDataReceived", $"Type: {val}");

            switch (val)
            {
                case SpeakType.Open:
                case SpeakType.Close:
                case SpeakType.Error:
                    Log.Debug("ProcessTextMessage", "Calling base.ProcessTextMessage...");
                    base.ProcessTextMessage(result, ms);
                    break;
                case SpeakType.Metadata:
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
                    InvokeParallel(_metadataReceived, metadataResponse);
                    break;
                case SpeakType.Flushed:
                    var flushedResponse = data.Deserialize<FlushedResponse>();
                    if (_flushedReceived == null)
                    {
                        Log.Debug("ProcessDataReceived", "_flushedReceived has no listeners");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }
                    if (flushedResponse == null)
                    {
                        Log.Warning("ProcessDataReceived", "FlushedResponse is invalid");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }

                    // auto flush
                    if (_deepgramClientOptions.InspectSpeakMessage())
                    {
                        DecrementCounter().Wait();
                    }

                    Log.Debug("ProcessDataReceived", $"Invoking FlushedResponse. event: {flushedResponse}");
                    InvokeParallel(_flushedReceived, flushedResponse);
                    break;
                case SpeakType.Cleared:
                    var clearResponse = data.Deserialize<ClearedResponse>();
                    if (_clearedReceived == null)
                    {
                        Log.Debug("ProcessDataReceived", "_clearedReceived has no listeners");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }
                    if (clearResponse == null)
                    {
                        Log.Warning("ProcessDataReceived", "ClearedResponse is invalid");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessDataReceived", $"Invoking ClearedResponse. event: {clearResponse}");
                    InvokeParallel(_clearedReceived, clearResponse);
                    break;
                case SpeakType.Warning:
                    var warningResponse = data.Deserialize<WarningResponse>();
                    if (_warningReceived == null)
                    {
                        Log.Debug("ProcessDataReceived", "_warningReceived has no listeners");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }
                    if (warningResponse == null)
                    {
                        Log.Warning("ProcessDataReceived", "WarningResponse is invalid");
                        Log.Verbose("ProcessDataReceived", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessDataReceived", $"Invoking WarningResponse. event: {warningResponse}");
                    InvokeParallel(_warningReceived, warningResponse);
                    break;
                default:
                    Log.Debug("ProcessTextMessage", "Calling base.ProcessTextMessage...");
                    base.ProcessTextMessage(result, ms);
                    break;
            }
        }
        catch (JsonException ex)
        {
            Log.Error("ProcessDataReceived", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessDataReceived", $"Excepton: {ex}");
            Log.Verbose("SpeakWSClient.ProcessDataReceived", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessDataReceived", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessDataReceived", $"Excepton: {ex}");
            Log.Verbose("SpeakWSClient.ProcessDataReceived", "LEAVE");
        }
    }

    #region Helpers
    /// <summary>
    /// Get the URI for the WebSocket connection
    /// </summary> 
    internal static Uri GetUri(IDeepgramClientOptions options, SpeakSchema parameter, Dictionary<string, string>? addons = null)
    {
        var propertyInfoList = parameter.GetType()
            .GetProperties()
            .Where(v => v.GetValue(parameter) is not null);

        var queryString = QueryParameterUtil.UrlEncode(parameter, propertyInfoList, addons);

        return new Uri($"{options.BaseAddress}/{UriSegments.SPEAK}?{queryString}");
    }

    private async void InspectMessage()
    {
        Log.Verbose("InspectMessage", "ENTER");

        if (_deepgramClientOptions.AutoFlushSpeakDelta > 0)
        {
            var now = DateTime.Now;
            Log.Debug("InspectMessage", $"AutoFlush last received. Time: {now}");
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

        Log.Debug("InspectMessage", "Succeeded");
        Log.Verbose("InspectMessage", "LEAVE");
    }

    private async Task<bool> DecrementCounter()
    {
        await _mutexLastDatagram.WaitAsync();
        try
        {
            _flushCount -= 1;
            Log.Debug("DecrementCounter", $"Decrement Flush count: {_flushCount}");
        }
        finally
        {
            _mutexLastDatagram.Release();
        }

        return true;
    }

    private async Task<bool> IncrementCounter()
    {
        await _mutexLastDatagram.WaitAsync();
        try
        {
            _flushCount += 1;
            Log.Debug("IncrementCounter", $"Increment Flush count: {_flushCount}");
        }
        finally
        {
            _mutexLastDatagram.Release();
        }

        return true;
    }

    internal string GetMessageType(byte[] msg)
    {
        // Convert the byte array to a string
        string response = Encoding.UTF8.GetString(msg);
        if (response == null)
        {
            return "";
        }

        Log.Verbose("ProcessDataReceived", $"raw response: {response}");
        var data = JsonDocument.Parse(response);

        string val = data.RootElement.GetProperty("type").GetString() ?? "";
        Log.Debug("ProcessDataReceived", $"Type: {val}");

        return val;
    }
    #endregion
}
