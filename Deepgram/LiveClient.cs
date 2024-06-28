// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Clients.Live.v1;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram;

/// <summary>
// *********** WARNING ***********
// This class provides the LiveClient implementation
//
// Deprecated: This class is deprecated. Use the `ListenWebSocketClient` class instead.
// This will be removed in a future release.
//
// This class is frozen and no new functionality will be added.
// *********** WARNING ***********
/// </summary>
[Obsolete("Please use ListenWebSocketClient instead", false)]
public class LiveClient : Client
{
    public LiveClient(string apiKey = "", DeepgramWsClientOptions? deepgramClientOptions = null) : base(apiKey, deepgramClientOptions)
    {
    }
}
