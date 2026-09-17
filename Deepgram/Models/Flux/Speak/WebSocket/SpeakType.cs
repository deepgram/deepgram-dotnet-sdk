// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.Speak.WebSocket;

using Deepgram.Models.Common.v2.WebSocket;

/// <summary>
/// Message types used by the Deepgram Flux (v2 speak) text-to-speech WebSocket API.
/// </summary>
public enum SpeakType
{
    Open = WebSocketType.Open,
    Close = WebSocketType.Close,
    Unhandled = WebSocketType.Unhandled,
    /// <summary>
    /// Fatal error message. The wire value of the "type" property is literally "Error".
    /// </summary>
    Error = WebSocketType.Error,
    /// <summary>
    /// Local-only type for a binary audio chunk received from the server.
    /// </summary>
    Audio,
    Connected,
    SpeechStarted,
    SpeechMetadata,
    SpeechInterrupted,
    Flushed,
    SessionMetadata,
    Warning,
    ConfigureSuccess,
    ConfigureFailure,
}
