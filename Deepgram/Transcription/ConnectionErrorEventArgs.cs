using System;

namespace Deepgram.Transcription
{
    public class ConnectionErrorEventArgs : EventArgs
    {
        public Exception Exception;

        public ConnectionErrorEventArgs(Exception e)
        {
            Exception = e;
        }
    }
}
