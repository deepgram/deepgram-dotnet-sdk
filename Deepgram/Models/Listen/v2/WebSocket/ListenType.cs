// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Listen.v2.WebSocket;

using Deepgram.Models.Common.v2.WebSocket;

public enum ListenType
{
    Open = WebSocketType.Open,
    Close = WebSocketType.Close,
    Unhandled = WebSocketType.Unhandled,
    Error = WebSocketType.Error,
    Metadata,
    Results,
    UtteranceEnd,
    SpeechStarted,
}
