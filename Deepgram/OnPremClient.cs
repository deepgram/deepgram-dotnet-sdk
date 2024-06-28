// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Clients.OnPrem.v1;
using Deepgram.Models.Authenticate.v1;

namespace Deepgram;

/// <summary>
// *********** WARNING ***********
// This class provides the OnPrem Client implementation
//
// Deprecated: This class is deprecated. Use SelfHostedClient instead.
// This will be removed in a future release.
//
// This package is frozen and no new functionality will be added.
// *********** WARNING ***********
/// </summary>
[Obsolete("Please use SelfHostedClient instead", false)]
public class OnPremClient : Client
{
    public OnPremClient(string apiKey = "", DeepgramHttpClientOptions? deepgramClientOptions = null,
        string? httpId = null) : base(apiKey, deepgramClientOptions, httpId)
    {
    }
}
