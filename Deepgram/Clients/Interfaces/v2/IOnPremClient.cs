// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Clients.Interfaces.v2;

/// <summary>
// *********** WARNING ***********
// This class provides the IOnPremClient implementation
//
// Deprecated: This class is deprecated. Use ISelfHostedClient instead.
// This will be removed in a future release.
//
// This package is frozen and no new functionality will be added.
// *********** WARNING ***********
/// </summary>
[Obsolete("Please use ISelfHostedClient instead", false)]
public interface IOnPremClient : ISelfHostedClient
{
}
