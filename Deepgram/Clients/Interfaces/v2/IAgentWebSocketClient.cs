// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Agent.v2.WebSocket;

namespace Deepgram.Clients.Interfaces.v2;

/// <summary>
/// Implements version 2 of the Agent Client.
/// </summary>
public interface IAgentWebSocketClient
{
    #region Connect and Disconnect
    /// <summary>
    /// Connects to the Deepgram WebSocket API
    /// </summary>
    public Task<bool> Connect(SettingsSchema options, CancellationTokenSource? cancelToken = null, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null);

    /// <summary>
    /// Disconnects from the Deepgram WebSocket API
    /// </summary>
    public Task<bool> Stop(CancellationTokenSource? cancelToken = null, bool nullByte = false);
    #endregion

    #region Subscribe Event
    /// <summary>
    /// Subscribe to an Open event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<OpenResponse> eventHandler);

    /// <summary>
    /// Subscribe to an Audio Binary event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<AudioResponse> eventHandler);

    /// <summary>
    /// Subscribe to a AgentAudioDone event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<AgentAudioDoneResponse> eventHandler);

    /// <summary>
    /// Subscribe to a AgentStartedSpeaking event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<AgentStartedSpeakingResponse> eventHandler)
;
    /// <summary>
    /// Subscribe to an AgentThinking event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<AgentThinkingResponse> eventHandler);

    /// <summary>
    /// Subscribe to a ConversationText event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<ConversationTextResponse> eventHandler);

    /// <summary>
    /// Subscribe to a FunctionCallRequest event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<FunctionCallRequestResponse> eventHandler);

    /// <summary>
    /// Subscribe to a UserStartedSpeaking event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<UserStartedSpeakingResponse> eventHandler);

    /// <summary>
    /// Subscribe to a Welcome event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<WelcomeResponse> eventHandler);

    /// <summary>
    /// Subscribe to a Close event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<CloseResponse> eventHandler);

    /// <summary>
    /// Subscribe to an Unhandled event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<UnhandledResponse> eventHandler);

    /// <summary>
    /// Subscribe to an Error event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<ErrorResponse> eventHandler);

    /// <summary>
    /// Subscribe to a SettingsApplied event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<SettingsAppliedResponse> eventHandler);

    /// <summary>
    /// Subscribe to an InjectionRefused event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<InjectionRefusedResponse> eventHandler);

    /// <summary>
    /// Subscribe to an InstructionsUpdated event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<PromptUpdatedResponse> eventHandler);

    /// <summary>
    /// Subscribe to a SpeakUpdated event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<SpeakUpdatedResponse> eventHandler);

    /// <summary>
    /// Subscribe to a History event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<HistoryResponse> eventHandler);
    #endregion

    #region Send Functions
    /// <summary>
    /// Sends a KeepAlive message to Deepgram
    /// </summary>
    public Task SendKeepAlive();

    /// <summary>
    /// Sends an InjectUserMessage to the agent
    /// </summary>
    /// <param name="content">The specific phrase or statement the agent should respond to</param>
    public Task SendInjectUserMessage(string content);

    /// <summary>
    /// Sends an InjectUserMessage to the agent using a schema object
    /// </summary>
    /// <param name="injectUserMessageSchema">The InjectUserMessage schema containing the message details</param>
    public Task SendInjectUserMessage(InjectUserMessageSchema injectUserMessageSchema);

    /// <summary>
    /// Sends a Close message to Deepgram
    /// </summary>
    public Task SendClose(bool nullByte = false, CancellationTokenSource? _cancellationToken = null);

    /// <summary>
    /// This method sends a binary message over the WebSocket connection.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="length">The number of bytes from the data to send. Use `Constants.UseArrayLengthForSend` to send the entire array.</param>
    public void SendBinary(byte[] data, int length = Constants.UseArrayLengthForSend);

    /// <summary>
    /// This method sends a text message over the WebSocket connection.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="length">The number of bytes from the data to send. Use `Constants.UseArrayLengthForSend` to send the entire array.</param>
    public void SendMessage(byte[] data, int length = Constants.UseArrayLengthForSend);

    /// <summary>
    /// This method sends a binary message over the WebSocket connection immediately without queueing.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="length">The number of bytes from the data to send. Use `Constants.UseArrayLengthForSend` to send the entire array.</param>
    /// /// <param name="_cancellationToken">Provide a cancel token to be used for the send function or use the internal one</param>
    public Task SendBinaryImmediately(byte[] data, int length = Constants.UseArrayLengthForSend, CancellationTokenSource? _cancellationToken = null);

    /// <summary>
    /// This method sends a text message over the WebSocket connection immediately without queueing.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="length">The number of bytes from the data to send. Use `Constants.UseArrayLengthForSend` to send the entire array.</param>
    /// /// <param name="_cancellationToken">Provide a cancel token to be used for the send function or use the internal one</param>
    public Task SendMessageImmediately(byte[] data, int length = Constants.UseArrayLengthForSend, CancellationTokenSource? _cancellationToken = null);

    /// <summary>
    /// Sends a history conversation text message to the agent
    /// </summary>
    /// <param name="role">The role (user or assistant) who spoke the statement</param>
    /// <param name="content">The actual statement that was spoken</param>
    public Task SendHistoryConversationText(string role, string content);

    /// <summary>
    /// Sends a history conversation text message to the agent using a schema object
    /// </summary>
    /// <param name="historyConversationText">The history conversation text schema containing the message details</param>
    public Task SendHistoryConversationText(HistoryConversationText historyConversationText);

    /// <summary>
    /// Sends a history function calls message to the agent
    /// </summary>
    /// <param name="functionCalls">List of function call objects to send as history</param>
    public Task SendHistoryFunctionCalls(List<HistoryFunctionCall> functionCalls);

    /// <summary>
    /// Sends a history function calls message to the agent using a schema object
    /// </summary>
    /// <param name="historyFunctionCalls">The history function calls schema containing the function call details</param>
    public Task SendHistoryFunctionCalls(HistoryFunctionCalls historyFunctionCalls);

    /// <summary>
    /// Sends a function call response back to the agent
    /// </summary>
    /// <param name="functionCallResponse">The function call response schema</param>
    public Task SendFunctionCallResponse(FunctionCallResponseSchema functionCallResponse);
    #endregion

    #region Helpers
    /// <summary>
    /// Retrieves the connection state of the WebSocket
    /// </summary>
    /// <returns>Returns the connection state of the WebSocket</returns>
    public WebSocketState State();

    /// <summary>
    /// Indicates whether the WebSocket is connected
    /// </summary>
    /// <returns>Returns true if the WebSocket is connected</returns>
    public bool IsConnected();
    #endregion
}
