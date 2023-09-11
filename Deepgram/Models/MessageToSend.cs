using System;
using System.Net.WebSockets;

namespace Deepgram.Models
{
    internal readonly struct MessageToSend
    {
        public MessageToSend(byte[] message, WebSocketMessageType type)
        {
            Message = new ArraySegment<byte>(message);
            MessageType = type;
        }

        public ArraySegment<byte> Message { get; }

        public WebSocketMessageType MessageType { get; }
    }
}
