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
        : this(message, type, length, null)
    {
    }

    /// <param name="owner">
    /// The connection an SDK-internal message belongs to, or null for a user message. Another
    /// connection's sender drops a message that belongs to a different connection.
    /// </param>
    internal WebSocketMessage(byte[] message, WebSocketMessageType type, int length, AbstractWebSocketClient.ConnectionState? owner)
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
        Owner = owner;
    }

    private WebSocketMessage(TaskCompletionSource<bool> flushSignal, AbstractWebSocketClient.ConnectionState? owner)
    {
        Message = new ArraySegment<byte>(Array.Empty<byte>());
        MessageType = WebSocketMessageType.Binary;
        FlushSignal = flushSignal;
        Owner = owner;
    }

    /// <summary>
    /// Creates a flush marker: a queue entry that carries no payload to send, and instead
    /// completes <paramref name="flushSignal"/> once the sender thread reaches it — i.e. once
    /// every message enqueued before it has been sent.
    /// </summary>
    public static WebSocketMessage CreateFlushMarker(TaskCompletionSource<bool> flushSignal) =>
        new WebSocketMessage(flushSignal, null);

    /// <summary>
    /// Creates a flush marker that belongs to one connection: another connection's sender
    /// completes it with false instead of treating it as reached.
    /// </summary>
    internal static WebSocketMessage CreateFlushMarker(TaskCompletionSource<bool> flushSignal, AbstractWebSocketClient.ConnectionState? owner) =>
        new WebSocketMessage(flushSignal, owner);

    public int Length => Message.Count;

    public ArraySegment<byte> Message { get; }

    public WebSocketMessageType MessageType { get; }

    /// <summary>
    /// When non-null, this entry is a flush marker rather than data to send. The sender thread
    /// completes it in queue order instead of writing it to the socket.
    /// </summary>
    public TaskCompletionSource<bool>? FlushSignal { get; }

    /// <summary>
    /// The connection this SDK-internal message belongs to; null for user messages, which go to
    /// whichever connection's sender reads them.
    /// </summary>
    internal AbstractWebSocketClient.ConnectionState? Owner { get; }
}
