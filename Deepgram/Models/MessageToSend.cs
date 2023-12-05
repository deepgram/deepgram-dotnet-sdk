using System.Net.WebSockets;

namespace Deepgram.Models;

internal readonly struct MessageToSend(byte[] message, WebSocketMessageType type)
{
    public ArraySegment<byte> Message { get; } = new ArraySegment<byte>(message);

    public WebSocketMessageType MessageType { get; } = type;
}
