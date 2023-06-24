using System;

namespace Deepgram.CustomEventArgs
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
