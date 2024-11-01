// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Common.v2.WebSocket;

namespace Deepgram.Models.Speak.v2.WebSocket;

public enum SpeakType
{
    Open = WebSocketType.Open,
    Close = WebSocketType.Close,
    Unhandled = WebSocketType.Unhandled,
    Error = WebSocketType.Error,
    Metadata,
    Flushed,
    Cleared,
    Reset,
    Audio,
    Warning,
}
