// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Abstractions.v2;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Agent.v2.WebSocket;
using Common = Deepgram.Models.Common.v2.WebSocket;
using Deepgram.Clients.Interfaces.v2;

namespace Deepgram.Clients.Agent.v2.WebSocket;

/// <summary>
/// Implements version 2 of the Listen WebSocket Client.
/// </summary>
public class Client : AbstractWebSocketClient, IAgentWebSocketClient
{
    /// <param name="apiKey">Required DeepgramApiKey</param>
    /// <param name="deepgramClientOptions"><see cref="IDeepgramClientOptions"/> for HttpClient Configuration</param>
    public Client(string? apiKey = null, IDeepgramClientOptions? options = null) : base(apiKey, options)
    {
        Log.Verbose("AgentWSClient", "ENTER");
        Log.Debug("AgentWSClient", $"KeepAlive: {_deepgramClientOptions.KeepAlive}");
        Log.Verbose("AgentWSClient", "LEAVE");
    }

    #region Event Handlers
    /// <summary>
    /// Fires when an event is received from the Deepgram API
    /// </summary>
    private event EventHandler<AudioResponse>? _audioReceived;
    private event EventHandler<AgentAudioDoneResponse>? _agentAudioDoneReceived;
    private event EventHandler<AgentStartedSpeakingResponse>? _agentStartedSpeakingReceived;
    private event EventHandler<AgentThinkingResponse>? _agentThinkingReceived;
    private event EventHandler<ConversationTextResponse>? _conversationTextReceived;
    private event EventHandler<FunctionCallingResponse>? _functionCallingReceived;
    private event EventHandler<FunctionCallRequestResponse>? _functionCallRequestReceived;
    private event EventHandler<UserStartedSpeakingResponse>? _userStartedSpeakingReceived;
    private event EventHandler<WelcomeResponse>? _welcomeReceived;
    #endregion

    /// <summary>
    /// Connect to a Deepgram API Web Socket to begin transcribing audio
    /// </summary>
    /// <param name="options">Options to use when transcribing audio</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task<bool> Connect(SettingsConfigurationSchema options, CancellationTokenSource? cancelToken = null, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null)
    {
        Log.Verbose("AgentWSClient.Connect", "ENTER");
        Log.Information("Connect", $"options:\n{JsonSerializer.Serialize(options, JsonSerializeOptions.DefaultOptions)}");
        Log.Debug("Connect", $"addons: {addons}");

        try
        {
            var myURI = GetUri(_deepgramClientOptions);
            Log.Debug("Connect", $"uri: {myURI}");
            bool bConnected = await base.Connect(myURI.ToString(), cancelToken, headers);
            if (!bConnected)
            {
                Log.Warning("Connect", "Connect failed");
                Log.Verbose("AgentWSClient.Connect", "LEAVE");
                return false;
            }

            // send the settings configuration
            var bytes = Encoding.UTF8.GetBytes(options.ToString());
            await SendMessageImmediately(bytes);

            // keepalive thread
            if (_deepgramClientOptions.KeepAlive)
            {
                Log.Debug("Connect", "Starting KeepAlive Thread...");
                StartKeepAliveBackgroundThread();
            }

            Log.Debug("Connect", "Connect Succeeded");
            Log.Verbose("AgentWSClient.Connect", "LEAVE");

            return true;
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("Connect", "Connect cancelled.");
            Log.Verbose("Connect", $"Connect cancelled. Info: {ex}");
            Log.Verbose("AgentWSClient.Connect", "LEAVE");

            return false;
        }
        catch (Exception ex)
        {
            Log.Error("Connect", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("Connect", $"Exception: {ex}");
            Log.Verbose("AgentWSClient.Connect", "LEAVE");
            throw;
        }

        void StartKeepAliveBackgroundThread() => Task.Run(async () => await ProcessKeepAlive());
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
    /// Subscribe to a Audio event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
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
    /// Subscribe to a AgentAudioDone event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<AgentAudioDoneResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _agentAudioDoneReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a AgentStartedSpeaking event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<AgentStartedSpeakingResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _agentStartedSpeakingReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to an AgentThinking event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<AgentThinkingResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _agentThinkingReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a ConversationText event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<ConversationTextResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _conversationTextReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a FunctionCalling event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<FunctionCallingResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _functionCallingReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a FunctionCallRequest event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<FunctionCallRequestResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _functionCallRequestReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a UserStartedSpeaking event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<UserStartedSpeakingResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _userStartedSpeakingReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a Welcome event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<WelcomeResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _welcomeReceived += (sender, e) => eventHandler(sender, e);
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
        ControlMessage message = new ControlMessage(AgentClientTypes.KeepAlive);
        byte[] data = Encoding.ASCII.GetBytes(message.ToString());
        await SendMessageImmediately(data);
    }

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

        ControlMessage message = new ControlMessage(AgentClientTypes.Close);
        byte[] data = Encoding.ASCII.GetBytes(message.ToString());
        await SendMessageImmediately(data);
    }
    #endregion

