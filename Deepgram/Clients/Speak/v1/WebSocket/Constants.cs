// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Clients.Speak.v1.WebSocket;

/// <summary> 
/// Headers of interest in the return values from the Deepgram Speak API.
/// </summary> 
public static class Constants
{
    // WS buffer size
    public const int BufferSize = 1024 * 16;
    public const int UseArrayLengthForSend = -1;

    // Default timeout for connect/disconnect
    public const int DefaultConnectTimeout = 5000;
    public const int DefaultDisconnectTimeout = 5000;

    public const int DefaultFlushPeriodInMs = 500;

    // user message types
    public const string Speak = "Speak";
    public const string Flush = "Flush";
    public const string Clear = "Clear";
    public const string Close = "Close";
}

