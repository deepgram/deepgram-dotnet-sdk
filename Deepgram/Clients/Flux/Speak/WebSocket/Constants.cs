// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Clients.Flux.Speak.WebSocket;

/// <summary>
/// (PREVIEW) Constants for the Flux (v2 speak) text-to-speech WebSocket client.
/// </summary>
public static class Constants
{
    // Client message types. The Early Access surface sends exactly three: Speak, Flush, Close.
    // (Interrupt and Configure are GA-only and not yet available on /v2/speak.)
    public const string Speak = "Speak";
    public const string Flush = "Flush";
    public const string Close = "Close";

    /// <summary>
    /// After sending Close, how long to wait (in milliseconds) for the server to drain all
    /// remaining and queued audio, emit a final SessionMetadata, and close the connection before
    /// forcing the socket shut. Kept below the Abstractions.v2.Constants.DefaultDisconnectTimeout
    /// (5000ms) used by Stop().
    /// </summary>
    public const int DefaultCloseGracePeriodMs = 3000;
}
