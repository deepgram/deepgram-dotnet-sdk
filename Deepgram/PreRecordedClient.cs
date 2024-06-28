// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Clients.PreRecorded.v1;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram;

/// <summary>
// *********** WARNING ***********
// This class provides the PreRecordedClient implementation
//
// Deprecated: This class is deprecated. Use the `ListenRESTClient` class instead.
// This will be removed in a future release.
//
// This class is frozen and no new functionality will be added.
// *********** WARNING ***********
[Obsolete("Please use ListenRESTClient instead", false)]
public class PreRecordedClient : Client
{
    public PreRecordedClient(string apiKey = "", DeepgramHttpClientOptions? deepgramClientOptions = null,
        string? httpId = null) : base(apiKey, deepgramClientOptions, httpId)
    {
    }
}
