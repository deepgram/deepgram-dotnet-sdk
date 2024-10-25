// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Clients.Interfaces.v2;

/// <summary>
// *********** WARNING ***********
// This is the IPreRecordedClient interface
//
// Deprecated: This class is deprecated. Use the `IListenRESTClient` function instead.
// This will be removed in a future release.
//
// This class is frozen and no new functionality will be added.
// *********** WARNING ***********
/// </summary>
[Obsolete("Please use IListenRESTClient instead", false)]
public interface IPreRecordedClient : IListenRESTClient
{
}
