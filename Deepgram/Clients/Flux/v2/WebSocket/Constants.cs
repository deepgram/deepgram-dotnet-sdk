// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Clients.Flux.v2.WebSocket;

/// <summary>
/// (PREVIEW) Constants for the Flux (v2 listen) WebSocket client.
/// </summary>
public static class Constants
{
    // user message types.
    // Flux accepts exactly two client control messages: CloseStream and Configure.
    // KeepAlive and Finalize are v1-only and are rejected by the v2 endpoint
    // with an UNPARSABLE_CLIENT_MESSAGE error.
    public const string CloseStream = "CloseStream";
    public const string Configure = "Configure";

    /// <summary>
    /// After sending CloseStream, how long to wait (in milliseconds) for the server to deliver
    /// the final results and close the connection before forcing the socket shut. Kept below the
    /// Abstractions.v2.Constants.DefaultDisconnectTimeout (5000ms) used by Stop().
    /// </summary>
    public const int DefaultCloseStreamGracePeriodMs = 3000;
}
