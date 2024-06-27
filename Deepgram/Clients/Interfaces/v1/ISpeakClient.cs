// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Clients.Interfaces.v1;

/// <summary>
// *********** WARNING ***********
// This class provides the ISpeakClient implementation for the Deepgram API
//
// Deprecated: This class is deprecated. Use the ISpeakRESTClient interface instead.
// This will be removed in a future release.
//
// This package is frozen and no new functionality will be added.
// *********** WARNING ***********
/// </summary>
[Obsolete("Please use ISpeakRESTClient instead", false)]
public interface ISpeakClient : ISpeakRESTClient
{
}
