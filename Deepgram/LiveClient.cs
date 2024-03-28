// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Clients.Live.v1;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram;

/// <summary>
/// Implements the latest supported version of the Live Client.
/// </summary>
public class LiveClient : Client
{
    public LiveClient(string apiKey = "", DeepgramWsClientOptions? deepgramClientOptions = null) : base(apiKey, deepgramClientOptions)
    {
    }
}
