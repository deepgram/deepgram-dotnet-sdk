// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

using WS = Deepgram.Models.Listen.v1.WebSocket;

namespace Deepgram.Models.Live.v1;

/// <summary>
// *********** WARNING ***********
// This class provides the Channel implementation
//
// Deprecated: This class is deprecated. Use the `Deepgram.Clients.Listen.v1.WebSocket` namespace instead.
// This will be removed in a future release.
//
// This package is frozen and no new functionality will be added.
// *********** WARNING ***********
/// </summary>
[Obsolete("Please use Deepgram.Clients.Listen.v1.WebSocket instead", false)]
public record Channel : WS.Channel
{
}
