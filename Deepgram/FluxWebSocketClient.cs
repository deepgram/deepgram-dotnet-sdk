// Copyright 2026 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Clients.Flux.WebSocket;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram;

/// <summary>
/// (PREVIEW) Implements the latest supported version of the Flux (v2 listen) Client.
/// </summary>
public class FluxWebSocketClient : Client
{
    public FluxWebSocketClient(string apiKey = "", DeepgramWsClientOptions? deepgramClientOptions = null) : base(apiKey, deepgramClientOptions)
    {
    }
}
