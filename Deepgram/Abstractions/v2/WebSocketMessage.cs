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
        FlushSignal = null;
    }

    private WebSocketMessage(TaskCompletionSource<bool> flushSignal)
    {
        Message = new ArraySegment<byte>(Array.Empty<byte>());
        MessageType = WebSocketMessageType.Binary;
        FlushSignal = flushSignal;
    }

    /// <summary>
    /// Creates a flush marker: a queue entry that carries no payload to send, and instead
    /// completes <paramref name="flushSignal"/> once the sender thread reaches it — i.e. once
    /// every message enqueued before it has been sent.
    /// </summary>
    public static WebSocketMessage CreateFlushMarker(TaskCompletionSource<bool> flushSignal) =>
        new WebSocketMessage(flushSignal);

    public int Length => Message.Count;

    public ArraySegment<byte> Message { get; }

    public WebSocketMessageType MessageType { get; }

    /// <summary>
    /// When non-null, this entry is a flush marker rather than data to send. The sender thread
    /// completes it in queue order instead of writing it to the socket.
    /// </summary>
    public TaskCompletionSource<bool>? FlushSignal { get; }
}
