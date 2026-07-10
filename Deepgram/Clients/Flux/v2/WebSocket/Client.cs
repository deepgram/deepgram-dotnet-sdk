// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using System.Globalization;

using Deepgram.Abstractions.v2;
using Abstract = Deepgram.Abstractions.v2;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Flux.v2.WebSocket;
using Common = Deepgram.Models.Common.v2.WebSocket;
using Deepgram.Clients.Interfaces.v2;
using Deepgram.Models.Exceptions.v1;

namespace Deepgram.Clients.Flux.v2.WebSocket;

/// <summary>
/// (PREVIEW) Implements the Flux (v2 listen) WebSocket Client for conversational
/// speech-to-text with contextual turn detection.
///
/// Flux differs from classic streaming transcription:
/// results arrive as TurnInfo messages carrying a turn lifecycle event
/// (StartOfTurn, Update, EagerEndOfTurn, TurnResumed, EndOfTurn), and the endpoint accepts
/// only two client control messages: CloseStream and Configure. There is no KeepAlive and
/// no Finalize; idle connections rely on WebSocket protocol-level ping/pong.
///
/// Note: event handlers are invoked from the receive loop; a slow handler delays delivery of
/// subsequent messages (including EndOfTurn). Keep handlers fast and offload heavy work.
/// <see href="https://developers.deepgram.com/docs/flux/quickstart"/>
/// </summary>
public class Client : AbstractWebSocketClient, IFluxWebSocketClient
{
    #region Fields
    // Guards against the Close event being raised twice for one connection, which can happen
    // when a user-initiated Stop() and the receive loop's server-close handling overlap.
    private int _closeEventFired = 0;
    #endregion

    /// <param name="apiKey">Required DeepgramApiKey</param>
    /// <param name="options"><see cref="IDeepgramClientOptions"/> for WebSocket Configuration</param>
    public Client(string? apiKey = null, IDeepgramClientOptions? options = null) : base(apiKey, options)
    {
        Log.Verbose("FluxWSClient", "ENTER");
        if (_deepgramClientOptions.KeepAlive)
        {
            // Flux does not support the KeepAlive control message; sending it would terminate
            // the connection with an UNPARSABLE_CLIENT_MESSAGE error. The option is ignored.
            Log.Warning("FluxWSClient", "KeepAlive option is not supported by Flux and will be ignored");
        }
        Log.Verbose("FluxWSClient", "LEAVE");
    }

    #region Event Handlers
    /// <summary>
    /// Fires when an event is received from the Deepgram API
    /// </summary>
    private event EventHandler<ConnectedResponse>? _connectedReceived;
    private event EventHandler<TurnInfoResponse>? _turnInfoReceived;
    private event EventHandler<ConfigureSuccessResponse>? _configureSuccessReceived;
    private event EventHandler<ConfigureFailureResponse>? _configureFailureReceived;
    private event EventHandler<ErrorResponse>? _fluxErrorReceived;
    private event EventHandler<UnhandledResponse>? _fluxUnhandledReceived;
    #endregion

