// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Models.Speak.v1.WebSocket;

public enum SpeakType
{
    Open,
    Metadata,
    Flushed,
    Reset,
    Audio,
    Close,
    Unhandled,
    Warning,
    Error,
}
