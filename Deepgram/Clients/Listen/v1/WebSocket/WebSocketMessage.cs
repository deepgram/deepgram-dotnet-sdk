// Copyright 2021-2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Clients.Listen.v1.WebSocket;

internal readonly struct WebSocketMessage
{
    public WebSocketMessage(byte[] message, WebSocketMessageType type)
        : this(message, type, Constants.UseArrayLengthForSend)
    {
    }

    public WebSocketMessage(byte[] message, WebSocketMessageType type, int length)
    {
        if (length != Constants.UseArrayLengthForSend || length < Constants.UseArrayLengthForSend)
        {
            Message = new ArraySegment<byte>(message, 0, length);
        }
        else
        {
            Message = new ArraySegment<byte>(message, 0, message.Length);
        }
        MessageType = type;
        Length = length;
    }

    public int Length { get; }

    public ArraySegment<byte> Message { get; }

    public WebSocketMessageType MessageType { get; }
}
