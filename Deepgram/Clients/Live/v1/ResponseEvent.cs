// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using WS = Deepgram.Clients.Listen.v1.WebSocket;
using Deepgram.Clients.Interfaces.v1;

namespace Deepgram.Clients.Live.v1;

/// <summary>
// *********** WARNING ***********
// This class provides the ResponseEvent implementation
//
// Deprecated: This class is deprecated. Use the `Deepgram.Clients.Listen.v1.WebSocket` namespace instead.
// This will be removed in a future release.
//
// This package is frozen and no new functionality will be added.
// *********** WARNING ***********
/// </summary>
[Obsolete("Please use Deepgram.Clients.Listen.v1.WebSocket instead", false)]
public class ResponseEvent<T>(T? response) : WS.ResponseEvent<T>(response)
{
}
