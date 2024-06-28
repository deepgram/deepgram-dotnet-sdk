// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using Deepgram.Models.Authenticate.v1;
using Deepgram.Clients.Interfaces.v1;
using REST = Deepgram.Clients.Listen.v1.REST;

namespace Deepgram.Clients.PreRecorded.v1;

/// <summary>
// *********** WARNING ***********
// This class provides the PreRecorded Client implementation
//
// Deprecated: This class is deprecated. Use the `Deepgram.Clients.Listen.v1.REST` namespace instead.
// This will be removed in a future release.
//
// This package is frozen and no new functionality will be added.
// *********** WARNING ***********
/// </summary>
[Obsolete("Please use Deepgram.Clients.Listen.v1.REST.Client instead", false)]
public class Client(string? apiKey = null, IDeepgramClientOptions? deepgramClientOptions = null, string? httpId = null)
    : REST.Client(apiKey, deepgramClientOptions, httpId), IPreRecordedClient
{
}
