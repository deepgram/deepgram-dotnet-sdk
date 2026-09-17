// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Flux.WebSocket;

using Deepgram.Models.Common.v2.WebSocket;

/// <summary>
/// (PREVIEW) Message types used by the Deepgram Flux (v2 listen) WebSocket API.
/// </summary>
public enum FluxType
{
    Open = WebSocketType.Open,
    Close = WebSocketType.Close,
    Unhandled = WebSocketType.Unhandled,
    /// <summary>
    /// Fatal error message. The wire value of the "type" property is literally "Error".
    /// </summary>
    Error = WebSocketType.Error,
    Connected,
    TurnInfo,
    ConfigureSuccess,
    ConfigureFailure,
}
