// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Listen.v1.WebSocket;

namespace Deepgram.Clients.Interfaces.v1;

/// <summary>
/// Implements version 1 of the Live Client.
/// </summary>
public interface IListenWebSocketClient
{
    #region Connect and Disconnect
    public Task Connect(LiveSchema options, CancellationTokenSource? cancelToken = null, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null);

    public Task Stop(CancellationTokenSource? cancelToken = null, bool nullByte = false);
    #endregion

    #region Subscribe Event
    /// <summary>
    /// Subscribe to an Open event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<OpenResponse> eventHandler);

    /// <summary>
    /// Subscribe to a Metadata event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<MetadataResponse> eventHandler);

    /// <summary>
    /// Subscribe to a Results event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<ResultResponse> eventHandler);

    /// <summary>
    /// Subscribe to an UtteranceEnd event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<UtteranceEndResponse> eventHandler);

    /// <summary>
    /// Subscribe to a SpeechStarted event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<SpeechStartedResponse> eventHandler);

    /// <summary>
    /// Subscribe to a Close event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<CloseResponse> eventHandler);

    /// <summary>
    /// Subscribe to an Unhandled event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<UnhandledResponse> eventHandler);

    /// <summary>
    /// Subscribe to an Error event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public bool Subscribe(EventHandler<ErrorResponse> eventHandler);
    #endregion

    #region Send Functions
    /// <summary>
    /// Sends a KeepAlive message to Deepgram
    /// </summary>
    public void SendKeepAlive();

    /// <summary>
    /// Sends a Finalize message to Deepgram
    /// </summary>
    public void SendFinalize();

    /// <summary>
    /// Sends a Close message to Deepgram
    /// </summary>
    public void SendClose(bool nullByte = false);

    /// <summary>
    /// Sends a binary message over the WebSocket connection.
    /// </summary>
    /// <param name="data">The data to be sent over the WebSocket.</param>
    public void Send(byte[] data, int length = Constants.UseArrayLengthForSend);

    /// <summary>
    /// This method sends a binary message over the WebSocket connection.
    /// </summary>
    /// <param name="data"></param>
    public void SendBinary(byte[] data, int length = Constants.UseArrayLengthForSend);

    /// <summary>
    /// This method sends a text message over the WebSocket connection.
    /// </summary>
    public void SendMessage(byte[] data, int length = Constants.UseArrayLengthForSend);

    /// <summary>
    /// This method sends a binary message over the WebSocket connection immediately without queueing.
    /// </summary>
    public void SendBinaryImmediately(byte[] data, int length = Constants.UseArrayLengthForSend);

    /// <summary>
    /// This method sends a text message over the WebSocket connection immediately without queueing.
    /// </summary>
    public void SendMessageImmediately(byte[] data, int length = Constants.UseArrayLengthForSend);
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
