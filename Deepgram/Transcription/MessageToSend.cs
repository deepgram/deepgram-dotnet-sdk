using System;

namespace Deepgram.Transcription
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
