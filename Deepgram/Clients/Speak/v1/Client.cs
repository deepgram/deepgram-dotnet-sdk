// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Speak.v1;
using Deepgram.Models.Authenticate.v1;
using Deepgram.Clients.Interfaces.v1;

namespace Deepgram.Clients.Speak.v1;

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
[Obsolete("Please use Deepgram.Clients.Speak.v1.REST.Client instead", false)]
public class Client(string? apiKey = null, IDeepgramClientOptions? deepgramClientOptions = null, string? httpId = null)
    : REST.Client(apiKey, deepgramClientOptions, httpId), ISpeakClient
{
}
