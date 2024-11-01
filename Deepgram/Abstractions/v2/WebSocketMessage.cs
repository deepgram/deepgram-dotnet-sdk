// Copyright 2024 Deepgram .NET SDK contributors. All Rights Reserved.
// Use of this source code is governed by a MIT license that can be found in the LICENSE file.
// SPDX-License-Identifier: MIT

namespace Deepgram.Abstractions.v2;

internal readonly struct WebSocketMessage
{
    public WebSocketMessage(byte[] message, WebSocketMessageType type)
        : this(message, type, Constants.UseArrayLengthForSend)
    {
    }

    public WebSocketMessage(byte[] message, WebSocketMessageType type, int length)
    {
        if (length != Constants.UseArrayLengthForSend && length <= message.Length && length > 0)
        {
            Message = new ArraySegment<byte>(message, 0, length);
        }
        else
        {
            Message = new ArraySegment<byte>(message, 0, message.Length);
        }
        MessageType = type;
    }

    public int Length => Message.Count;

    public ArraySegment<byte> Message { get; }

    public WebSocketMessageType MessageType { get; }
}
