// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Speak.v2.WebSocket;

namespace Deepgram.Clients.Interfaces.v2;

/// <summary>
/// Implements version 2 of the Live Client.
/// </summary>
public interface ISpeakWebSocketClient
{
    #region Connect and Disconnect
    public Task<bool> Connect(SpeakSchema options, CancellationTokenSource? cancelToken = null, Dictionary<string, string>? addons = null,
        Dictionary<string, string>? headers = null);

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
    /// Subscribe to a Metadata event from the Deepgram API
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<MetadataResponse> eventHandler);

    /// <summary>
    /// Subscribe to a Flushed event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<FlushedResponse> eventHandler);

    /// <summary>
    /// Subscribe to a Cleared event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<ClearedResponse> eventHandler);

    /// <summary>
    /// Subscribe to a Audio buffer/event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<AudioResponse> eventHandler);

    /// <summary>
    /// Subscribe to a Close event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<CloseResponse> eventHandler);

    /// <summary>
    /// Subscribe to an Warning event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<WarningResponse> eventHandler);


    /// <summary>
    /// Subscribe to an Error event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<ErrorResponse> eventHandler);

    /// <summary>
    /// Subscribe to an Unhandled event from the Deepgram API
    /// </summary>
    /// <returns>True if successful</returns>
    public Task<bool> Subscribe(EventHandler<UnhandledResponse> eventHandler);
    #endregion

    #region Send Functions
    /// <summary>
    /// Sends text data over the WebSocket connection.
    /// </summary>
    /// <param name="data"></param>
    public void SpeakWithText(string data);

    ///// <summary>
    /////  This method Flushes the text buffer on Deepgram to be converted to audio
    ///// </summary>
    public void Flush();

    ///// <summary>
    /////  This method Resets the text buffer on Deepgram to be converted to audio
    ///// </summary>
    public void Clear();

    ///// <summary>
    /////  This method tells Deepgram to initiate the close server-side.
    ///// </summary>
    public void Close(bool nullByte = false);

    ///// <summary>
    ///// This method sends a binary message over the WebSocket connection.
    ///// </summary>
    ///// <param name="data"></param>
    //public void SpeakWithStream(byte[] data);

    /// <summary>
    /// Sends a Close message to Deepgram
    /// </summary>
    public Task SendClose(bool nullByte = false);

    /// <summary>
    /// Sends a binary message over the WebSocket connection.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="length">The number of bytes from the data to send. Use `Constants.UseArrayLengthForSend` to send the entire array.</param>
    public void Send(byte[] data, int length = Constants.UseArrayLengthForSend);

    ///// <summary>
    ///// This method sends a binary message over the WebSocket connection.
    ///// </summary>
    ///// <param name="data"></param>
    //public void SendBinary(byte[] data, int length = Constants.UseArrayLengthForSend);

    /// <summary>
    /// This method sends a text message over the WebSocket connection.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="length">The number of bytes from the data to send. Use `Constants.UseArrayLengthForSend` to send the entire array.</param>
    public void SendMessage(byte[] data, int length = Constants.UseArrayLengthForSend);

    ///// <summary>
    ///// This method sends a binary message over the WebSocket connection immediately without queueing.
    ///// </summary>
    //public Task SendBinaryImmediately(byte[] data, int length = Constants.UseArrayLengthForSend);

    /// <summary>
    /// This method sends a text message over the WebSocket connection immediately without queueing.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="length">The number of bytes from the data to send. Use `Constants.UseArrayLengthForSend` to send the entire array.</param>
    public Task SendMessageImmediately(byte[] data, int length = Constants.UseArrayLengthForSend);
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
