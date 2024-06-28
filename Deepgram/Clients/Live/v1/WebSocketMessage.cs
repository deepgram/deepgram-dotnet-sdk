// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Clients.Live.v1;

/// <summary>
// *********** WARNING ***********
// This class provides the WebSocketMessage implementation
//
// Deprecated: This class is deprecated. Use the `Deepgram.Clients.Listen.v1.WebSocket` namespace instead.
// This will be removed in a future release.
//
// This package is frozen and no new functionality will be added.
// *********** WARNING ***********
/// </summary>
[Obsolete("Please use Deepgram.Clients.Listen.v1.WebSocket instead", false)]
internal readonly struct WebSocketMessage(byte[] message, WebSocketMessageType type)
{
    public ArraySegment<byte> Message { get; } = new ArraySegment<byte>(message);

    public WebSocketMessageType MessageType { get; } = type;
}