    internal async Task ProcessKeepAlive()
    {
        Log.Verbose("AgentWSClient.ProcessKeepAlive", "ENTER");

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
            Log.Verbose("AgentWSClient.ProcessKeepAlive", "LEAVE");
        }
        catch (TaskCanceledException ex)
        {
            Log.Debug("ProcessKeepAlive", "KeepAliveThread cancelled.");
            Log.Verbose("ProcessKeepAlive", $"KeepAliveThread cancelled. Info: {ex}");
            Log.Verbose("AgentWSClient.ProcessKeepAlive", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessKeepAlive", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessKeepAlive", $"Exception: {ex}");
            Log.Verbose("AgentWSClient.ProcessKeepAlive", "LEAVE");
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
        Log.Verbose("AgentWSClient.ProcessTextMessage", "ENTER");

        ms.Seek(0, SeekOrigin.Begin);

        var response = Encoding.UTF8.GetString(ms.ToArray());
        if (response == null)
        {
            Log.Warning("ProcessTextMessage", "Response is null");
            Log.Verbose("AgentWSClient.ProcessTextMessage", "LEAVE");
            return;
        }

        try
        {
            Log.Verbose("ProcessTextMessage", $"raw response: {response}");
            var data = JsonDocument.Parse(response);
            var val = Enum.Parse(typeof(AgentType), data.RootElement.GetProperty("type").GetString()!);

            Log.Verbose("ProcessTextMessage", $"Type: {val}");

            switch (val)
            {
                case AgentType.Open:
                case AgentType.Close:
                case AgentType.Error:
                    Log.Debug("ProcessTextMessage", "Calling base.ProcessTextMessage...");
                    base.ProcessTextMessage(result, ms);
                    break;
                case AgentType.AgentAudioDone:
                    var agentAudioDoneResponse = data.Deserialize<AgentAudioDoneResponse>();
                    if (_agentAudioDoneReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_agentAudioDoneReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (agentAudioDoneResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "AgentAudioDoneResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessTextMessage", $"Invoking AgentAudioDoneResponse. event: {agentAudioDoneResponse}");
                    InvokeParallel(_agentAudioDoneReceived, agentAudioDoneResponse);
                    break;
                case AgentType.AgentStartedSpeaking:
                    var agentStartedSpeakingResponse = data.Deserialize<AgentStartedSpeakingResponse>();
                    if (_agentStartedSpeakingReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_agentStartedSpeakingReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (agentStartedSpeakingResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "AgentStartedSpeakingResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessTextMessage", $"Invoking AgentStartedSpeakingResponse. event: {agentStartedSpeakingResponse}");
                    InvokeParallel(_agentStartedSpeakingReceived, agentStartedSpeakingResponse);
                    break;
                case AgentType.AgentThinking:
                    var agentThinkingResponse = data.Deserialize<AgentThinkingResponse>();
                    if (_agentThinkingReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_agentThinkingReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (agentThinkingResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "AgentThinkingResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessTextMessage", $"Invoking AgentThinkingResponse. event: {agentThinkingResponse}");
                    InvokeParallel(_agentThinkingReceived, agentThinkingResponse);
                    break;
                case AgentType.ConversationText:
                    var conversationTextResponse = data.Deserialize<ConversationTextResponse>();
                    if (_conversationTextReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_conversationTextReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (conversationTextResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "ConversationTextResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessTextMessage", $"Invoking ConversationTextResponse. event: {conversationTextResponse}");
                    InvokeParallel(_conversationTextReceived, conversationTextResponse);
                    break;
                case AgentType.FunctionCalling:
                    var functionCallingResponse = data.Deserialize<FunctionCallingResponse>();
                    if (_functionCallingReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_functionCallingReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (functionCallingResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "FunctionCallingResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessTextMessage", $"Invoking FunctionCallingResponse. event: {functionCallingResponse}");
                    InvokeParallel(_functionCallingReceived, functionCallingResponse);
                    break;
                case AgentType.FunctionCallRequest:
                    var functionCallRequestResponse = data.Deserialize<FunctionCallRequestResponse>();
                    if (_functionCallRequestReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_functionCallRequestReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (functionCallRequestResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "FunctionCallRequestResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessTextMessage", $"Invoking FunctionCallRequestResponse. event: {functionCallRequestResponse}");
                    InvokeParallel(_functionCallRequestReceived, functionCallRequestResponse);
                    break;
                case AgentType.UserStartedSpeaking:
                    var userStartedSpeakingResponse = data.Deserialize<UserStartedSpeakingResponse>();
                    if (_userStartedSpeakingReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_userStartedSpeakingReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (userStartedSpeakingResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "UserStartedSpeakingResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessTextMessage", $"Invoking UserStartedSpeakingResponse. event: {userStartedSpeakingResponse}");
                    InvokeParallel(_userStartedSpeakingReceived, userStartedSpeakingResponse);
                    break;
                case AgentType.Welcome:
                    var welcomeResponse = data.Deserialize<WelcomeResponse>();
                    if (_welcomeReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_welcomeReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (welcomeResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "WelcomeResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessTextMessage", $"Invoking WelcomeResponse. event: {welcomeResponse}");
                    InvokeParallel(_welcomeReceived, welcomeResponse);
                    break;
                default:
                    Log.Debug("ProcessTextMessage", "Calling base.ProcessTextMessage...");
                    base.ProcessTextMessage(result, ms);
                    break;
            }

            Log.Debug("ProcessTextMessage", "Succeeded");
            Log.Verbose("AgentWSClient.ProcessTextMessage", "LEAVE");
        }
        catch (JsonException ex)
        {
            Log.Error("ProcessTextMessage", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessTextMessage", $"Exception: {ex}");
            Log.Verbose("AgentWSClient.ProcessTextMessage", "LEAVE");
        }
        catch (Exception ex)
        {
            Log.Error("ProcessTextMessage", $"{ex.GetType()} thrown {ex.Message}");
            Log.Verbose("ProcessTextMessage", $"Exception: {ex}");
            Log.Verbose("AgentWSClient.ProcessTextMessage", "LEAVE");
        }
    }

    #region Helpers
    /// <summary>
    /// Get the URI for the WebSocket connection
    /// </summary> 
    internal static Uri GetUri(IDeepgramClientOptions options)
    {
        var baseAddress = options.BaseAddress;

        // if base address contains "api.deepgram.com", then replace with "agent.deepgram.com"
        // which is the default URI for the Agent API. This will preserve the wss or ws prefix.
        // If this is a custom URI, then the we don't need to modify anything because DeepgramWSClientOptions
        // will attach the protocol to the URI and the URI will be used as is.
        if (baseAddress.Contains("api.deepgram.com"))
        {
            Log.Debug("GetUri", "Replacing baseAddress with agent.deepgram.com");
            baseAddress = baseAddress.Replace("api.deepgram.com", UriSegments.AGENT_URI);
        }
        Log.Debug("GetUri", $"baseAddress: {baseAddress}");

        // if the base address has an v1, v2, etc remove it from the URI
        Regex regex = new Regex(@"\b(\/v[0-9]+)\b", RegexOptions.IgnoreCase);
        if (regex.IsMatch(baseAddress))
        {
            Log.Information("GetUri", $"BaseAddress contains API version: {baseAddress}");
            baseAddress = regex.Replace(baseAddress, "");
            Log.Debug("GetUri", $"BaseAddress: {baseAddress}");
        }

        return new Uri($"{baseAddress}/{UriSegments.AGENT}");
    }
    #endregion
}
