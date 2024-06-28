// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Live.v1;

namespace Deepgram.Clients.Interfaces.v1;

/// <summary>
// *********** WARNING ***********
// This is the ILiveClient interface
//
// Deprecated: This class is deprecated. Use the `IListenWebSocketClient` function instead.
// This will be removed in a future release.
//
// This class is frozen and no new functionality will be added.
// *********** WARNING ***********
/// </summary>
[Obsolete("Please use IListenWebSocketClient instead", false)]
public interface ILiveClient : IListenWebSocketClient
{
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
}