    /// <summary>
    /// Connect to the Deepgram Flux API WebSocket to begin transcribing audio
    /// </summary>
    /// <param name="options">Options to use when transcribing audio, <see cref="FluxSchema.Model"/> is required</param>
    /// <param name="cancelToken">Provide a cancel token to be used for the connect or use the internal one</param>
    /// <param name="addons">Additional query parameters to pass to the API</param>
    /// <param name="headers">Additional HTTP headers to send on the WebSocket upgrade</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task<bool> Connect(FluxSchema options, CancellationTokenSource? cancelToken = null, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null)
    {
        Log.Verbose("FluxWSClient.Connect", "ENTER");

        if (options is null || string.IsNullOrWhiteSpace(options.Model))
        {
            throw new DeepgramException("Flux requires a model to be specified (e.g. \"flux-general-en\").");
        }

        Log.Information("Connect", $"options:\n{JsonSerializer.Serialize(options, JsonSerializeOptions.DefaultOptions)}");
        Log.Debug("Connect", $"addons: {addons}");

        try
        {
            Interlocked.Exchange(ref _closeEventFired, 0);

            var myURI = GetUri(_deepgramClientOptions, options, addons);
            Log.Debug("Connect", $"uri: {myURI}");
            bool bConnected = await base.Connect(myURI.ToString(), cancelToken, headers);
            if (!bConnected)
            {
                Log.Warning("Connect", "Connect failed");
                Log.Verbose("FluxWSClient.Connect", "LEAVE");
                return false;
            }

            // NOTE: Unlike the classic listen client, no KeepAlive and no AutoFlush background
            // threads are started. Flux does not accept those control messages.

            Log.Debug("Connect", "Connect Succeeded");
            Log.Verbose("FluxWSClient.Connect", "LEAVE");

            return true;
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("Connect", "Connect cancelled.");
            Log.Verbose("Connect", $"Connect cancelled. Info: {ex}");
            Log.Verbose("FluxWSClient.Connect", "LEAVE");

            return false;
        }
        catch (Exception ex)
        {
            Log.Error("Connect", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Connect", $"Exception: {ex}");
            Log.Verbose("FluxWSClient.Connect", "LEAVE");
            throw;
        }
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
    /// Subscribe to a Connected event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<ConnectedResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _connectedReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a TurnInfo event from the Deepgram API. The turn lifecycle
    /// (StartOfTurn, Update, EagerEndOfTurn, TurnResumed, EndOfTurn) is carried in
    /// <see cref="TurnInfoResponse.Event"/> / <see cref="TurnInfoResponse.EventType"/>.
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<TurnInfoResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _turnInfoReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a ConfigureSuccess event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<ConfigureSuccessResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _configureSuccessReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a ConfigureFailure event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<ConfigureFailureResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _configureFailureReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to an Error event from the Deepgram API. Flux fatal errors carry a
    /// sequence_id, code, and description; the connection closes after one is received.
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<ErrorResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _fluxErrorReceived += (sender, e) => eventHandler(sender, e);
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
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<CloseResponse> eventHandler)
    {
        // Create a new event handler that wraps the original one
        EventHandler<Common.CloseResponse> wrappedHandler = (sender, args) =>
        {
            // A user-initiated Stop() and the receive loop's handling of the server close can
            // overlap and both raise the base Close event; deliver only the first to subscribers.
            if (Interlocked.Exchange(ref _closeEventFired, 1) == 1)
            {
                Log.Debug("CloseEvent", "Close event already delivered for this connection. Skipping...");
                return;
            }

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
    /// Subscribe to an Unhandled event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<UnhandledResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _fluxUnhandledReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }
    #endregion

    #region Send Functions
    /// <summary>
    /// Sends a Configure message to Deepgram to update end-of-turn thresholds, keyterms, or
    /// language hints mid-stream. Parameters left null keep their current values. The API
    /// responds with a ConfigureSuccess or ConfigureFailure message.
    /// </summary>
    /// <param name="configure">The configuration update to send</param>
    public async Task SendConfigure(ConfigureSchema configure)
    {
        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }
        configure.Type ??= Constants.Configure;

        Log.Debug("SendConfigure", $"Sending Configure Message Immediately... {configure}");
        byte[] data = Encoding.UTF8.GetBytes(configure.ToString());
        await SendMessageImmediately(data);
    }

    /// <summary>
    /// Sends a binary audio message over the WebSocket connection. Flux works best with
    /// audio sent in chunks of roughly 80ms.
    /// </summary>
    /// <param name="data">The audio to be sent over the WebSocket.</param>
    /// <param name="length">The number of bytes from the data to send. Use `Constants.UseArrayLengthForSend` to send the entire array.</param>
    public void Send(byte[] data, int length = Abstract.Constants.UseArrayLengthForSend) => SendBinary(data, length);

    /// <summary>
    /// Sends a CloseStream message to Deepgram, then waits (up to
    /// <see cref="Constants.DefaultCloseStreamGracePeriodMs"/>) for the server to deliver the
    /// final turn results and close the connection, so the end of the conversation is not lost.
    /// </summary>
    /// <param name="nullByte">Send a null byte instead of the CloseStream message</param>
    /// <param name="_cancellationToken">Provide a cancel token to be used for the send or use the internal one</param>
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

        // Flux delivers the final turn results AFTER CloseStream and then closes the connection
        // from its side. Give it a grace period instead of tearing the socket down immediately.
        try
        {
            var deadline = Constants.DefaultCloseStreamGracePeriodMs;
            while (deadline > 0 && _clientWebSocket != null && _clientWebSocket.State == WebSocketState.Open)
            {
                await Task.Delay(50, _cancelToken.Token).ConfigureAwait(false);
                deadline -= 50;
            }
            Log.Debug("SendClose", deadline > 0 ? "Server closed the connection" : "Grace period elapsed");
        }
        catch (OperationCanceledException)
        {
            Log.Debug("SendClose", "Grace period cancelled.");
        }
        catch (Exception ex)
        {
            Log.Verbose("SendClose", $"Ignoring exception while waiting for server close: {ex}");
        }
    }
    #endregion

    internal override void ProcessBinaryMessage(WebSocketReceiveResult result, MemoryStream ms)
    {
        // The Flux API is not expected to send binary frames; ignore them rather than fault
        // the receive loop.
        Log.Warning("ProcessBinaryMessage", "Received unexpected binary message from the Flux API. Ignoring...");
    }

    internal override void ProcessTextMessage(WebSocketReceiveResult result, MemoryStream ms)
    {
        Log.Verbose("FluxWSClient.ProcessTextMessage", "ENTER");

        ms.Seek(0, SeekOrigin.Begin);

        var response = Encoding.UTF8.GetString(ms.ToArray());
        if (response == null)
        {
            Log.Warning("ProcessTextMessage", "Response is null");
            Log.Verbose("FluxWSClient.ProcessTextMessage", "LEAVE");
            return;
        }

        try
        {
            if (Log.IsEnabled(LogLevel.Verbose))
            {
                Log.Verbose("ProcessTextMessage", $"raw response: {response}");
            }

            var data = JsonDocument.Parse(response);
            if (!data.RootElement.TryGetProperty("type", out var typeElement) ||
                !Enum.TryParse<FluxType>(typeElement.GetString(), out var val))
            {
                Log.Debug("ProcessTextMessage", "Message type is missing or unknown. Invoking UnhandledResponse...");
                InvokeUnhandled(response);
                Log.Verbose("FluxWSClient.ProcessTextMessage", "LEAVE");
                return;
            }

            if (Log.IsEnabled(LogLevel.Verbose))
            {
                Log.Verbose("ProcessTextMessage", $"Type: {val}");
            }

            switch (val)
            {
                case FluxType.Open:
                case FluxType.Close:
                    Log.Debug("ProcessTextMessage", "Calling base.ProcessTextMessage...");
                    base.ProcessTextMessage(result, ms);
                    break;
                case FluxType.Connected:
                    var connectedResponse = data.Deserialize<ConnectedResponse>();
                    if (_connectedReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_connectedReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (connectedResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "ConnectedResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.Debug("ProcessTextMessage", $"Invoking ConnectedResponse. event: {connectedResponse}");
                    }
                    InvokeParallel(_connectedReceived, connectedResponse);
                    break;
                case FluxType.TurnInfo:
                    var turnInfoResponse = data.Deserialize<TurnInfoResponse>();
                    if (_turnInfoReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_turnInfoReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (turnInfoResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "TurnInfoResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.Debug("ProcessTextMessage", $"Invoking TurnInfoResponse. event: {turnInfoResponse}");
                    }
                    InvokeParallel(_turnInfoReceived, turnInfoResponse);
                    break;
                case FluxType.ConfigureSuccess:
                    var configureSuccessResponse = data.Deserialize<ConfigureSuccessResponse>();
                    if (_configureSuccessReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_configureSuccessReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (configureSuccessResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "ConfigureSuccessResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.Debug("ProcessTextMessage", $"Invoking ConfigureSuccessResponse. event: {configureSuccessResponse}");
                    }
                    InvokeParallel(_configureSuccessReceived, configureSuccessResponse);
                    break;
                case FluxType.ConfigureFailure:
                    var configureFailureResponse = data.Deserialize<ConfigureFailureResponse>();
                    if (_configureFailureReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_configureFailureReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (configureFailureResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "ConfigureFailureResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.Debug("ProcessTextMessage", $"Invoking ConfigureFailureResponse. event: {configureFailureResponse}");
                    }
                    InvokeParallel(_configureFailureReceived, configureFailureResponse);
                    break;
                case FluxType.Error:
                    // Flux fatal errors use the wire type "Error" but carry a different shape
                    // (sequence_id/code/description) than the common error message, so they are
                    // handled here rather than by the base class.
                    var errorResponse = data.Deserialize<ErrorResponse>();
                    if (_fluxErrorReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_fluxErrorReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (errorResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "ErrorResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    if (Log.IsEnabled(LogLevel.Debug))
                    {
                        Log.Debug("ProcessTextMessage", $"Invoking ErrorResponse. event: {errorResponse}");
                    }
                    InvokeParallel(_fluxErrorReceived, errorResponse);
                    break;
                default:
                    Log.Debug("ProcessTextMessage", "Unknown message type. Invoking UnhandledResponse...");
                    InvokeUnhandled(response);
                    break;
            }

            Log.Debug("ProcessTextMessage", "Succeeded");
            Log.Verbose("FluxWSClient.ProcessTextMessage", "LEAVE");
        }
        catch (JsonException ex)
        {
            Log.Error("ProcessTextMessage", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessTextMessage", $"Exception: {ex}");
            Log.Verbose("FluxWSClient.ProcessTextMessage", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessTextMessage", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessTextMessage", $"Exception: {ex}");
            Log.Verbose("FluxWSClient.ProcessTextMessage", "LEAVE");
        }
    }

    #region Helpers
    private void InvokeUnhandled(string response)
    {
        if (_fluxUnhandledReceived == null)
        {
            Log.Debug("InvokeUnhandled", "_fluxUnhandledReceived has no listeners");
            return;
        }

        var unhandledResponse = new UnhandledResponse();
        unhandledResponse.Type = Common.WebSocketType.Unhandled;
        unhandledResponse.Raw = response;

        InvokeParallel(_fluxUnhandledReceived, unhandledResponse);
    }

    /// <summary>
    /// Get the URI for the WebSocket connection
    /// </summary>
    internal static Uri GetUri(IDeepgramClientOptions options, FluxSchema parameter, Dictionary<string, string>? addons = null)
    {
        var baseAddress = options.BaseAddress;

        // If the base address carries an API version (the SDK default appends /v1), remove it.
        // Flux lives on /v2/listen; the version is part of UriSegments.LISTEN_V2. This follows
        // the same pattern the Agent client uses for its own endpoint.
        Regex regex = new Regex(@"\b(\/v[0-9]+)\b", RegexOptions.IgnoreCase);
        if (regex.IsMatch(baseAddress))
        {
            Log.Information("GetUri", $"BaseAddress contains API version: {baseAddress}");
            baseAddress = regex.Replace(baseAddress, "");
            Log.Debug("GetUri", $"BaseAddress: {baseAddress}");
        }
        baseAddress = baseAddress.TrimEnd('/');

        var queryString = BuildQueryString(parameter, addons);

        return new Uri($"{baseAddress}/{UriSegments.LISTEN_V2}?{queryString}");
    }

    /// <summary>
    /// Builds the query string for the Flux connection. Numbers are formatted with the
    /// invariant culture so threshold values like 0.7 are never sent as "0,7" on machines
    /// with a comma decimal separator, and list values are repeated (keyterm=a&amp;keyterm=b).
    /// </summary>
    internal static string BuildQueryString(FluxSchema parameter, Dictionary<string, string>? addons = null)
    {
        var sb = new StringBuilder();

        var propertyInfoList = parameter.GetType()
            .GetProperties()
            .Where(v => v.GetValue(parameter) is not null);

        foreach (var pInfo in propertyInfoList)
        {
            var name = pInfo.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
            if (name is null)
            {
                continue;
            }
            var pValue = pInfo.GetValue(parameter);

            switch (pValue)
            {
                case bool boolean:
                    Append(name, boolean ? "true" : "false");
                    break;
                case System.Collections.IList list:
                    foreach (var value in list)
                    {
                        Append(name, Convert.ToString(value, CultureInfo.InvariantCulture) ?? "");
                    }
                    break;
                case IFormattable formattable:
                    Append(name, formattable.ToString(null, CultureInfo.InvariantCulture));
                    break;
                default:
                    Append(name, pValue?.ToString() ?? "");
                    break;
            }
        }

        if (addons != null)
        {
            foreach (var kvp in addons)
            {
                Append(kvp.Key, kvp.Value);
            }
        }

        return sb.ToString().TrimEnd('&');

        void Append(string name, string value) =>
            sb.Append($"{name}={HttpUtility.UrlEncode(value)}&");
    }
    #endregion
}
