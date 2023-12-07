namespace Deepgram.DeepgramEventArgs;

public class ConnectionErrorEventArgs : EventArgs
{
    public Exception Exception { get; set; }
    public ConnectionErrorEventArgs(Exception ex)
    {
        Exception = ex;
    }
}
