// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Abstractions.v2;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Models.Agent.v2.WebSocket;
using Common = Deepgram.Models.Common.v2.WebSocket;
using Deepgram.Clients.Interfaces.v2;
using Deepgram.Models.Exceptions.v1;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Deepgram.Clients.Agent.v2.WebSocket;

/// <summary>
/// Implements version 2 of the Listen WebSocket Client.
/// </summary>
public class Client : AbstractWebSocketClient, IAgentProtocolClient
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
    private event EventHandler<FunctionCallRequestResponse>? _functionCallRequestReceived;
    private event EventHandler<UserStartedSpeakingResponse>? _userStartedSpeakingReceived;
    private event EventHandler<WelcomeResponse>? _welcomeReceived;
    private event EventHandler<SettingsAppliedResponse>? _settingsAppliedReceived;
    private event EventHandler<InjectionRefusedResponse>? _injectionRefusedReceived;
    private event EventHandler<PromptUpdatedResponse>? _promptUpdatedReceived;
    private event EventHandler<SpeakUpdatedResponse>? _speakUpdatedReceived;
    private event EventHandler<ListenUpdatedResponse>? _listenUpdatedReceived;
    private event EventHandler<ThinkUpdatedResponse>? _thinkUpdatedReceived;
    private event EventHandler<FunctionCallCancelledResponse>? _functionCallCancelledReceived;
    private event EventHandler<FunctionCallResponse>? _functionCallResponseReceived;
    private event EventHandler<CustomFromThinkProviderResponse>? _customFromThinkProviderReceived;
    private event EventHandler<LatencyReportResponse>? _latencyReportReceived;
    private event EventHandler<AgentWarningResponse>? _warningReceived;
    private event EventHandler<AgentHistoryResponse>? _historyReceived;
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
    /// connection that is not stopping. Connect() sends the Settings message before raising Open and
    /// before any queued message; if Settings cannot be sent, Connect() fails and raises neither Open
    /// nor Close. When Connect() returns true for a connection that another Connect() opened, that
    /// connection keeps the Settings it was opened with: this call's options are not sent.
    /// A Stop() that begins before Connect() returns (for example, from an Open handler) makes
    /// Connect() return false even after Open fired; Close then follows.
    /// </remarks>
    /// <param name="options">Options to use when transcribing audio</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task<bool> Connect(SettingsSchema options, CancellationTokenSource? cancelToken = null, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null)
    {
        Log.Verbose("AgentWSClient.Connect", "ENTER");
        Log.Information("Connect", $"options:\n{JsonSerializer.Serialize(options, JsonSerializeOptions.DefaultOptions)}");
        Log.Debug("Connect", $"addons: {addons}");

        try
        {
            var myURI = GetUri(_deepgramClientOptions);
            Log.Debug("Connect", $"uri: {myURI}");
            var settings = BuildSettingsPayload(options);
            // owned is the connection this call opened. It is null when another connection was
            // already published ("already initialized"): that connection's own Connect() sent its
            // Settings message, so this call does no post-connect work (#455). The Settings message
            // is sent by the pre-open step, before Open and before anything queued (#455).
            var (bConnected, owned) = await ConnectCore(myURI.ToString(), cancelToken, headers,
                (connection, connectToken) => SendSettings(connection, settings, connectToken)).ConfigureAwait(false);
            if (!bConnected)
            {
                Log.Warning("Connect", "Connect failed");
                Log.Verbose("AgentWSClient.Connect", "LEAVE");
                return false;
            }

            if (owned == null)
            {
                Log.Debug("Connect", "Client has already been initialized. Not sending Settings.");
                Log.Verbose("AgentWSClient.Connect", "LEAVE");
                return true;
            }

            // keepalive thread, handed the connection it belongs to (#455)
            if (_deepgramClientOptions.KeepAlive)
            {
                Log.Debug("Connect", "Starting KeepAlive Thread...");
                StartKeepAliveBackgroundThread(owned);
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

        void StartKeepAliveBackgroundThread(ConnectionState owned) => Task.Run(async () => await ProcessKeepAlive(owned));
    }

    /// <summary>
    /// Serializes the Settings message, dropping empty provider objects and agent sections.
    /// </summary>
    private static byte[] BuildSettingsPayload(SettingsSchema options)
    {
        var json = options.ToString(); // Assuming options is a serializable object
        var root = JsonNode.Parse(json)!.AsObject();

        // Check agent exists
        if (root.TryGetPropertyValue("agent", out var agentNode) && agentNode is JsonObject agent)
        {
            void DeleteProviderIfEmpty(JsonObject parent)
            {
                if (parent.TryGetPropertyValue("provider", out var providerNode))
                {
                    if (providerNode is JsonObject providerObj && providerObj.Count == 0)
                    {
                        parent.Remove("provider");
                    }
                }
            }

            string[] sections = { "think", "speak", "listen" };

            foreach (var section in sections)
            {
                if (agent.TryGetPropertyValue(section, out var sectionNode) && sectionNode is JsonObject sectionObj)
                {
                    DeleteProviderIfEmpty(sectionObj);
                    if (sectionObj.Count == 0)
                    {
                        agent.Remove(section);
                    }
                }
            }
        }

        // Re-serialize the cleaned object
        var cleanedJson = root.ToJsonString();
        Log.Debug("Connect", $"cleanedJson: {cleanedJson}");
        return Encoding.UTF8.GetBytes(cleanedJson);
    }

    /// <summary>
    /// The pre-open step of <see cref="Connect"/>: sends the Settings message on the connection
    /// being opened, before its sender starts and before Open. True when the connection is still
    /// open afterwards. Without its Settings message the agent connection is unusable, so false
    /// fails the connect (#455).
    /// </summary>
    private async Task<bool> SendSettings(ConnectionState owned, byte[] settings, CancellationToken connectToken)
    {
        if (!owned.IsOpen)
        {
            Log.Warning("Connect", "The connection closed before the Settings message was sent");
            return false;
        }
        if (owned.IsStopping)
        {
            // A Stop() has begun: the connect fails, and the Settings message is not sent.
            Log.Warning("Connect", "The connection was stopped before the Settings message was sent");
            return false;
        }

        try
        {
            // Bounded by the connect token, and by the connection's own token so a release ends
            // the send at once.
            using var sendToken = CancellationTokenSource.CreateLinkedTokenSource(owned.Token, connectToken);
            await SendOnConnection(owned, new ArraySegment<byte>(settings), WebSocketMessageType.Text, sendToken.Token)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (connectToken.IsCancellationRequested)
        {
            Log.Warning("Connect", "The connect token was cancelled while the Settings message was being sent");
            return false;
        }
        catch (ObjectDisposedException) when (owned.IsReleased)
        {
            // The connection was released (its token source disposed) before the send started.
            return false;
        }

        // The send does nothing once the connection is released, and the connection can drop
        // during the send. Either way the agent was never configured. A Stop() that began during
        // the send fails the connect too.
        if (!owned.IsOpen || owned.IsStopping)
        {
            Log.Warning("Connect", "The connection closed or was stopped while the Settings message was being sent");
            return false;
        }

        return true;
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
    /// Subscribe to a SettingsApplied event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<SettingsAppliedResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _settingsAppliedReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to an InjectionRefused event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<InjectionRefusedResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _injectionRefusedReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a PromptUpdated event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<PromptUpdatedResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _promptUpdatedReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    /// <summary>
    /// Subscribe to a SpeakUpdated event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public async Task<bool> Subscribe(EventHandler<SpeakUpdatedResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _speakUpdatedReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    public async Task<bool> Subscribe(EventHandler<ListenUpdatedResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _listenUpdatedReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    public async Task<bool> Subscribe(EventHandler<ThinkUpdatedResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _thinkUpdatedReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    public async Task<bool> Subscribe(EventHandler<FunctionCallCancelledResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _functionCallCancelledReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    public async Task<bool> Subscribe(EventHandler<FunctionCallResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _functionCallResponseReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    public async Task<bool> Subscribe(EventHandler<CustomFromThinkProviderResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _customFromThinkProviderReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    public async Task<bool> Subscribe(EventHandler<LatencyReportResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _latencyReportReceived += (sender, e) => eventHandler(sender, e);
        }
        finally
        {
            _mutexSubscribe.Release();
        }
        return true;
    }

    public async Task<bool> Subscribe(EventHandler<AgentWarningResponse> eventHandler)
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

    public async Task<bool> Subscribe(EventHandler<AgentHistoryResponse> eventHandler)
    {
        await _mutexSubscribe.WaitAsync();
        try
        {
            _historyReceived += (sender, e) => eventHandler(sender, e);
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
        ControlMessage message = new ControlMessage(AgentClientTypes.KeepAlive);
        byte[] data = Encoding.ASCII.GetBytes(message.ToString());
        await SendMessageImmediately(data);
    }

    /// <summary>
    /// Sends an InjectUserMessage to the agent
    /// </summary>
    /// <remarks>
    /// Sent immediately, not queued: dropped while the client is not connected, including while
    /// Connect() is still in progress.
    /// </remarks>
    /// <param name="content">The specific phrase or statement the agent should respond to</param>
    public async Task SendInjectUserMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            Log.Warning("SendInjectUserMessage", "Content cannot be null or empty");
            throw new ArgumentException("Content cannot be null or empty", nameof(content));
        }

        var injectUserMessage = new InjectUserMessageSchema
        {
            Content = content
        };

        await SendInjectUserMessage(injectUserMessage);
    }

    /// <summary>
    /// Sends an InjectUserMessage to the agent using a schema object
    /// </summary>
    /// <remarks>
    /// Sent immediately, not queued: dropped while the client is not connected, including while
    /// Connect() is still in progress.
    /// </remarks>
    /// <param name="injectUserMessageSchema">The InjectUserMessage schema containing the message details</param>
    public async Task SendInjectUserMessage(InjectUserMessageSchema injectUserMessageSchema)
    {
        if (injectUserMessageSchema == null)
        {
            Log.Warning("SendInjectUserMessage", "InjectUserMessageSchema cannot be null");
            throw new ArgumentNullException(nameof(injectUserMessageSchema));
        }

        if (string.IsNullOrWhiteSpace(injectUserMessageSchema.Content))
        {
            Log.Warning("SendInjectUserMessage", "Content cannot be null or empty");
            throw new ArgumentException("Content cannot be null or empty", nameof(injectUserMessageSchema.Content));
        }

        Log.Debug("SendInjectUserMessage", $"Sending InjectUserMessage: {injectUserMessageSchema.Content}");

        byte[] data = Encoding.UTF8.GetBytes(injectUserMessageSchema.ToString());
        await SendMessageImmediately(data);
    }

    /// <summary>
    /// Flushes queued audio and then ends the current user turn. This control is supported when
    /// the Agent session uses a Flux STT listen provider.
    /// </summary>
    /// <remarks>
    /// Sent immediately, not queued: dropped while the client is not connected, including while
    /// Connect() is still in progress.
    /// </remarks>
    public async Task SendForceEndTurn()
    {
        await Flush();
        await SendMessageImmediately(Encoding.UTF8.GetBytes(new AgentForceEndTurnSchema().ToString()));
    }
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

        ControlMessage message = new ControlMessage(AgentClientTypes.Close);
        byte[] data = Encoding.ASCII.GetBytes(message.ToString());
        // The caller's token bounds the send, so Stop(token) keeps its timeout.
        await SendOnConnection(owned, new ArraySegment<byte>(data), WebSocketMessageType.Text, _cancellationToken?.Token ?? owned.Token);
    }
    #endregion

    /// <summary>SendKeepAlive() for one connection: the keepalive loop sends on its own connection only (#455).</summary>
    private async Task SendKeepAlive(ConnectionState owned)
    {
        ControlMessage message = new ControlMessage(AgentClientTypes.KeepAlive);
        byte[] data = Encoding.ASCII.GetBytes(message.ToString());
        await SendOnConnection(owned, new ArraySegment<byte>(data), WebSocketMessageType.Text, owned.Token);
    }

    internal async Task ProcessKeepAlive(ConnectionState owned)
    {
        Log.Verbose("AgentWSClient.ProcessKeepAlive", "ENTER");

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
            // Use TryParse so Agent message types unknown to this SDK version (e.g. new server
            // messages like "History"/"Warning") are routed to the base handler and surfaced as
            // Unhandled instead of throwing an ArgumentException. See #395.
            if (!data.RootElement.TryGetProperty("type", out var typeElement) ||
                typeElement.ValueKind != JsonValueKind.String)
            {
                Log.Debug("ProcessTextMessage", "Message type is missing or unknown. Routing to base handler...");
                base.ProcessTextMessage(result, ms);
                Log.Verbose("AgentWSClient.ProcessTextMessage", "LEAVE");
                return;
            }

            var type = typeElement.GetString();
            AgentType val;
            if (string.Equals(type, AgentClientTypes.CustomFromThinkProvider, StringComparison.Ordinal))
            {
                val = AgentType.CustomFromThinkProvider;
            }
            else if (!Enum.TryParse(type, out val))
            {
                Log.Debug("ProcessTextMessage", "Message type is unknown. Routing to base handler...");
                base.ProcessTextMessage(result, ms);
                Log.Verbose("AgentWSClient.ProcessTextMessage", "LEAVE");
                return;
            }

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
                case AgentType.SettingsApplied:
                    var settingsAppliedResponse = data.Deserialize<SettingsAppliedResponse>();
                    if (_settingsAppliedReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_settingsAppliedReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (settingsAppliedResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "SettingsAppliedResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessTextMessage", $"Invoking SettingsAppliedResponse. event: {settingsAppliedResponse}");
                    InvokeParallel(_settingsAppliedReceived, settingsAppliedResponse);
                    break;
                case AgentType.InjectionRefused:
                    var injectionRefusedResponse = data.Deserialize<InjectionRefusedResponse>();
                    if (_injectionRefusedReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_injectionRefusedReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (injectionRefusedResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "InjectionRefusedResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessTextMessage", $"Invoking InjectionRefusedResponse. event: {injectionRefusedResponse}");
                    InvokeParallel(_injectionRefusedReceived, injectionRefusedResponse);
                    break;
                case AgentType.PromptUpdated:
                    var promptUpdatedResponse = data.Deserialize<PromptUpdatedResponse>();
                    if (_promptUpdatedReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_promptUpdatedReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (promptUpdatedResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "PromptUpdatedResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessTextMessage", $"Invoking PromptUpdatedResponse. event: {promptUpdatedResponse}");
                    InvokeParallel(_promptUpdatedReceived, promptUpdatedResponse);
                    break;
                case AgentType.SpeakUpdated:
                    var speakUpdatedResponse = data.Deserialize<SpeakUpdatedResponse>();
                    if (_speakUpdatedReceived == null)
                    {
                        Log.Debug("ProcessTextMessage", "_speakUpdatedReceived has no listeners");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }
                    if (speakUpdatedResponse == null)
                    {
                        Log.Warning("ProcessTextMessage", "SpeakUpdatedResponse is invalid");
                        Log.Verbose("ProcessTextMessage", "LEAVE");
                        return;
                    }

                    Log.Debug("ProcessTextMessage", $"Invoking SpeakUpdatedResponse. event: {speakUpdatedResponse}");
                    InvokeParallel(_speakUpdatedReceived, speakUpdatedResponse);
                    break;
                case AgentType.ListenUpdated:
                    if (_listenUpdatedReceived == null)
                    {
                        base.ProcessTextMessage(result, ms);
                        return;
                    }
                    var listenUpdatedResponse = data.Deserialize<ListenUpdatedResponse>();
                    if (listenUpdatedResponse == null)
                    {
                        base.ProcessTextMessage(result, ms);
                        return;
                    }
                    InvokeParallel(_listenUpdatedReceived, listenUpdatedResponse);
                    break;
                case AgentType.ThinkUpdated:
                    if (_thinkUpdatedReceived == null)
                    {
                        base.ProcessTextMessage(result, ms);
                        return;
                    }
                    var thinkUpdatedResponse = data.Deserialize<ThinkUpdatedResponse>();
                    if (thinkUpdatedResponse == null)
                    {
                        base.ProcessTextMessage(result, ms);
                        return;
                    }
                    InvokeParallel(_thinkUpdatedReceived, thinkUpdatedResponse);
                    break;
                case AgentType.FunctionCallCancelled:
                    if (_functionCallCancelledReceived == null)
                    {
                        base.ProcessTextMessage(result, ms);
                        return;
                    }
                    var functionCallCancelledResponse = data.Deserialize<FunctionCallCancelledResponse>();
                    if (functionCallCancelledResponse == null)
                    {
                        base.ProcessTextMessage(result, ms);
                        return;
                    }
                    InvokeParallel(_functionCallCancelledReceived, functionCallCancelledResponse);
                    break;
                case AgentType.FunctionCallResponse:
                    if (_functionCallResponseReceived == null)
                    {
                        base.ProcessTextMessage(result, ms);
                        return;
                    }
                    var functionCallResponse = data.Deserialize<FunctionCallResponse>();
                    if (functionCallResponse == null)
                    {
                        base.ProcessTextMessage(result, ms);
                        return;
                    }
                    InvokeParallel(_functionCallResponseReceived, functionCallResponse);
                    break;
                case AgentType.CustomFromThinkProvider:
                    if (_customFromThinkProviderReceived == null)
                    {
                        base.ProcessTextMessage(result, ms);
                        return;
                    }
                    var customFromThinkProviderResponse = data.Deserialize<CustomFromThinkProviderResponse>();
                    if (customFromThinkProviderResponse == null ||
                        customFromThinkProviderResponse.Content.ValueKind == JsonValueKind.Undefined)
                    {
                        base.ProcessTextMessage(result, ms);
                        return;
                    }
                    InvokeParallel(_customFromThinkProviderReceived, customFromThinkProviderResponse);
                    break;
                case AgentType.LatencyReport:
                    if (_latencyReportReceived == null)
                    {
                        base.ProcessTextMessage(result, ms);
                        return;
                    }
                    var latencyReportResponse = data.Deserialize<LatencyReportResponse>();
                    if (latencyReportResponse == null)
                    {
                        base.ProcessTextMessage(result, ms);
                        return;
                    }
                    InvokeParallel(_latencyReportReceived, latencyReportResponse);
                    break;
                case AgentType.Warning:
                    if (_warningReceived == null)
                    {
                        base.ProcessTextMessage(result, ms);
                        return;
                    }
                    var warningResponse = data.Deserialize<AgentWarningResponse>();
                    if (warningResponse == null)
                    {
                        base.ProcessTextMessage(result, ms);
                        return;
                    }
                    InvokeParallel(_warningReceived, warningResponse);
                    break;
                case AgentType.History:
                    if (_historyReceived == null)
                    {
                        base.ProcessTextMessage(result, ms);
                        return;
                    }
                    var historyResponse = data.Deserialize<AgentHistoryResponse>();
                    if (historyResponse == null)
                    {
                        base.ProcessTextMessage(result, ms);
                        return;
                    }
                    InvokeParallel(_historyReceived, historyResponse);
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
