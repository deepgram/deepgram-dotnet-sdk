namespace Deepgram.DeepgramEventArgs;

public class LiveErrorEventArgs : EventArgs
{
    public Exception Exception { get; set; }
    public LiveErrorEventArgs(Exception ex)
    {
        Exception = ex;
    }
}
