// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Clients.Speak.v1;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram;

/// <summary>
// *********** WARNING ***********
// This package provides the Speak Client implementation for the Deepgram API
//
// Deprecated: This class is deprecated. Use the namespace `Deepgram.Clients.Speak.v1.REST` instead.
// This will be removed in a future release.
//
// This class is frozen and no new functionality will be added.
// *********** WARNING ***********
/// </summary>
[Obsolete("Please use SpeakRESTClient instead", false)]
public class SpeakClient : Client
{
    public SpeakClient(string apiKey = "", DeepgramHttpClientOptions? deepgramClientOptions = null,
        string? httpId = null) : base(apiKey, deepgramClientOptions, httpId)
    {
    }
}
