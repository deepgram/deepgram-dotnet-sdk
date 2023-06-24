using System;

namespace Deepgram.Models
{
    internal readonly struct MessageToSend
    {
        public MessageToSend(byte[] message)
        {
            Message = new ArraySegment<byte>(message);
        }

        public ArraySegment<byte> Message { get; }
    }
}
