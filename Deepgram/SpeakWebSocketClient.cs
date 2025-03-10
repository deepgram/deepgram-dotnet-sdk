﻿// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Clients.Speak.v2.WebSocket;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram;

/// <summary>
/// Implements the latest supported version of the Speak Client.
/// </summary>
public class SpeakWebSocketClient : Client
{
    public SpeakWebSocketClient(string apiKey = "", DeepgramWsClientOptions? deepgramClientOptions = null) : base(apiKey, deepgramClientOptions)
    {
    }
}
